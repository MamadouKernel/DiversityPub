namespace DiversityPub.DTOs
{
    public class FeedbackDto
    {
        public Guid Id { get; set; }
        public int Note { get; set; }
        public string Commentaire { get; set; }
        public DateTime DateFeedback { get; set; }
        public Guid CampagneId { get; set; }
        public CampagneDto Campagne { get; set; }
    }

    public class FeedbackCreateDto
    {
        public int Note { get; set; }
        public string Commentaire { get; set; }
        public Guid CampagneId { get; set; }
    }

    public class FeedbackUpdateDto
    {
        public int Note { get; set; }
        public string Commentaire { get; set; }
    }
} 