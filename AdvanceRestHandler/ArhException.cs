using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Arh
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    internal class ArhException : Exception
    {
        private Dictionary<Type, Exception> FallbackExceptions { get; }
        public ArhException()
        {
        }

        public ArhException(Dictionary<Type, Exception> fallbackExceptions)
        {
            FallbackExceptions = fallbackExceptions;
        }

        public ArhException(string message) : base(message)
        {
        }

        public ArhException(string message, Dictionary<Type, Exception> fallbackExceptions) : base(message)
        {
            FallbackExceptions = fallbackExceptions;
        }

        public ArhException(string message, Exception innerException, Dictionary<Type, Exception> fallbackExceptions) : base(message, innerException)
        {
            FallbackExceptions = fallbackExceptions;
        }
    }
}