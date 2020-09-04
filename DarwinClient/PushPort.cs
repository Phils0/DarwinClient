using System;
using System.Collections.Generic;
using System.Linq;
using Apache.NMS;
using Serilog;

namespace DarwinClient
{
    /// <summary>
    /// Simple Pushport Interface that puts messages onto an in-memory queue
    /// </summary>
    /// <remarks>
    /// Does not store messages.
    /// Usage:
    /// 1. Create PushPort instance, by default will configure using darwin.pushport-v16 topic
    /// 2. <see cref="CreateQueue"/>.    
    /// 4. <see cref="Connect"/> to the pushport.  Starts relaying messages to any created queues
    /// If create queue after connect only going to get messages after subscription
    /// Stops sending messages to the queues and closes the connection to the pushport when disposed
    /// </remarks>
    public interface IPushPort : IDisposable
    {
        /// <summary>
        /// Create a  queue to hold Darwin events and register it with the pushport 
        /// </summary>
        /// <returns>Message Queue</returns>
        MessageQueue CreateQueue();
        /// <summary>
        /// Connect to the pushport.  Once connected immediately starts pushing events to any created queues
        /// </summary>
        /// <param name="user">Darwin user</param>
        /// <param name="password">Password</param>
        /// <remarks>Stops sending when the instance is disposed</remarks>
        void Connect(string user, string password);
    }

    /// <summary>
    /// Pushport Interface.
    /// </summary>
    /// <remarks>Lower level interface giving the client control of what
    /// Usage:
    /// 1. Create PushPort instance, by default will configure using darwin.pushport-v16 topic
    /// 2. <see cref="AddTopic"/> (if not using default configuration)
    /// 3. <see cref="Subscribe"/>  your own listeners to the pushport  
    /// 4. <see cref="Connect"/> to the pushport.  Starts relaying messages to any subscribers
    /// If subscribe after connect only going to get messages after subscription
    /// Alternatively 3. can add own subscribers.
    /// </remarks>
    public interface ISubscriptionPushPort : IDisposable
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
        /// <summary>
        /// Connect to the pushport.  Start sending events to the subscribers
        /// </summary>
        /// <param name="user">Darwin user</param>
        /// <param name="password">Password</param>
        /// <remarks>Stops sending when the instance is disposed</remarks>
        void Connect(string user, string password);
    }
    
    /// <summary>
    /// Darwin PushPort Facade
    /// </summary>
    /// <remarks>
    /// Does not store messages.
    /// </remarks>
    public class PushPort : ISubscriptionPushPort, IPushPort, IDisposable
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

        public MessageQueue CreateQueue()
        {
            var queue = new MessageQueue(_logger);
            queue.SubscribeTo(this);
            return queue;
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