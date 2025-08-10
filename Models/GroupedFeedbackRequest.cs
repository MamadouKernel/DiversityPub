namespace DiversityPub.Models
{
    public class GroupedFeedbackRequest
    {
        public List<FeedbackData> Feedbacks { get; set; } = new List<FeedbackData>();
    }

    public class FeedbackData
    {
        public int Note { get; set; }
        public string Commentaire { get; set; } = string.Empty;
        public Guid? CampagneId { get; set; }
        public Guid? ActivationId { get; set; }
    }
} 