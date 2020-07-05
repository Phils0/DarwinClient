using System;
using Apache.NMS;
using DarwinClient.SchemaV16;

namespace DarwinClient
{
    /// <summary>
    /// Base Message class
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// Active MQ message ID
        /// </summary>
        public string MessageId { get; }
        /// <summary>
        /// Pushport Sequence number
        /// </summary>
        /// <remarks>
        /// Increments from 0 to 9,999,999 then resets
        /// Indicates if miss a message 
        /// </remarks>
        public string PushportSequence { get; }
        /// <summary>
        /// Darwin Message Type <see cref="MessageTypes"/>
        /// </summary>
        public string MessageType { get; }
        /// <summary>
        /// Active MQ message timestamp
        /// </summary>
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
    
    /// <summary>
    /// Darwin message
    /// </summary>
    /// <remarks>
    /// Has the deserialised update message <see cref="SchemaV16.Pport"/> 
    /// </remarks>
    public sealed class DarwinMessage: Message
    {
        public Pport Updates { get; }

        internal DarwinMessage(Pport data, IMessage message): base(message)
        {
            Updates = data;
            Updates.Message = this;
        }
    }
    
    /// <summary>
    /// Message content as a string
    /// </summary>
    /// <remarks>
    /// This will normally be xml <see cref="Parsers.ToXmlParser"/> but can be any text representation
    /// </remarks>
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
    
    /// <summary>
    /// Unknown message.  Should not happen.
    /// </summary>
    public sealed class UnknownMessage : Message
    {
        public IMessage Message { get; }
        
        public UnknownMessage(IMessage message): base(message)
        {
            Message = message;
        }
    }
}