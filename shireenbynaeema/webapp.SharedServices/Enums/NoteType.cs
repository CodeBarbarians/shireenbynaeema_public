namespace SharedServices
{
    /// <summary>
    /// Enumeration representing different types of notes that can be associated with work orders or other entities within the application.
    /// Each note type corresponds to a specific category or context, such as operations, public information, management, accounting, main notes,
    /// property-related notes, subclient notes, and contractor notes. This enumeration helps to categorize and organize notes based on their purpose
    /// and relevance to different aspects of the business or workflow processes. By using this enumeration, developers can easily identify and
    /// manage notes according to their type, ensuring that relevant information is properly categorized and accessible when needed.
    /// </summary>
    public enum NoteType
    {
        /// <summary>
        /// Notes related to operations activities.
        /// </summary>
        NOperations = 0,

        /// <summary>
        /// Public notes visible to all relevant users.
        /// </summary>
        NPublic = 1,

        /// <summary>
        /// Notes related to management-level information.
        /// </summary>
        NManagement = 2,

        /// <summary>
        /// Notes related to accounting and financial data.
        /// </summary>
        NAccounting = 3,

        /// <summary>
        /// General or main notes not tied to a specific category.
        /// </summary>
        NMain = 4,

        /// <summary>
        /// Notes associated with property-related data.
        /// </summary>
        NProperty = 5,

        /// <summary>
        /// Notes related to sub-client information.
        /// </summary>
        NSubclient = 6,

        /// <summary>
        /// Notes associated with contractors or vendors.
        /// </summary>
        NContractor = 7,
    }
}