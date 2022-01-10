using System;

namespace SSSpy.MsSQL.Exceptions
{
    public class InvalidSQLException : Exception
    {
        public InvalidSQLException(string message) : base(message)
        {

        }
    }
}
