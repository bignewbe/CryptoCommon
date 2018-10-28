using System;

namespace CryptoCommon
{
    public class WithdrawIdNotExistException : Exception
    {
        public WithdrawIdNotExistException(string message) : base(message)
        {
        }
        public WithdrawIdNotExistException(string message, Exception innerException): base(message, innerException)
        {
        }
    }

    public class OrderIdNotExistException : Exception
    {
        public OrderIdNotExistException(string message) : base(message)
        {
        }
        public OrderIdNotExistException(string message, Exception innerException): base(message, innerException)
        {
        }
    }

    public class ErrorReturnedException : Exception
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public ErrorReturnedException(string errorCode, string message) : base(message)
        {
            this.ErrorCode = errorCode;
            this.ErrorMessage = message;
        }
        public ErrorReturnedException(string errorCode, string message, Exception innerException): base(message, innerException)
        {
            this.ErrorCode = errorCode;
            this.ErrorMessage = message;
        }
    }

    //public class InsufficientFundException : Exception
    //{
    //    public string ErrorCode { get; set; }
    //    public string ErrorMessage { get; set; }
    //    public InsufficientFundException(string errorCode, string message) : base(message)
    //    {
    //        this.ErrorCode = errorCode;
    //        this.ErrorMessage = message;
    //    }
    //    public InsufficientFundException(string errorCode, string message, Exception innerException) : base(message, innerException)
    //    {
    //        this.ErrorCode = errorCode;
    //        this.ErrorMessage = message;
    //    }
    //}
}
