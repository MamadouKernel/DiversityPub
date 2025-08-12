using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using DiversityPub.Models.enums;
using DiversityPub.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DiversityPub.Controllers
{
    [Authorize(Roles = "AgentTerrain,Admin,ChefProjet")]
    public class AgentTerrainController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public AgentTerrainController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: AgentTerrain
        public async Task<IActionResult> Index()
        {
            try
            {
                var userEmail = User.Identity.Name;
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agent == null)
                {
                    return View("Error", new { Message = "Agent terrain non trouvé." });
                }

                // Récupérer les activations de l'agent
                var activations = await _context.Activations
                    .Include(a => a.Campagne)
                    .Include(a => a.Lieu)
                    .Include(a => a.AgentsTerrain)
                        .ThenInclude(at => at.Utilisateur)
                    .Where(a => a.AgentsTerrain.Any(at => at.Id == agent.Id))
                    .OrderByDescending(a => a.DateActivation)
                    .ToListAsync();

                ViewBag.Agent = agent;
                return View(activations);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des données: {ex.Message}" });
            }
        }

        // GET: AgentTerrain/Missions
        public async Task<IActionResult> Missions()
        {
            try
            {
                var userEmail = User.Identity.Name;
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agent == null)
                {
                    return View("Error", new { Message = "Agent terrain non trouvé." });
                }

                // Récupérer les activations de l'agent avec plus de détails
                var activations = await _context.Activations
                    .Include(a => a.Campagne)
                    .Include(a => a.Lieu)
                    .Include(a => a.AgentsTerrain)
                        .ThenInclude(at => at.Utilisateur)
                    .Include(a => a.Medias)
                    .Include(a => a.Incidents)
                    .Where(a => a.AgentsTerrain.Any(at => at.Id == agent.Id))
                    .OrderByDescending(a => a.DateActivation)
                    .ToListAsync();

                ViewBag.Agent = agent;
                return View(activations);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des missions: {ex.Message}" });
            }
        }

        // GET: AgentTerrain/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            Console.WriteLine($"=== DEBUG AgentTerrain/Details ===");
            Console.WriteLine($"ID demandé: {id}");
            
            if (id == null)
            {
                Console.WriteLine("❌ ID est null");
                return NotFound();
            }

            try
            {
                var userEmail = User.Identity.Name;
                Console.WriteLine($"Email utilisateur: {userEmail}");
                
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                Console.WriteLine($"Agent trouvé: {(agent != null ? $"{agent.Utilisateur.Prenom} {agent.Utilisateur.Nom} (ID: {agent.Id})" : "NULL")}");

                if (agent == null)
                {
                    Console.WriteLine("❌ Agent terrain non trouvé");
                    return View("Error", new { Message = "Agent terrain non trouvé." });
                }

                // Vérifier d'abord si l'activation existe
                var activationExists = await _context.Activations.AnyAsync(a => a.Id == id);
                Console.WriteLine($"Activation existe: {activationExists}");
                
                // Vérifier si l'agent est assigné à cette activation
                var isAssigned = await _context.Activations
                    .AnyAsync(a => a.Id == id && a.AgentsTerrain.Any(at => at.Id == agent.Id));
                Console.WriteLine($"Agent assigné à cette activation: {isAssigned}");
                
                // Debug: Lister toutes les activations de cet agent
                var agentActivations = await _context.Activations
                    .Where(a => a.AgentsTerrain.Any(at => at.Id == agent.Id))
                    .Select(a => new { a.Id, a.Campagne.Nom })
                    .ToListAsync();
                Console.WriteLine($"Activations de l'agent:");
                foreach (var act in agentActivations)
                {
                    Console.WriteLine($"  - {act.Id}: {act.Nom}");
                }

                var activation = await _context.Activations
                    .Include(a => a.Campagne)
                    .Include(a => a.Lieu)
                    .Include(a => a.AgentsTerrain)
                        .ThenInclude(at => at.Utilisateur)
                    .Include(a => a.Medias)
                    .Include(a => a.Incidents)
                    .FirstOrDefaultAsync(a => a.Id == id && a.AgentsTerrain.Any(at => at.Id == agent.Id));

                if (activation == null)
                {
                    Console.WriteLine("❌ Activation non trouvée avec restriction agent");
                    
                    // Tentative de récupération sans restriction d'agent pour diagnostic
                    var activationSansRestriction = await _context.Activations
                        .Include(a => a.Campagne)
                        .Include(a => a.Lieu)
                        .Include(a => a.AgentsTerrain)
                            .ThenInclude(at => at.Utilisateur)
                        .Include(a => a.Medias)
                        .Include(a => a.Incidents)
                        .FirstOrDefaultAsync(a => a.Id == id);
                    
                    if (activationSansRestriction == null)
                    {
                        Console.WriteLine("❌ Activation n'existe pas du tout");
                        return NotFound();
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Activation existe mais agent non assigné - accès temporaire pour diagnostic");
                        ViewBag.Agent = agent;
                        ViewBag.WarningMessage = "Attention: Vous n'êtes pas assigné à cette activation.";
                        return View(activationSansRestriction);
                    }
                }

                ViewBag.Agent = agent;
                return View(activation);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des détails: {ex.Message}" });
            }
        }

        // Méthode helper pour vérifier si l'agent est responsable d'une activation
        private async Task<bool> EstResponsableActivation(Guid activationId, Guid agentId)
        {
            var activation = await _context.Activations
                .FirstOrDefaultAsync(a => a.Id == activationId);
            
            return activation?.ResponsableId == agentId;
        }

        // POST: AgentTerrain/DemarrerActivation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DemarrerActivation(Guid activationId)
        {
            try
            {
                var userEmail = User.Identity.Name;
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agent == null)
                {
                    return Json(new { success = false, message = "Agent non trouvé." });
                }

                // Vérifier si l'agent est responsable de l'activation
                if (!await EstResponsableActivation(activationId, agent.Id))
                {
                    return Json(new { success = false, message = "Seul le responsable de l'activation peut effectuer cette action." });
                }

                var activation = await _context.Activations
                    .Include(a => a.AgentsTerrain)
                    .Include(a => a.Campagne)
                    .FirstOrDefaultAsync(a => a.Id == activationId);

                if (activation == null)
                {
                    return Json(new { success = false, message = "Activation non trouvée." });
                }

                // Vérifier que l'agent est bien affecté à cette activation
                if (!activation.AgentsTerrain.Any(at => at.Id == agent.Id))
                {
                    return Json(new { success = false, message = "Vous n'êtes pas autorisé à démarrer cette activation." });
                }

                // Vérifier que l'activation est planifiée
                if (activation.Statut != StatutActivation.Planifiee)
                {
                    return Json(new { success = false, message = "Cette activation ne peut pas être démarrée." });
                }

                // Vérifier que la date d'activation est aujourd'hui
                if (activation.DateActivation.Date != DateTime.Today)
                {
                    return Json(new { success = false, message = "Cette activation ne peut être démarrée qu'à sa date prévue." });
                }

                // Démarrer l'activation
                activation.Statut = StatutActivation.EnCours;
                
                // Mettre automatiquement la campagne en cours si elle ne l'est pas déjà
                if (activation.Campagne != null)
                {
                    Console.WriteLine($"=== DEBUG DÉMARRAGE CAMPAGNE ===");
                    Console.WriteLine($"Campagne: {activation.Campagne.Nom} (ID: {activation.Campagne.Id})");
                    Console.WriteLine($"Statut actuel de la campagne: {activation.Campagne.Statut}");
                    Console.WriteLine($"Statut EnCours: {StatutCampagne.EnCours}");
                    Console.WriteLine($"Condition: {activation.Campagne.Statut != StatutCampagne.EnCours}");
                    
                    if (activation.Campagne.Statut != StatutCampagne.EnCours)
                    {
                        activation.Campagne.Statut = StatutCampagne.EnCours;
                        Console.WriteLine($"✅ Campagne '{activation.Campagne.Nom}' mise en cours automatiquement.");
                    }
                    else
                    {
                        Console.WriteLine($"ℹ️ Campagne '{activation.Campagne.Nom}' était déjà en cours.");
                    }
                }
                else
                {
                    Console.WriteLine("=== ERREUR: Campagne est null ===");
                }
                
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Activation démarrée avec succès !",
                    activationId = activation.Id,
                    statut = activation.Statut.ToString()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors du démarrage: {ex.Message}" });
            }
        }

        // POST: AgentTerrain/TerminerActivation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TerminerActivation(Guid activationId)
        {
            try
            {
                var userEmail = User.Identity.Name;
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agent == null)
                {
                    return Json(new { success = false, message = "Agent non trouvé." });
                }

                // Vérifier si l'agent est responsable de l'activation
                if (!await EstResponsableActivation(activationId, agent.Id))
                {
                    return Json(new { success = false, message = "Seul le responsable de l'activation peut effectuer cette action." });
                }

                var activation = await _context.Activations
                    .Include(a => a.AgentsTerrain)
                    .Include(a => a.Campagne)
                    .FirstOrDefaultAsync(a => a.Id == activationId);

                if (activation == null)
                {
                    return Json(new { success = false, message = "Activation non trouvée." });
                }

                // Vérifier que l'activation est en cours
                if (activation.Statut != StatutActivation.EnCours)
                {
                    return Json(new { success = false, message = "Cette activation ne peut pas être terminée." });
                }

                // Terminer l'activation
                activation.Statut = StatutActivation.Terminee;
                
                // Vérifier si toutes les activations de la campagne sont terminées
                if (activation.Campagne != null)
                {
                    Console.WriteLine($"=== DEBUG TERMINAISON CAMPAGNE ===");
                    Console.WriteLine($"Campagne: {activation.Campagne.Nom} (ID: {activation.Campagne.Id})");
                    Console.WriteLine($"Statut actuel de la campagne: {activation.Campagne.Statut}");
                    
                    var activationsCampagne = await _context.Activations
                        .Where(a => a.CampagneId == activation.CampagneId)
                        .ToListAsync();
                    
                    Console.WriteLine($"Nombre total d'activations dans la campagne: {activationsCampagne.Count}");
                    
                    foreach (var act in activationsCampagne)
                    {
                        Console.WriteLine($"- Activation '{act.Nom}': {act.Statut}");
                    }
                    
                    var activationsTerminees = activationsCampagne.Where(a => a.Statut == StatutActivation.Terminee).Count();
                    var toutesTerminees = activationsCampagne.All(a => a.Statut == StatutActivation.Terminee);
                    
                    Console.WriteLine($"Activations terminées: {activationsTerminees}/{activationsCampagne.Count}");
                    Console.WriteLine($"Toutes terminées: {toutesTerminees}");
                    
                    if (toutesTerminees && activation.Campagne.Statut != StatutCampagne.Terminee)
                    {
                        activation.Campagne.Statut = StatutCampagne.Terminee;
                        Console.WriteLine($"✅ Campagne '{activation.Campagne.Nom}' terminée automatiquement (toutes les activations terminées).");
                    }
                    else if (!toutesTerminees)
                    {
                        Console.WriteLine($"⏳ Campagne '{activation.Campagne.Nom}' reste en cours (activations non terminées: {activationsCampagne.Count - activationsTerminees})");
                        
                        // S'assurer que la campagne est en cours si elle n'est pas terminée
                        if (activation.Campagne.Statut != StatutCampagne.EnCours && activation.Campagne.Statut != StatutCampagne.Terminee)
                        {
                            activation.Campagne.Statut = StatutCampagne.EnCours;
                            Console.WriteLine($"🔄 Campagne '{activation.Campagne.Nom}' remise en cours.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("=== ERREUR: Campagne est null ===");
                }
                
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Activation terminée avec succès !",
                    activationId = activation.Id,
                    statut = activation.Statut.ToString()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la terminaison: {ex.Message}" });
            }
        }

        // POST: AgentTerrain/SuspendreActivation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendreActivation(Guid activationId, string motifSuspension)
        {
            try
            {
                var userEmail = User.Identity.Name;
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agent == null)
                {
                    return Json(new { success = false, message = "Agent non trouvé." });
                }

                // Vérifier si l'agent est responsable de l'activation
                if (!await EstResponsableActivation(activationId, agent.Id))
                {
                    return Json(new { success = false, message = "Seul le responsable de l'activation peut effectuer cette action." });
                }

                var activation = await _context.Activations
                    .Include(a => a.AgentsTerrain)
                    .Include(a => a.Campagne)
                    .FirstOrDefaultAsync(a => a.Id == activationId);

                if (activation == null)
                {
                    return Json(new { success = false, message = "Activation non trouvée." });
                }

                // Vérifier que l'activation est en cours
                if (activation.Statut != StatutActivation.EnCours)
                {
                    return Json(new { success = false, message = "Cette activation ne peut pas être suspendue." });
                }

                // Vérifier que le motif de suspension est fourni
                if (string.IsNullOrWhiteSpace(motifSuspension))
                {
                    return Json(new { success = false, message = "Le motif de suspension est obligatoire." });
                }

                // Suspendre l'activation avec motif
                activation.Statut = StatutActivation.Suspendue;
                activation.MotifSuspension = motifSuspension.Trim();
                activation.DateSuspension = DateTime.Now;
                
                Console.WriteLine($"=== SUSPENSION ACTIVATION ===");
                Console.WriteLine($"Activation: {activation.Nom}");
                Console.WriteLine($"Motif: {activation.MotifSuspension}");
                Console.WriteLine($"Date: {activation.DateSuspension}");
                
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Activation suspendue avec succès !",
                    activationId = activation.Id,
                    statut = activation.Statut.ToString(),
                    motif = activation.MotifSuspension
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la suspension: {ex.Message}" });
            }
        }

        // POST: AgentTerrain/ReprendreActivation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReprendreActivation(Guid activationId)
        {
            try
            {
                var userEmail = User.Identity.Name;
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agent == null)
                {
                    return Json(new { success = false, message = "Agent non trouvé." });
                }

                // Vérifier si l'agent est responsable de l'activation
                if (!await EstResponsableActivation(activationId, agent.Id))
                {
                    return Json(new { success = false, message = "Seul le responsable de l'activation peut effectuer cette action." });
                }

                var activation = await _context.Activations
                    .Include(a => a.AgentsTerrain)
                    .Include(a => a.Campagne)
                    .FirstOrDefaultAsync(a => a.Id == activationId);

                if (activation == null)
                {
                    return Json(new { success = false, message = "Activation non trouvée." });
                }

                // Vérifier que l'activation est suspendue
                if (activation.Statut != StatutActivation.Suspendue)
                {
                    return Json(new { success = false, message = "Cette activation ne peut pas être reprise." });
                }

                // Reprendre l'activation
                activation.Statut = StatutActivation.EnCours;
                
                // Remettre la campagne en cours si elle était suspendue
                if (activation.Campagne != null && activation.Campagne.Statut == StatutCampagne.Annulee)
                {
                    activation.Campagne.Statut = StatutCampagne.EnCours;
                    Console.WriteLine($"Campagne '{activation.Campagne.Nom}' remise en cours automatiquement.");
                }
                
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Activation reprise avec succès !",
                    activationId = activation.Id,
                    statut = activation.Statut.ToString()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la reprise: {ex.Message}" });
            }
        }

        // GET: AgentTerrain/Incidents
        public async Task<IActionResult> Incidents()
        {
            try
            {
                var userEmail = User.Identity.Name;
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agent == null)
                {
                    return View("Error", new { Message = "Agent terrain non trouvé." });
                }

                var incidents = await _context.Incidents
                    .Include(i => i.Activation)
                        .ThenInclude(a => a.Campagne)
                    .Include(i => i.Activation)
                        .ThenInclude(a => a.Lieu)
                    .Where(i => i.AgentTerrainId == agent.Id)
                    .OrderByDescending(i => i.DateCreation)
                    .ToListAsync();

                ViewBag.Agent = agent;
                return View(incidents);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des incidents: {ex.Message}" });
            }
        }

        // GET: AgentTerrain/DetailsPreuve/id
        public async Task<IActionResult> DetailsPreuve(Guid? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var userEmail = User.Identity.Name;
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agent == null)
                {
                    return View("Error", new { Message = "Agent terrain non trouvé." });
                }

                var media = await _context.Medias
                    .Include(m => m.Activation)
                        .ThenInclude(a => a.Campagne)
                    .Include(m => m.Activation)
                        .ThenInclude(a => a.Lieu)
                    .Include(m => m.AgentTerrain)
                        .ThenInclude(at => at.Utilisateur)
                    .FirstOrDefaultAsync(m => m.Id == id && m.AgentTerrainId == agent.Id);

                if (media == null)
                    return NotFound();

                ViewBag.Agent = agent;
                return View(media);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement de la preuve: {ex.Message}" });
            }
        }

        // GET: AgentTerrain/Preuves
        public async Task<IActionResult> Preuves()
        {
            try
            {
                var userEmail = User.Identity.Name;
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agent == null)
                {
                    return View("Error", new { Message = "Agent terrain non trouvé." });
                }

                var medias = await _context.Medias
                    .Include(m => m.Activation)
                        .ThenInclude(a => a.Campagne)
                    .Include(m => m.Activation)
                        .ThenInclude(a => a.Lieu)
                    .Where(m => m.AgentTerrainId == agent.Id)
                    .OrderByDescending(m => m.DateUpload)
                    .ToListAsync();

                ViewBag.Agent = agent;
                return View(medias);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des preuves: {ex.Message}" });
            }
        }

        // GET: AgentTerrain/Profil
        public async Task<IActionResult> Profil()
        {
            try
            {
                var userEmail = User.Identity.Name;
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .Include(at => at.Activations)
                        .ThenInclude(a => a.Campagne)
                    .Include(at => at.Incidents)
                    .Include(at => at.Medias)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agent == null)
                {
                    return View("Error", new { Message = "Agent terrain non trouvé." });
                }

                ViewBag.Agent = agent;
                return View(agent);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement du profil: {ex.Message}" });
            }
        }

        // GET: AgentTerrain/SignalerIncident
        public async Task<IActionResult> SignalerIncident(Guid? activationId = null)
        {
            try
            {
                var userEmail = User.Identity.Name;
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                // Si l'utilisateur n'est pas un agent terrain, permettre l'accès aux Admin/ChefProjet
                if (agent == null && !User.IsInRole("Admin") && !User.IsInRole("ChefProjet"))
                {
                    return View("Error", new { Message = "Agent terrain non trouvé." });
                }

                // Récupérer les activations
                List<Activation> activations;
                if (agent != null)
                {
                    // Si c'est un agent terrain, récupérer ses activations
                    activations = await _context.Activations
                        .Include(a => a.Campagne)
                        .Include(a => a.Lieu)
                        .Where(a => a.AgentsTerrain.Any(at => at.Id == agent.Id))
                        .OrderByDescending(a => a.DateActivation)
                        .ToListAsync();
                }
                else
                {
                    // Si c'est un Admin/ChefProjet, récupérer toutes les activations
                    activations = await _context.Activations
                        .Include(a => a.Campagne)
                        .Include(a => a.Lieu)
                        .OrderByDescending(a => a.DateActivation)
                        .ToListAsync();
                }

                ViewBag.Agent = agent;
                ViewBag.Activations = activations;
                ViewBag.ActivationId = activationId;

                return View();
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement: {ex.Message}" });
            }
        }

        // POST: AgentTerrain/SignalerIncident
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignalerIncident(Incident incident)
        {
            try
            {
                var userEmail = User.Identity.Name;
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                // Si l'utilisateur n'est pas un agent terrain, permettre l'accès aux Admin/ChefProjet
                if (agent == null && !User.IsInRole("Admin") && !User.IsInRole("ChefProjet"))
                {
                    TempData["Error"] = "Agent terrain non trouvé.";
                    return RedirectToAction("Index");
                }

                if (ModelState.IsValid)
                {
                    // Si incident lié à une activation, vérifier les permissions responsable
                    if (incident.ActivationId.HasValue && agent != null)
                    {
                        if (!await EstResponsableActivation(incident.ActivationId.Value, agent.Id))
                        {
                            TempData["Error"] = "Seul le responsable de l'activation peut signaler un incident lié à celle-ci.";
                            return RedirectToAction("SignalerIncident", new { activationId = incident.ActivationId });
                        }
                    }

                    incident.Id = Guid.NewGuid();
                    
                    // Gérer le cas où agent peut être null pour Admin/ChefProjet
                    if (agent != null)
                    {
                        incident.AgentTerrainId = agent.Id;
                    }
                    else
                    {
                        // Pour Admin/ChefProjet, on peut laisser null
                        incident.AgentTerrainId = null;
                    }
                    
                    incident.DateCreation = DateTime.Now;
                    incident.Statut = "Ouvert";

                    _context.Incidents.Add(incident);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Incident signalé avec succès !";
                    return RedirectToAction("Incidents");
                }

                // En cas d'erreur de validation, recharger les données
                List<Activation> activations;
                if (agent != null)
                {
                    activations = await _context.Activations
                        .Include(a => a.Campagne)
                        .Include(a => a.Lieu)
                        .Where(a => a.AgentsTerrain.Any(at => at.Id == agent.Id))
                        .OrderByDescending(a => a.DateActivation)
                        .ToListAsync();
                }
                else
                {
                    activations = await _context.Activations
                        .Include(a => a.Campagne)
                        .Include(a => a.Lieu)
                        .OrderByDescending(a => a.DateActivation)
                        .ToListAsync();
                }

                ViewBag.Agent = agent;
                ViewBag.Activations = activations;
                ViewBag.ActivationId = incident.ActivationId;

                return View(incident);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors du signalement: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: AgentTerrain/EnvoyerPreuve
        public async Task<IActionResult> EnvoyerPreuve(Guid? activationId = null)
        {
            try
            {
                var userEmail = User.Identity.Name;
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agent == null)
                {
                    return View("Error", new { Message = "Agent terrain non trouvé." });
                }

                // Récupérer les activations de l'agent
                var activations = await _context.Activations
                    .Include(a => a.Campagne)
                    .Include(a => a.Lieu)
                    .Where(a => a.AgentsTerrain.Any(at => at.Id == agent.Id))
                    .OrderByDescending(a => a.DateActivation)
                    .ToListAsync();

                ViewBag.Agent = agent;
                ViewBag.Activations = activations;
                ViewBag.ActivationId = activationId;

                return View();
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement: {ex.Message}" });
            }
        }

        // POST: AgentTerrain/EnvoyerPreuve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnvoyerPreuve(string Description, string Type, Guid? ActivationId, IFormFile? fichier)
        {
            try
            {
                var userEmail = User.Identity.Name;
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agent == null)
                {
                    TempData["Error"] = "Agent terrain non trouvé.";
                    return RedirectToAction("Index");
                }

                // Debug: Log des données reçues
                Console.WriteLine($"=== DEBUG ENVOI PREUVE ===");
                Console.WriteLine($"Agent: {agent.Utilisateur.Prenom} {agent.Utilisateur.Nom}");
                Console.WriteLine($"Description: {Description}");
                Console.WriteLine($"Type: {Type}");
                Console.WriteLine($"ActivationId: {ActivationId}");
                Console.WriteLine($"Fichier paramètre reçu: {(fichier != null ? $"{fichier.FileName} ({fichier.Length} bytes)" : "NULL")}");
                Console.WriteLine($"ModelState.IsValid AVANT validation: {ModelState.IsValid}");
                Console.WriteLine($"Request.HasFormContentType: {Request.HasFormContentType}");
                Console.WriteLine($"Request.ContentType: {Request.ContentType}");
                
                // Log détaillé de tous les champs du formulaire
                Console.WriteLine("=== DONNÉES DU FORMULAIRE ===");
                foreach (var key in Request.Form.Keys)
                {
                    Console.WriteLine($"{key}: {Request.Form[key]}");
                }
                
                Console.WriteLine("=== FICHIERS REÇUS ===");
                Console.WriteLine($"Request.Form.Files.Count: {Request.Form.Files.Count}");
                foreach (var file in Request.Form.Files)
                {
                    Console.WriteLine($"File Key: {file.Name}, FileName: {file.FileName}, Length: {file.Length}");
                }
                
                // Logique de récupération alternative du fichier
                Console.WriteLine($"=== RÉCUPÉRATION FICHIER ===");
                Console.WriteLine($"Fichier paramètre initial: {(fichier != null ? $"{fichier.FileName} ({fichier.Length} bytes)" : "NULL")}");
                
                if (fichier == null)
                {
                    Console.WriteLine("Fichier paramètre est null, tentative de récupération alternative...");
                    
                    // Test direct de récupération du fichier par nom
                    var fichierDirect = Request.Form.Files["fichier"];
                    Console.WriteLine($"Fichier direct par nom 'fichier': {(fichierDirect != null ? $"{fichierDirect.FileName} ({fichierDirect.Length} bytes)" : "NULL")}");
                    
                    if (fichierDirect != null)
                    {
                        fichier = fichierDirect;
                        Console.WriteLine($"✅ Utilisation du fichier trouvé par nom: {fichier.FileName}");
                    }
                    // Si toujours null mais qu'il y a des fichiers, utiliser le premier
                    else if (Request.Form.Files.Count > 0)
                    {
                        fichier = Request.Form.Files[0];
                        Console.WriteLine($"✅ Utilisation du premier fichier trouvé: {fichier.FileName}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Aucun fichier trouvé dans Request.Form.Files");
                    }
                }
                else
                {
                    Console.WriteLine($"✅ Fichier paramètre déjà présent: {fichier.FileName}");
                }
                
                Console.WriteLine($"Fichier final après récupération: {(fichier != null ? $"{fichier.FileName} ({fichier.Length} bytes)" : "NULL")}");
                
                // Validation manuelle des champs obligatoires (APRÈS récupération alternative du fichier)
                if (string.IsNullOrWhiteSpace(Description))
                {
                    ModelState.AddModelError("Description", "La description est obligatoire.");
                }

                if (!Enum.TryParse<DiversityPub.Models.enums.TypeMedia>(Type, out var typeMedia))
                {
                    ModelState.AddModelError("Type", "Le type de média est invalide.");
                }

                if (!ActivationId.HasValue || ActivationId.Value == Guid.Empty)
                {
                    ModelState.AddModelError("ActivationId", "La sélection d'une mission est obligatoire.");
                }

                // Validation du fichier APRÈS récupération alternative
                if (fichier == null || fichier.Length == 0)
                {
                    ModelState.AddModelError("fichier", "Un fichier doit être sélectionné.");
                }
                
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("=== ERREURS DE VALIDATION ===");
                    foreach (var kvp in ModelState)
                    {
                        if (kvp.Value.Errors.Any())
                        {
                            Console.WriteLine($"Champ '{kvp.Key}':");
                            foreach (var error in kvp.Value.Errors)
                            {
                                Console.WriteLine($"  - {error.ErrorMessage}");
                            }
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    // Si preuve liée à une activation, vérifier les permissions responsable
                    if (ActivationId.HasValue)
                    {
                        if (!await EstResponsableActivation(ActivationId.Value, agent.Id))
                        {
                            TempData["Error"] = "Seul le responsable de l'activation peut envoyer des preuves liées à celle-ci.";
                            return RedirectToAction("EnvoyerPreuve", new { activationId = ActivationId });
                        }
                    }

                    // Créer l'objet Media
                    var media = new Media
                    {
                        Id = Guid.NewGuid(),
                        Description = Description,
                        Type = typeMedia,
                        ActivationId = ActivationId,
                        AgentTerrainId = agent.Id,
                        DateUpload = DateTime.Now
                    };

                    Console.WriteLine($"=== CRÉATION MÉDIA ===");
                    Console.WriteLine($"ID: {media.Id}");
                    Console.WriteLine($"Description: {media.Description}");
                    Console.WriteLine($"Type: {media.Type}");
                    Console.WriteLine($"ActivationId: {media.ActivationId}");
                    Console.WriteLine($"AgentTerrainId: {media.AgentTerrainId}");
                    Console.WriteLine($"DateUpload: {media.DateUpload}");

                    // Traitement du fichier uploadé
                    if (fichier != null && fichier.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(fichier.FileName);
                        var filePath = Path.Combine("wwwroot", "uploads", fileName);
                        
                        Console.WriteLine($"=== TRAITEMENT FICHIER ===");
                        Console.WriteLine($"Nom original: {fichier.FileName}");
                        Console.WriteLine($"Nom généré: {fileName}");
                        Console.WriteLine($"Chemin: {filePath}");
                        
                        // Créer le dossier s'il n'existe pas
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await fichier.CopyToAsync(stream);
                        }
                        
                        media.Url = "/uploads/" + fileName;
                        Console.WriteLine($"URL sauvegardée: {media.Url}");
                    }
                    else
                    {
                        Console.WriteLine("=== ERREUR: Aucun fichier reçu ===");
                        TempData["Error"] = "Aucun fichier n'a été sélectionné.";
                        
                        // Recharger les données pour l'affichage
                        var activationsReload = await _context.Activations
                            .Include(a => a.Campagne)
                            .Include(a => a.Lieu)
                            .Where(a => a.AgentsTerrain.Any(at => at.Id == agent.Id))
                            .OrderByDescending(a => a.DateActivation)
                            .ToListAsync();

                        ViewBag.Agent = agent;
                        ViewBag.Activations = activationsReload;
                        ViewBag.ActivationId = ActivationId;

                        return View(media);
                    }

                    Console.WriteLine($"=== SAUVEGARDE EN BASE ===");
                    _context.Medias.Add(media);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Média sauvegardé avec succès!");

                    TempData["Success"] = "Preuve envoyée avec succès !";
                    return RedirectToAction("Preuves");
                }

                // En cas d'erreur de validation, recharger les données
                Console.WriteLine("=== RECHARGEMENT DONNÉES ===");
                var activationsForView = await _context.Activations
                    .Include(a => a.Campagne)
                    .Include(a => a.Lieu)
                    .Where(a => a.AgentsTerrain.Any(at => at.Id == agent.Id))
                    .OrderByDescending(a => a.DateActivation)
                    .ToListAsync();

                ViewBag.Agent = agent;
                ViewBag.Activations = activationsForView;
                ViewBag.ActivationId = ActivationId;

                // Créer un objet Media pour conserver les valeurs du formulaire
                var mediaForView = new Media
                {
                    Description = Description ?? string.Empty,
                    ActivationId = ActivationId
                };

                if (Enum.TryParse<DiversityPub.Models.enums.TypeMedia>(Type, out var parsedType))
                {
                    mediaForView.Type = parsedType;
                }

                return View(mediaForView);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERREUR EXCEPTION ===");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                TempData["Error"] = $"Erreur lors de l'envoi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: AgentTerrain/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            try
            {
                var userEmail = User.Identity.Name;
                var agent = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agent == null)
                {
                    TempData["Error"] = "Agent terrain non trouvé.";
                    return RedirectToAction("Profil");
                }

                // Validation des champs
                if (string.IsNullOrWhiteSpace(CurrentPassword))
                {
                    TempData["Error"] = "Le mot de passe actuel est obligatoire.";
                    return RedirectToAction("Profil");
                }

                if (string.IsNullOrWhiteSpace(NewPassword))
                {
                    TempData["Error"] = "Le nouveau mot de passe est obligatoire.";
                    return RedirectToAction("Profil");
                }

                if (NewPassword.Length < 6)
                {
                    TempData["Error"] = "Le nouveau mot de passe doit contenir au moins 6 caractères.";
                    return RedirectToAction("Profil");
                }

                if (NewPassword != ConfirmPassword)
                {
                    TempData["Error"] = "Les mots de passe ne correspondent pas.";
                    return RedirectToAction("Profil");
                }

                // Vérifier l'ancien mot de passe (gérer les mots de passe hashés et non hashés)
                bool currentPasswordValid = false;
                
                // D'abord essayer de vérifier si c'est un hash BCrypt
                if (agent.Utilisateur.MotDePasse.StartsWith("$2a$") || agent.Utilisateur.MotDePasse.StartsWith("$2b$"))
                {
                    try
                    {
                        currentPasswordValid = BCrypt.Net.BCrypt.Verify(CurrentPassword, agent.Utilisateur.MotDePasse);
                    }
                    catch
                    {
                        currentPasswordValid = false;
                    }
                }
                else
                {
                    // Si ce n'est pas un hash BCrypt, comparer directement (pour les mots de passe en clair)
                    currentPasswordValid = agent.Utilisateur.MotDePasse == CurrentPassword;
                }

                if (!currentPasswordValid)
                {
                    TempData["Error"] = "Le mot de passe actuel est incorrect.";
                    return RedirectToAction("Profil");
                }

                // Changer le mot de passe avec BCrypt
                agent.Utilisateur.MotDePasse = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                await _context.SaveChangesAsync();

                Console.WriteLine($"=== CHANGEMENT MOT DE PASSE ===");
                Console.WriteLine($"Agent: {agent.Utilisateur.Prenom} {agent.Utilisateur.Nom}");
                Console.WriteLine($"Email: {agent.Utilisateur.Email}");
                Console.WriteLine($"Mot de passe changé avec succès!");

                TempData["Success"] = "Mot de passe changé avec succès !";
                return RedirectToAction("Profil");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERREUR CHANGEMENT MOT DE PASSE ===");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                TempData["Error"] = $"Erreur lors du changement de mot de passe: {ex.Message}";
                return RedirectToAction("Profil");
            }
        }
    }
} 