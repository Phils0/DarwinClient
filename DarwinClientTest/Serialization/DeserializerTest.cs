using System.IO;
using DarwinClient.SchemaV16;
using DarwinClient.Serialization;
using DarwinClient.Test.Helpers;
using NSubstitute;
using Serilog;
using Xunit;

namespace DarwinClient.Test.Serialization
{
    public class DeserializerTest
    {
        
        [Fact]
        public void DeserializeReference()
        {
            var deserializer = new ReferenceDataDeserializer(Substitute.For<ILogger>());
            using var stream = File.OpenRead(DummyS3.ReferenceData);
            var refData = deserializer.Deserialize(stream, DummyS3.ReferenceData);
            Assert.NotNull(refData);
        }
        
        [Fact]
        public void DeserializationError()
        {
            var deserializer = new ReferenceDataDeserializer(Substitute.For<ILogger>());
            using var stream = File.OpenRead(DummyS3.ReferenceDataV2);
            Assert.Throws<DarwinException>(() => deserializer.Deserialize(stream, DummyS3.ReferenceDataV2));
        }

        [Fact]
        public void DeserializeTimetable()
        {
            var deserializer = new TimetableDeserializer(Substitute.For<ILogger>());
            using var stream = File.OpenRead(DummyS3.Timetable);
            var timetable = deserializer.Deserialize(stream, DummyS3.Timetable);
            Assert.NotNull(timetable);
        }
        
        [Fact]
        public void DeserializeMessage()
        {
            var byteMessage = MessageGenerator.CreateByteMessage();
            using var stream = new MemoryStream(byteMessage.Content); 
            
            var deserializer = new MessageDeserializer(Substitute.For<ILogger>());
            
            var msg = deserializer.Deserialize(stream, "Test");
            Assert.IsType<Pport>(msg);
            Assert.NotNull(msg.Item);
            Assert.Equal(ItemChoiceType1.uR, msg.ItemElementName);
        }
    }
}