using System;
using Apache.NMS;
using DarwinClient.SchemaV16;

namespace DarwinClient
{
    public abstract class Message
    {
        public string MessageId { get; }
        public string PushportSequence { get; }
        public string MessageType { get; }
        public DateTime Timestamp { get; }

        protected Message(IMessage message) : 
            this (message.NMSMessageId, 
                message.Properties.TryGetProperty("PushPortSequence"), 
                message.Properties.TryGetProperty("MessageType", MessageTypes.Unknown), 
                message.NMSTimestamp)
        {
        }
        
        protected Message(string messageId, string pushportSequence, string messageType, DateTime timestamp)
        {
            MessageId = messageId;
            PushportSequence = pushportSequence;
            MessageType = messageType;
            Timestamp = timestamp;
        }

        public override string ToString()
        {
            return $"{Timestamp}|{MessageType}|{PushportSequence}";
        }
    }
    
    public sealed class DarwinMessage: Message
    {
        public Pport Updates { get; }

        internal DarwinMessage(Pport data, IMessage message): base(message)
        {
            Updates = data;
        }
    }
    
    public sealed class TextMessage: Message
    {
        public string Content { get; }

        internal TextMessage(string text, IMessage message): base(message)
        {
            Content = text;
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{Content}";
        }
    }
    
    public sealed class UnknownMessage : Message
    {
        public UnknownMessage(IMessage message): base(message)
        {
        }
    }
}