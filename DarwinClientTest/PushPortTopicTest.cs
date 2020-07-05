using System;
using Apache.NMS;
using DarwinClient.Test.Helpers;
using NSubstitute;
using Serilog;
using Xunit;

namespace DarwinClient.Test
{
    public class PushPortTopicTest
    {
        [Fact]
        public void ConsumePushport()
        {
            var consumer = CreateMockPushportConsumer();
            var publisher = Substitute.For<IMessagePublisher>();
            var topic = CreateAndConsumeTopic(consumer, publisher);

            var message = MessageGenerator.CreateByteMessage();
            consumer.Listener += Raise.Event<MessageListener>(message);
            
            publisher.Received().Publish(message);
        }
        
        private static IMessageConsumer CreateMockPushportConsumer()
        {
            var consumer = Substitute.For<IMessageConsumer>();
            return consumer;
        }

        private static PushPortTopic CreateAndConsumeTopic(IMessageConsumer consumer, IMessagePublisher publisher)
        {
            var logger = Substitute.For<ILogger>();
            var session = Substitute.For<ISession>();
            var mqTopic = Substitute.For<ITopic>();
            session.GetTopic("Test").Returns(mqTopic);
            session.CreateConsumer(mqTopic).Returns(consumer);
            
            var topic = new PushPortTopic("Test", publisher, logger);
            topic.StartConsuming(session);
            return topic;
        }
        
        [Fact]
        public void DisconnectFromConsumer()
        {
            var consumer = CreateMockPushportConsumer();
            var publisher = Substitute.For<IMessagePublisher>();
            var topic = CreateAndConsumeTopic(consumer, publisher);
            
            topic.Disconnect(false);

            consumer.Received().Close();
        }
        
        [Fact]
        public void DisconnectUnsubscribesFromPublisher()
        {
            var consumer = CreateMockPushportConsumer();
            var publisher = Substitute.For<IMessagePublisher>();
            var topic = CreateAndConsumeTopic(consumer, publisher);
            
            topic.Disconnect(false);

            var message = MessageGenerator.CreateByteMessage();
            consumer.Listener += Raise.Event<MessageListener>(message);
            
            publisher.Received().Unsubscribe(false);
        }
        
        [Fact]
        public void MessagesNotForwardedAfterDisconnect()
        {
            var consumer = CreateMockPushportConsumer();
            var publisher = Substitute.For<IMessagePublisher>();
            var topic = CreateAndConsumeTopic(consumer, publisher);
            
            topic.Disconnect(false);

            publisher.Received().Unsubscribe(false);
        }
        
        [Fact]
        public void OnErrorDisconnectsEverything()
        {
            var consumer = CreateMockPushportConsumer();
            var publisher = Substitute.For<IMessagePublisher>();
            var topic = CreateAndConsumeTopic(consumer, publisher);
            
            topic.OnError(new Exception("Test"));
            
            consumer.Received().Close();
            publisher.Received().Unsubscribe(true);
        }
    }
}