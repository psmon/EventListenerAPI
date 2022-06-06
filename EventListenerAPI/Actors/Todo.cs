using System.Collections.Immutable;

using Akka.Actor;

namespace EventListenerAPI.Actors
{
    // states
    public enum State
    {
        Idle,
        Active
    }

    // data
    public interface IData { }

    public class Uninitialized : IData
    {
        public static Uninitialized Instance = new Uninitialized();

        private Uninitialized() { }
    }

    public class Todo : IData
    {
        public Todo(IActorRef target, ImmutableList<object> queue)
        {
            Target = target;
            Queue = queue;
        }

        public IActorRef Target { get; }

        public ImmutableList<object> Queue { get; }

        public Todo Copy(ImmutableList<object> queue)
        {
            return new Todo(Target, queue);
        }
    }

}
