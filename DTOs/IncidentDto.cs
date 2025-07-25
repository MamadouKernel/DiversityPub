namespace DiversityPub.DTOs
{
    public class IncidentDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateTime DateDeclaration { get; set; }
        public Guid AgentTerrainId { get; set; }
        public AgentTerrainDto AgentTerrain { get; set; }
    }

    public class IncidentCreateDto
    {
        public string Description { get; set; }
        public Guid AgentTerrainId { get; set; }
    }

    public class IncidentUpdateDto
    {
        public string Description { get; set; }
    }
} 