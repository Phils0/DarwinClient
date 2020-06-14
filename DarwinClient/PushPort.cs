using System;
using Apache.NMS;
using DarwinClient.Parsers;
using Serilog;

namespace DarwinClient
{
    /// <summary>
    /// Darwin PushPort
    /// </summary>
    /// <remarks>
    /// Does not store messages.
    /// Usage:
    /// 1. Create Pushport instance
    /// 2. Add subscribers
    /// 3. Connect, will automatically start relaying messages
    /// If subscribe after start only going to get messages after subscription
    /// </remarks>
    public class PushPort : IObservable<Message>, IDisposable
    {
        public const string V16PushPortTopic = "darwin.pushport-v16";
        
        public Uri Url { get; }
        public string Topic { get; }

        private readonly MessagePublisher _publisher;
        private readonly ILogger _logger;

        private IConnection _connection;
        private ISession _session;
        private IMessageConsumer _consumer;
        
        public PushPort(string url, IMessageParser parser, ILogger logger, string topic = V16PushPortTopic) : this(new Uri(url), topic, parser, logger)
        {
        }

        public PushPort(Uri url, string topic, IMessageParser parser, ILogger logger)
        {
            _logger = logger;
            _publisher = new MessagePublisher(parser, logger);
            Url = url;
            Topic = topic;
        }
        
        public IDisposable Subscribe(IObserver<Message> observer)
        {
            return _publisher.Subscribe(observer);
        }

        public void Connect(string user, string password)
        {
            try
            {
                var factory = new NMSConnectionFactory(Url);
                _connection = factory.CreateConnection(user, password);
                _connection.ClientId = user;
                _connection.ExceptionListener += new ExceptionListener(OnConnectionException);
                _session = _connection.CreateSession();
                var topic = _session.GetTopic(Topic);
                _consumer = _session.CreateConsumer(topic);
                _consumer.Listener += new MessageListener(OnMessageReceived);
                _connection.Start();
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Failed to connect to pushport {url}  Topic:{topic}", Url, Topic);
                Disconnect(true);
                throw new DarwinConnectionException("Failed to connect to pushport.", exception);
            }
        }

        public void Dispose()
        {
            Disconnect(false);
        }

        private void Disconnect(bool isError)
        {
            try
            {
                _connection?.Stop();
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error stopping connection to pushport {url}  Topic:{topic}", Url, Topic);
            }

            try
            {
                _consumer?.Close();
                _consumer = null;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error closing consumer to pushport {url}  Topic:{topic}", Url, Topic);
            }

            try
            {
                _session?.Close();
                _session = null;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error closing session to pushport {url}  Topic:{topic}", Url, Topic);
            }

            try
            {
                _connection?.Close();
                _connection = null;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error closing connection to pushport {url}  Topic:{topic}", Url, Topic);
            }

            _publisher.Unsubscribe(isError);
        }
        
        private void OnConnectionException(Exception exception)
        {
            _logger.Error(exception, "Error on pushport connection {url} Topic:{topic}", Url, Topic);
            _publisher.PublishError(exception);
            Disconnect(true);
        }

        private void OnMessageReceived(IMessage message)
        {
            _publisher.Publish(message);      
        }
    }
}