using System;
using System.Collections.Generic;
using Apache.NMS;
using Serilog;

namespace DarwinClient.Parsers
{
    public interface IMessageParser
    {
        Type MessageType { get; }
        bool TryParse(IMessage source, out Message parsed);
    }


    public static class Parsers
    {
        public static ISet<IMessageParser> Defaults(ILogger logger)
        {
            return new HashSet<IMessageParser>(
                new IMessageParser[]
                {
                    new ToDarwinMessageParser(logger), new ToXmlParser(logger)
                });
        }
        
    }
}