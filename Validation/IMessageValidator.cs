namespace SqsProcessor.Validation;

public interface IMessageValidator
{
    bool Validate(string messageBody, out string validationError);
}
