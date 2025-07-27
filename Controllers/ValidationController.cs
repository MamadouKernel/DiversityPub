using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using Microsoft.AspNetCore.Authorization;
using DiversityPub.Models.enums;

namespace DiversityPub.Controllers
{
    [Authorize(Roles = "Admin,ChefProjet")]
    public class ValidationController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public ValidationController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: Validation - Vue principale de validation
        public async Task<IActionResult> Index()
        {
            try
            {
                var activationsEnAttente = await _context.Activations
                    .Include(a => a.Campagne)
                    .Include(a => a.Lieu)
                    .Include(a => a.AgentsTerrain)
                        .ThenInclude(at => at.Utilisateur)
                    .Include(a => a.Medias.Where(m => !m.Valide))
                    .Include(a => a.Incidents.Where(i => i.Statut == "Ouvert" || i.Statut == "EnCours"))
                    .Where(a => a.Statut == StatutActivation.Terminee && !a.PreuvesValidees)
                    .OrderByDescending(a => a.DateActivation)
                    .ToListAsync();

                return View(activationsEnAttente);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors du chargement des validations: {ex.Message}";
                return View(new List<Activation>());
            }
        }

        // GET: Validation/Details/5 - Détails d'une activation à valider
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var activation = await _context.Activations
                .Include(a => a.Campagne)
                .Include(a => a.Lieu)
                .Include(a => a.AgentsTerrain)
                    .ThenInclude(at => at.Utilisateur)
                .Include(a => a.Medias.OrderByDescending(m => m.DateUpload))
                .Include(a => a.Incidents.OrderByDescending(i => i.DateCreation))
                .Include(a => a.Responsable)
                    .ThenInclude(r => r.Utilisateur)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (activation == null)
                return NotFound();

            return View(activation);
        }

        // POST: Validation/ValiderMedia - Valider un média
        [HttpPost]
        public async Task<IActionResult> ValiderMedia(Guid mediaId, bool valide, string? commentaire = null)
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var utilisateur = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur == null)
                {
                    return Json(new { success = false, message = "Utilisateur non trouvé." });
                }

                var media = await _context.Medias
                    .Include(m => m.Activation)
                    .FirstOrDefaultAsync(m => m.Id == mediaId);

                if (media == null)
                {
                    return Json(new { success = false, message = "Média non trouvé." });
                }

                media.Valide = valide;
                media.DateValidation = DateTime.Now;
                media.ValideParId = utilisateur.Id;
                media.CommentaireValidation = commentaire;

                await _context.SaveChangesAsync();

                var message = valide ? "Média validé avec succès." : "Média rejeté.";
                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la validation: {ex.Message}" });
            }
        }

        // POST: Validation/ValiderActivation - Valider toutes les preuves d'une activation
        [HttpPost]
        public async Task<IActionResult> ValiderActivation(Guid activationId, bool validee, string? commentaire = null)
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var utilisateur = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur == null)
                {
                    return Json(new { success = false, message = "Utilisateur non trouvé." });
                }

                var activation = await _context.Activations
                    .Include(a => a.Medias)
                    .FirstOrDefaultAsync(a => a.Id == activationId);

                if (activation == null)
                {
                    return Json(new { success = false, message = "Activation non trouvée." });
                }

                // Valider tous les médias de l'activation
                foreach (var media in activation.Medias.Where(m => !m.Valide))
                {
                    media.Valide = validee;
                    media.DateValidation = DateTime.Now;
                    media.ValideParId = utilisateur.Id;
                    media.CommentaireValidation = commentaire;
                }

                // Marquer l'activation comme validée
                activation.PreuvesValidees = validee;
                activation.DateValidationPreuves = DateTime.Now;
                activation.ValideParId = utilisateur.Id;

                await _context.SaveChangesAsync();

                var message = validee ? "Activation validée avec succès. Les preuves sont maintenant visibles par le client." : "Activation rejetée.";
                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la validation: {ex.Message}" });
            }
        }

        // GET: Validation/Media - Vue pour valider les médias
        public async Task<IActionResult> Media()
        {
            try
            {
                var mediasEnAttente = await _context.Medias
                    .Include(m => m.Activation)
                        .ThenInclude(a => a.Campagne)
                    .Include(m => m.Activation)
                        .ThenInclude(a => a.Lieu)
                    .Include(m => m.AgentTerrain)
                        .ThenInclude(at => at.Utilisateur)
                    .Where(m => !m.Valide)
                    .OrderByDescending(m => m.DateUpload)
                    .ToListAsync();

                return View(mediasEnAttente);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors du chargement des médias: {ex.Message}";
                return View(new List<Media>());
            }
        }

        // GET: Validation/Incidents - Vue pour gérer les incidents
        public async Task<IActionResult> Incidents()
        {
            try
            {
                var incidents = await _context.Incidents
                    .Include(i => i.Activation)
                        .ThenInclude(a => a.Campagne)
                    .Include(i => i.Activation)
                        .ThenInclude(a => a.Lieu)
                    .Include(i => i.AgentTerrain)
                        .ThenInclude(at => at.Utilisateur)
                    .Where(i => i.Statut == "Ouvert" || i.Statut == "EnCours")
                    .OrderByDescending(i => i.DateCreation)
                    .ToListAsync();

                return View(incidents);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors du chargement des incidents: {ex.Message}";
                return View(new List<Incident>());
            }
        }

        // POST: Validation/ResoudreIncident - Résoudre un incident
        [HttpPost]
        public async Task<IActionResult> ResoudreIncident(Guid incidentId, string statut, string? commentaire = null)
        {
            try
            {
                var incident = await _context.Incidents
                    .FirstOrDefaultAsync(i => i.Id == incidentId);

                if (incident == null)
                {
                    return Json(new { success = false, message = "Incident non trouvé." });
                }

                incident.Statut = statut;
                incident.DateResolution = DateTime.Now;
                incident.CommentaireResolution = commentaire;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Incident mis à jour avec succès." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la mise à jour: {ex.Message}" });
            }
        }
    }
} 