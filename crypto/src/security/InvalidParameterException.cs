using System;

namespace Org.BouncyCastle.Security
{
    [Serializable]
    public class InvalidParameterException : KeyException
    {
        public InvalidParameterException() : base() { }
        public InvalidParameterException(string message) : base(message) { }
        public InvalidParameterException(string message, Exception exception) : base(message, exception) { }
    }
}
