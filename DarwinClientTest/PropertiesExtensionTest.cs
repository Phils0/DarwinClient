using Apache.NMS;
using NSubstitute;
using Xunit;

namespace DarwinClient.Test
{
    public class PropertiesExtensionTest
    {
        [Fact]
        public void ReturnsStringProperty()
        {
            var properties = Substitute.For<IPrimitiveMap>();
            properties.Contains("Test").Returns(true);
            properties.GetString("Test").Returns("Property");

            var property = properties.TryGetProperty("Test", "Default");
            Assert.Equal("Property", property); 
        }
        
        [Fact]
        public void ReturnsDefaultValueIfNotExists()
        {
            var properties = Substitute.For<IPrimitiveMap>();
            properties.Contains(Arg.Any<string>()).Returns(false);

            var property = properties.TryGetProperty("Test", "Default");
            Assert.Equal("Default", property);
        }
    }
}