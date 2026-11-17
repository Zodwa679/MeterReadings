namespace MeterReadings.Application.Common;

public sealed class ConflictException(string message, Exception innerException)
    : Exception(message, innerException);
