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
            var consumer = CreateMockPushportConsumer(out var pushport);
            var publisher = Substitute.For<IMessagePublisher>();
            var topic = CreateAndConsumeTopic(pushport, publisher);

            var message = MessageGenerator.CreateByteMessage();
            consumer.Listener += Raise.Event<MessageListener>(message);
            
            publisher.Received().Publish(message);
        }
        private static IMessageConsumer CreateMockPushportConsumer(out IPushPort pushport)
        {
            var consumer = Substitute.For<IMessageConsumer>();
            pushport = Substitute.For<IPushPort>();
            pushport.Consume("Test").Returns(consumer);
            return consumer;
        }

        private static PushPortTopic CreateAndConsumeTopic(IPushPort pushport, IMessagePublisher publisher)
        {
            var logger = Substitute.For<ILogger>();
            var topic = new PushPortTopic(pushport, "Test", publisher, logger);
            topic.Consume();
            return topic;
        }
        
        [Fact]
        public void DisconnectFromConsumer()
        {
            var consumer = CreateMockPushportConsumer(out var pushport);
            var publisher = Substitute.For<IMessagePublisher>();
            var topic = CreateAndConsumeTopic(pushport, publisher);
            
            topic.Disconnect(false);

            consumer.Received().Close();
        }
        
        [Fact]
        public void DisconnectUnsubscribesFromPublisher()
        {
            var consumer = CreateMockPushportConsumer(out var pushport);
            var publisher = Substitute.For<IMessagePublisher>();
            var topic = CreateAndConsumeTopic(pushport, publisher);
            
            topic.Disconnect(false);

            var message = MessageGenerator.CreateByteMessage();
            consumer.Listener += Raise.Event<MessageListener>(message);
            
            publisher.Received().Unsubscribe(false);
        }
        
        [Fact]
        public void MessagesNotForwardedAfterDisconnect()
        {
            var consumer = CreateMockPushportConsumer(out var pushport);
            var publisher = Substitute.For<IMessagePublisher>();
            var topic = CreateAndConsumeTopic(pushport, publisher);
            
            topic.Disconnect(false);

            publisher.Received().Unsubscribe(false);
        }
        
        [Fact]
        public void OnErrorDisconnectsEverything()
        {
            var consumer = CreateMockPushportConsumer(out var pushport);
            var publisher = Substitute.For<IMessagePublisher>();
            var topic = CreateAndConsumeTopic(pushport, publisher);
            
            topic.OnError(new Exception("Test"));
            
            consumer.Received().Close();
            publisher.Received().Unsubscribe(true);
        }
    }
}