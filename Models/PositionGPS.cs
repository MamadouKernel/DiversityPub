namespace DiversityPub.Models
{
    public class PositionGPS
    {
        public Guid Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Horodatage { get; set; }
        public double Precision { get; set; }

        public Guid AgentTerrainId { get; set; }
        public AgentTerrain AgentTerrain { get; set; } = null!;
    }
} 