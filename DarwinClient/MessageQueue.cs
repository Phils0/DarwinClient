﻿using System;
using System.Collections.Generic;
using DarwinClient.SchemaV16;
using Serilog;

namespace DarwinClient
{
    public class MessageQueue : Queue<Pport>, IObserver<Message>, IDisposable
    {
        private readonly ILogger _logger;

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

        public void SubscribeTo(IObservable<Message> source)
        {
            _unsubscriber = source.Subscribe(this);
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