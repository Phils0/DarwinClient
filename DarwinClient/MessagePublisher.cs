using System;
using System.Collections.Generic;
using System.Linq;
using Apache.NMS;
using DarwinClient.Parsers;
using Serilog;

namespace DarwinClient
{
    public interface IMessagePublisher
    {
        IDisposable Subscribe(IPushPortObserver observer);
        void Unsubscribe(bool isError);
        void PublishError(Exception exception);
        void Publish(IMessage message);
    }

    internal class MessagePublisher: IDisposable, IMessagePublisher
    {
        private readonly ISet<IMessageParser> _parsers;
        private readonly ILogger _logger;
        private ISet<PushPortObservers> _observers = new HashSet<PushPortObservers>(new PushPortObserverParserComparer());
        
        internal MessagePublisher(ISet<IMessageParser> parsers, ILogger logger)
        {
            _parsers = parsers;
            _logger = logger;
        }
        
        public IDisposable Subscribe(IPushPortObserver observer)
        {
            var messageType = observer.MessageType;
            var observers = _observers.SingleOrDefault(o => o.Parser.MessageType.Equals(messageType));
            if (observers == default)
            {
                var parser = _parsers.Single(p => p.MessageType.Equals(messageType));
                observers = new PushPortObservers(parser);
                _observers.Add(observers);
            }
            
            return observers.Subscribe(observer);
        }
        
        public void Unsubscribe(bool isError)
        {
            foreach (var observers in _observers)
            {
                observers.UnsubscribeAll(isError);
            }
        }
        
        public void PublishError(Exception exception)
        {
            foreach (var observers in _observers)
            {
                foreach (var observer in observers)
                {
                    observer.OnError(exception);
                }
            }
        }

        public void Publish(IMessage message)
        {
            foreach (var observers in _observers)
            {
                Message darwinMessage;
                if (!observers.Parser.TryParse(message, out darwinMessage))
                {
                    _logger.Warning("UnknownMessage @{timestamp}:{id} {msg}", message.NMSTimestamp,
                        message.NMSMessageId, message.NMSType);
                    darwinMessage = new UnknownMessage(message);
                }

                foreach (var observer in observers)
                {
                    observer.OnNext(darwinMessage);
                }
            }
        }

        public void Dispose()
        {
            foreach (var observers in _observers)
            {
                observers.Dispose();
            }
        }
    }
}