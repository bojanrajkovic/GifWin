using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace GifWin.Core.Services
{
    [PublicAPI]
    public static class MessagingService
    {
        static readonly Dictionary<Type, object> Subscribers = new Dictionary<Type, object>();

        delegate bool ProcessMessage<in T>(T messageType);

        class Subscription<T> : IDisposable
        {
            public Subscription(Func<T, bool> handler) =>
                Handler = new ProcessMessage<T>(handler);

            public ProcessMessage<T> Handler { get; }

            public void Dispose()
            {
                var subs = (LinkedList<Subscription<T>>)Subscribers[typeof(T)];
                subs.Remove(this);
            }
        }

        public static void Send<T>(T message)
        {
            var messageType = typeof(T);

            if (Subscribers.TryGetValue(messageType, out var untypedSubscribers)) {
                var typedSubscribers = (LinkedList<Subscription<T>>)untypedSubscribers;
                foreach (var typedSubscriber in typedSubscribers) {
                    var result = typedSubscriber.Handler(message);

                    // If the handler returns `true`, that means we should stop processing this
                    // message.
                    if (result)
                        break;
                }
            }
        }

        public static IDisposable Subscribe<T>(Func<T, bool> handler)
        {
            var messageType = typeof(T);
            var sub = new Subscription<T>(handler);

            if (Subscribers.TryGetValue(messageType, out var untypedSubscribers)) {
                var typedSubscribers = (LinkedList<Subscription<T>>)untypedSubscribers;
                typedSubscribers.AddLast(sub);
            } else {
                var subscribersList = new LinkedList<Subscription<T>>();
                subscribersList.AddLast(sub);
                Subscribers[messageType] = subscribersList;
            }

            return sub;
        }
    }
}
