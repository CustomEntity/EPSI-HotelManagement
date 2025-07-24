namespace HotelManagement.Domain.Common;

public class BusinessRuleValidationException : Exception
{
    public IBusinessRule BrokenRule { get; }

    public BusinessRuleValidationException(IBusinessRule brokenRule) 
        : base(brokenRule.Message)
    {
        BrokenRule = brokenRule;
    }
}