namespace DiversityPub.Models
{
    public class Lieu
    {
        public Guid Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;

        public ICollection<Activation> Activations { get; set; } = new List<Activation>();
        public ICollection<DemandeActivation> DemandesActivation { get; set; } = new List<DemandeActivation>();
    }
} 