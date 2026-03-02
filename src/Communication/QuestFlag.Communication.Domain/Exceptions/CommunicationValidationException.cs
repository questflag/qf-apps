namespace QuestFlag.Communication.Domain.Exceptions;

public class CommunicationValidationException : Exception
{
    public CommunicationValidationException(string message) : base(message) { }
}
