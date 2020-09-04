using System;
using System.Collections.Generic;
using Apache.NMS;
using DarwinClient.SchemaV16;
using DarwinClient.Serialization;
using NSubstitute;
using Serilog;

namespace DarwinClient.Test.Helpers
{
    public static class TestMessage
    {
        public const string PushportSequence = "123456";
        public const string MessageId = "ID:nrdp-prod-01.dsg.caci.co.uk-34724-1585330094093-8:605:1:1:312607";
        public static readonly DateTime Timestamp = new DateTime(2020, 6, 13, 9, 36, 57);        
    }
    
    public static class MessageGenerator
    {
        public const string Base64UrMessage =
            "H4sIAAAAAAAAAJ2U0Y6bMBBFf8Xya5VgwsYhKLCKilZtte1GCVUfKwtcQCK2ZQ+h/fsanISlUqVlXyxm8D2+M3jYPf4+N+jCtamliLG/JBhxkcuiFmWMv2dPixAjA0wUrJGCx/gPN/gx2R2U1ICsVpgYVwAq8ryu65ZQsYabUstWLXN59jRA7R1aUx3sfu/iU+xEkTCrGcJTXvGitRu8SzASgvcRViPhYQbhSeozA9umKWI9D8FzZmBaBn2nCX9EbOZ0AgbAV24MK/mUE87gZJrVYt9wDVPGdi7jRRdcTxA+mcNIUwZsqvdn6PcN0+dpCf54N//VHrPs813bt7I1npYSflo52FlYkRVZELrwg4xso4BG683ygZIwWNEPxI+Ina9x2qgdt2TXHlGrCgb8RddlbfNZarPZCem6cEBC/SDcrCndrjFq++yPa2BM8fpMq7M3MnqW+fCNEagmxqdPx+z5W4pRByzGzlVA+rBw4QYjdXs1PN/zDse0Rgxczp7J4GPDjK1134Lsb2Nufeh88O05RcHVXUHfpFANA9QvQ36PUS7FrxiDbjlO/J132+N2N1yUUCWhy18jF9xq/38nvvSlK2cuvJaorD3E4d6DwUXKdFeLm8M3nellJ7u0R7sMv8jkL1pvL9FhBQAA";

        public const string XmlUrMessage = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Pport xmlns=\"http://www.thalesgroup.com/rtti/PushPort/v16\" xmlns:ns2=\"http://www.thalesgroup.com/rtti/PushPort/Schedules/v3\" xmlns:ns3=\"http://www.thalesgroup.com/rtti/PushPort/Schedules/v2\" xmlns:ns4=\"http://www.thalesgroup.com/rtti/PushPort/Formations/v2\" xmlns:ns5=\"http://www.thalesgroup.com/rtti/PushPort/Forecasts/v3\" xmlns:ns6=\"http://www.thalesgroup.com/rtti/PushPort/Formations/v1\" xmlns:ns7=\"http://www.thalesgroup.com/rtti/PushPort/StationMessages/v1\" xmlns:ns8=\"http://www.thalesgroup.com/rtti/PushPort/TrainAlerts/v1\" xmlns:ns9=\"http://www.thalesgroup.com/rtti/PushPort/TrainOrder/v1\" xmlns:ns10=\"http://www.thalesgroup.com/rtti/PushPort/TDData/v1\" xmlns:ns11=\"http://www.thalesgroup.com/rtti/PushPort/Alarms/v1\" xmlns:ns12=\"http://thalesgroup.com/RTTI/PushPortStatus/root_1\" ts=\"2020-06-13T09:36:51.3145114+01:00\" version=\"16.0\"><uR updateOrigin=\"TD\"><TS rid=\"202006138006830\" uid=\"P06830\" ssd=\"2020-06-13\"><ns5:Location tpl=\"THPLESK\" wta=\"09:31\" wtd=\"09:37\" pta=\"09:31\" ptd=\"09:37\"><ns5:arr at=\"09:29\" atClass=\"Automatic\" src=\"TD\"/><ns5:dep at=\"09:36\" atClass=\"Automatic\" src=\"TD\"/><ns5:plat platsup=\"true\" cisPlatsup=\"true\">1</ns5:plat></ns5:Location></TS></uR></Pport>";

        public static IBytesMessage CreateByteMessage(string sequence = TestMessage.PushportSequence, DateTime? timestamp = null)
        {
            var message = CreateEmptyByteMessage(sequence, timestamp);
            message.Content = Convert.FromBase64String(Base64UrMessage);
            return message;
        }

        public static IBytesMessage CreateEmptyByteMessage(string sequence = TestMessage.PushportSequence, DateTime? timestamp = null)
        {
            var message = Substitute.For<IBytesMessage>();
            message.NMSTimestamp = timestamp ?? TestMessage.Timestamp;
            message.NMSMessageId = TestMessage.MessageId;
            var properties = CreateProperties(sequence);
            message.Properties.Returns(properties);
            return message;
        }
        
        private static IPrimitiveMap CreateProperties(string sequence, string messageType = MessageTypes.ActualOrForcast)
        {
            var lookup = new Dictionary<string, string>()
            {
                {"PushPortSequence", sequence},
                {"MessageType", messageType},
            };
            
            var properties = Substitute.For<IPrimitiveMap>();
            properties.Contains(Arg.Any<string>()).Returns(x => lookup.ContainsKey(x[0] as string));
            properties.GetString(Arg.Any<string>()).Returns(x => lookup[x[0] as string]);
            return properties;
        }
        
        public static ITextMessage CreateTextMessage(string xml = XmlUrMessage, string sequence = TestMessage.PushportSequence, DateTime? timestamp = null)
        {
            var message = Substitute.For<ITextMessage>();
            message.NMSTimestamp = timestamp ?? TestMessage.Timestamp;
            message.NMSMessageId = TestMessage.MessageId;
            var properties = CreateProperties(sequence);
            message.Properties.Returns(properties);
            message.Text = xml;
            return message;
        }
        
        public static DarwinMessage CreateDarwinMessage(string xml = XmlUrMessage, string sequenceNumber = TestMessage.PushportSequence)
        {
            return new DarwinMessage(CreateDarwinUpdates(xml, sequenceNumber), CreateEmptyByteMessage(sequenceNumber));
        }
        
        public static Pport CreateDarwinUpdates(string xml = XmlUrMessage, string sequenceNumber = TestMessage.PushportSequence)
        {
            var deserializer = new MessageDeserializer(Substitute.For<ILogger>());
            return deserializer.Deserialize(xml, sequenceNumber);
        }
    }
}