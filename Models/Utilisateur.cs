using DiversityPub.Models.enums;

namespace DiversityPub.Models
{
    public class Utilisateur
    {
        public Guid Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MotDePasse { get; set; } = string.Empty;
        public int Supprimer { get; set; } = 0;

        public Client? Client { get; set; }
        public AgentTerrain? AgentTerrain { get; set; }
        public Role Role { get; set; }
    }
} 