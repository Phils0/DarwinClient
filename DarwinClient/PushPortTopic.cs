using System;
using Apache.NMS;
using Serilog;

namespace DarwinClient
{
    internal class PushPortTopic : IDisposable
    { 
        internal string Topic { get; }

        private readonly IMessagePublisher _publisher;
        private readonly ILogger _logger;
        
        private IMessageConsumer _consumer;
        
        internal PushPortTopic(string topic, IMessagePublisher publisher, ILogger logger)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Topic = topic;
        }
        
        internal IDisposable Subscribe(IPushPortObserver observer)
        {
            return _publisher.Subscribe(observer);
        }

        internal void StartConsuming(ISession session)
        {
            try
            {
                var topic = session.GetTopic(Topic);
                _consumer = session.CreateConsumer(topic);
                _consumer.Listener += OnMessageReceived;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Failed to start listening to pushport topic: {topic}",  Topic);
                Disconnect(true);
                throw new DarwinConnectionException("Failed to connect to pushport.", exception);
            }
        }

        public void Dispose()
        {
            Disconnect(false);
        }

        internal void Disconnect(bool isError)
        {
            try
            {
                _consumer?.Close();
                _consumer = null;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error closing consumer to pushport topic:{topic}", Topic);
            }
            
            _publisher.Unsubscribe(isError);
        }

        internal void OnError(Exception ex)
        {
            Disconnect(true);
        }

        private void OnMessageReceived(IMessage message)
        {
            _publisher.Publish(message);      
        }

        public override string ToString()
        {
            return Topic;
        }
    }
}