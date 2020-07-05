using System;
using Apache.NMS;
using NSubstitute;
using Serilog;
using Xunit;

namespace DarwinClient.Test
{
    public class PushPortTest
    {
        [Fact]
        public void PushportUrl()
        {
            var factory = Substitute.For<IConnectionFactory>();
            factory.BrokerUri = new Uri("activemq:Test");
            var pushport = new PushPort(factory, Substitute.For<ILogger>(), true);
            Assert.Equal("activemq:Test", pushport.Url.ToString());
        }
        
        [Fact]
        public void PushportIsConfiguredWithDarwinPushport()
        {
            var pushport = new PushPort(Substitute.For<IConnectionFactory>(), Substitute.For<ILogger>(), true);
            Assert.NotEmpty(pushport.Topics);
            Assert.Contains(PushPort.V16PushPortTopic, pushport.Topics);
        }

        [Fact]
        public void SubscribeToTopic()
        {
            var pushport = new PushPort(Substitute.For<IConnectionFactory>(), Substitute.For<ILogger>(), true);
             
        }
        
        [Fact]
        public void MultipleSubscriptionsToTopic()
        {
        }
        
        [Fact]
        public void FailsToSubscribeIfTopicNotAdded()
        {
        }
    }
}