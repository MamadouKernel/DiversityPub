namespace DiversityPub.DTOs
{
    public class LieuDto
    {
        public Guid Id { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
    }

    public class LieuCreateDto
    {
        public string Nom { get; set; }
        public string Adresse { get; set; }
    }

    public class LieuUpdateDto
    {
        public string Nom { get; set; }
        public string Adresse { get; set; }
    }

    public class LieuDetailDto
    {
        public Guid Id { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public ICollection<ActivationDto> Activations { get; set; }
    }
} 