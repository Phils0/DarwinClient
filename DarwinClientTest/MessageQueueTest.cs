using System;
using DarwinClient.Test.Helpers;
using NSubstitute;
using Serilog;
using Xunit;

namespace DarwinClient.Test
{
    public class MessageQueueTest
    {
        [Fact]
        public void MessageType()
        {
            var queue = new MessageQueue(Substitute.For<ILogger>());
            Assert.Equal(typeof(DarwinMessage), queue.MessageType);            
        }
        
        [Fact]
        public void SubscribeTo()
        {
            var queue = new MessageQueue(Substitute.For<ILogger>());
            
            Assert.False(queue.IsLive);
            queue.SubscribeTo(Substitute.For<IPushPort>());
            Assert.True(queue.IsLive);
        }
        
        [Fact]
        public void ObserveMessageReceived()
        {
            var queue = new MessageQueue(Substitute.For<ILogger>());

            var msg = MessageGenerator.CreateDarwinMessage();
            queue.OnNext(msg);
            
            Assert.NotEmpty(queue);
        }
        
        [Fact]
        public void ObserveCompleteReceived()
        {
            var queue = new MessageQueue(Substitute.For<ILogger>());
            queue.SubscribeTo(Substitute.For<IPushPort>());
            
            Assert.True(queue.IsLive);
            queue.OnCompleted();
            Assert.False(queue.IsLive);
        }
        
        [Fact]
        public void ObserveErrorReceived()
        {
            var queue = new MessageQueue(Substitute.For<ILogger>());
            queue.SubscribeTo(Substitute.For<IPushPort>());
            
            Assert.True(queue.IsLive);
            queue.OnError(new Exception("Errored"));
            Assert.False(queue.IsLive);
        }
    }
}