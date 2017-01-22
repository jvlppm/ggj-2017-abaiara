using System;
using System.Collections.Generic;
using System.Linq;

namespace System
{
    public class AggregateException : Exception
    {
        public readonly Exception[] InnerExceptions;

        public AggregateException(IEnumerable<Exception> innerExceptions)
        {
            InnerExceptions = innerExceptions.ToArray();
        }
    }
}