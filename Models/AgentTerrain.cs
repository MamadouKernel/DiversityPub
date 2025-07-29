namespace DiversityPub.Models
{
    public class AgentTerrain
    {
        public Guid Id { get; set; }
        public Guid UtilisateurId { get; set; }
        public Utilisateur Utilisateur { get; set; } = null!;

        public string Telephone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Propriété pour le statut de connexion en temps réel
        public bool EstConnecte { get; set; } = false;
        public DateTime? DerniereConnexion { get; set; }
        public DateTime? DerniereDeconnexion { get; set; }

        public ICollection<Activation> Activations { get; set; } = new List<Activation>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
        public ICollection<PositionGPS> PositionsGPS { get; set; } = new List<PositionGPS>();
        public ICollection<Media> Medias { get; set; } = new List<Media>();
    }
} 