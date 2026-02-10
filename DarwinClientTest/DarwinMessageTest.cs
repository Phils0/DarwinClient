using System;
using Apache.NMS;
using DarwinClient.Schema;
using DarwinClient.Test.Helpers;
using NSubstitute;
using Xunit;

namespace DarwinClient.Test
{
    public class DarwinMessageTest
    {
        [Fact]
        public void Create()
        {
            var activeMqMsg = MessageGenerator.CreateByteMessage();
            var content = new Pport();
            var msg = new DarwinMessage(content, activeMqMsg);
            
            Assert.Equal(content, msg.Updates);
            Assert.Equal(TestMessage.MessageId, msg.MessageId);
            Assert.Equal(TestMessage.PushportSequence, msg.PushportSequence);
            Assert.Equal(TestMessage.Timestamp, msg.Timestamp);
            Assert.Equal(MessageTypes.ActualOrForcast, msg.MessageType);
        }
        
        [Fact]
        public void CreateWithNoProperties()
        {            
            var activeMqMsg = Substitute.For<IBytesMessage>();
            activeMqMsg.NMSTimestamp = TestMessage.Timestamp;
            activeMqMsg.NMSMessageId = TestMessage.MessageId;
            
            var properties = Substitute.For<IPrimitiveMap>();
            properties.Contains(Arg.Any<string>()).Returns(false);
            activeMqMsg.Properties.Returns(properties);

            var content = new Pport();
            var msg = new DarwinMessage(content, activeMqMsg);
            
            Assert.Equal("", msg.PushportSequence);
            Assert.Equal(MessageTypes.Unknown, msg.MessageType);
        }
        
        [Fact]
        public void PportReferencesMessage()
        {            
            var activeMqMsg = MessageGenerator.CreateByteMessage();
            var content = new Pport();
            var msg = new DarwinMessage(content, activeMqMsg);
            
            Assert.Same(msg, msg.Updates.Message);
        }
    }
}