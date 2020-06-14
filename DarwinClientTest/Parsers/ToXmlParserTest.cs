using System.Xml;
using DarwinClient.Parsers;
using DarwinClient.Test.Helpers;
using NSubstitute;
using Serilog;
using Xunit;

namespace DarwinClient.Test.Parsers
{
    public class ToXmlParserTest
    {
        [Fact]
        public void ParseByteMessage()
        {
            var source = MessageGenerator.CreateByteMessage();
            
            var parser = new ToXmlParser(Substitute.For<ILogger>());
            
            Assert.True(parser.TryParse(source, out var parsed));
            Assert.IsType<TextMessage>(parsed);
            
            var xmlMsg = parsed as TextMessage;
            var doc = new XmlDocument();
            doc.LoadXml(xmlMsg.Content);
            Assert.NotEmpty(doc.ChildNodes);
        }
        
        [Fact]
        public void ParseTextMessage()
        {
            var source = MessageGenerator.CreateTextMessage();
            
            var parser = new ToXmlParser(Substitute.For<ILogger>());
            
            Assert.True(parser.TryParse(source, out var parsed));
            Assert.IsType<TextMessage>(parsed);
            
            var xmlMsg = parsed as TextMessage;
            var doc = new XmlDocument();
            doc.LoadXml(xmlMsg.Content);
            Assert.NotEmpty(doc.ChildNodes);
        }
    }
}