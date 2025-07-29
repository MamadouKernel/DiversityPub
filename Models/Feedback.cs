namespace DiversityPub.Models
{
    public class Feedback
    {
        public Guid Id { get; set; }
        public int Note { get; set; }
        public string Commentaire { get; set; } = string.Empty;
        public DateTime DateFeedback { get; set; }

        // Feedback sur campagne (optionnel)
        public Guid? CampagneId { get; set; }
        public Campagne? Campagne { get; set; }

        // Feedback sur activation (optionnel)
        public Guid? ActivationId { get; set; }
        public Activation? Activation { get; set; }
    }
} 