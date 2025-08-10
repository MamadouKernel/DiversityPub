using Microsoft.EntityFrameworkCore;
using DiversityPub.Models;
using DiversityPub.Models.enums;

namespace DiversityPub.Data
{
    public class DiversityPubDbContext : DbContext
    {
        public DiversityPubDbContext(DbContextOptions<DiversityPubDbContext> options) : base(options)
        {
        }

        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<AgentTerrain> AgentsTerrain { get; set; }
        public DbSet<Campagne> Campagnes { get; set; }
        public DbSet<Activation> Activations { get; set; }
        public DbSet<Lieu> Lieux { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<PositionGPS> PositionsGPS { get; set; }
        public DbSet<Media> Medias { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<DemandeActivation> DemandesActivation { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Relation many-to-many entre Activation et AgentTerrain avec suppression sécurisée
            modelBuilder.Entity<Activation>()
                .HasMany(a => a.AgentsTerrain)
                .WithMany(at => at.Activations)
                .UsingEntity<Dictionary<string, object>>(
                    "ActivationAgentTerrain",
                    join => join
                        .HasOne<AgentTerrain>()
                        .WithMany()
                        .HasForeignKey("AgentsTerrainId")
                        .OnDelete(DeleteBehavior.Restrict), // 🔐 Empêche le cascade
                    join => join
                        .HasOne<Activation>()
                        .WithMany()
                        .HasForeignKey("ActivationsId")
                        .OnDelete(DeleteBehavior.Cascade)
                );

            // ✅ Relations 1:1
            modelBuilder.Entity<AgentTerrain>()
                .HasOne(at => at.Utilisateur)
                .WithOne(u => u.AgentTerrain)
                .HasForeignKey<AgentTerrain>(at => at.UtilisateurId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Client>()
                .HasOne(c => c.Utilisateur)
                .WithOne(u => u.Client)
                .HasForeignKey<Client>(c => c.UtilisateurId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Relations avec AgentTerrain — PAS de cascade
            modelBuilder.Entity<Document>()
                .HasOne(d => d.AgentTerrain)
                .WithMany(at => at.Documents)
                .HasForeignKey(d => d.AgentTerrainId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Incident>()
                .HasOne(i => i.AgentTerrain)
                .WithMany(at => at.Incidents)
                .HasForeignKey(i => i.AgentTerrainId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PositionGPS>()
                .HasOne(p => p.AgentTerrain)
                .WithMany(at => at.PositionsGPS)
                .HasForeignKey(p => p.AgentTerrainId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Media>()
                .HasOne(m => m.AgentTerrain)
                .WithMany(at => at.Medias)
                .HasForeignKey(m => m.AgentTerrainId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Relations Campagne
            modelBuilder.Entity<Campagne>()
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Campagnes)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Activation>()
                .HasOne(a => a.Campagne)
                .WithMany(c => c.Activations)
                .HasForeignKey(a => a.CampagneId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Activation>()
                .HasOne(a => a.Lieu)
                .WithMany(l => l.Activations)
                .HasForeignKey(a => a.LieuId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Configuration de la relation Responsable
            modelBuilder.Entity<Activation>()
                .HasOne(a => a.Responsable)
                .WithMany()
                .HasForeignKey(a => a.ResponsableId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false); // Le responsable est optionnel

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Campagne)
                .WithMany(c => c.Feedbacks)
                .HasForeignKey(f => f.CampagneId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Activation)
                .WithMany(a => a.Feedbacks)
                .HasForeignKey(f => f.ActivationId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // ✅ Relations DemandeActivation
            modelBuilder.Entity<DemandeActivation>()
                .HasOne(d => d.Campagne)
                .WithMany(c => c.DemandesActivation)
                .HasForeignKey(d => d.CampagneId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DemandeActivation>()
                .HasOne(d => d.Client)
                .WithMany(cl => cl.DemandesActivation)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DemandeActivation>()
                .HasOne(d => d.Lieu)
                .WithMany(l => l.DemandesActivation)
                .HasForeignKey(d => d.LieuId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DemandeActivation>()
                .HasOne(d => d.ReponduPar)
                .WithMany()
                .HasForeignKey(d => d.ReponduParId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Utilisateur admin par défaut
            var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            modelBuilder.Entity<Utilisateur>().HasData(new Utilisateur
            {
                Id = adminId,
                Nom = "Super",
                Prenom = "Admin",
                Email = "admin@diversitypub.ci",
                MotDePasse = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = Role.Admin
            });
        }
    }
}
