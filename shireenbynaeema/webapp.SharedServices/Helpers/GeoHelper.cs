namespace SharedServices
{
    /// <summary>
    /// Helper class for geospatial calculations, such as determining if a location is within a certain radius of another location. This class provides methods to
    /// calculate distances between geographic coordinates and to check proximity based on specified radius values. It can be used in various scenarios, such as
    /// validating if a user's location is within a service area or calculating distances for location-based features.
    /// </summary>
    public static class GeoHelper
    {
        /// <summary>
        /// The radius of the Earth in meters, used for distance calculations in geospatial functions. This constant is essential for converting angular distances
        /// (in radians) to linear distances (in meters) when calculating the distance between two geographic coordinates using the Haversine formula or similar methods.
        /// </summary>
        private const double EarthRadiusMeters = 6371000; // Earth's radius in meters

        /// <summary>
        /// Determines if the check location is within a specified radius (in meters) of the base location. This method uses the Haversine formula to calculate the distance
        /// between the two locations.
        /// </summary>
        /// <param name="baseloc">baseloc.</param>
        /// <param name="checkloc">checkloc.</param>
        /// <param name="radiusMeters">radiusMeters.</param>
        /// <returns>bool.</returns>
        public static bool IsWithinRadius(LocationRequest baseloc, LocationRequest checkloc, double radiusMeters)
        {
            if (!baseloc.Latitude.HasValue || !baseloc.Longitude.HasValue ||
                !checkloc.Latitude.HasValue || !checkloc.Longitude.HasValue)
            {
                return false; // cannot calculate distance if any coordinate is missing
            }

            double dLat = DegreesToRadians(checkloc.Latitude.Value - baseloc.Latitude.Value);
            double dLon = DegreesToRadians(checkloc.Longitude.Value - baseloc.Longitude.Value);

            double a = (Math.Sin(dLat / 2) * Math.Sin(dLat / 2)) +
                       (Math.Cos(DegreesToRadians(baseloc.Latitude.Value)) *
                       Math.Cos(DegreesToRadians(checkloc.Latitude.Value)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2));

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = EarthRadiusMeters * c;

            return distance <= radiusMeters;
        }

        /// <summary>
        /// Converts degrees to radians, which is necessary for performing trigonometric calculations in geospatial functions. This method takes an angle in degrees and converts it to radians.
        /// </summary>
        /// <param name="degrees">degrees.</param>
        /// <returns>double.</returns>
        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}