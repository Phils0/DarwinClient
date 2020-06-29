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

        [Fact]
        public void ParseByteMessage()
        {
            var source = MessageGenerator.CreateByteMessage();
            
            var parser = new ToDarwinMessageParser(Substitute.For<ILogger>());
            
            Assert.True(parser.TryParse(source, out var parsed));
            Assert.IsType<DarwinMessage>(parsed);
            var darwinMsg = parsed as DarwinMessage;
            Assert.NotNull(darwinMsg.Updates);
        }
        
        [Fact]
        public void SequenceNumberSet()
        {
            var source = MessageGenerator.CreateByteMessage();
            
            var parser = new ToDarwinMessageParser(Substitute.For<ILogger>());
            
            Assert.True(parser.TryParse(source, out var parsed));

            var darwinMsg = parsed as DarwinMessage;
            Assert.Equal(darwinMsg.PushportSequence, darwinMsg.PushportSequence);
        }
        
        [Fact]
        public void ParseTextMessage()
        {
            var source = MessageGenerator.CreateTextMessage();
            
            var parser = new ToDarwinMessageParser(Substitute.For<ILogger>());
            
            Assert.True(parser.TryParse(source, out var parsed));
            Assert.IsType<DarwinMessage>(parsed);
            var darwinMsg = parsed as DarwinMessage;
            Assert.NotNull(darwinMsg.Updates);
        }
    }
}