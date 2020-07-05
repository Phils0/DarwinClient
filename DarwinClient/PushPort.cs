using System;
using System.Collections.Generic;
using System.Linq;
using Apache.NMS;
using Serilog;

namespace DarwinClient
{
    public interface IPushPort
    {
        /// <summary>
        /// Pushport URL
        /// </summary>
        Uri Url { get; }
        /// <summary>
        /// Topics connecting/connected to
        /// </summary>
        string[] Topics { get; }
        /// <summary>
        /// Create a queue to contain messages from the pushport
        /// </summary>
        /// <returns>Message Queue</returns>
        MessageQueue CreateQueue();
        /// <summary>
        /// Add an ActiveMQ topic to connect to
        /// </summary>
        /// <param name="topic">Topic name</param>
        /// <param name="publisher">Publisher to distribute topic messages to</param>
        void AddTopic(string topic, IMessagePublisher publisher);
        /// <summary>
        /// Subscribe to a topic
        /// </summary>
        /// <param name="topic">Topic to subscribe to</param>
        /// <param name="observer">Observer</param>
        /// <returns>A disposable instance to allow the observer to stop subscribing</returns>
        IDisposable Subscribe(string topic, IPushPortObserver observer);
        void Connect(string user, string password);
    }


    /// <summary>
    /// Darwin PushPort
    /// </summary>
    /// <remarks>
    /// Does not store messages.
    /// Usage:
    /// 1. Create PushPort instance, by default will configure using darwin.pushport-v16 topic
    /// 2. Add topic(s) (if not using default configuration)
    /// 3. Create queue.    
    /// 4. Connect.  Will start relaying messages into the queue.
    /// If subscribe after connect only going to get messages after subscription
    /// Alternatively 3. can add own subscribers.
    /// </remarks>
    public class PushPort : IPushPort, IDisposable
    {
        public const string V16PushPortTopic = "darwin.pushport-v16";
        public const string StatusTopic = "darwin.status";

        public Uri Url => _factory.BrokerUri;
        public string[] Topics => _topics.Keys.ToArray();
        
        private readonly IConnectionFactory _factory;
        private readonly ILogger _logger;
        private readonly Dictionary<string, PushPortTopic> _topics = new Dictionary<string, PushPortTopic>();

        private IConnection _connection;
        private ISession _session;

        public PushPort(string url, ILogger logger, bool useDefaultTopics = true) : 
            this(new NMSConnectionFactory(new Uri(url)), logger, useDefaultTopics)
        {
        }

        public PushPort(IConnectionFactory factory, ILogger logger, bool useDefaultTopics)
        {
            _logger = logger;
            _factory = factory;
            if (useDefaultTopics)
            {
                AddTopic(V16PushPortTopic, Publisher.CreateDefault(_logger));
            }
        }

        public MessageQueue CreateQueue()
        {
            var queue = new MessageQueue(_logger);
            queue.SubscribeTo(this);
            return queue;
        }
        
        public void AddTopic(string topic, IMessagePublisher publisher)
        {
            if (!_topics.TryGetValue(topic, out var listener))
            {
                listener = new PushPortTopic(topic, publisher, _logger);
                _topics.Add(topic, listener);
            }
        }
        
        public IDisposable Subscribe(string topic, IPushPortObserver observer)
        {
            if (!_topics.TryGetValue(topic, out var pushPortTopic))
            {
                throw new DarwinException("Topic not added");
            }
            return pushPortTopic.Subscribe(observer);
        } 

        public void Connect(string user, string password)
        {
            try
            {
                _connection = _factory.CreateConnection(user, password);
                _connection.ClientId = user;
                _connection.ExceptionListener += new ExceptionListener(OnConnectionException);
                _session = _connection.CreateSession();
                
                foreach (var listener in _topics.Values)
                {
                    listener.StartConsuming(_session);
                }
                
                _connection.Start();
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Failed to connect to pushport {url}", this);
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
                _logger.Error(exception, "Error stopping connection to pushport {url}", this);
            }

            foreach (var topic in _topics.Values)
            {
                topic.Disconnect(isError);
            }
            
            try
            {
                _session?.Close();
                _session = null;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error closing session to pushport {url}", this);
            }

            try
            {
                _connection?.Close();
                _connection = null;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error closing connection to pushport {url}", this);
            }
        }
        
        private void OnConnectionException(Exception exception)
        {
            _logger.Error(exception, "Error on pushport connection {url}", this);
            var error = new DarwinConnectionException("Pushport connection errored", exception);
            foreach (var topic in _topics.Values)
            {
                topic.OnError(error);
            }
            
            Disconnect(true);
        }
        
        public override string ToString()
        {
            return Url.ToString();
        }
    }
}