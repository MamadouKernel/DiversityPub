using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using DiversityPub.Models.enums;
using DiversityPub.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DiversityPub.Controllers
{
    [Authorize]
    public class AgentTerrainController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public AgentTerrainController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: AgentTerrain - Vue optimisée pour tablette
        [Authorize(Roles = "Admin,ChefProjet")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var agentsTerrain = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .Include(at => at.Activations)
                        .ThenInclude(a => a.Campagne)
                    .Include(at => at.Activations)
                        .ThenInclude(a => a.Lieu)
                    .Include(at => at.PositionsGPS.OrderByDescending(p => p.Horodatage).Take(1))
                    .OrderBy(at => at.Utilisateur.Nom)
                    .ToListAsync();

                if (agentsTerrain.Count == 0)
                {
                    TempData["Info"] = "👥 Aucun agent terrain trouvé. Créez votre premier agent !";
                }
                else
                {
                    TempData["Success"] = $"👥 {agentsTerrain.Count} agent(s) terrain trouvé(s)";
                }

                return View(agentsTerrain);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Erreur lors du chargement des agents: {ex.Message}";
                return View(new List<AgentTerrain>());
            }
        }

        // GET: AgentTerrain/Details/5 - Vue détaillée pour tablette
        [Authorize(Roles = "Admin,ChefProjet")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var agentTerrain = await _context.AgentsTerrain
                .Include(at => at.Utilisateur)
                .Include(at => at.Activations)
                    .ThenInclude(a => a.Campagne)
                .Include(at => at.Activations)
                    .ThenInclude(a => a.Lieu)
                .Include(at => at.Documents)
                .Include(at => at.Incidents)
                .Include(at => at.PositionsGPS.OrderByDescending(p => p.Horodatage))
                .Include(at => at.Medias)
                .FirstOrDefaultAsync(at => at.Id == id);

            if (agentTerrain == null)
                return NotFound();

            return View(agentTerrain);
        }

        // GET: AgentTerrain/Create
        [Authorize(Roles = "Admin,ChefProjet")]
        public async Task<IActionResult> Create()
        {
            // Charger les utilisateurs qui ne sont pas encore agents terrain
            var utilisateursDisponibles = await _context.Utilisateurs
                .Where(u => u.AgentTerrain == null && u.Role == Role.AgentTerrain)
                .ToListAsync();

            ViewBag.UtilisateursDisponibles = utilisateursDisponibles;
            return View();
        }

        // POST: AgentTerrain/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ChefProjet")]
        public async Task<IActionResult> Create([Bind("Telephone,Email,UtilisateurId")] AgentTerrain agentTerrain)
        {
            if (ModelState.IsValid)
            {
                agentTerrain.Id = Guid.NewGuid();
                _context.Add(agentTerrain);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Agent terrain créé avec succès !";
                return RedirectToAction(nameof(Index));
            }

            var utilisateursDisponibles = await _context.Utilisateurs
                .Where(u => u.AgentTerrain == null && u.Role == Role.AgentTerrain)
                .ToListAsync();

            ViewBag.UtilisateursDisponibles = utilisateursDisponibles;
            return View(agentTerrain);
        }

        // GET: AgentTerrain/Edit/5
        [Authorize(Roles = "Admin,ChefProjet")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var agentTerrain = await _context.AgentsTerrain
                .Include(at => at.Utilisateur)
                .FirstOrDefaultAsync(at => at.Id == id);

            if (agentTerrain == null)
                return NotFound();

            return View(agentTerrain);
        }

        // POST: AgentTerrain/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ChefProjet")]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Telephone,Email,UtilisateurId")] AgentTerrain agentTerrain)
        {
            if (id != agentTerrain.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(agentTerrain);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "✅ Agent terrain modifié avec succès !";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AgentTerrainExists(agentTerrain.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var agentTerrainWithUser = await _context.AgentsTerrain
                .Include(at => at.Utilisateur)
                .FirstOrDefaultAsync(at => at.Id == id);

            return View(agentTerrainWithUser);
        }

        // GET: AgentTerrain/Delete/5
        [Authorize(Roles = "Admin,ChefProjet")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var agentTerrain = await _context.AgentsTerrain
                .Include(at => at.Utilisateur)
                .Include(at => at.Activations)
                .FirstOrDefaultAsync(at => at.Id == id);

            if (agentTerrain == null)
                return NotFound();

            return View(agentTerrain);
        }

        // POST: AgentTerrain/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ChefProjet")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var agentTerrain = await _context.AgentsTerrain.FindAsync(id);
            if (agentTerrain != null)
            {
                _context.AgentsTerrain.Remove(agentTerrain);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Agent terrain supprimé avec succès !";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: AgentTerrain/Position/5 - Vue de position GPS pour tablette
        public async Task<IActionResult> Position(Guid? id)
        {
            if (id == null)
                return NotFound();

            var agentTerrain = await _context.AgentsTerrain
                .Include(at => at.Utilisateur)
                .Include(at => at.PositionsGPS.OrderByDescending(p => p.Horodatage))
                .FirstOrDefaultAsync(at => at.Id == id);

            if (agentTerrain == null)
                return NotFound();

            return View(agentTerrain);
        }

        // GET: AgentTerrain/Activations/5 - Vue des activations de l'agent
        public async Task<IActionResult> Activations(Guid? id)
        {
            if (id == null)
                return NotFound();

            var agentTerrain = await _context.AgentsTerrain
                .Include(at => at.Utilisateur)
                .Include(at => at.Activations)
                    .ThenInclude(a => a.Campagne)
                .Include(at => at.Activations)
                    .ThenInclude(a => a.Lieu)
                .FirstOrDefaultAsync(at => at.Id == id);

            if (agentTerrain == null)
                return NotFound();

            return View(agentTerrain);
        }

        // GET: AgentTerrain/Missions - Interface mobile pour les agents terrain
        [Authorize(Roles = "AgentTerrain")]
        public async Task<IActionResult> Missions()
        {
            try
            {
                // Récupérer l'agent terrain connecté
                var userEmail = User.Identity?.Name;
                var agentTerrain = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .Include(at => at.Activations.Where(a => a.Statut == StatutActivation.EnCours || a.Statut == StatutActivation.Planifiee))
                        .ThenInclude(a => a.Campagne)
                    .Include(at => at.Activations.Where(a => a.Statut == StatutActivation.EnCours || a.Statut == StatutActivation.Planifiee))
                        .ThenInclude(a => a.Lieu)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agentTerrain == null)
                {
                    return View("Error", new { Message = "Agent terrain non trouvé." });
                }

                return View(agentTerrain);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des missions: {ex.Message}" });
            }
        }

        // POST: AgentTerrain/UpdatePosition - Mise à jour de la position GPS
        [HttpPost]
        [Authorize(Roles = "AgentTerrain")]
        public async Task<IActionResult> UpdatePosition([FromBody] PositionGPSDto positionDto)
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var agentTerrain = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agentTerrain == null)
                {
                    return Json(new { success = false, message = "Agent terrain non trouvé." });
                }

                var position = new PositionGPS
                {
                    Id = Guid.NewGuid(),
                    AgentTerrainId = agentTerrain.Id,
                    Latitude = positionDto.Latitude,
                    Longitude = positionDto.Longitude,
                    Horodatage = DateTime.Now,
                    Precision = positionDto.Precision
                };

                _context.PositionsGPS.Add(position);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Position mise à jour avec succès." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la mise à jour de la position: {ex.Message}" });
            }
        }

        // POST: AgentTerrain/StartMission - Démarrer une activation (seulement par le responsable)
        [HttpPost]
        [Authorize(Roles = "AgentTerrain")]
        public async Task<IActionResult> StartMission(Guid activationId)
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var agentTerrain = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agentTerrain == null)
                {
                    return Json(new { success = false, message = "Agent terrain non trouvé." });
                }

                var activation = await _context.Activations
                    .Include(a => a.AgentsTerrain)
                    .Include(a => a.Campagne)
                    .FirstOrDefaultAsync(a => a.Id == activationId);

                if (activation == null)
                {
                    return Json(new { success = false, message = "Activation non trouvée." });
                }

                // Vérifier que l'agent est bien assigné à cette activation
                if (!activation.AgentsTerrain.Any(at => at.Id == agentTerrain.Id))
                {
                    return Json(new { success = false, message = "Vous n'êtes pas assigné à cette activation." });
                }

                // Vérifier que l'agent est le responsable de l'activation
                if (activation.ResponsableId != agentTerrain.Id)
                {
                    return Json(new { success = false, message = "Seul le responsable peut démarrer l'activation." });
                }

                // Vérifier que l'activation est planifiée
                if (activation.Statut != StatutActivation.Planifiee)
                {
                    return Json(new { success = false, message = "Cette activation ne peut pas être démarrée." });
                }

                // Démarrer l'activation
                activation.Statut = StatutActivation.EnCours;
                
                // Mettre à jour le statut de la campagne en "EnCours"
                if (activation.Campagne != null)
                {
                    activation.Campagne.Statut = StatutCampagne.EnCours;
                }
                
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Activation démarrée avec succès. La campagne est maintenant en cours." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors du démarrage de l'activation: {ex.Message}" });
            }
        }

        // POST: AgentTerrain/FinishMission - Terminer une mission
        [HttpPost]
        [Authorize(Roles = "AgentTerrain")]
        public async Task<IActionResult> FinishMission(Guid activationId)
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var agentTerrain = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agentTerrain == null)
                {
                    return Json(new { success = false, message = "Agent terrain non trouvé." });
                }

                var activation = await _context.Activations
                    .Include(a => a.AgentsTerrain)
                    .FirstOrDefaultAsync(a => a.Id == activationId);

                if (activation == null)
                {
                    return Json(new { success = false, message = "Activation non trouvée." });
                }

                // Vérifier que l'agent est bien assigné à cette activation
                if (!activation.AgentsTerrain.Any(at => at.Id == agentTerrain.Id))
                {
                    return Json(new { success = false, message = "Vous n'êtes pas assigné à cette activation." });
                }

                // Vérifier que l'activation est en cours
                if (activation.Statut != StatutActivation.EnCours)
                {
                    return Json(new { success = false, message = "Cette activation ne peut pas être terminée." });
                }

                // Terminer l'activation
                activation.Statut = StatutActivation.Terminee;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Mission terminée avec succès." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la finalisation de la mission: {ex.Message}" });
            }
        }

        // POST: AgentTerrain/UploadMedia - Upload de médias
        [HttpPost]
        [Authorize(Roles = "AgentTerrain")]
        public async Task<IActionResult> UploadMedia([FromBody] MediaUploadDto mediaDto)
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var agentTerrain = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agentTerrain == null)
                {
                    return Json(new { success = false, message = "Agent terrain non trouvé." });
                }

                var media = new Media
                {
                    Id = Guid.NewGuid(),
                    AgentTerrainId = agentTerrain.Id,
                    ActivationId = mediaDto.ActivationId,
                    Type = mediaDto.Type,
                    Url = mediaDto.Url,
                    Description = mediaDto.Description,
                    DateUpload = DateTime.Now
                };

                _context.Medias.Add(media);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Média uploadé avec succès." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de l'upload du média: {ex.Message}" });
            }
        }

        // POST: AgentTerrain/CreateIncident - Créer un incident
        [HttpPost]
        [Authorize(Roles = "AgentTerrain")]
        public async Task<IActionResult> CreateIncident([FromBody] IncidentCreateDto incidentDto)
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var agentTerrain = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agentTerrain == null)
                {
                    return Json(new { success = false, message = "Agent terrain non trouvé." });
                }

                var incident = new Incident
                {
                    Id = Guid.NewGuid(),
                    AgentTerrainId = agentTerrain.Id,
                    ActivationId = incidentDto.ActivationId,
                    Titre = incidentDto.Titre,
                    Description = incidentDto.Description,
                    Priorite = incidentDto.Priorite,
                    Statut = "Ouvert",
                    DateCreation = DateTime.Now
                };

                _context.Incidents.Add(incident);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Incident créé avec succès." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la création de l'incident: {ex.Message}" });
            }
        }

        private bool AgentTerrainExists(Guid id)
        {
            return _context.AgentsTerrain.Any(e => e.Id == id);
        }
    }
} 