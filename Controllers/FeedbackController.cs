using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using Microsoft.AspNetCore.Authorization;

namespace DiversityPub.Controllers
{
    [Authorize(Roles = "Admin,ChefProjet")]
    public class FeedbackController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public FeedbackController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: Feedback
        public async Task<IActionResult> Index()
        {
            try
            {
                var feedbacks = await _context.Feedbacks
                    .Include(f => f.Campagne)
                        .ThenInclude(c => c.Client)
                    .OrderByDescending(f => f.DateFeedback)
                    .ToListAsync();

                if (feedbacks.Count == 0)
                {
                    TempData["Info"] = "💬 Aucun feedback trouvé.";
                }
                else
                {
                    TempData["Info"] = $"💬 {feedbacks.Count} feedback(s) trouvé(s)";
                }

                return View(feedbacks);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Erreur lors du chargement des feedbacks: {ex.Message}";
                return View(new List<Feedback>());
            }
        }

        // GET: Feedback/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var feedback = await _context.Feedbacks
                .Include(f => f.Campagne)
                    .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feedback == null)
                return NotFound();

            return View(feedback);
        }

        // GET: Feedback/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var feedback = await _context.Feedbacks
                .Include(f => f.Campagne)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feedback == null)
                return NotFound();

            return View(feedback);
        }

        // POST: Feedback/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Note,Commentaire,DateFeedback,CampagneId")] Feedback feedback)
        {
            if (id != feedback.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(feedback);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "✅ Feedback modifié avec succès !";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeedbackExists(feedback.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Recharger les données pour la vue
            feedback.Campagne = await _context.Campagnes.FindAsync(feedback.CampagneId);
            return View(feedback);
        }

        // GET: Feedback/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var feedback = await _context.Feedbacks
                .Include(f => f.Campagne)
                    .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feedback == null)
                return NotFound();

            return View(feedback);
        }

        // POST: Feedback/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Feedback supprimé avec succès !";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FeedbackExists(Guid id)
        {
            return _context.Feedbacks.Any(e => e.Id == id);
        }
    }
} 