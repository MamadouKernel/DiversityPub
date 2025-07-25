namespace DiversityPub.DTOs
{
    public class ClientDto
    {
        public Guid Id { get; set; }
        public Guid UtilisateurId { get; set; }
        public string RaisonSociale { get; set; }
        public string Adresse { get; set; }
        public string RegistreCommerce { get; set; }
        public string NomDirigeant { get; set; }
        public string NomContactPrincipal { get; set; }
        public string TelephoneContactPrincipal { get; set; }
        public string EmailContactPrincipal { get; set; }
        public UtilisateurDto Utilisateur { get; set; }
    }

    public class ClientCreateDto
    {
        public string RaisonSociale { get; set; }
        public string Adresse { get; set; }
        public string RegistreCommerce { get; set; }
        public string NomDirigeant { get; set; }
        public string NomContactPrincipal { get; set; }
        public string TelephoneContactPrincipal { get; set; }
        public string EmailContactPrincipal { get; set; }
        public UtilisateurCreateDto Utilisateur { get; set; }
    }

    public class ClientUpdateDto
    {
        public string RaisonSociale { get; set; }
        public string Adresse { get; set; }
        public string RegistreCommerce { get; set; }
        public string NomDirigeant { get; set; }
        public string NomContactPrincipal { get; set; }
        public string TelephoneContactPrincipal { get; set; }
        public string EmailContactPrincipal { get; set; }
    }
} 