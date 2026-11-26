namespace MedLink.Application.DTOs.Common.RecommendationDTOs
{
    public class RecommendationRequestDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RadiusInMeters { get; set; } = 5000;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
