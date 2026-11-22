namespace MedLink.Application.DTOs.Common.MedicalSearchDTOs
{
    public class MedicalSearchRequestDto
    {
        public string Keyword { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RadiusInMeters { get; set; } = 5000;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
