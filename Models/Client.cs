namespace DiversityPub.Models
{
    public class Client
    {
        public Guid Id { get; set; }
        public Guid UtilisateurId { get; set; }
        public Utilisateur? Utilisateur { get; set; }

        public string RaisonSociale { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public string RegistreCommerce { get; set; } = string.Empty;
        public string NomDirigeant { get; set; } = string.Empty;
        public string NomContactPrincipal { get; set; } = string.Empty;
        public string TelephoneContactPrincipal { get; set; } = string.Empty;
        public string EmailContactPrincipal { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; } = DateTime.Now;

        public ICollection<Campagne> Campagnes { get; set; } = new List<Campagne>();
        public ICollection<DemandeActivation> DemandesActivation { get; set; } = new List<DemandeActivation>();
    }
} 