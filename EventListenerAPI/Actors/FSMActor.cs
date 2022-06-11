using System.Collections.Immutable;

using Akka.Actor;
using Akka.Event;

namespace EventListenerAPI.Actors
{
    public class FSMActor : FSM<State, IData>
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public FSMActor()
        {
            // <StartWith>
            StartWith(State.Idle, Uninitialized.Instance);
            // </StartWith>

            When(State.Idle, state =>
            {
                if (state.FsmEvent is SetTarget target && state.StateData is Uninitialized)
                {
                    return Stay().Using(new Todo(target.Ref, ImmutableList<object>.Empty));
                }

                return null;
            });

            When(State.Active, state =>
            {
                if ((state.FsmEvent is Flush || state.FsmEvent is StateTimeout) 
                    && state.StateData is Todo t)
                {
                    return GoTo(State.Idle).Using(t.Copy(ImmutableList<object>.Empty));
                }

                return null;
            }, TimeSpan.FromSeconds(1));


            WhenUnhandled(state =>
            {
                if (state.FsmEvent is Queue && state.StateData is Todo)
                {
                    Todo t = state.StateData as Todo;
                    Queue q = state.FsmEvent as Queue;
                    var data = t.Copy(t.Queue.Add(q.Obj));

                    t.Target.Tell(data.Queue.Count);
                    
                    return GoTo(State.Active).Using(data);
                }
                else
                {
                    _log.Warning("Received unhandled request {0} in state {1}/{2}", state.FsmEvent, StateName, state.StateData);
                    return Stay();
                }
            });
 
            OnTransition((initialState, nextState) =>
            {
                if (initialState == State.Active && nextState == State.Idle)
                {
                    if (StateData is Todo)
                    {
                        Todo todo = StateData as Todo;
                        todo.Target.Tell(new Batch(todo.Queue));
                    }
                    else
                    {
                        // nothing to do
                    }
                }
            });


            Initialize();
        }
    }
}
