using MedLink.Domain.Entities.Provider;
using MedLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Application.Specifications.ProviderProfiles
{
    public class MedicalSearchSpecification : BaseSpecification<ProviderProfile>
    {
        public MedicalSearchSpecification(
            string keyword,
            double latitude,
            double longitude,
            int radiusInMeters,
            int pageNum,
            int pageSize)
            : base(provider =>
                provider.verificationStatus == VerificationStatus.Approved &&
                provider.ProviderServices.Any(service =>
                    service.IsActive &&
                    (EF.Functions.Like(service.ServiceName, $"%{keyword}%") ||
                     EF.Functions.Like(service.Description, $"%{keyword}%")))
            )
        {
            AddInclude(provider => provider.user);
            AddInclude(provider => provider.ProviderServices);
            AddInclude(provider => provider.ProviderLocations);

            AddOrderByDescending(provider => provider.CreatedAt);

            ApplyPaging((pageNum - 1) * pageSize, pageSize);

            SearchLatitude = latitude;
            SearchLongitude = longitude;
            SearchRadius = radiusInMeters;
        }

        public double SearchLatitude { get; }
        public double SearchLongitude { get; }
        public int SearchRadius { get; }

        /// <summary>
        /// Calculates distance using Haversine formula (in meters)
        /// </summary>
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371;

            double dLat = (lat2 - lat1) * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distanceKm = earthRadiusKm * c;

            return distanceKm * 1000; // Convert to meters
        }

        /// <summary>
        /// Checks if provider location is within search radius
        /// </summary>
        public bool IsWithinRadius(ProviderProfile provider)
        {
            var locations = provider.ProviderLocations
                .Where(loc => loc.IsActive && loc.Latitude.HasValue && loc.Longitude.HasValue)
                .ToList();

            if (!locations.Any())
                return false;

            return locations.Any(loc =>
                CalculateDistance(SearchLatitude, SearchLongitude, loc.Latitude.Value, loc.Longitude.Value) <= SearchRadius);
        }
    }
}
