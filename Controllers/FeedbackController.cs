using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
                    .Include(f => f.Activation)
                        .ThenInclude(a => a.Campagne)
                            .ThenInclude(c => c.Client)
                    .OrderByDescending(f => f.DateFeedback)
                    .ToListAsync();

                return View(feedbacks);
            }
            catch (Exception ex)
            {
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
                .Include(f => f.Activation)
                    .ThenInclude(a => a.Campagne)
                        .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feedback == null)
                return NotFound();

            return View(feedback);
        }

        // GET: Feedback/Repondre/5
        public async Task<IActionResult> Repondre(Guid? id)
        {
            if (id == null)
                return NotFound();

            var feedback = await _context.Feedbacks
                .Include(f => f.Campagne)
                    .ThenInclude(c => c.Client)
                .Include(f => f.Activation)
                    .ThenInclude(a => a.Campagne)
                        .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feedback == null)
                return NotFound();

            return View(feedback);
        }

        // POST: Feedback/Repondre/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Repondre(Guid id, [Bind("Id,ReponseAdmin")] Feedback feedback)
        {
            if (id != feedback.Id)
                return NotFound();

            var feedbackExistant = await _context.Feedbacks.FindAsync(id);
            if (feedbackExistant == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    feedbackExistant.ReponseAdmin = feedback.ReponseAdmin;
                    feedbackExistant.DateReponseAdmin = DateTime.Now;
                    feedbackExistant.AdminRepondant = User.Identity?.Name;

                    _context.Update(feedbackExistant);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "✅ Réponse envoyée avec succès !";
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
            feedbackExistant = await _context.Feedbacks
                .Include(f => f.Campagne)
                    .ThenInclude(c => c.Client)
                .Include(f => f.Activation)
                    .ThenInclude(a => a.Campagne)
                        .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(f => f.Id == id);

            return View(feedbackExistant);
        }

        // POST: Feedback/Masquer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Masquer(Guid id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            feedback.EstMasque = true;
            feedback.DateMasquage = DateTime.Now;
            feedback.AdminMasquant = User.Identity?.Name;

            _context.Update(feedback);
            await _context.SaveChangesAsync();
            TempData["Success"] = "✅ Feedback masqué avec succès !";

            return RedirectToAction(nameof(Index));
        }

        // POST: Feedback/Afficher/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Afficher(Guid id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            feedback.EstMasque = false;
            feedback.DateMasquage = null;
            feedback.AdminMasquant = null;

            _context.Update(feedback);
            await _context.SaveChangesAsync();
            TempData["Success"] = "✅ Feedback affiché avec succès !";

            return RedirectToAction(nameof(Index));
        }

        // GET: Feedback/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var feedback = await _context.Feedbacks
                .Include(f => f.Campagne)
                    .ThenInclude(c => c.Client)
                .Include(f => f.Activation)
                    .ThenInclude(a => a.Campagne)
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