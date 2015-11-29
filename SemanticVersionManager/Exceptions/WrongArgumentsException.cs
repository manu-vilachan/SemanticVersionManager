using System;
using System.Runtime.Serialization;

namespace SemanticVersionManager.Exceptions
{
    [Serializable]
    public class WrongArgumentsException : Exception
    {
        public string Command { get; set; }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Command", Command);
        }
    }
}