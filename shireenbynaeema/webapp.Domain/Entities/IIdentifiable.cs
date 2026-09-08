namespace Domain
{
    /// <summary>
    /// Defines a contract for entities that have a unique identifier.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        Guid Id { get; set; }
    }
}