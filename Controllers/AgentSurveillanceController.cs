using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using Microsoft.AspNetCore.Authorization;

namespace DiversityPub.Controllers
{
    [Authorize(Roles = "Admin,ChefProjet")]
    public class AgentSurveillanceController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public AgentSurveillanceController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: AgentSurveillance - Vue de surveillance des agents
        public async Task<IActionResult> Index()
        {
            try
            {
                var agentsWithPositions = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .Include(at => at.PositionsGPS.OrderByDescending(p => p.Horodatage).Take(1))
                    .Include(at => at.Activations.Where(a => a.Statut == DiversityPub.Models.enums.StatutActivation.EnCours))
                        .ThenInclude(a => a.Campagne)
                    .Include(at => at.Activations.Where(a => a.Statut == DiversityPub.Models.enums.StatutActivation.EnCours))
                        .ThenInclude(a => a.Lieu)
                    .Include(at => at.Incidents.Where(i => i.Statut == "Ouvert" || i.Statut == "EnCours"))
                    .OrderBy(at => at.Utilisateur.Nom)
                    .ToListAsync();

                return View(agentsWithPositions);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors du chargement des agents: {ex.Message}";
                return View(new List<AgentTerrain>());
            }
        }

                // GET: AgentSurveillance/Positions - API pour récupérer les positions en temps réel
        [HttpGet]
        public async Task<IActionResult> GetPositions()
        {
            try
            {
                var positions = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .Include(at => at.PositionsGPS.OrderByDescending(p => p.Horodatage).Take(1))
                    .Include(at => at.Activations)
                    .Include(at => at.Incidents)
                    .Select(at => new
                    {
                        agentId = at.Id,
                        agentName = $"{at.Utilisateur.Prenom} {at.Utilisateur.Nom}",
                        agentEmail = at.Utilisateur.Email,
                        agentPhone = at.Telephone,
                        lastPosition = at.PositionsGPS.FirstOrDefault() != null ? new
                        {
                            latitude = at.PositionsGPS.FirstOrDefault().Latitude,
                            longitude = at.PositionsGPS.FirstOrDefault().Longitude,
                            precision = at.PositionsGPS.FirstOrDefault().Precision,
                            horodatage = at.PositionsGPS.FirstOrDefault().Horodatage
                        } : null,
                        isOnline = at.EstConnecte, // Utiliser le statut de connexion direct
                        activeActivations = at.Activations.Count(a => a.Statut == DiversityPub.Models.enums.StatutActivation.EnCours),
                        openIncidents = at.Incidents.Count(i => i.Statut == "Ouvert" || i.Statut == "EnCours")
                    })
                    .ToListAsync();

                // Log pour diagnostiquer
                var agentsWithPositions = positions.Where(p => p.lastPosition != null).ToList();
                var agentsWithoutPositions = positions.Where(p => p.lastPosition == null).ToList();
                
                Console.WriteLine($"Agents avec positions: {agentsWithPositions.Count}");
                Console.WriteLine($"Agents sans positions: {agentsWithoutPositions.Count}");
                Console.WriteLine($"Agents connectés: {positions.Count(p => p.isOnline)}");
                
                // Log détaillé de chaque agent
                foreach (var agent in positions)
                {
                    Console.WriteLine($"Agent: {agent.agentName} - EstConnecte: {agent.isOnline}");
                }
                
                // Si aucun agent n'a de position, créer des positions de test pour les agents connectés
                if (!agentsWithPositions.Any() && positions.Any(p => p.isOnline))
                {
                    var connectedAgents = positions.Where(p => p.isOnline).ToList();
                    var coteDIvoirePositions = new[]
                    {
                        new { lat = 5.3600, lng = -4.0083, name = "Abidjan" },
                        new { lat = 6.8500, lng = -7.3500, name = "Man" },
                        new { lat = 7.6900, lng = -5.0300, name = "Bouaké" },
                        new { lat = 5.4800, lng = -3.2000, name = "Grand-Bassam" },
                        new { lat = 6.1300, lng = -5.9500, name = "Yamoussoukro" }
                    };

                    for (int i = 0; i < Math.Min(connectedAgents.Count, coteDIvoirePositions.Length); i++)
                    {
                        var agent = connectedAgents[i];
                        var position = coteDIvoirePositions[i];
                        
                        var testPosition = new PositionGPS
                        {
                            Id = Guid.NewGuid(),
                            AgentTerrainId = agent.agentId,
                            Latitude = position.lat,
                            Longitude = position.lng,
                            Precision = 50.0,
                            Horodatage = DateTime.Now.AddMinutes(-i * 2)
                        };

                        _context.PositionsGPS.Add(testPosition);
                    }

                    await _context.SaveChangesAsync();
                    
                    // Recharger les données avec les nouvelles positions
                    positions = await _context.AgentsTerrain
                        .Include(at => at.Utilisateur)
                        .Include(at => at.PositionsGPS.OrderByDescending(p => p.Horodatage).Take(1))
                        .Include(at => at.Activations)
                        .Include(at => at.Incidents)
                        .Select(at => new
                        {
                            agentId = at.Id,
                            agentName = $"{at.Utilisateur.Prenom} {at.Utilisateur.Nom}",
                            agentEmail = at.Utilisateur.Email,
                            agentPhone = at.Telephone,
                            lastPosition = at.PositionsGPS.FirstOrDefault() != null ? new
                            {
                                latitude = at.PositionsGPS.FirstOrDefault().Latitude,
                                longitude = at.PositionsGPS.FirstOrDefault().Longitude,
                                precision = (double)at.PositionsGPS.FirstOrDefault().Precision,
                                horodatage = at.PositionsGPS.FirstOrDefault().Horodatage
                            } : null,
                            isOnline = at.EstConnecte,
                            activeActivations = at.Activations.Count(a => a.Statut == DiversityPub.Models.enums.StatutActivation.EnCours),
                            openIncidents = at.Incidents.Count(i => i.Statut == "Ouvert" || i.Statut == "EnCours")
                        })
                        .ToListAsync();
                    
                    Console.WriteLine($"Positions créées pour {connectedAgents.Count} agents connectés");
                }

                // Retourner TOUS les agents (avec ou sans positions) pour les statistiques correctes
                Console.WriteLine($"Agents totaux: {positions.Count}");
                Console.WriteLine($"Agents connectés: {positions.Count(p => p.isOnline)}");

                // Convertir en format simple pour éviter les références circulaires
                var simpleData = positions.Select(p => new
                {
                    agentId = p.agentId.ToString(),
                    agentName = p.agentName,
                    agentEmail = p.agentEmail,
                    agentPhone = p.agentPhone,
                    lastPosition = p.lastPosition,
                    isOnline = p.isOnline,
                    activeActivations = p.activeActivations,
                    openIncidents = p.openIncidents
                }).ToList();

                return Json(new { success = true, data = simpleData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: AgentSurveillance/Details/5 - Détails d'un agent
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var agentTerrain = await _context.AgentsTerrain
                .Include(at => at.Utilisateur)
                .Include(at => at.PositionsGPS.OrderByDescending(p => p.Horodatage))
                .Include(at => at.Activations)
                    .ThenInclude(a => a.Campagne)
                .Include(at => at.Activations)
                    .ThenInclude(a => a.Lieu)
                .Include(at => at.Incidents.OrderByDescending(i => i.DateCreation))
                .Include(at => at.Medias.OrderByDescending(m => m.DateUpload))
                .FirstOrDefaultAsync(at => at.Id == id);

            if (agentTerrain == null)
                return NotFound();

            return View(agentTerrain);
        }

        // GET: AgentSurveillance/Activity - Redirection vers l'index (temporaire)
        public IActionResult Activity()
        {
            return RedirectToAction("Index");
        }

        // GET: AgentSurveillance/TestConnection - Test pour vérifier les données de connexion
        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var agents = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .ToListAsync();

                var result = new List<object>();
                foreach (var at in agents)
                {
                    result.Add(new
                    {
                        id = at.Id.ToString(),
                        name = $"{at.Utilisateur.Prenom} {at.Utilisateur.Nom}",
                        email = at.Utilisateur.Email,
                        estConnecte = at.EstConnecte,
                        derniereConnexion = at.DerniereConnexion.HasValue ? at.DerniereConnexion.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                        derniereDeconnexion = at.DerniereDeconnexion.HasValue ? at.DerniereDeconnexion.Value.ToString("yyyy-MM-dd HH:mm:ss") : null
                    });
                }

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }




    }
} 