namespace Application
{
    /// <summary>
    /// Provides geocoding functionality for converting addresses into latitude and longitude coordinates using Azure
    /// Maps.
    /// </summary>
    /// <remarks>Implementations of this interface use Azure Maps to resolve addresses to geographic
    /// coordinates. Results may vary depending on the accuracy and completeness of the input address. This interface is
    /// typically used to integrate location-based features into applications. Thread safety depends on the
    /// implementation.</remarks>
    public interface IGeocodingService
    {
        /// <summary>
        /// Geocodes an address and returns latitude and longitude coordinates using Azure Maps.
        /// </summary>
        /// <param name="searchAddress">The address to geocode (e.g., "123 Main Street, Toronto, ON, M5V 3A8, Canada").</param>
        /// <returns>A task that returns coordinates or null if geocoding fails.</returns>
        Task<GeocodingResult?> GeocodeAddressAsync(string searchAddress);
    }

    /// <summary>
    /// Represents the result of a geocoding operation, containing latitude and longitude coordinates.
    /// </summary>
    public class GeocodingResult
    {
        /// <summary>
        /// Gets or sets the latitude component of the geographic coordinate.
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// Gets or sets the geographic longitude coordinate.
        /// </summary>
        public decimal Longitude { get; set; }
    }
}