namespace DiversityPub.Models
{
    public class Feedback
    {
        public Guid Id { get; set; }
        public int Note { get; set; }
        public string Commentaire { get; set; } = string.Empty;
        public DateTime DateFeedback { get; set; }

        public Guid CampagneId { get; set; }
        public Campagne Campagne { get; set; } = null!;
    }
} 