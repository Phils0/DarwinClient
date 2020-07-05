using System;
using System.Collections.Generic;
using DarwinClient.Parsers;
using DarwinClient.Test.Helpers;
using NSubstitute;
using Serilog;
using Xunit;

namespace DarwinClient.Test
{
    public class MessagePublisherTest
    {
        [Fact]
        public void PublishToSubscriber()
        {
            var parser =  new HashSet<IMessageParser>(new [] { new ToDarwinMessageParser(Substitute.For<ILogger>())});
            var target = new MessageQueue(Substitute.For<ILogger>());
            var publisher = new MessagePublisher(parser, Substitute.For<ILogger>());
            var message = MessageGenerator.CreateByteMessage();

            var unsubscribe1 = publisher.Subscribe(target);
            publisher.Publish(message);
            
            AssertHasMessage(target);
        }

        private static void AssertHasMessage(MessageQueue target)
        {
            Assert.NotEmpty(target);
            var darwinMessage = target.Dequeue();
            Assert.Equal(TestMessage.PushportSequence, darwinMessage.Message.PushportSequence);
        }

        [Fact]
        public void PublishToMultipleSubscriber()
        {
            var parser =  new HashSet<IMessageParser>(new [] { new ToDarwinMessageParser(Substitute.For<ILogger>())});
            var target1 = new MessageQueue(Substitute.For<ILogger>());
            var target2 = new MessageQueue(Substitute.For<ILogger>());
            var publisher = new MessagePublisher(parser, Substitute.For<ILogger>());
            var message = MessageGenerator.CreateByteMessage();

            var unsubscribe1 = publisher.Subscribe(target1);
            var unsubscribe2 = publisher.Subscribe(target2);
            publisher.Publish(message);
            
            AssertHasMessage(target1);            
            AssertHasMessage(target2);
        }
        
        [Fact]
        public void PublishToMultipleSubscribersWithDifferentMessageTypes()
        {
            var parser =  Publisher.DefaultParsers(Substitute.For<ILogger>());
            var target1 = new MessageQueue(Substitute.For<ILogger>());
            var target2 = CreateMockObserver(typeof(TextMessage));
            var publisher = new MessagePublisher(parser, Substitute.For<ILogger>());
            var message = MessageGenerator.CreateByteMessage();

            var unsubscribe1 = publisher.Subscribe(target1);
            var unsubscribe2 = publisher.Subscribe(target2);
            publisher.Publish(message);
            
            AssertHasMessage(target1);            
            target2.Received().OnNext(Arg.Any<TextMessage>());
        }
        
        [Fact]
        public void DoesNotPublishToUnsubscribed()
        {
            var parser =  new HashSet<IMessageParser>(new [] { new ToDarwinMessageParser(Substitute.For<ILogger>())});
            var target1 = new MessageQueue(Substitute.For<ILogger>());
            var target2 = new MessageQueue(Substitute.For<ILogger>());
           
            var publisher = new MessagePublisher(parser, Substitute.For<ILogger>());
            var unsubscribe1 = publisher.Subscribe(target1);
            var unsubscribe2 = publisher.Subscribe(target2);
            
            unsubscribe1.Dispose();
            var message = MessageGenerator.CreateByteMessage();
            publisher.Publish(message);
            
            Assert.Empty(target1);           
            AssertHasMessage(target2);
        }
        
        [Fact]
        public void PublishErrorToSubscriber()
        {
            var parser =  new HashSet<IMessageParser>(new [] { new ToDarwinMessageParser(Substitute.For<ILogger>())});
            var target = CreateMockObserver();

            var publisher = new MessagePublisher(parser, Substitute.For<ILogger>());
            var unsubscribe = publisher.Subscribe(target);

            var ex = new Exception("Test");
            publisher.PublishError(ex);
            
            target.Received().OnError(ex);
        }
        
        [Fact]
        public void PublishErrorToMultipleSubscriber()
        {
            var parser =  new HashSet<IMessageParser>(new [] { new ToDarwinMessageParser(Substitute.For<ILogger>())});
            var target1 = CreateMockObserver();
            var target2 = CreateMockObserver();
           
            var publisher = new MessagePublisher(parser, Substitute.For<ILogger>());
            var unsubscribe1 = publisher.Subscribe(target1);
            var unsubscribe2 = publisher.Subscribe(target2);
            
            var ex = new Exception("Test");
            publisher.PublishError(ex);
            
            target1.Received().OnError(ex);
            target2.Received().OnError(ex);
        }
        
        [Fact]
        public void WhenUnsunscribedRemovesAllSubscribers()
        {
            var parser =  new HashSet<IMessageParser>(new [] { new ToDarwinMessageParser(Substitute.For<ILogger>())});
            var target1 = CreateMockObserver();
            var target2 = CreateMockObserver();
           
            var publisher = new MessagePublisher(parser, Substitute.For<ILogger>());
            var unsubscribe1 = publisher.Subscribe(target1);
            var unsubscribe2 = publisher.Subscribe(target2);
            
            publisher.Unsubscribe(false);
            target1.Received().OnCompleted();
            target1.Received().OnCompleted();
        }

        private static IPushPortObserver CreateMockObserver(Type messageType = null)
        {
            messageType = messageType ?? typeof(DarwinMessage);
            var target = Substitute.For<IPushPortObserver>();
            target.MessageType.Returns(messageType);
            return target;
        }

        [Fact]
        public void DisposeUnsubscribes()
        {
            var parser =  new HashSet<IMessageParser>(new [] { new ToDarwinMessageParser(Substitute.For<ILogger>())});
            var target1 = CreateMockObserver();
            var target2 = CreateMockObserver();
           
            var publisher = new MessagePublisher(parser, Substitute.For<ILogger>());
            var unsubscribe1 = publisher.Subscribe(target1);
            var unsubscribe2 = publisher.Subscribe(target2);
            
            publisher.Dispose();
            target1.Received().OnCompleted();
            target1.Received().OnCompleted();
        }
    }
}