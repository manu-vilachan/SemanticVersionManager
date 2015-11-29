using System;

namespace SemanticVersionManager.Exceptions
{
    [Serializable]
    internal class ProcessCommandException : Exception
    {
        public ProcessCommandException()
        {
            
        }

        public ProcessCommandException(string message) : base(message)
        {

        }

        public ProcessCommandException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}