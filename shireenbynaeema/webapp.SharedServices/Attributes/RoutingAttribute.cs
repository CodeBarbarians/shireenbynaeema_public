namespace SharedServices
{
    /// <summary>
    /// Custom attribute to define routing information for fields, allowing multiple routes to be associated with a single field. This attribute can be used to
    /// specify the URL and whether the route should be included in the menu. It provides a way to organize and manage routing information directly on the fields
    /// of a class, making it easier to maintain and understand the routing structure of the application.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class RoutingAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingAttribute"/> class with the specified URL and menu indicator.
        /// </summary>
        /// <param name="url">The route URL pattern to associate with the attribute. Cannot be null or empty.</param>
        /// <param name="isMenu">Indicates whether the route should be included in menu navigation. Set to <see langword="true"/> to mark the
        /// route as a menu item; otherwise, <see langword="false"/>.</param>
        public RoutingAttribute(string url, bool isMenu = false)
        {
            this.Url = url;
            this.IsMenu = isMenu;
        }

        /// <summary>
        /// Gets the route URL pattern associated with this attribute. This property is set through the constructor and cannot be modified afterward. It represents the URL pattern that will be used for routing purposes in the application.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets a value indicating whether the route associated with this attribute should be included in menu navigation. This property is set through the constructor and cannot be modified afterward. If set to <see langword="true"/>, it indicates that the route should be treated as a menu item in the application's navigation structure; otherwise, it will not be included in the menu.
        /// </summary>
        public bool IsMenu { get; }
    }
}