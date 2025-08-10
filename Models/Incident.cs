namespace DiversityPub.Models
{
    public class Incident
    {
        public Guid Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priorite { get; set; } = "Normale";
        public string Statut { get; set; } = "Ouvert";
        public DateTime DateCreation { get; set; }
        public DateTime? DateResolution { get; set; }
        public string? CommentaireResolution { get; set; }

        public Guid? AgentTerrainId { get; set; }
        public AgentTerrain? AgentTerrain { get; set; }

        public Guid? ActivationId { get; set; }
        public Activation? Activation { get; set; }
    }
} 