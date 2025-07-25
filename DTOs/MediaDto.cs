using DiversityPub.Models.enums;

namespace DiversityPub.DTOs
{
    public class MediaDto
    {
        public Guid Id { get; set; }
        public TypeMedia Type { get; set; }
        public string Url { get; set; }
        public DateTime DateUpload { get; set; }
        public Guid AgentTerrainId { get; set; }
        public AgentTerrainDto AgentTerrain { get; set; }
    }

    public class MediaCreateDto
    {
        public TypeMedia Type { get; set; }
        public string Url { get; set; }
        public Guid AgentTerrainId { get; set; }
    }

    public class MediaUpdateDto
    {
        public TypeMedia Type { get; set; }
        public string Url { get; set; }
    }
} 