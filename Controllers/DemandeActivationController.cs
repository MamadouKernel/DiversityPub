using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using DiversityPub.Models.enums;
using Microsoft.AspNetCore.Authorization;

namespace DiversityPub.Controllers
{
    [Authorize(Roles = "Admin,ChefProjet,Client")]
    public class DemandeActivationController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public DemandeActivationController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: DemandeActivation
        public async Task<IActionResult> Index()
        {
            try
            {
                var demandes = await _context.DemandesActivation
                    .Include(d => d.Campagne)
                    .Include(d => d.Client)
                    .Include(d => d.Lieu)
                    .Include(d => d.ReponduPar)
                    .OrderByDescending(d => d.DateDemande)
                    .ToListAsync();

                if (demandes.Count == 0)
                {
                    TempData["Info"] = "📋 Aucune demande d'activation trouvée.";
                }
                else
                {
                    TempData["Info"] = $"📋 {demandes.Count} demande(s) d'activation trouvée(s)";
                }

                return View(demandes);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Erreur lors du chargement des demandes: {ex.Message}";
                return View(new List<DemandeActivation>());
            }
        }

        // GET: DemandeActivation/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Récupérer les campagnes non clôturées
                var campagnesDisponibles = await _context.Campagnes
                    .Where(c => c.Statut != StatutCampagne.Terminee && c.Statut != StatutCampagne.Annulee)
                    .Include(c => c.Client)
                    .ToListAsync();

                if (!campagnesDisponibles.Any())
                {
                    TempData["Warning"] = "⚠️ Aucune campagne disponible pour les demandes d'activation.";
                    return RedirectToAction("Index", "Campagne");
                }

                var lieux = await _context.Lieux.ToListAsync();

                if (!lieux.Any())
                {
                    TempData["Warning"] = "⚠️ Aucun lieu disponible. Veuillez d'abord créer des lieux.";
                    return RedirectToAction("Index", "Lieu");
                }

                TempData["Info"] = $"✅ Prêt à créer une demande d'activation avec {campagnesDisponibles.Count} campagne(s) disponible(s) et {lieux.Count} lieu(x).";

                ViewBag.Campagnes = campagnesDisponibles;
                ViewBag.Lieux = lieux;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Erreur lors du chargement des données: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: DemandeActivation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Description,DateActivation,HeureDebut,HeureFin,LieuId,CampagneId,Instructions")] DemandeActivation demande)
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

            // Validation de la date d'activation par rapport à la campagne
            if (demande.CampagneId != Guid.Empty)
            {
                var campagne = await _context.Campagnes.FindAsync(demande.CampagneId);
                if (campagne != null)
                {
                    if (demande.DateActivation < campagne.DateDebut || demande.DateActivation > campagne.DateFin)
                    {
                        ModelState.AddModelError("DateActivation",
                            $"La date d'activation doit être comprise entre {campagne.DateDebut.ToString("dd/MM/yyyy")} et {campagne.DateFin.ToString("dd/MM/yyyy")}");
                    }

                    // Vérifier que la campagne n'est pas clôturée
                    if (campagne.Statut == StatutCampagne.Terminee || campagne.Statut == StatutCampagne.Annulee)
                    {
                        ModelState.AddModelError("CampagneId", "Impossible de faire une demande pour une campagne clôturée.");
                    }
                }
            }

            // Validation des heures
            if (demande.HeureFin <= demande.HeureDebut)
            {
                ModelState.AddModelError("HeureFin", "L'heure de fin doit être postérieure à l'heure de début.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    demande.Id = Guid.NewGuid();
                    demande.Statut = StatutDemande.EnAttente;
                    demande.DateDemande = DateTime.Now;

                    // Récupérer le client connecté
                    var userEmail = User.Identity?.Name;
                    var client = await _context.Clients
                        .Include(c => c.Utilisateur)
                        .FirstOrDefaultAsync(c => c.Utilisateur.Email == userEmail);

                    if (client != null)
                    {
                        demande.ClientId = client.Id;
                    }
                    else
                    {
                        TempData["Error"] = "❌ Impossible d'identifier le client connecté.";
                        ViewBag.Campagnes = await _context.Campagnes.Where(c => c.Statut != StatutCampagne.Terminee && c.Statut != StatutCampagne.Annulee).ToListAsync();
                        ViewBag.Lieux = await _context.Lieux.ToListAsync();
                        return View(demande);
                    }

                    _context.Add(demande);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"✅ Demande d'activation '{demande.Nom}' créée avec succès !";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"❌ Erreur lors de la création de la demande: {ex.Message}";
                }
            }

            ViewBag.Campagnes = await _context.Campagnes.Where(c => c.Statut != StatutCampagne.Terminee && c.Statut != StatutCampagne.Annulee).ToListAsync();
            ViewBag.Lieux = await _context.Lieux.ToListAsync();
            return View(demande);
        }

        // GET: DemandeActivation/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var demande = await _context.DemandesActivation
                .Include(d => d.Campagne)
                    .ThenInclude(c => c.Client)
                .Include(d => d.Client)
                .Include(d => d.Lieu)
                .Include(d => d.ReponduPar)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (demande == null)
                return NotFound();

            return View(demande);
        }

        // POST: DemandeActivation/Approuver/5
        [HttpPost]
        [Authorize(Roles = "Admin,ChefProjet")]
        public async Task<IActionResult> Approuver(Guid id)
        {
            try
            {
                var demande = await _context.DemandesActivation
                    .Include(d => d.Campagne)
                    .Include(d => d.Client)
                    .Include(d => d.Lieu)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (demande == null)
                {
                    return Json(new { success = false, message = "Demande non trouvée." });
                }

                if (demande.Statut != StatutDemande.EnAttente)
                {
                    return Json(new { success = false, message = "Cette demande a déjà été traitée." });
                }

                // Vérifier que la campagne n'est pas clôturée
                if (demande.Campagne.Statut == StatutCampagne.Terminee || demande.Campagne.Statut == StatutCampagne.Annulee)
                {
                    return Json(new { success = false, message = "Impossible d'approuver une demande pour une campagne clôturée." });
                }

                // Créer l'activation
                var activation = new Activation
                {
                    Id = Guid.NewGuid(),
                    Nom = demande.Nom,
                    Description = demande.Description,
                    DateActivation = demande.DateActivation,
                    HeureDebut = demande.HeureDebut,
                    HeureFin = demande.HeureFin,
                    Instructions = demande.Instructions,
                    LieuId = demande.LieuId,
                    CampagneId = demande.CampagneId,
                    Statut = StatutActivation.Planifiee
                };

                _context.Add(activation);

                // Mettre à jour le statut de la demande
                demande.Statut = StatutDemande.Approuvee;
                demande.DateReponse = DateTime.Now;
                demande.ReponduParId = Guid.Parse(User.FindFirst("UserId")?.Value ?? "11111111-1111-1111-1111-111111111111");

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Demande approuvée et activation créée avec succès !" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de l'approbation : {ex.Message}" });
            }
        }

        // POST: DemandeActivation/Refuser/5
        [HttpPost]
        [Authorize(Roles = "Admin,ChefProjet")]
        public async Task<IActionResult> Refuser(Guid id, [FromBody] RefusDemandeModel model)
        {
            try
            {
                var demande = await _context.DemandesActivation
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (demande == null)
                {
                    return Json(new { success = false, message = "Demande non trouvée." });
                }

                if (demande.Statut != StatutDemande.EnAttente)
                {
                    return Json(new { success = false, message = "Cette demande a déjà été traitée." });
                }

                demande.Statut = StatutDemande.Refusee;
                demande.MotifRefus = model.MotifRefus;
                demande.DateReponse = DateTime.Now;
                demande.ReponduParId = Guid.Parse(User.FindFirst("UserId")?.Value ?? "11111111-1111-1111-1111-111111111111");

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Demande refusée avec succès." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors du refus : {ex.Message}" });
            }
        }

        // GET: DemandeActivation/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var demande = await _context.DemandesActivation
                .Include(d => d.Campagne)
                .Include(d => d.Client)
                .Include(d => d.Lieu)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (demande == null)
                return NotFound();

            return View(demande);
        }

        // POST: DemandeActivation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var demande = await _context.DemandesActivation.FindAsync(id);
            if (demande != null)
            {
                _context.DemandesActivation.Remove(demande);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }

    public class RefusDemandeModel
    {
        public string MotifRefus { get; set; } = string.Empty;
    }
} 