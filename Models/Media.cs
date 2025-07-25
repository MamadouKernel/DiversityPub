using DiversityPub.Models.enums;

namespace DiversityPub.Models
{
    public class Media
    {
        public Guid Id { get; set; }
        public TypeMedia Type { get; set; }
        public string Url { get; set; } = string.Empty;
        public DateTime DateUpload { get; set; }

        public Guid AgentTerrainId { get; set; }
        public AgentTerrain AgentTerrain { get; set; } = null!;
    }
} 