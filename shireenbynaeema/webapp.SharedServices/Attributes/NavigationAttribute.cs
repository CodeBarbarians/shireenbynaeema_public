namespace SharedServices
{
    /// <summary>
    /// Custom attribute to specify the navigation URL for a class. This attribute can be applied to classes to indicate the URL that should be used for
    /// navigation purposes in the application. It allows developers to easily associate a specific URL with a class, which can be useful for routing
    /// and navigation within the application.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]

    public class NavigationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationAttribute"/> class with the specified URL.
        /// </summary>
        /// <param name="url">The URL to associate with the navigation attribute. Cannot be null or empty.</param>
        public NavigationAttribute(string url)
        {
            this.Url = url;
        }

        /// <summary>
        /// Gets the navigation URL associated with the class. This URL can be used for routing and navigation purposes within the application.
        /// </summary>
        public string Url { get; }
    }
}