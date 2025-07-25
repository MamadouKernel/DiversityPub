namespace DiversityPub.DTOs
{
    public class UtilisateurDto
    {
        public Guid Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Email { get; set; }
    }

    public class UtilisateurCreateDto
    {
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Email { get; set; }
        public string MotDePasse { get; set; }
    }

    public class UtilisateurUpdateDto
    {
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Email { get; set; }
    }

    public class UtilisateurLoginDto
    {
        public string Email { get; set; }
        public string MotDePasse { get; set; }
    }
} 