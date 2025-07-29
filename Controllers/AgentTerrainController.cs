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
    [Authorize(Roles = "AgentTerrain")]
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

                var activation = await _context.Activations
                    .Include(a => a.Campagne)
                    .Include(a => a.Lieu)
                    .Include(a => a.AgentsTerrain)
                        .ThenInclude(at => at.Utilisateur)
                    .Include(a => a.Medias)
                    .Include(a => a.Incidents)
                    .FirstOrDefaultAsync(a => a.Id == id && a.AgentsTerrain.Any(at => at.Id == agent.Id));

                if (activation == null)
                    return NotFound();

                ViewBag.Agent = agent;
                return View(activation);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des détails: {ex.Message}" });
            }
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
                    return Json(new { success = false, message = "Vous n'êtes pas autorisé à terminer cette activation." });
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
                    return Json(new { success = false, message = "Vous n'êtes pas autorisé à suspendre cette activation." });
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
                    return Json(new { success = false, message = "Vous n'êtes pas autorisé à reprendre cette activation." });
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

                if (agent == null)
                {
                    TempData["Error"] = "Agent terrain non trouvé.";
                    return RedirectToAction("Index");
                }

                if (ModelState.IsValid)
                {
                    incident.Id = Guid.NewGuid();
                    incident.AgentTerrainId = agent.Id;
                    incident.DateCreation = DateTime.Now;
                    incident.Statut = "Ouvert";

                    _context.Incidents.Add(incident);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Incident signalé avec succès !";
                    return RedirectToAction("Incidents");
                }

                // En cas d'erreur de validation, recharger les données
                var activations = await _context.Activations
                    .Include(a => a.Campagne)
                    .Include(a => a.Lieu)
                    .Where(a => a.AgentsTerrain.Any(at => at.Id == agent.Id))
                    .OrderByDescending(a => a.DateActivation)
                    .ToListAsync();

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
        public async Task<IActionResult> EnvoyerPreuve(Media media, IFormFile? fichier)
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
                Console.WriteLine($"Media Description: {media?.Description}");
                Console.WriteLine($"Media Type: {media?.Type}");
                Console.WriteLine($"Media ActivationId: {media?.ActivationId}");
                Console.WriteLine($"Fichier reçu: {(fichier != null ? fichier.FileName : "Aucun fichier")}");
                Console.WriteLine($"Fichier taille: {(fichier != null ? fichier.Length : 0)} bytes");
                Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
                
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("=== ERREURS DE VALIDATION ===");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine($"- {error.ErrorMessage}");
                    }
                }

                if (ModelState.IsValid)
                {
                    media.Id = Guid.NewGuid();
                    media.AgentTerrainId = agent.Id;
                    media.DateUpload = DateTime.Now;

                    Console.WriteLine($"=== CRÉATION MÉDIA ===");
                    Console.WriteLine($"ID: {media.Id}");
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
                        ViewBag.ActivationId = media.ActivationId;

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
                ViewBag.ActivationId = media.ActivationId;

                return View(media);
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

                // Vérifier l'ancien mot de passe
                if (agent.Utilisateur.MotDePasse != CurrentPassword)
                {
                    TempData["Error"] = "Le mot de passe actuel est incorrect.";
                    return RedirectToAction("Profil");
                }

                // Changer le mot de passe
                agent.Utilisateur.MotDePasse = NewPassword;
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