namespace DiversityPub.DTOs
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string Nom { get; set; }
        public string Url { get; set; }
        public DateTime DateUpload { get; set; }
        public Guid AgentTerrainId { get; set; }
        public AgentTerrainDto AgentTerrain { get; set; }
    }

    public class DocumentCreateDto
    {
        public string Nom { get; set; }
        public string Url { get; set; }
        public Guid AgentTerrainId { get; set; }
    }

    public class DocumentUpdateDto
    {
        public string Nom { get; set; }
        public string Url { get; set; }
    }
} 