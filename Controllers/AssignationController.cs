using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using DiversityPub.Models.enums;
using Microsoft.AspNetCore.Authorization;

namespace DiversityPub.Controllers
{
    [Authorize(Roles = "Admin,ChefProjet")]
    public class AssignationController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public AssignationController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: Assignation
        public async Task<IActionResult> Index()
        {
            var activations = await _context.Activations
                .Include(a => a.Campagne)
                .Include(a => a.Lieu)
                .Include(a => a.AgentsTerrain)
                    .ThenInclude(at => at.Utilisateur)
                .Include(a => a.Responsable)
                    .ThenInclude(r => r.Utilisateur)
                .Where(a => a.Statut != StatutActivation.Terminee)
                .OrderByDescending(a => a.DateCreation)
                .ToListAsync();

            return View(activations);
        }

        // GET: Assignation/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var activation = await _context.Activations
                .Include(a => a.Campagne)
                .Include(a => a.Lieu)
                .Include(a => a.AgentsTerrain)
                    .ThenInclude(at => at.Utilisateur)
                .Include(a => a.Responsable)
                    .ThenInclude(r => r.Utilisateur)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (activation == null)
                return NotFound();

            // Charger tous les agents terrain avec leurs utilisateurs
            var tousLesAgents = await _context.AgentsTerrain
                .Include(at => at.Utilisateur)
                .ToListAsync();

            ViewBag.AgentsTerrain = tousLesAgents;
            return View(activation);
        }

        // POST: Assignation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, List<Guid> agentIds, Guid? responsableId)
        {
            var activation = await _context.Activations
                .Include(a => a.AgentsTerrain)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (activation == null)
                return NotFound();

            // Validation : Permettre la modification des assignations même pour les activations en cours
            // Suppression de la restriction qui empêchait de retirer tous les agents d'une activation en cours
            
            // Validation des agents terrain - vérifier qu'ils ne sont pas déjà affectés à d'autres activations non terminées
            if (agentIds != null && agentIds.Any())
            {
                var agentsEnConflit = new List<string>();
                
                foreach (var agentId in agentIds)
                {
                    // Vérifier si l'agent est déjà affecté à une activation non terminée à la même date (exclure l'activation actuelle)
                    var activationsConflitantes = await _context.Activations
                        .Include(a => a.AgentsTerrain)
                        .Where(a => a.Id != id // Exclure l'activation en cours de modification
                                   && a.DateActivation == activation.DateActivation 
                                   && a.Statut != StatutActivation.Terminee
                                   && a.AgentsTerrain.Any(at => at.Id == agentId))
                        .ToListAsync();

                    if (activationsConflitantes.Any())
                    {
                        var agent = await _context.AgentsTerrain
                            .Include(at => at.Utilisateur)
                            .FirstOrDefaultAsync(at => at.Id == agentId);
                        
                        if (agent != null)
                        {
                            var nomAgent = $"{agent.Utilisateur.Prenom} {agent.Utilisateur.Nom}";
                            var activations = string.Join(", ", activationsConflitantes.Select(a => a.Nom));
                            agentsEnConflit.Add($"{nomAgent} (déjà affecté à: {activations})");
                        }
                    }
                }

                if (agentsEnConflit.Any())
                {
                    TempData["Error"] = $"❌ Les agents suivants ne peuvent pas être affectés car ils sont déjà engagés dans d'autres activations non terminées: {string.Join("; ", agentsEnConflit)}";
                    return RedirectToAction(nameof(Edit), new { id });
                }
            }

            try
            {
                // Mettre à jour les agents terrain
                if (agentIds != null && agentIds.Any())
                {
                    var agents = await _context.AgentsTerrain
                        .Where(at => agentIds.Contains(at.Id))
                        .ToListAsync();
                    
                    activation.AgentsTerrain = agents;
                    
                    // Gérer le responsable
                    if (responsableId.HasValue)
                    {
                        // Vérifier que le responsable est bien parmi les agents sélectionnés
                        if (agentIds.Contains(responsableId.Value))
                        {
                            activation.ResponsableId = responsableId.Value;
                        }
                        else
                        {
                            TempData["Warning"] = "⚠️ Le responsable doit être sélectionné parmi les agents assignés.";
                            activation.ResponsableId = null;
                        }
                    }
                    else
                    {
                        activation.ResponsableId = null;
                    }
                }
                else
                {
                    activation.AgentsTerrain.Clear();
                    activation.ResponsableId = null; // Pas de responsable si aucun agent
                }

                _context.Update(activation);
                await _context.SaveChangesAsync();
                
                // Validation post-mise à jour : si l'activation est en cours et n'a plus d'agents, proposer de la suspendre
                if (activation.Statut == StatutActivation.EnCours && (!agentIds?.Any() ?? true))
                {
                    TempData["Warning"] = "⚠️ L'activation est en cours mais n'a plus d'agents assignés. Considérez de la suspendre ou de la terminer.";
                }
                
                var message = "✅ Assignation des agents mise à jour avec succès !";
                if (responsableId.HasValue && agentIds.Contains(responsableId.Value))
                {
                    var responsable = await _context.AgentsTerrain
                        .Include(at => at.Utilisateur)
                        .FirstOrDefaultAsync(at => at.Id == responsableId.Value);
                    if (responsable != null)
                    {
                        message += $" Responsable désigné : {responsable.Utilisateur.Prenom} {responsable.Utilisateur.Nom}";
                    }
                }
                TempData["Success"] = message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Erreur lors de la mise à jour de l'assignation: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
} 