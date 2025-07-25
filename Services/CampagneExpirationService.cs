using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models.enums;

namespace DiversityPub.Services
{
    public class CampagneExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CampagneExpirationService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Vérifier toutes les heures

        public CampagneExpirationService(IServiceProvider serviceProvider, ILogger<CampagneExpirationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service de vérification des campagnes expirées démarré");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndUpdateExpiredCampagnesAsync();
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la vérification des campagnes expirées");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Attendre 5 minutes en cas d'erreur
                }
            }
        }

        private async Task CheckAndUpdateExpiredCampagnesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DiversityPubDbContext>();

            var campagnesExpirees = await context.Campagnes
                .Where(c => c.DateFin < DateTime.Today 
                           && c.Statut != StatutCampagne.Terminee 
                           && c.Statut != StatutCampagne.Annulee)
                .ToListAsync();

            if (campagnesExpirees.Any())
            {
                foreach (var campagne in campagnesExpirees)
                {
                    campagne.Statut = StatutCampagne.Terminee;
                    _logger.LogInformation($"Campagne '{campagne.Nom}' automatiquement terminée (date de fin dépassée)");
                }

                await context.SaveChangesAsync();
                _logger.LogInformation($"{campagnesExpirees.Count} campagne(s) automatiquement terminée(s)");
            }
        }
    }
} 