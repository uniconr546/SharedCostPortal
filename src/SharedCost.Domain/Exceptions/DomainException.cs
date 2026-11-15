namespace SharedCost.Domain.Exceptions;

public class DomainException : Exception
{
    public string ErrorCode { get; }

    public DomainException(string message, string errorCode = "BUSINESS_RULE_VIOLATION")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
