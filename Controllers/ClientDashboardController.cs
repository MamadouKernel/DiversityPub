using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DiversityPub.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientDashboardController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public ClientDashboardController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: ClientDashboard
        public async Task<IActionResult> Index()
        {
            try
            {
                // Récupérer l'utilisateur connecté et son client associé
                var userEmail = User.Identity.Name;
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur?.Client == null)
                {
                    return View("Error", new { Message = "Votre compte client n'est pas correctement configuré. Veuillez contacter l'administrateur." });
                }

                var clientId = utilisateur.Client.Id;

                // Récupérer les données du client
                var dashboardData = new
                {
                    Client = utilisateur.Client,
                    Campagnes = await _context.Campagnes
                        .Where(c => c.ClientId == clientId)
                        .OrderByDescending(c => c.DateDebut)
                        .ToListAsync(),
                    Activations = await _context.Activations
                        .Include(a => a.Campagne)
                        .Include(a => a.Lieu)
                        .Include(a => a.AgentsTerrain)
                            .ThenInclude(at => at.Utilisateur)
                        .Include(a => a.Responsable)
                            .ThenInclude(r => r.Utilisateur)
                        .Where(a => a.Campagne.ClientId == clientId)
                        .OrderByDescending(a => a.DateActivation)
                        .ToListAsync(),
                    Feedbacks = await _context.Feedbacks
                        .Include(f => f.Campagne)
                        .Where(f => f.Campagne.ClientId == clientId)
                        .OrderByDescending(f => f.DateFeedback)
                        .ToListAsync()
                };

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement du dashboard: {ex.Message}" });
            }
        }

        // GET: ClientDashboard/Campagnes
        public async Task<IActionResult> Campagnes()
        {
            try
            {
                var userEmail = User.Identity.Name;
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur?.Client == null)
                {
                    return View("Error", new { Message = "Votre compte client n'est pas correctement configuré. Veuillez contacter l'administrateur." });
                }

                var campagnes = await _context.Campagnes
                    .Include(c => c.Activations)
                    .Include(c => c.Feedbacks)
                    .Where(c => c.ClientId == utilisateur.Client.Id)
                    .OrderByDescending(c => c.DateDebut)
                    .ToListAsync();

                if (campagnes.Count == 0)
                {
                    TempData["Info"] = "📋 Aucune campagne trouvée.";
                }
                else
                {
                    TempData["Info"] = $"📋 {campagnes.Count} campagne(s) trouvée(s)";
                }

                return View(campagnes);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des campagnes: {ex.Message}" });
            }
        }

        // GET: ClientDashboard/Activations
        public async Task<IActionResult> Activations()
        {
            try
            {
                var userEmail = User.Identity.Name;
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur?.Client == null)
                {
                    return View("Error", new { Message = "Votre compte client n'est pas correctement configuré. Veuillez contacter l'administrateur." });
                }

                var activations = await _context.Activations
                    .Include(a => a.Campagne)
                    .Include(a => a.Lieu)
                    .Include(a => a.AgentsTerrain)
                        .ThenInclude(at => at.Utilisateur)
                    .Include(a => a.Responsable)
                        .ThenInclude(r => r.Utilisateur)
                    .Include(a => a.Medias.Where(m => m.Valide)) // Seulement les médias validés
                    .Where(a => a.Campagne.ClientId == utilisateur.Client.Id && a.PreuvesValidees) // Seulement les activations validées
                    .OrderByDescending(a => a.DateActivation)
                    .ToListAsync();

                if (activations.Count == 0)
                {
                    TempData["Info"] = "📋 Aucune activation trouvée.";
                }
                else
                {
                    TempData["Info"] = $"📋 {activations.Count} activation(s) trouvée(s)";
                }

                return View(activations);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des activations: {ex.Message}" });
            }
        }

        // GET: ClientDashboard/Feedbacks
        public async Task<IActionResult> Feedbacks()
        {
            try
            {
                var userEmail = User.Identity.Name;
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur?.Client == null)
                {
                    return View("Error", new { Message = "Votre compte client n'est pas correctement configuré. Veuillez contacter l'administrateur." });
                }

                var feedbacks = await _context.Feedbacks
                    .Include(f => f.Campagne)
                    .Where(f => f.Campagne.ClientId == utilisateur.Client.Id)
                    .OrderByDescending(f => f.DateFeedback)
                    .ToListAsync();

                if (feedbacks.Count == 0)
                {
                    TempData["Info"] = "📋 Aucun feedback trouvé.";
                }
                else
                {
                    TempData["Info"] = $"📋 {feedbacks.Count} feedback(s) trouvé(s)";
                }

                return View(feedbacks);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des feedbacks: {ex.Message}" });
            }
        }

        // GET: ClientDashboard/CreateFeedback
        public async Task<IActionResult> CreateFeedback()
        {
            try
            {
                var userEmail = User.Identity.Name;
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur?.Client == null)
                {
                    return View("Error", new { Message = "Votre compte client n'est pas correctement configuré. Veuillez contacter l'administrateur." });
                }

                // Récupérer les campagnes terminées du client qui n'ont pas encore de feedback
                var campagnesTerminees = await _context.Campagnes
                    .Where(c => c.ClientId == utilisateur.Client.Id && 
                               c.Statut == DiversityPub.Models.enums.StatutCampagne.Terminee)
                    .ToListAsync();

                var campagnesAvecFeedback = await _context.Feedbacks
                    .Where(f => f.Campagne.ClientId == utilisateur.Client.Id)
                    .Select(f => f.CampagneId)
                    .ToListAsync();

                var campagnesDisponibles = campagnesTerminees
                    .Where(c => !campagnesAvecFeedback.Contains(c.Id))
                    .ToList();

                if (!campagnesDisponibles.Any())
                {
                    TempData["Warning"] = "⚠️ Aucune campagne terminée disponible pour un feedback.";
                    return RedirectToAction("Feedbacks");
                }

                ViewBag.Campagnes = campagnesDisponibles;
                return View(new Feedback());
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement du formulaire de feedback: {ex.Message}" });
            }
        }

        // POST: ClientDashboard/CreateFeedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFeedback([Bind("Note,Commentaire,CampagneId")] Feedback feedback)
        {
            try
            {
                var userEmail = User.Identity.Name;
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur?.Client == null)
                {
                    return View("Error", new { Message = "Votre compte client n'est pas correctement configuré. Veuillez contacter l'administrateur." });
                }

                if (ModelState.IsValid)
                {
                    // Vérifier que la campagne appartient bien au client
                    var campagne = await _context.Campagnes
                        .FirstOrDefaultAsync(c => c.Id == feedback.CampagneId && c.ClientId == utilisateur.Client.Id);

                    if (campagne == null)
                    {
                        return View("Error", new { Message = "Campagne non trouvée ou non autorisée." });
                    }

                    // Vérifier qu'il n'y a pas déjà un feedback pour cette campagne
                    var feedbackExistant = await _context.Feedbacks
                        .FirstOrDefaultAsync(f => f.CampagneId == feedback.CampagneId);

                    if (feedbackExistant != null)
                    {
                        TempData["Error"] = "❌ Un feedback existe déjà pour cette campagne.";
                        return RedirectToAction("Feedbacks");
                    }

                    feedback.DateFeedback = DateTime.Now;
                    _context.Add(feedback);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "✅ Feedback créé avec succès !";
                    return RedirectToAction("Feedbacks");
                }

                // En cas d'erreur de validation, recharger les campagnes disponibles
                var campagnesTerminees = await _context.Campagnes
                    .Where(c => c.ClientId == utilisateur.Client.Id && 
                               c.Statut == DiversityPub.Models.enums.StatutCampagne.Terminee)
                    .ToListAsync();

                var campagnesAvecFeedback = await _context.Feedbacks
                    .Where(f => f.Campagne.ClientId == utilisateur.Client.Id)
                    .Select(f => f.CampagneId)
                    .ToListAsync();

                var campagnesDisponibles = campagnesTerminees
                    .Where(c => !campagnesAvecFeedback.Contains(c.Id))
                    .ToList();

                ViewBag.Campagnes = campagnesDisponibles;
                return View(feedback);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors de la création du feedback: {ex.Message}" });
            }
        }

        // GET: ClientDashboard/DetailsCampagne
        public async Task<IActionResult> DetailsCampagne(Guid? id)
        {
            try
            {
                var userEmail = User.Identity.Name;
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur?.Client == null)
                {
                    return View("Error", new { Message = "Votre compte client n'est pas correctement configuré. Veuillez contacter l'administrateur." });
                }

                if (id == null)
                {
                    return View("Error", new { Message = "ID de campagne non fourni." });
                }

                var campagne = await _context.Campagnes
                    .Include(c => c.Activations)
                        .ThenInclude(a => a.Lieu)
                    .Include(c => c.Activations)
                        .ThenInclude(a => a.AgentsTerrain)
                            .ThenInclude(at => at.Utilisateur)
                    .Include(c => c.Activations)
                        .ThenInclude(a => a.Responsable)
                            .ThenInclude(r => r.Utilisateur)
                    .Include(c => c.Feedbacks)
                    .FirstOrDefaultAsync(c => c.Id == id && c.ClientId == utilisateur.Client.Id);

                if (campagne == null)
                {
                    return View("Error", new { Message = "Campagne non trouvée ou non autorisée." });
                }

                return View(campagne);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des détails de la campagne: {ex.Message}" });
            }
        }
    }
} 