using DiversityPub.Models.enums;

namespace DiversityPub.Models
{
    public class Media
    {
        public Guid Id { get; set; }
        public TypeMedia Type { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateUpload { get; set; }
        
        // Validation du média
        public bool Valide { get; set; } = false;
        public DateTime? DateValidation { get; set; }
        public Guid? ValideParId { get; set; }
        public Utilisateur? ValidePar { get; set; }
        public string? CommentaireValidation { get; set; }

        public Guid AgentTerrainId { get; set; }
        public AgentTerrain AgentTerrain { get; set; } = null!;

        public Guid? ActivationId { get; set; }
        public Activation? Activation { get; set; }
    }
} 