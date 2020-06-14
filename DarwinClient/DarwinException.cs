using System;

namespace DarwinClient
{
    public class DarwinException : Exception
    {
        public DarwinException() : base()
        {
        }
        
        public DarwinException(string message) : base(message)
        {
        }

        public DarwinException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    
    public class DarwinConnectionException : DarwinException
    {
        public DarwinConnectionException() : base()
        {
        }
        
        public DarwinConnectionException(string message) : base(message)
        {
        }

        public DarwinConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}