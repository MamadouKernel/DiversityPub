namespace DiversityPub.DTOs
{
    public class AgentTerrainDto
    {
        public Guid Id { get; set; }
        public Guid UtilisateurId { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public UtilisateurDto Utilisateur { get; set; }
    }

    public class AgentTerrainCreateDto
    {
        public string Telephone { get; set; }
        public string Email { get; set; }
        public UtilisateurCreateDto Utilisateur { get; set; }
    }

    public class AgentTerrainUpdateDto
    {
        public string Telephone { get; set; }
        public string Email { get; set; }
    }

    public class AgentTerrainDetailDto
    {
        public Guid Id { get; set; }
        public Guid UtilisateurId { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public UtilisateurDto Utilisateur { get; set; }
        public ICollection<ActivationDto> Activations { get; set; }
        public ICollection<DocumentDto> Documents { get; set; }
        public ICollection<IncidentDto> Incidents { get; set; }
        public ICollection<PositionGPSDto> PositionsGPS { get; set; }
        public ICollection<MediaDto> Medias { get; set; }
    }
} 