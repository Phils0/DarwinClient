using System;
using System.Collections.Generic;
using System.Linq;
using Apache.NMS;
using DarwinClient.Parsers;
using Serilog;

namespace DarwinClient
{
    /// <summary>
    /// Publishes Pushport Messages
    /// Acts as an intermediary between the topic and the observer(s)
    /// is equivalent to <see cref="System.IObservable{Message}"/>
    /// Responsible for converting the ActiveMQ message to a <see cref="Message"/> type
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Subscribe to publisher
        /// </summary>
        /// <param name="observer">observer</param>
        /// <returns>A disposable instance to allow the observer to stop subscribing</returns>
        IDisposable Subscribe(IPushPortObserver observer);
        /// <summary>
        /// Unsubscribe all observers
        /// </summary>
        /// <param name="isError">If unsubscribing due to error</param>
        void Unsubscribe(bool isError);
        /// <summary>
        /// Publish error to observers
        /// </summary>
        /// <param name="exception"></param>
        void PublishError(Exception exception);
        /// <summary>
        /// Publish topic message to observers
        /// </summary>
        /// <param name="message"></param>
        void Publish(IMessage message);
    }
    
    /// <summary>
    /// With <seealso cref="PushPortObservers"/> handles broadcasting the pushport
    /// messages to observer(s).
    /// </summary>
    /// <remarks>
    /// Responsible for converting the Active MQ Message to the correct internal <see cref="Message"/> type
    /// Supports publishing into multiple different internal <see cref="Message"/> formats
    /// </remarks>
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
            var observers = _observers.SingleOrDefault(o => o.Parser.MessageType == messageType);
            if (observers == default)
            {
                var parser = _parsers.Single(p => p.MessageType == messageType);
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
    
    public static class Publisher
    {
        public static IMessagePublisher CreateDefault(ILogger logger)
        {
            return new MessagePublisher(GetParsers(logger), logger);
        }   
        
        internal static HashSet<IMessageParser> GetParsers(ILogger logger)
        {
            return new HashSet<IMessageParser>(
                new IMessageParser[]
                {
                    new ToDarwinMessageParser(logger), new ToXmlParser(logger)
                });
        }  
    }
}