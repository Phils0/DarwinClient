using System;
using Apache.NMS;
using DarwinClient.Parsers;
using Serilog;

namespace DarwinClient
{
    internal class MessagePublisher: IObservable<Message>, IDisposable
    {
        private readonly IMessageParser _parser;
        private readonly ILogger _logger;
        private PushPortObservers _observers = new PushPortObservers();

        internal MessagePublisher(IMessageParser parser, ILogger logger)
        {
            _parser = parser;
            _logger = logger;
        }
        
        public IDisposable Subscribe(IObserver<Message> observer)
        {
            return _observers.Subscribe(observer);
        }
        
        internal void Unsubscribe(bool isError)
        {
            _observers.UnsubscribeAll(false);
        }
        
        internal void PublishError(Exception exception)
        {
            foreach (var observer in _observers)
            {
                observer.OnError(new DarwinConnectionException("Problem with pushport connection", exception));
            }
        }

        internal void Publish(IMessage message)
        {
            Message darwinMessage;
            if (!_parser.TryParse(message, out darwinMessage))
            {
                _logger.Warning("UnknownMessage @{timestamp}:{id} {msg}", message.NMSTimestamp, message.NMSMessageId, message.NMSType);
                darwinMessage = new UnknownMessage(message);
            }
            
            foreach (var observer in _observers)
            {
                observer.OnNext(darwinMessage);
            }       
        }

        public void Dispose()
        {
            _observers.Dispose();
        }
    }
}