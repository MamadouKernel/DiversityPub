using DiversityPub.Models.enums;

namespace DiversityPub.DTOs
{
    public class MediaDto
    {
        public Guid Id { get; set; }
        public Guid AgentTerrainId { get; set; }
        public Guid? ActivationId { get; set; }
        public TypeMedia Type { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime DateUpload { get; set; }
    }

    public class MediaCreateDto
    {
        public Guid? ActivationId { get; set; }
        public TypeMedia Type { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
    }

    public class MediaUpdateDto
    {
        public string Description { get; set; }
    }

    public class MediaUploadDto
    {
        public Guid? ActivationId { get; set; }
        public TypeMedia Type { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
    }
} 