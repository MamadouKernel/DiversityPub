using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using Microsoft.AspNetCore.Authorization;

namespace DiversityPub.Controllers
{
    [Authorize(Roles = "Admin,ChefProjet")]
    public class IncidentController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public IncidentController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: Incident
        public async Task<IActionResult> Index()
        {
            try
            {
                var incidents = await _context.Incidents
                    .Include(i => i.AgentTerrain)
                        .ThenInclude(at => at.Utilisateur)
                    .Include(i => i.Activation)
                        .ThenInclude(a => a.Campagne)
                    .OrderByDescending(i => i.DateCreation)
                    .ToListAsync();

                if (incidents.Count == 0)
                {
                    TempData["Info"] = "🚨 Aucun incident trouvé.";
                }
                else
                {
                    TempData["Info"] = $"🚨 {incidents.Count} incident(s) trouvé(s)";
                }

                return View(incidents);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Erreur lors du chargement des incidents: {ex.Message}";
                return View(new List<Incident>());
            }
        }

        // GET: Incident/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var incident = await _context.Incidents
                .Include(i => i.AgentTerrain)
                    .ThenInclude(at => at.Utilisateur)
                .Include(i => i.Activation)
                    .ThenInclude(a => a.Campagne)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (incident == null)
                return NotFound();

            return View(incident);
        }

        // GET: Incident/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var incident = await _context.Incidents
                .Include(i => i.AgentTerrain)
                    .ThenInclude(at => at.Utilisateur)
                .Include(i => i.Activation)
                    .ThenInclude(a => a.Campagne)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (incident == null)
                return NotFound();

            return View(incident);
        }

        // POST: Incident/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Titre,Description,Priorite,Statut,AgentTerrainId,ActivationId")] Incident incident)
        {
            if (id != incident.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingIncident = await _context.Incidents
                        .FirstOrDefaultAsync(i => i.Id == id);

                    if (existingIncident == null)
                        return NotFound();

                    // Mettre à jour les propriétés
                    existingIncident.Titre = incident.Titre;
                    existingIncident.Description = incident.Description;
                    existingIncident.Priorite = incident.Priorite;
                    existingIncident.Statut = incident.Statut;

                    // Si l'incident est marqué comme résolu, ajouter la date de résolution
                    if (incident.Statut == "Fermé" && existingIncident.Statut != "Fermé")
                    {
                        existingIncident.DateResolution = DateTime.Now;
                    }

                    _context.Update(existingIncident);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "✅ Incident mis à jour avec succès !";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IncidentExists(incident.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            
            return View(incident);
        }

        // GET: Incident/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var incident = await _context.Incidents
                .Include(i => i.AgentTerrain)
                    .ThenInclude(at => at.Utilisateur)
                .Include(i => i.Activation)
                    .ThenInclude(a => a.Campagne)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (incident == null)
                return NotFound();

            return View(incident);
        }

        // POST: Incident/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var incident = await _context.Incidents.FindAsync(id);
            if (incident != null)
            {
                _context.Incidents.Remove(incident);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Incident supprimé avec succès !";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Incident/Resoudre/5 - Résoudre un incident
        [HttpPost]
        public async Task<IActionResult> Resoudre(Guid id)
        {
            try
            {
                var incident = await _context.Incidents.FindAsync(id);
                if (incident == null)
                {
                    return Json(new { success = false, message = "Incident non trouvé." });
                }

                incident.Statut = "Fermé";
                incident.DateResolution = DateTime.Now;

                _context.Update(incident);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Incident résolu avec succès." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la résolution: {ex.Message}" });
            }
        }

        // GET: Incident/ParPriorite - Filtrer par priorité
        public async Task<IActionResult> ParPriorite(string priorite)
        {
            try
            {
                var query = _context.Incidents
                    .Include(i => i.AgentTerrain)
                        .ThenInclude(at => at.Utilisateur)
                    .Include(i => i.Activation)
                        .ThenInclude(a => a.Campagne)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(priorite))
                {
                    query = query.Where(i => i.Priorite == priorite);
                }

                var incidents = await query.OrderByDescending(i => i.DateCreation).ToListAsync();

                ViewBag.PrioriteSelectionnee = priorite;
                ViewBag.Priorites = new[] { "Basse", "Normale", "Haute", "Critique" };

                return View("Index", incidents);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Erreur lors du filtrage: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Incident/ParStatut - Filtrer par statut
        public async Task<IActionResult> ParStatut(string statut)
        {
            try
            {
                var query = _context.Incidents
                    .Include(i => i.AgentTerrain)
                        .ThenInclude(at => at.Utilisateur)
                    .Include(i => i.Activation)
                        .ThenInclude(a => a.Campagne)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(statut))
                {
                    query = query.Where(i => i.Statut == statut);
                }

                var incidents = await query.OrderByDescending(i => i.DateCreation).ToListAsync();

                ViewBag.StatutSelectionne = statut;
                ViewBag.Statuts = new[] { "Ouvert", "En Cours", "Fermé" };

                return View("Index", incidents);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Erreur lors du filtrage: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool IncidentExists(Guid id)
        {
            return _context.Incidents.Any(e => e.Id == id);
        }
    }
} 