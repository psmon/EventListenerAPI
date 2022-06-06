using System.Collections.Immutable;

using Akka.Actor;

namespace EventListenerAPI.Actors
{

    // received events
    public class SetTarget
    {
        public SetTarget(IActorRef @ref)
        {
            Ref = @ref;
        }

        public IActorRef Ref { get; }
    }

    public class Queue
    {
        public Queue(object obj)
        {
            Obj = obj;
        }

        public Object Obj { get; }
    }

    public class Flush { }

    // send events
    public class Batch
    {
        public Batch(ImmutableList<object> obj)
        {
            Obj = obj;
        }

        public ImmutableList<object> Obj { get; }
    }

}
