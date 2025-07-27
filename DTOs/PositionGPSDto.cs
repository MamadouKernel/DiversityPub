namespace DiversityPub.DTOs
{
    public class PositionGPSDto
    {
        public Guid Id { get; set; }
        public Guid AgentTerrainId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Horodatage { get; set; }
        public double Precision { get; set; }
    }

    public class PositionGPSCreateDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Precision { get; set; }
    }

    public class PositionGPSUpdateDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Precision { get; set; }
    }
} 