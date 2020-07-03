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
        
        private IPushPort _pushport;
        private IMessageConsumer _consumer;
        
        internal PushPortTopic(IPushPort pushport, string topic, IMessagePublisher publisher, ILogger logger)
        {
            _pushport = pushport ?? throw new ArgumentNullException(nameof(pushport));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Topic = topic;
        }
        
        public IDisposable Subscribe(IPushPortObserver observer)
        {
            return _publisher.Subscribe(observer);
        }

        internal void Consume()
        {
            try
            {
                _consumer = _pushport.Consume(Topic);
                _consumer.Listener += OnMessageReceived;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Failed to start listening to pushport {url}  Topic:{topic}", _pushport, Topic);
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
                _logger.Error(exception, "Error closing consumer to pushport {url}  Topic:{topic}", _pushport, Topic);
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