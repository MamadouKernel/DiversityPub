using DiversityPub.Data;
using DiversityPub.Models;
using Microsoft.EntityFrameworkCore;

namespace DiversityPub.Services
{
    public class GeolocationService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private System.Threading.Timer? _timer;
        private readonly ILogger<GeolocationService> _logger;

        public GeolocationService(IServiceProvider serviceProvider, ILogger<GeolocationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service de géolocalisation démarré.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5)); // Mise à jour toutes les 5 minutes

            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DiversityPubDbContext>();

                // Récupérer tous les agents terrain actifs
                var agentsTerrain = await context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .Where(at => at.Utilisateur.Role == DiversityPub.Models.enums.Role.AgentTerrain)
                    .ToListAsync();

                foreach (var agent in agentsTerrain)
                {
                    // Simuler une position GPS (en production, vous utiliseriez une vraie API GPS)
                    var position = await GetAgentPosition(agent);
                    
                    if (position != null)
                    {
                        var positionGPS = new PositionGPS
                        {
                            Id = Guid.NewGuid(),
                            AgentTerrainId = agent.Id,
                            Latitude = position.Latitude,
                            Longitude = position.Longitude,
                            Horodatage = DateTime.Now,
                            Precision = position.Precision
                        };

                        context.PositionsGPS.Add(positionGPS);
                    }
                }

                await context.SaveChangesAsync();
                _logger.LogInformation($"Positions mises à jour pour {agentsTerrain.Count} agents.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour des positions GPS.");
            }
        }

        private async Task<PositionGPS?> GetAgentPosition(AgentTerrain agent)
        {
            // En production, cette méthode devrait récupérer la vraie position GPS
            // depuis l'appareil de l'agent ou une API de localisation
            
            try
            {
                // TODO: Remplacer par la vraie récupération GPS
                // Exemples d'implémentation :
                
                // Option 1: API de localisation par IP (pour test)
                // var position = await GetLocationByIP(agent);
                
                // Option 2: Récupération depuis l'appareil de l'agent
                // var position = await GetDeviceLocation(agent);
                
                // Option 3: API GPS externe
                // var position = await GetGPSLocation(agent);
                
                // Pour l'instant, retourner null pour éviter la simulation
                // L'agent devra envoyer sa position via l'API UpdatePosition
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération de la position pour l'agent {agent.Id}");
                return null;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service de géolocalisation arrêté.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
} 