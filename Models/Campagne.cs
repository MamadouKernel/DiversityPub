namespace DiversityPub.Models
{
    public class Campagne
    {
        public Guid Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public string? Objectifs { get; set; }
        public Guid ClientId { get; set; }
        public Client? Client { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DiversityPub.Models.enums.StatutCampagne Statut { get; set; } = DiversityPub.Models.enums.StatutCampagne.EnPreparation;

        public ICollection<Activation> Activations { get; set; } = new List<Activation>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public ICollection<DemandeActivation> DemandesActivation { get; set; } = new List<DemandeActivation>();
    }
} 