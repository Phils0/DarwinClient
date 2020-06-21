﻿using System;
using System.Collections.Generic;
using System.Linq;
using Apache.NMS;
using DarwinClient.Parsers;
using Serilog;

namespace DarwinClient
{
    public interface IPushPort
    {
        IMessageConsumer CreateConsumer(string topic);
        IDisposable Subscribe(string topic, IObserver<Message> observer, IMessageParser parser = null);
    }
    
    /// <summary>
    /// Darwin PushPort
    /// </summary>
    /// <remarks>
    /// Does not store messages.
    /// Usage:
    /// 1. Create PushPort instance
    /// 2. Create queue.  Alternatively add own subscribers.  
    /// 3. Connect, will automatically start relaying messages
    /// If subscribe after start only going to get messages after subscription
    /// </remarks>
    public class PushPort : IPushPort, IDisposable
    {
        public const string V16PushPortTopic = "darwin.pushport-v16";
        public const string StatusTopic = "darwin.status";
        
        public Uri Url { get; }
        public string[] Topics => _queues.Keys.ToArray();
        
        private readonly ILogger _logger;
        private readonly Dictionary<string, PushPortTopic> _queues = new Dictionary<string, PushPortTopic>();

        private IConnection _connection;
        private ISession _session;

        public PushPort(string url, ILogger logger) : this(new Uri(url), logger)
        {
        }

        public PushPort(Uri url, ILogger logger)
        {
            _logger = logger;
            Url = url;
        }

        public MessageQueue CreateQueue()
        {
            var queue = new MessageQueue(_logger);
            queue.SubscribeTo(this);
            return queue;
        }
        
        public IDisposable Subscribe(string topic, IObserver<Message> observer, IMessageParser parser = null)
        {
            if (!_queues.TryGetValue(topic, out var listener))
            {
                listener = new PushPortTopic(this, topic, parser, _logger);
                _queues.Add(topic, listener);
            }
            return listener.Subscribe(observer);
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
                
                foreach (var listener in _queues.Values)
                {
                    listener.Listen();
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

            foreach (var topic in _queues.Values)
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
            foreach (var topic in _queues.Values)
            {
                topic.OnError(error);
            }
            
            Disconnect(true);
        }
        
        public IMessageConsumer CreateConsumer(string topicName)
        {
            var topic = _session.GetTopic(topicName);
            return _session.CreateConsumer(topic);
        }

        public override string ToString()
        {
            return Url.ToString();
        }
    }
}