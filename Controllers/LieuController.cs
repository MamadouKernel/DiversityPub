using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using Microsoft.AspNetCore.Authorization;

namespace DiversityPub.Controllers
{
    [Authorize(Roles = "Admin,ChefProjet")]
    public class LieuController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public LieuController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: Lieu
        public async Task<IActionResult> Index()
        {
            try
            {
                var lieux = await _context.Lieux
                    .Include(l => l.Activations)
                    .OrderBy(l => l.Nom)
                    .ToListAsync();
                
                if (lieux.Count == 0)
                {
                    TempData["Info"] = "📍 Aucun lieu trouvé. Créez votre premier lieu !";
                }
                else
                {
                    TempData["Info"] = $"📍 {lieux.Count} lieu(x) trouvé(s)";
                }
                
                return View(lieux);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Erreur lors du chargement des lieux: {ex.Message}";
                return View(new List<Lieu>());
            }
        }

        // GET: Lieu/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var lieu = await _context.Lieux
                .Include(l => l.Activations)
                    .ThenInclude(a => a.Campagne)
                .Include(l => l.Activations)
                    .ThenInclude(a => a.AgentsTerrain)
                        .ThenInclude(at => at.Utilisateur)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lieu == null)
                return NotFound();

            return View(lieu);
        }

        // GET: Lieu/Create
        public IActionResult Create()
        {
            TempData["Info"] = "📍 Prêt à créer un nouveau lieu d'activation.";
            return View();
        }

        // POST: Lieu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Adresse")] Lieu lieu)
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
                    lieu.Id = Guid.NewGuid();
                    _context.Add(lieu);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"✅ Lieu '{lieu.Nom}' créé avec succès !";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"❌ Erreur lors de la création du lieu: {ex.Message}";
                }
            }
            return View(lieu);
        }

        // GET: Lieu/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var lieu = await _context.Lieux.FindAsync(id);
            if (lieu == null)
                return NotFound();

            return View(lieu);
        }

        // POST: Lieu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nom,Adresse")] Lieu lieu)
        {
            if (id != lieu.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lieu);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Lieu modifié avec succès !";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LieuExists(lieu.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(lieu);
        }

        // GET: Lieu/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var lieu = await _context.Lieux
                .Include(l => l.Activations)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lieu == null)
                return NotFound();

            return View(lieu);
        }

        // POST: Lieu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var lieu = await _context.Lieux
                .Include(l => l.Activations)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lieu != null)
            {
                if (lieu.Activations.Any())
                {
                    TempData["Error"] = "Impossible de supprimer ce lieu car il est utilisé par des activations.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Lieux.Remove(lieu);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Lieu supprimé avec succès !";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool LieuExists(Guid id)
        {
            return _context.Lieux.Any(e => e.Id == id);
        }

        // Action pour créer un lieu de test
        public async Task<IActionResult> CreateTestLieu()
        {
            try
            {
                var testLieu = new Lieu
                {
                    Id = Guid.NewGuid(),
                    Nom = "Place du Marché",
                    Adresse = "123 Avenue de la République, 75001 Paris"
                };

                _context.Add(testLieu);
                await _context.SaveChangesAsync();

                return Json(new { 
                    Success = true, 
                    Message = "Lieu de test créé avec succès",
                    LieuId = testLieu.Id,
                    Nom = testLieu.Nom
                });
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }
    }
} 