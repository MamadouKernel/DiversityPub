using DiversityPub.Models.enums;

namespace DiversityPub.Models
{
    public class Activation
    {
        public Guid Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Instructions { get; set; }
        public DateTime DateActivation { get; set; }
        public TimeSpan HeureDebut { get; set; }
        public TimeSpan HeureFin { get; set; }
        public StatutActivation Statut { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        // Motif de suspension (obligatoire pour suspendre)
        public string? MotifSuspension { get; set; }
        public DateTime? DateSuspension { get; set; }
        
        // Validation des preuves
        public bool PreuvesValidees { get; set; } = false;
        public DateTime? DateValidationPreuves { get; set; }
        public Guid? ValideParId { get; set; }
        public Utilisateur? ValidePar { get; set; }

        public Guid CampagneId { get; set; }
        public Campagne? Campagne { get; set; }

        public Guid LieuId { get; set; }
        public Lieu? Lieu { get; set; }

        public ICollection<AgentTerrain> AgentsTerrain { get; set; } = new List<AgentTerrain>();
        
        // Responsable de l'activation (optionnel)
        public Guid? ResponsableId { get; set; }
        public AgentTerrain? Responsable { get; set; }
        
        // Navigation vers les médias et incidents
        public ICollection<Media> Medias { get; set; } = new List<Media>();
        public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
        
        // Navigation vers les feedbacks
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
} 