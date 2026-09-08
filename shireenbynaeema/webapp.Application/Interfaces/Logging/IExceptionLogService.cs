namespace Application
{
    using Domain;

    /// <summary>
    /// Defines a service for managing and retrieving exception log entries.
    /// </summary>
    /// <remarks>Implementations of this interface provide operations for accessing and manipulating exception
    /// logs, typically for auditing or diagnostic purposes. This interface extends the generic IService interface for
    /// ExceptionLog entities.</remarks>
    public interface IExceptionLogService : IService<ExceptionLog>
    {
    }
}