using System;
using System.Collections.Generic;
using System.Linq;

namespace SSSpy.Exceptions
{
    public class InvalidSQLException : Exception
    {
        public InvalidSQLException(string message) : base(message)
        {

        }
    }
}
