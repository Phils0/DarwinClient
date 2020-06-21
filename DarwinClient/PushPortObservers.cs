using System;
using System.Collections.Generic;

namespace DarwinClient
{
    internal class PushPortObservers : List<IObserver<Message>>, IDisposable
    {
        internal IDisposable Subscribe(IObserver<Message> observer)
        {
            // Check whether observer is already registered. If not, add it
            if (!Contains(observer)) {
                Add(observer);
            }
            return new Unsubscriber(this, observer);
        }

        private void UnSubscribe(IObserver<Message> observer)
        {
            if (Contains(observer))
                Remove(observer);
        }
        
        private class Unsubscriber : IDisposable
        {
            private PushPortObservers _observers;
            private IObserver<Message> _observer;

            internal Unsubscriber(PushPortObservers observers, IObserver<Message> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                _observers.UnSubscribe(_observer);
            }
        }
        
        internal void UnsubscribeAll(bool isError)
        {
            if (!isError)
            {
                // Clone into an array so removing does not affect enumeration
                foreach (var observer in this.ToArray())
                    observer.OnCompleted();                
            }

            Clear();
        }

        public void Dispose()
        {
            UnsubscribeAll(false);
        }
    }
}