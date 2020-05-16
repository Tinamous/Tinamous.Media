using System;
using System.Runtime.Serialization;

namespace AnalysisUK.Tinamous.Media.DataAccess.Interfaces.Exceptions
{
    [Serializable]
    public class DataAccessException : Exception
    {
        public DataAccessException()
        {
        }

        public DataAccessException(string message)
            : base(message)
        {
        }

        public DataAccessException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected DataAccessException(SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}