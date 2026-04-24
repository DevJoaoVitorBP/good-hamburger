namespace GoodHamburger.Application.Exceptions;

public sealed class BusinessRuleValidationException(string message) : Exception(message);
