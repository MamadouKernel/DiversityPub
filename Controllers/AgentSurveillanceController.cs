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
                    .Select(at => new
                    {
                        AgentId = at.Id,
                        AgentName = $"{at.Utilisateur.Prenom} {at.Utilisateur.Nom}",
                        AgentEmail = at.Utilisateur.Email,
                        AgentPhone = at.Telephone,
                        LastPosition = at.PositionsGPS.FirstOrDefault(),
                        IsOnline = at.PositionsGPS.Any(p => p.Horodatage > DateTime.Now.AddMinutes(-10)), // En ligne si position < 10 min
                        ActiveActivations = at.Activations.Count(a => a.Statut == DiversityPub.Models.enums.StatutActivation.EnCours),
                        OpenIncidents = at.Incidents.Count(i => i.Statut == "Ouvert" || i.Statut == "EnCours")
                    })
                    .ToListAsync();

                return Json(new { success = true, data = positions });
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

        // GET: AgentSurveillance/Map - Vue carte avec tous les agents
        public async Task<IActionResult> Map()
        {
            try
            {
                var agentsWithPositions = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .Include(at => at.PositionsGPS.OrderByDescending(p => p.Horodatage).Take(1))
                    .Include(at => at.Activations.Where(a => a.Statut == DiversityPub.Models.enums.StatutActivation.EnCours))
                    .ToListAsync();

                return View(agentsWithPositions);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors du chargement de la carte: {ex.Message}";
                return View(new List<AgentTerrain>());
            }
        }

        // GET: AgentSurveillance/Activity - Activité des agents
        public async Task<IActionResult> Activity()
        {
            try
            {
                var today = DateTime.Today;
                var agentsActivity = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .Include(at => at.PositionsGPS.Where(p => p.Horodatage >= today))
                    .Include(at => at.Activations.Where(a => a.DateActivation >= today))
                    .Include(at => at.Medias.Where(m => m.DateUpload >= today))
                    .Include(at => at.Incidents.Where(i => i.DateCreation >= today))
                    .Select(at => new
                    {
                        AgentId = at.Id,
                        AgentName = $"{at.Utilisateur.Prenom} {at.Utilisateur.Nom}",
                        PositionsCount = at.PositionsGPS.Count,
                        ActivationsCount = at.Activations.Count,
                        MediasCount = at.Medias.Count,
                        IncidentsCount = at.Incidents.Count,
                        LastActivity = at.PositionsGPS.Max(p => p.Horodatage)
                    })
                    .ToListAsync();

                return View(agentsActivity);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors du chargement de l'activité: {ex.Message}";
                return View(new List<object>());
            }
        }
    }
} 