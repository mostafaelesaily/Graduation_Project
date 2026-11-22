namespace MedLink.Application.DTOs.Common.MedicalSearchDTOs
{
    public class ExternalProviderDto
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string WorkingHours { get; set; }
        public string GoogleMapsUrl { get; set; }
        public string Source { get; set; } = "Gemini";
    }
}
