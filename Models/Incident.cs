namespace DiversityPub.Models
{
    public class Incident
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime DateDeclaration { get; set; }

        public Guid AgentTerrainId { get; set; }
        public AgentTerrain AgentTerrain { get; set; } = null!;
    }
} 