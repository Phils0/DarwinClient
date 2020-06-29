using System;
using DarwinClient.Parsers;
using NSubstitute;
using Xunit;

namespace DarwinClient.Test
{
    public class PushportObserversTest
    {
        [Fact]
        public void Subscribe()
        {
            var observer = Substitute.For<IPushPortObserver>();
            
            var observers = new PushPortObservers(Substitute.For<IMessageParser>());
            observers.Subscribe(observer);
            Assert.Contains(observer, observers);
        }
        
        [Fact]
        public void Unsubscribe()
        {
            var observer = Substitute.For<IPushPortObserver>();
            
            var observers = new PushPortObservers(Substitute.For<IMessageParser>());
            var unsubscribe = observers.Subscribe(observer);
            Assert.Contains(observer, observers);
            
            unsubscribe.Dispose();
            Assert.DoesNotContain(observer, observers);
        }
        
        [Fact]
        public void UnsubscribeAll()
        {
            var observer1 = Substitute.For<IPushPortObserver>();
            var observer2 = Substitute.For<IPushPortObserver>();
           
            var observers = new PushPortObservers(Substitute.For<IMessageParser>());
            observers.Subscribe(observer1);
            observers.Subscribe(observer2);
            Assert.NotEmpty(observers);
            
            observers.UnsubscribeAll(false);
            Assert.Empty(observers);
            observer1.Received().OnCompleted();
            observer2.Received().OnCompleted();
        }
        
        [Fact]
        public void UnsubscribeAllDoesNotRaiseOnCompletedIfIsError()
        {
            var observer1 = Substitute.For<IPushPortObserver>();
            var observer2 = Substitute.For<IPushPortObserver>();
           
            var observers = new PushPortObservers(Substitute.For<IMessageParser>());
            observers.Subscribe(observer1);
            observers.Subscribe(observer2);
            Assert.NotEmpty(observers);
            
            observers.UnsubscribeAll(true);
            Assert.Empty(observers);
            observer1.DidNotReceive().OnCompleted();
            observer2.DidNotReceive().OnCompleted();
        }
    }
}