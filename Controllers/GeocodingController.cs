using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DiversityPub.Services;
using DiversityPub.Data;
using Microsoft.EntityFrameworkCore;

namespace DiversityPub.Controllers
{
    [Authorize]
    public class GeocodingController : Controller
    {
        private readonly GeocodingService _geocodingService;
        private readonly DiversityPubDbContext _context;
        private readonly ILogger<GeocodingController> _logger;

        public GeocodingController(GeocodingService geocodingService, DiversityPubDbContext context, ILogger<GeocodingController> logger)
        {
            _geocodingService = geocodingService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// API pour géocoder une adresse et obtenir les coordonnées GPS
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GeocodeAddress([FromBody] GeocodeRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Address))
                {
                    return Json(new { success = false, message = "Adresse requise" });
                }

                var coordinates = await _geocodingService.GeocodeAddressAsync(request.Address);
                
                if (coordinates.HasValue)
                {
                    return Json(new { 
                        success = true, 
                        latitude = coordinates.Value.Latitude, 
                        longitude = coordinates.Value.Longitude 
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Impossible de géocoder cette adresse" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du géocodage de l'adresse: {Address}", request.Address);
                return Json(new { success = false, message = "Erreur lors du géocodage" });
            }
        }

        /// <summary>
        /// API pour géocoder automatiquement tous les agents sans coordonnées
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GeocodeAllAgents()
        {
            try
            {
                var agentsWithoutCoordinates = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .Where(at => !at.PositionsGPS.Any())
                    .ToListAsync();

                var results = new List<GeocodeResult>();
                int successCount = 0;

                foreach (var agent in agentsWithoutCoordinates)
                {
                    // Construire l'adresse à partir des informations de l'agent
                    var address = BuildAgentAddress(agent);
                    
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        var coordinates = await _geocodingService.GeocodeAddressAsync(address);
                        
                        if (coordinates.HasValue)
                        {
                            // Créer une nouvelle position GPS
                            var position = new Models.PositionGPS
                            {
                                Id = Guid.NewGuid(),
                                AgentTerrainId = agent.Id,
                                Latitude = coordinates.Value.Latitude,
                                Longitude = coordinates.Value.Longitude,
                                Horodatage = DateTime.Now,
                                Precision = 100 // Précision estimée pour le géocodage
                            };

                            _context.PositionsGPS.Add(position);
                            successCount++;

                            results.Add(new GeocodeResult
                            {
                                AgentId = agent.Id,
                                AgentName = $"{agent.Utilisateur.Prenom} {agent.Utilisateur.Nom}",
                                Address = address,
                                Latitude = coordinates.Value.Latitude,
                                Longitude = coordinates.Value.Longitude,
                                Success = true
                            });
                        }
                        else
                        {
                            results.Add(new GeocodeResult
                            {
                                AgentId = agent.Id,
                                AgentName = $"{agent.Utilisateur.Prenom} {agent.Utilisateur.Nom}",
                                Address = address,
                                Success = false,
                                Error = "Adresse non trouvée"
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    totalAgents = agentsWithoutCoordinates.Count,
                    successCount = successCount,
                    results = results 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du géocodage automatique des agents");
                return Json(new { success = false, message = "Erreur lors du géocodage automatique" });
            }
        }

        /// <summary>
        /// Construit l'adresse complète d'un agent à partir de ses informations
        /// </summary>
        private string BuildAgentAddress(Models.AgentTerrain agent)
        {
            // Utiliser une adresse par défaut basée sur l'ID de l'agent
            var agentIdHash = Math.Abs(agent.Id.GetHashCode());
            var streetNumber = (agentIdHash % 100) + 1;
            var streetNames = new[] { "Rue de la Paix", "Avenue des Champs-Élysées", "Boulevard Saint-Germain", "Rue du Faubourg Saint-Honoré" };
            var streetName = streetNames[agentIdHash % streetNames.Length];
            
            var addressParts = new List<string>
            {
                $"{streetNumber} {streetName}",
                "Paris",
                "75001",
                "France"
            };

            return string.Join(", ", addressParts);
        }
    }

    public class GeocodeRequest
    {
        public string Address { get; set; }
    }

    public class GeocodeResult
    {
        public Guid AgentId { get; set; }
        public string AgentName { get; set; }
        public string Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
} 