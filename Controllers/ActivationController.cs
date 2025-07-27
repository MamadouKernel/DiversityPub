using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using DiversityPub.Models.enums;
using Microsoft.AspNetCore.Authorization;
using DiversityPub.Services;

namespace DiversityPub.Controllers
{
    [Authorize(Roles = "Admin,ChefProjet")]
    public class ActivationController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public ActivationController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: Activation
        public async Task<IActionResult> Index()
        {
            // Vérifier et mettre à jour automatiquement les campagnes et activations expirées
            await CheckAndUpdateExpiredCampagnesAsync();
            await CheckAndUpdateExpiredActivationsAsync();
            
            var activations = await _context.Activations
                .Include(a => a.Campagne)
                .Include(a => a.Lieu)
                .Include(a => a.AgentsTerrain)
                    .ThenInclude(at => at.Utilisateur)
                .Include(a => a.Responsable)
                    .ThenInclude(r => r.Utilisateur)
                .OrderByDescending(a => a.DateActivation)
                .ToListAsync();
            
            return View(activations);
        }

        // GET: Activation/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            // Vérifier et mettre à jour automatiquement les campagnes et activations expirées
            await CheckAndUpdateExpiredCampagnesAsync();
            await CheckAndUpdateExpiredActivationsAsync();

            var activation = await _context.Activations
                .Include(a => a.Campagne)
                    .ThenInclude(c => c.Client)
                .Include(a => a.Lieu)
                .Include(a => a.AgentsTerrain)
                    .ThenInclude(at => at.Utilisateur)
                .Include(a => a.Responsable)
                    .ThenInclude(r => r.Utilisateur)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (activation == null)
                return NotFound();

            return View(activation);
        }

        // GET: Activation/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var campagnes = await _context.Campagnes.ToListAsync();
                var lieux = await _context.Lieux.ToListAsync();
                var agentsTerrain = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .ToListAsync();
                
                // Afficher un message d'avertissement seulement si les données essentielles manquent
                if (campagnes.Count == 0)
                {
                    TempData["Warning"] = "⚠️ Aucune campagne disponible. Veuillez d'abord créer une campagne.";
                }
                else if (lieux.Count == 0)
                {
                    TempData["Warning"] = "⚠️ Aucun lieu disponible. Veuillez d'abord créer des lieux.";
                }
                
                ViewBag.Campagnes = campagnes;
                ViewBag.Lieux = lieux;
                ViewBag.AgentsTerrain = new List<AgentTerrain>(); // Commencer avec une liste vide
                ViewBag.TousLesAgents = agentsTerrain; // Garder tous les agents pour l'affichage initial
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Erreur lors du chargement des données: {ex.Message}";
                return View();
            }
        }

        // POST: Activation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Description,DateActivation,HeureDebut,HeureFin,LieuId,Instructions,CampagneId")] Activation activation, List<Guid> agentIds)
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
            if (activation.CampagneId != Guid.Empty)
            {
                var campagne = await _context.Campagnes.FindAsync(activation.CampagneId);
                if (campagne != null)
                {
                    // Pour une campagne passée, les activations doivent être dans l'intervalle de la campagne
                    if (activation.DateActivation < campagne.DateDebut || activation.DateActivation > campagne.DateFin)
                    {
                        ModelState.AddModelError("DateActivation", 
                            $"La date d'activation doit être comprise entre {campagne.DateDebut.ToString("dd/MM/yyyy")} et {campagne.DateFin.ToString("dd/MM/yyyy")}");
                    }
                }
            }

            // Validation des agents terrain - vérifier qu'ils ne sont pas déjà affectés à d'autres activations non terminées
            if (agentIds != null && agentIds.Any())
            {
                var agentsEnConflit = new List<string>();
                
                foreach (var agentId in agentIds)
                {
                    // Vérifier si l'agent est déjà affecté à une activation non terminée à la même date
                    var activationsConflitantes = await _context.Activations
                        .Include(a => a.AgentsTerrain)
                        .Where(a => a.DateActivation == activation.DateActivation 
                                   && a.Statut != DiversityPub.Models.enums.StatutActivation.Terminee
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
                    ModelState.AddModelError("", 
                        $"Les agents suivants ne peuvent pas être affectés car ils sont déjà engagés dans d'autres activations non terminées: {string.Join("; ", agentsEnConflit)}");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    activation.Id = Guid.NewGuid();
                    
                    // Vérifier si la date d'activation est passée pour définir le statut automatiquement
                    if (activation.DateActivation < DateTime.Today)
                    {
                        activation.Statut = DiversityPub.Models.enums.StatutActivation.Terminee;
                    }
                    else
                    {
                        activation.Statut = DiversityPub.Models.enums.StatutActivation.Planifiee;
                    }

                    // Assigner les agents terrain
                    if (agentIds != null && agentIds.Any())
                    {
                        var agents = await _context.AgentsTerrain
                            .Where(at => agentIds.Contains(at.Id))
                            .ToListAsync();
                        
                        activation.AgentsTerrain = agents;
                    }

                    _context.Add(activation);
                    await _context.SaveChangesAsync();
                     
                     // Mettre à jour automatiquement le statut de la campagne
                     await UpdateCampagneStatutAsync(activation.CampagneId);
                     
                     // Compter les agents affectés
                     var nbAgents = agentIds?.Count ?? 0;
                     var messageAgents = nbAgents > 0 ? $" avec {nbAgents} agent(s) terrain" : "";
                     
                     TempData["Success"] = $"✅ Activation '{activation.Nom}' créée avec succès{messageAgents} !";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"❌ Erreur lors de la création de l'activation: {ex.Message}";
                }
            }
            
            ViewBag.Campagnes = await _context.Campagnes.ToListAsync();
            ViewBag.Lieux = await _context.Lieux.ToListAsync();
            ViewBag.AgentsTerrain = await _context.AgentsTerrain
                .Include(at => at.Utilisateur)
                .ToListAsync();
            return View(activation);
        }

        // GET: Activation/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var activation = await _context.Activations
                .Include(a => a.Campagne)
                .Include(a => a.Lieu)
                .Include(a => a.AgentsTerrain)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (activation == null)
                return NotFound();

            ViewBag.Campagnes = await _context.Campagnes.ToListAsync();
            ViewBag.Lieux = await _context.Lieux.ToListAsync();
            
            // Charger tous les agents terrain avec leurs utilisateurs
            var tousLesAgents = await _context.AgentsTerrain
                .Include(at => at.Utilisateur)
                .ToListAsync();
            
            // Pour l'instant, afficher tous les agents sans filtrage
            ViewBag.AgentsTerrain = tousLesAgents;
            return View(activation);
        }

        // POST: Activation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nom,Description,DateActivation,HeureDebut,HeureFin,LieuId,Instructions,Statut,CampagneId")] Activation activation, List<Guid> agentIds)
        {
            if (id != activation.Id)
                return NotFound();

            // Récupérer l'activation existante pour vérifier le statut actuel
            var currentActivation = await _context.Activations
                .Include(a => a.AgentsTerrain)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (currentActivation == null)
                return NotFound();

            // Validation des transitions de statut
            if (!ActivationStatusService.IsTransitionAllowed(currentActivation.Statut, activation.Statut))
            {
                var errorMessage = ActivationStatusService.GetTransitionErrorMessage(currentActivation.Statut, activation.Statut);
                ModelState.AddModelError("Statut", errorMessage);
                TempData["Error"] = $"❌ {errorMessage}";
            }

            // Validation : Une activation ne peut pas démarrer sans agent terrain
            if (activation.Statut == StatutActivation.EnCours)
            {
                var hasAgents = agentIds != null && agentIds.Any();
                if (!hasAgents)
                {
                    ModelState.AddModelError("", "❌ Impossible de démarrer une activation sans agent terrain. Veuillez assigner au moins un agent.");
                    TempData["Error"] = "❌ Impossible de démarrer une activation sans agent terrain. Veuillez assigner au moins un agent.";
                }
            }

            // Validation de la date d'activation par rapport à la campagne
            if (activation.CampagneId != Guid.Empty)
            {
                var campagne = await _context.Campagnes.FindAsync(activation.CampagneId);
                if (campagne != null)
                {
                    // Pour une campagne passée, les activations doivent être dans l'intervalle de la campagne
                    if (activation.DateActivation < campagne.DateDebut || activation.DateActivation > campagne.DateFin)
                    {
                        ModelState.AddModelError("DateActivation", 
                            $"La date d'activation doit être comprise entre {campagne.DateDebut.ToString("dd/MM/yyyy")} et {campagne.DateFin.ToString("dd/MM/yyyy")}");
                    }
                }
            }

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
                                   && a.Statut != DiversityPub.Models.enums.StatutActivation.Terminee
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
                    ModelState.AddModelError("", 
                        $"Les agents suivants ne peuvent pas être affectés car ils sont déjà engagés dans d'autres activations non terminées: {string.Join("; ", agentsEnConflit)}");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // On utilise currentActivation qui a déjà été récupéré plus haut
                    if (currentActivation == null)
                        return NotFound();

                                        // Mettre à jour les propriétés de base
                    currentActivation.Nom = activation.Nom;
                    currentActivation.Description = activation.Description;
                    currentActivation.DateActivation = activation.DateActivation;
                    currentActivation.HeureDebut = activation.HeureDebut;
                    currentActivation.HeureFin = activation.HeureFin;
                    currentActivation.LieuId = activation.LieuId;
                    currentActivation.Instructions = activation.Instructions;
                    currentActivation.CampagneId = activation.CampagneId;
                    
                    // Vérifier si la date d'activation est passée pour définir le statut automatiquement
                    if (activation.DateActivation < DateTime.Today)
                    {
                        currentActivation.Statut = DiversityPub.Models.enums.StatutActivation.Terminee;
                    }
                    else
                    {
                        currentActivation.Statut = activation.Statut;
                    }

                    // Mettre à jour les agents terrain
                    if (agentIds != null && agentIds.Any())
                    {
                        var agents = await _context.AgentsTerrain
                            .Where(at => agentIds.Contains(at.Id))
                            .ToListAsync();
                        
                        currentActivation.AgentsTerrain = agents;
                    }
                    else
                    {
                        currentActivation.AgentsTerrain.Clear();
                    }

                    _context.Update(currentActivation);
                    await _context.SaveChangesAsync();
                    
                    // Mettre à jour automatiquement le statut de la campagne
                    await UpdateCampagneStatutAsync(currentActivation.CampagneId);
                     
                     TempData["Success"] = "Activation modifiée avec succès !";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActivationExists(activation.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Campagnes = await _context.Campagnes.ToListAsync();
            ViewBag.Lieux = await _context.Lieux.ToListAsync();
            ViewBag.AgentsTerrain = await _context.AgentsTerrain
                .Include(at => at.Utilisateur)
                .ToListAsync();
            return View(activation);
        }

        // GET: Activation/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var activation = await _context.Activations
                .Include(a => a.Campagne)
                .Include(a => a.Lieu)
                .Include(a => a.AgentsTerrain)
                    .ThenInclude(at => at.Utilisateur)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (activation == null)
                return NotFound();

            return View(activation);
        }

        // POST: Activation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var activation = await _context.Activations.FindAsync(id);
            if (activation != null)
            {
                _context.Activations.Remove(activation);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ActivationExists(Guid id)
        {
            return _context.Activations.Any(e => e.Id == id);
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

        // Méthode pour vérifier et mettre à jour automatiquement les activations expirées
        private async Task CheckAndUpdateExpiredActivationsAsync()
        {
            var activationsExpirees = await _context.Activations
                .Where(a => a.DateActivation < DateTime.Today 
                           && a.Statut != StatutActivation.Terminee)
                .ToListAsync();

            foreach (var activation in activationsExpirees)
            {
                activation.Statut = StatutActivation.Terminee;
            }

            if (activationsExpirees.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        // Action AJAX pour récupérer les agents disponibles pour une date donnée
        [HttpGet]
        public async Task<IActionResult> GetAgentsDisponibles(DateTime date, Guid? activationId = null)
        {
            try
            {
                // Récupérer tous les agents
                var tousLesAgents = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .ToListAsync();

                // Récupérer les agents déjà affectés à des activations non terminées à cette date
                var agentsOccupees = await _context.Activations
                    .Include(a => a.AgentsTerrain)
                        .ThenInclude(at => at.Utilisateur)
                    .Where(a => a.DateActivation == date && a.Statut != StatutActivation.Terminee)
                    .SelectMany(a => a.AgentsTerrain)
                    .Select(at => at.Id)
                    .Distinct()
                    .ToListAsync();

                // Si on est en mode édition, exclure les agents de l'activation actuelle
                if (activationId.HasValue)
                {
                    var agentsActivationActuelle = await _context.Activations
                        .Include(a => a.AgentsTerrain)
                        .Where(a => a.Id == activationId.Value)
                        .SelectMany(a => a.AgentsTerrain)
                        .Select(at => at.Id)
                        .ToListAsync();

                    // Retirer les agents de l'activation actuelle de la liste des agents occupés
                    agentsOccupees = agentsOccupees.Except(agentsActivationActuelle).ToList();
                }

                // Filtrer les agents disponibles
                var agentsDisponibles = tousLesAgents
                    .Where(at => !agentsOccupees.Contains(at.Id))
                    .Select(at => new
                    {
                        Id = at.Id,
                        Nom = $"{at.Utilisateur.Prenom} {at.Utilisateur.Nom}",
                        Email = at.Utilisateur.Email
                    })
                    .ToList();

                return Json(new { success = true, agents = agentsDisponibles });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 