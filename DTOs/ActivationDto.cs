using DiversityPub.Models.enums;

namespace DiversityPub.DTOs
{
    public class ActivationDto
    {
        public Guid Id { get; set; }
        public DateTime DateActivation { get; set; }
        public TimeSpan HeureDebut { get; set; }
        public TimeSpan HeureFin { get; set; }
        public StatutActivation Statut { get; set; }
        public Guid CampagneId { get; set; }
        public Guid LieuId { get; set; }
        public CampagneDto Campagne { get; set; }
        public LieuDto Lieu { get; set; }
    }

    public class ActivationCreateDto
    {
        public DateTime DateActivation { get; set; }
        public TimeSpan HeureDebut { get; set; }
        public TimeSpan HeureFin { get; set; }
        public StatutActivation Statut { get; set; }
        public Guid CampagneId { get; set; }
        public Guid LieuId { get; set; }
        public List<Guid> AgentTerrainIds { get; set; }
    }

    public class ActivationUpdateDto
    {
        public DateTime DateActivation { get; set; }
        public TimeSpan HeureDebut { get; set; }
        public TimeSpan HeureFin { get; set; }
        public StatutActivation Statut { get; set; }
        public Guid LieuId { get; set; }
        public List<Guid> AgentTerrainIds { get; set; }
    }

    public class ActivationDetailDto
    {
        public Guid Id { get; set; }
        public DateTime DateActivation { get; set; }
        public TimeSpan HeureDebut { get; set; }
        public TimeSpan HeureFin { get; set; }
        public StatutActivation Statut { get; set; }
        public Guid CampagneId { get; set; }
        public Guid LieuId { get; set; }
        public CampagneDto Campagne { get; set; }
        public LieuDto Lieu { get; set; }
        public ICollection<AgentTerrainDto> AgentsTerrain { get; set; }
    }
} 