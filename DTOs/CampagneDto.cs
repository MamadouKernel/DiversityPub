namespace DiversityPub.DTOs
{
    public class CampagneDto
    {
        public Guid Id { get; set; }
        public string Nom { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public Guid ClientId { get; set; }
        public ClientDto Client { get; set; }
    }

    public class CampagneCreateDto
    {
        public string Nom { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public Guid ClientId { get; set; }
    }

    public class CampagneUpdateDto
    {
        public string Nom { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
    }

    public class CampagneDetailDto
    {
        public Guid Id { get; set; }
        public string Nom { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public Guid ClientId { get; set; }
        public ClientDto Client { get; set; }
        public ICollection<ActivationDto> Activations { get; set; }
        public ICollection<FeedbackDto> Feedbacks { get; set; }
    }
} 