using System;
using System.Collections.Generic;
using DarwinClient.Parsers;

namespace DarwinClient
{
    internal class PushPortObserverParserComparer : IEqualityComparer<PushPortObservers>
    { 
        public bool Equals(PushPortObservers x, PushPortObservers y)
        {
            return x?.Parser == y?.Parser;
        }

        public int GetHashCode(PushPortObservers observers)
        {
            return observers.Parser.GetHashCode();
        }
    }
    
    /// <summary>
    /// List of <see cref="System.IObserver{Message}"/> subscribed to pushport
    /// Also has the Parser <see cref="IMessageParser"/> instance to convert the
    /// Active MQ Message to the correct internal <see cref="Message"/> type
    /// </summary>
    internal class PushPortObservers : List<IObserver<Message>>, IDisposable
    {
        internal IMessageParser Parser { get; }

        internal PushPortObservers(IMessageParser parser) : base()
        {
            Parser = parser;
        }
        
        internal IDisposable  Subscribe(IObserver<Message> observer)
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
                // Clone into an array to remove concurrency problem involving altering enumeration
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