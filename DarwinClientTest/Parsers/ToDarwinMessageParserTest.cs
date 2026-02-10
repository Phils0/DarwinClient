using DarwinClient.Parsers;
using DarwinClient.Test.Helpers;
using NSubstitute;
using Serilog;
using Xunit;

namespace DarwinClient.Test.Parsers
{
    public class ToDarwinMessageParserTest
    {
        [Fact]
        public void MessageType()
        {
            var parser = new ToDarwinMessageParser(Substitute.For<ILogger>());
            Assert.Equal(typeof(DarwinMessage), parser.MessageType);            
        }

        [Fact(Skip = "Update to v18 message")]
        public void ParseByteMessage()
        {
            var source = MessageGenerator.CreateByteMessage();
            
            var parser = new ToDarwinMessageParser(Substitute.For<ILogger>());
            
            Assert.True(parser.TryParse(source, out var parsed));
            Assert.IsType<DarwinMessage>(parsed);
            var darwinMsg = (DarwinMessage) parsed;
            Assert.NotNull(darwinMsg.Updates);
        }
        
        [Fact(Skip = "Update to v18 message")]
        public void SequenceNumberSet()
        {
            var source = MessageGenerator.CreateByteMessage();
            
            var parser = new ToDarwinMessageParser(Substitute.For<ILogger>());
            
            Assert.True(parser.TryParse(source, out var parsed));
            Assert.IsType<DarwinMessage>(parsed);
            var darwinMsg = (DarwinMessage) parsed;
            Assert.Equal(darwinMsg.PushportSequence, darwinMsg.PushportSequence);
        }
        
        [Fact(Skip = "Update to v18 message")]
        public void ParseTextMessage()
        {
            var source = MessageGenerator.CreateTextMessage();
            
            var parser = new ToDarwinMessageParser(Substitute.For<ILogger>());
            
            Assert.True(parser.TryParse(source, out var parsed));
            Assert.IsType<DarwinMessage>(parsed);
            var darwinMsg = (DarwinMessage) parsed;
            Assert.NotNull(darwinMsg.Updates);
        }
    }
}