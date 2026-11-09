using System;

namespace Cart.Api.Exceptions
{
    public class NotFoundException : Exception
    {
        public string ErrorCode { get; }

        public NotFoundException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}