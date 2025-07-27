namespace DiversityPub.DTOs
{
    public class IncidentDto
    {
        public Guid Id { get; set; }
        public Guid AgentTerrainId { get; set; }
        public Guid? ActivationId { get; set; }
        public string Titre { get; set; }
        public string Description { get; set; }
        public string Priorite { get; set; }
        public string Statut { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateResolution { get; set; }
    }

    public class IncidentCreateDto
    {
        public Guid? ActivationId { get; set; }
        public string Titre { get; set; }
        public string Description { get; set; }
        public string Priorite { get; set; }
    }

    public class IncidentUpdateDto
    {
        public string Titre { get; set; }
        public string Description { get; set; }
        public string Priorite { get; set; }
        public string Statut { get; set; }
    }
} 