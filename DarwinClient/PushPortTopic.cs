using System;
using Apache.NMS;
using DarwinClient.Parsers;
using Serilog;

namespace DarwinClient
{
    internal class PushPortTopic : IObservable<Message>, IDisposable
    { 
        internal string Topic { get; }

        private readonly MessagePublisher _publisher;
        private readonly ILogger _logger;
        
        private IPushPort _pushport;
        private IMessageConsumer _consumer;
        
        internal PushPortTopic(IPushPort pushport, string topic, IMessageParser parser, ILogger logger)
        {
            _logger = logger;
            _pushport = pushport ?? throw new ArgumentNullException(nameof(pushport));
            if(parser == null)
                throw new ArgumentNullException(nameof(parser));
            _publisher = new MessagePublisher(parser, logger);
            Topic = topic;
        }
        
        public IDisposable Subscribe(IObserver<Message> observer)
        {
            return _publisher.Subscribe(observer);
        }

        internal void Listen()
        {
            try
            {
                _consumer = _pushport.CreateConsumer(Topic);
                _consumer.Listener += new MessageListener(OnMessageReceived);
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
    }
}