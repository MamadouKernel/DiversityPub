using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using DiversityPub.Models.enums;
using Microsoft.AspNetCore.Authorization;

namespace DiversityPub.Controllers
{
    [Authorize(Roles = "Admin,ChefProjet")]
    public class CampagneController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public CampagneController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: Campagne
        public async Task<IActionResult> Index()
        {
            try
            {
                // Vérifier et mettre à jour automatiquement les campagnes expirées
                await CheckAndUpdateExpiredCampagnesAsync();
                
                var campagnes = await _context.Campagnes
                    .Include(c => c.Client)
                    .OrderByDescending(c => c.DateDebut)
                    .ToListAsync();
                
                if (campagnes.Count == 0)
                {
                    TempData["Info"] = "📋 Aucune campagne trouvée. Créez votre première campagne !";
                }
                else
                {
                    TempData["Info"] = $"📋 {campagnes.Count} campagne(s) trouvée(s)";
                }
                
                return View(campagnes);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Erreur lors du chargement des campagnes: {ex.Message}";
                return View(new List<Campagne>());
            }
        }

        // GET: Campagne/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            // Vérifier et mettre à jour automatiquement les campagnes expirées
            await CheckAndUpdateExpiredCampagnesAsync();

            var campagne = await _context.Campagnes
                .Include(c => c.Client)
                .Include(c => c.Activations)
                    .ThenInclude(a => a.Lieu)
                .Include(c => c.Activations)
                    .ThenInclude(a => a.AgentsTerrain)
                        .ThenInclude(at => at.Utilisateur)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campagne == null)
                return NotFound();

            return View(campagne);
        }

        // GET: Campagne/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var clients = await _context.Clients.ToListAsync();
                
                if (clients.Count == 0)
                {
                    TempData["Warning"] = "⚠️ Aucun client disponible. Veuillez d'abord créer au moins un client.";
                    return RedirectToAction("Index", "Client");
                }
                
                TempData["Info"] = $"✅ Prêt à créer une campagne avec {clients.Count} client(s) disponible(s).";
                
                ViewBag.Clients = clients;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Erreur lors du chargement des clients: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Campagne/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Description,DateDebut,DateFin,Objectifs,ClientId")] Campagne campagne)
        {
            // Afficher les erreurs de validation détaillées
            if (!ModelState.IsValid)
            {
                var errorMessages = new List<string>();
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        errorMessages.Add(error.ErrorMessage);
                    }
                }
                
                if (errorMessages.Any())
                {
                    TempData["Error"] = $"❌ Erreurs de validation: {string.Join(", ", errorMessages)}";
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    campagne.Id = Guid.NewGuid();
                    campagne.Statut = DiversityPub.Models.enums.StatutCampagne.EnPreparation;
                    _context.Add(campagne);
                    await _context.SaveChangesAsync();
                    
                    // Récupérer le nom du client pour le message
                    var client = await _context.Clients.FindAsync(campagne.ClientId);
                    var nomClient = client?.RaisonSociale ?? "Client inconnu";
                    
                    TempData["Success"] = $"✅ Campagne '{campagne.Nom}' créée avec succès pour le client '{nomClient}' !";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"❌ Erreur lors de la création de la campagne: {ex.Message}";
                }
            }
            
            ViewBag.Clients = await _context.Clients.ToListAsync();
            return View(campagne);
        }

        // GET: Campagne/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var campagne = await _context.Campagnes
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campagne == null)
                return NotFound();

            ViewBag.Clients = await _context.Clients.ToListAsync();
            return View(campagne);
        }

        // POST: Campagne/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nom,Description,DateDebut,DateFin,Objectifs,Statut,ClientId")] Campagne campagne)
        {
            if (id != campagne.Id)
                return NotFound();

            // Vérifier si on essaie d'annuler une campagne qui a des activations en cours
            if (campagne.Statut == StatutCampagne.Annulee)
            {
                var campagneExistante = await _context.Campagnes
                    .Include(c => c.Activations)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (campagneExistante != null)
                {
                    var hasActivationsEnCours = campagneExistante.Activations.Any(a => a.Statut == StatutActivation.EnCours);
                    
                    if (hasActivationsEnCours)
                    {
                        ModelState.AddModelError("Statut", "Impossible d'annuler une campagne qui a des activations en cours.");
                        TempData["Error"] = "❌ Impossible d'annuler une campagne qui a des activations en cours.";
                        
                        ViewBag.Clients = await _context.Clients.ToListAsync();
                        return View(campagne);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(campagne);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"✅ Campagne '{campagne.Nom}' modifiée avec succès !";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CampagneExists(campagne.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Clients = await _context.Clients.ToListAsync();
            return View(campagne);
        }

        // GET: Campagne/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var campagne = await _context.Campagnes
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campagne == null)
                return NotFound();

            return View(campagne);
        }

        // POST: Campagne/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var campagne = await _context.Campagnes.FindAsync(id);
            if (campagne != null)
            {
                _context.Campagnes.Remove(campagne);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CampagneExists(Guid id)
        {
            return _context.Campagnes.Any(e => e.Id == id);
        }

        // Méthode pour mettre à jour automatiquement le statut d'une campagne
        private async Task UpdateCampagneStatutAsync(Guid campagneId)
        {
            var campagne = await _context.Campagnes
                .Include(c => c.Activations)
                .FirstOrDefaultAsync(c => c.Id == campagneId);

            if (campagne == null) return;

            var activations = campagne.Activations.ToList();
            
            // Vérifier si la date de fin de la campagne est dépassée
            if (DateTime.Today > campagne.DateFin && campagne.Statut != StatutCampagne.Terminee && campagne.Statut != StatutCampagne.Annulee)
            {
                campagne.Statut = StatutCampagne.Terminee;
                await _context.SaveChangesAsync();
                return;
            }
            
            if (!activations.Any())
            {
                // Aucune activation, la campagne reste en préparation
                return;
            }

            // Vérifier s'il y a des activations en cours
            var hasActivationsEnCours = activations.Any(a => a.Statut == StatutActivation.EnCours);
            
            // Vérifier s'il y a des activations suspendues
            var hasActivationsSuspendues = activations.Any(a => a.Statut == StatutActivation.Suspendue);
            
            // Vérifier si toutes les activations sont terminées
            var allActivationsTerminees = activations.All(a => a.Statut == StatutActivation.Terminee);

            if (hasActivationsEnCours)
            {
                // Au moins une activation en cours = campagne en cours
                if (campagne.Statut != StatutCampagne.EnCours)
                {
                    campagne.Statut = StatutCampagne.EnCours;
                    await _context.SaveChangesAsync();
                }
            }
            else if (allActivationsTerminees)
            {
                // Toutes les activations terminées = campagne terminée
                if (campagne.Statut != StatutCampagne.Terminee)
                {
                    campagne.Statut = StatutCampagne.Terminee;
                    await _context.SaveChangesAsync();
                }
            }
            else if (hasActivationsSuspendues)
            {
                // Des activations suspendues mais pas d'activations en cours = campagne en préparation
                if (campagne.Statut != StatutCampagne.EnPreparation)
                {
                    campagne.Statut = StatutCampagne.EnPreparation;
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // Aucune activation en cours mais pas toutes terminées = campagne en préparation
                if (campagne.Statut != StatutCampagne.EnPreparation)
                {
                    campagne.Statut = StatutCampagne.EnPreparation;
                    await _context.SaveChangesAsync();
                }
            }
        }

        // Méthode pour vérifier et mettre à jour automatiquement les campagnes expirées
        private async Task CheckAndUpdateExpiredCampagnesAsync()
        {
            var campagnesExpirees = await _context.Campagnes
                .Where(c => c.DateFin < DateTime.Today 
                           && c.Statut != StatutCampagne.Terminee 
                           && c.Statut != StatutCampagne.Annulee)
                .ToListAsync();

            foreach (var campagne in campagnesExpirees)
            {
                campagne.Statut = StatutCampagne.Terminee;
            }

            if (campagnesExpirees.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        // Action pour valider une campagne (Chef Projet uniquement)
        [HttpPost]
        [Authorize(Roles = "ChefProjet")]
        public async Task<IActionResult> ValiderCampagne(Guid id)
        {
            try
            {
                var campagne = await _context.Campagnes
                    .Include(c => c.Activations)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (campagne == null)
                {
                    return Json(new { success = false, message = "Campagne non trouvée." });
                }

                // Vérifier que toutes les activations sont terminées
                var allActivationsTerminees = campagne.Activations.All(a => a.Statut == StatutActivation.Terminee);
                
                if (!allActivationsTerminees)
                {
                    return Json(new { success = false, message = "Impossible de valider la campagne : toutes les activations doivent être terminées." });
                }

                campagne.Statut = StatutCampagne.Terminee;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Campagne validée avec succès !" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la validation : {ex.Message}" });
            }
        }

        // Action de diagnostic pour tester la base de données
        public async Task<IActionResult> Diagnostic()
        {
            try
            {
                var clients = await _context.Clients.ToListAsync();
                var campagnes = await _context.Campagnes.ToListAsync();
                var utilisateurs = await _context.Utilisateurs.ToListAsync();
                
                var diagnostic = new
                {
                    ClientsCount = clients.Count,
                    CampagnesCount = campagnes.Count,
                    UtilisateursCount = utilisateurs.Count,
                    Clients = clients.Select(c => new { c.Id, c.RaisonSociale, c.EmailContactPrincipal }),
                    DatabaseConnection = "OK"
                };
                
                return Json(diagnostic);
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }

        // Action de test pour créer une campagne directement
        public async Task<IActionResult> TestCreateCampagne()
        {
            try
            {
                var clients = await _context.Clients.ToListAsync();
                if (!clients.Any())
                {
                    return Json(new { Error = "Aucun client disponible" });
                }

                var firstClient = clients.First();
                var testCampagne = new Campagne
                {
                    Id = Guid.NewGuid(),
                    Nom = "Campagne Test",
                    Description = "Campagne créée automatiquement pour test",
                    DateDebut = DateTime.Today,
                    DateFin = DateTime.Today.AddDays(30),
                    Objectifs = "Objectifs de test",
                    ClientId = firstClient.Id,
                    Statut = StatutCampagne.EnPreparation
                };

                _context.Add(testCampagne);
                await _context.SaveChangesAsync();

                return Json(new { 
                    Success = true, 
                    Message = "Campagne de test créée avec succès",
                    CampagneId = testCampagne.Id,
                    ClientId = testCampagne.ClientId
                });
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }

        // Action pour forcer la vérification des campagnes expirées (Admin uniquement)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ForceCheckExpiredCampagnes()
        {
            try
            {
                await CheckAndUpdateExpiredCampagnesAsync();
                
                var campagnesExpirees = await _context.Campagnes
                    .Where(c => c.DateFin < DateTime.Today 
                               && c.Statut == StatutCampagne.Terminee)
                    .CountAsync();

                return Json(new { 
                    success = true, 
                    message = $"Vérification terminée. {campagnesExpirees} campagne(s) expirée(s) trouvée(s)." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la vérification : {ex.Message}" });
            }
        }
    }
} 