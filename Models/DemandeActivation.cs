using System.ComponentModel.DataAnnotations;
using DiversityPub.Models.enums;

namespace DiversityPub.Models
{
    public class DemandeActivation
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Le nom de l'activation est requis")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string Nom { get; set; }
        
        [StringLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "La date d'activation est requise")]
        public DateTime DateActivation { get; set; }
        
        [Required(ErrorMessage = "L'heure de début est requise")]
        public TimeSpan HeureDebut { get; set; }
        
        [Required(ErrorMessage = "L'heure de fin est requise")]
        public TimeSpan HeureFin { get; set; }
        
        [StringLength(1000, ErrorMessage = "Les instructions ne peuvent pas dépasser 1000 caractères")]
        public string? Instructions { get; set; }
        
        [Required(ErrorMessage = "Le lieu est requis")]
        public Guid LieuId { get; set; }
        public virtual Lieu? Lieu { get; set; }
        
        [Required(ErrorMessage = "La campagne est requise")]
        public Guid CampagneId { get; set; }
        public virtual Campagne? Campagne { get; set; }
        
        [Required(ErrorMessage = "Le client est requis")]
        public Guid ClientId { get; set; }
        public virtual Client? Client { get; set; }
        
        public DateTime DateDemande { get; set; } = DateTime.Now;
        
        public StatutDemande Statut { get; set; } = StatutDemande.EnAttente;
        
        public string? MotifRefus { get; set; }
        
        public DateTime? DateReponse { get; set; }
        
        public Guid? ReponduParId { get; set; }
        public virtual Utilisateur? ReponduPar { get; set; }
    }
} 