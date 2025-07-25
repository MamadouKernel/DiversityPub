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

        public Guid CampagneId { get; set; }
        public Campagne? Campagne { get; set; }

        public Guid LieuId { get; set; }
        public Lieu? Lieu { get; set; }

        public ICollection<AgentTerrain> AgentsTerrain { get; set; } = new List<AgentTerrain>();
    }
} 