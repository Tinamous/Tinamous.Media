using System;
using System.Runtime.Serialization;

namespace AnalysisUK.Tinamous.Media.DataAccess.Interfaces.Exceptions
{
    [Serializable]
    public class StoreFileException : Exception
    {
        public StoreFileException()
        {
        }

        public StoreFileException(string message)
            : base(message)
        {
        }

        public StoreFileException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected StoreFileException(SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}