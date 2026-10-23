namespace AiNotes.Api.Services;

public sealed class AiServiceException(string message, Exception? innerException = null)
    : Exception(message, innerException);