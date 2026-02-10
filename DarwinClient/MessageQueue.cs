using System;
using System.Collections.Generic;
using DarwinClient.Schema;
using Serilog;

namespace DarwinClient
{
    public interface IPushPortObserver : IObserver<Message>, IDisposable
    {
        Type MessageType { get; }
    }
    
    public class MessageQueue : Queue<Pport>, IPushPortObserver
    {
        private readonly ILogger _logger;

        public Type MessageType { get; } = typeof(DarwinMessage);

        public bool IsLive { get; set; }

        private IDisposable _unsubscriber;
        
        internal MessageQueue(ILogger logger)
        {
            _logger = logger;
        }
        
        public void OnCompleted()
        {
            Disconnect();
        }

        public void OnError(Exception error)
        {
            Disconnect();
        }

        public void OnNext(Message msg)
        {
            if(msg is DarwinMessage darwinMessage)
                Enqueue(darwinMessage.Updates);
            else
                _logger.Warning("Unknown message: {msg}", msg);
        }

        internal void SubscribeTo(ISubscriptionPushPort source, string topic = PushPort.V16PushPortTopic)
        {
            _unsubscriber = source.Subscribe(topic, this);
            IsLive = true;
        }
        
        public void Disconnect()
        {
            if (_unsubscriber != null)
            {
                _unsubscriber.Dispose();
                _unsubscriber = null;
            }
            IsLive = false;
        }
        
        public void Dispose()
        {
            Disconnect();
        }
    }
}