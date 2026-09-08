namespace Infrastructure
{
    using System.Text.Json;

    using Application;
    using Domain;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Service for geocoding addresses to get latitude and longitude coordinates.
    /// Uses Azure Maps Geocoding API.
    /// </summary>
    public class GeocodingService(HttpClient httpClient, IAppSettingsConfig config, ILogger<GeocodingService> logger) : IGeocodingService
    {
        private const string AzureMapsGeocodingUrl = "https://atlas.microsoft.com/search/address/json";

        // Add static LoggerMessage delegates for improved performance and fixed message templates
        private static readonly Action<ILogger, string, Exception?> NoGeocodingResultsFound =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(1, nameof(NoGeocodingResultsFound)),
                "No geocoding results found for address: {Address}");

        private static readonly Action<ILogger, Exception?> PositionPropertyNotFound =
            LoggerMessage.Define(
                LogLevel.Warning,
                new EventId(2, nameof(PositionPropertyNotFound)),
                "Position property not found in Azure Maps response");

        private static readonly Action<ILogger, Exception?> InvalidCoordinatesReturned =
            LoggerMessage.Define(
                LogLevel.Warning,
                new EventId(3, nameof(InvalidCoordinatesReturned)),
                "Invalid coordinates returned from Azure Maps");

        private static readonly Action<ILogger, string, decimal, decimal, Exception?> SuccessfullyGeocodedAddress =
            LoggerMessage.Define<string, decimal, decimal>(
                LogLevel.Information,
                new EventId(4, nameof(SuccessfullyGeocodedAddress)),
                "Successfully geocoded address: {Address} -> Lat: {Latitude}, Lon: {Longitude}");

        private static readonly Action<ILogger, Exception?> ErrorParsingAzureMapsResponse =
            LoggerMessage.Define(
                LogLevel.Error,
                new EventId(5, nameof(ErrorParsingAzureMapsResponse)),
                "Error parsing Azure Maps response");

        private static readonly Action<ILogger, string, Exception?> ErrorGeocodingAddress =
            LoggerMessage.Define<string>(
                LogLevel.Error,
                new EventId(6, nameof(ErrorGeocodingAddress)),
                "Error geocoding address: {Address}");

        /// <summary>
        /// Geocodes an address using Azure Maps Geocoding API and returns latitude/longitude.
        /// </summary>
        /// <param name="searchAddress">The address to geocode.</param>
        /// <returns>GeocodingResult with Latitude and Longitude, or null if geocoding fails.</returns>
        public async Task<GeocodingResult?> GeocodeAddressAsync(string searchAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchAddress))
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(config.AzureMapsApiKey))
                {
                    return null;
                }

                var url = $"{AzureMapsGeocodingUrl}?api-version=1.0&subscription-key={config.AzureMapsApiKey}&query={Uri.EscapeDataString(searchAddress)}";

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var root = json.RootElement;

                if (!root.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                {
                    NoGeocodingResultsFound(logger, searchAddress, null);
                    return null;
                }

                var firstResult = results[0];

                if (!firstResult.TryGetProperty("position", out var position))
                {
                    PositionPropertyNotFound(logger, null);
                    return null;
                }

                decimal latitude = 0;
                decimal longitude = 0;

                if (position.TryGetProperty("lat", out var latElement))
                {
                    latitude = latElement.GetDecimal();
                }

                if (position.TryGetProperty("lon", out var lonElement))
                {
                    longitude = lonElement.GetDecimal();
                }

                if (latitude == 0 || longitude == 0)
                {
                    InvalidCoordinatesReturned(logger, null);
                    return null;
                }

                SuccessfullyGeocodedAddress(logger, searchAddress, latitude, longitude, null);

                return new GeocodingResult
                {
                    Latitude = latitude,
                    Longitude = longitude,
                };
            }
            catch (JsonException jsonEx)
            {
                ErrorParsingAzureMapsResponse(logger, jsonEx);
                return null;
            }
            catch (Exception ex)
            {
                ErrorGeocodingAddress(logger, searchAddress, ex);
                return null;
            }
        }
    }
}