namespace DiversityPub.Models
{
    public class Document
    {
        public Guid Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime DateUpload { get; set; }

        public Guid AgentTerrainId { get; set; }
        public AgentTerrain AgentTerrain { get; set; } = null!;
    }
} 