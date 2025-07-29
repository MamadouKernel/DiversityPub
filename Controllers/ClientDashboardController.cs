using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DiversityPub.Models.enums;

namespace DiversityPub.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientDashboardController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public ClientDashboardController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: ClientDashboard
        public async Task<IActionResult> Index()
        {
            try
            {
                // Récupérer l'utilisateur connecté et son client associé
                var userEmail = User.Identity.Name;
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur?.Client == null)
                {
                    return View("Error", new { Message = "Votre compte client n'est pas correctement configuré. Veuillez contacter l'administrateur." });
                }

                var clientId = utilisateur.Client.Id;

                // Récupérer les données du client
                var dashboardData = new
                {
                    Client = utilisateur.Client,
                                    Campagnes = await _context.Campagnes
                    .Where(c => c.ClientId == clientId)
                    .OrderByDescending(c => c.DateCreation)
                    .ToListAsync(),
                    Activations = await _context.Activations
                        .Include(a => a.Campagne)
                        .Include(a => a.Lieu)
                        .Include(a => a.AgentsTerrain)
                            .ThenInclude(at => at.Utilisateur)
                        .Include(a => a.Responsable)
                            .ThenInclude(r => r.Utilisateur)
                        .Where(a => a.Campagne.ClientId == clientId)
                        .OrderByDescending(a => a.DateCreation)
                        .ToListAsync(),
                    Feedbacks = await _context.Feedbacks
                        .Include(f => f.Campagne)
                        .Where(f => f.Campagne.ClientId == clientId)
                        .OrderByDescending(f => f.DateFeedback)
                        .ToListAsync()
                };

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement du dashboard: {ex.Message}" });
            }
        }

        // GET: ClientDashboard/Campagnes
        public async Task<IActionResult> Campagnes()
        {
            try
            {
                var userEmail = User.Identity.Name;
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur?.Client == null)
                {
                    return View("Error", new { Message = "Votre compte client n'est pas correctement configuré. Veuillez contacter l'administrateur." });
                }

                var campagnes = await _context.Campagnes
                    .Include(c => c.Activations)
                    .Include(c => c.Feedbacks)
                    .Where(c => c.ClientId == utilisateur.Client.Id)
                    .OrderByDescending(c => c.DateCreation)
                    .ToListAsync();



                return View(campagnes);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des campagnes: {ex.Message}" });
            }
        }

        // GET: ClientDashboard/Activations
        public async Task<IActionResult> Activations()
        {
            try
            {
                var userEmail = User.Identity.Name;
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur?.Client == null)
                {
                    return View("Error", new { Message = "Votre compte client n'est pas correctement configuré. Veuillez contacter l'administrateur." });
                }

                Console.WriteLine($"=== DEBUG ACTIVATIONS CLIENT ===");
                Console.WriteLine($"Client ID: {utilisateur.Client.Id}");
                Console.WriteLine($"Client Email: {userEmail}");

                // Vérifier d'abord les campagnes du client
                var campagnesClient = await _context.Campagnes
                    .Where(c => c.ClientId == utilisateur.Client.Id)
                    .ToListAsync();

                Console.WriteLine($"Campagnes du client: {campagnesClient.Count}");
                foreach (var campagne in campagnesClient)
                {
                    Console.WriteLine($"- Campagne: {campagne.Nom} (ID: {campagne.Id}, Statut: {campagne.Statut})");
                }

                // Vérifier toutes les activations liées aux campagnes du client
                var toutesActivations = await _context.Activations
                    .Include(a => a.Campagne)
                    .Where(a => a.Campagne.ClientId == utilisateur.Client.Id)
                    .ToListAsync();

                Console.WriteLine($"Toutes les activations du client: {toutesActivations.Count}");
                foreach (var activation in toutesActivations)
                {
                    Console.WriteLine($"- Activation: {activation.Nom} (Campagne: {activation.Campagne.Nom}, Statut: {activation.Statut}, PreuvesValidees: {activation.PreuvesValidees})");
                }

                var activations = await _context.Activations
                    .Include(a => a.Campagne)
                    .Include(a => a.Lieu)
                    .Include(a => a.AgentsTerrain)
                        .ThenInclude(at => at.Utilisateur)
                    .Include(a => a.Responsable)
                        .ThenInclude(r => r.Utilisateur)
                    .Include(a => a.Medias)
                    .Where(a => a.Campagne.ClientId == utilisateur.Client.Id) // Suppression de la condition PreuvesValidees
                    .OrderByDescending(a => a.DateCreation)
                    .ToListAsync();

                Console.WriteLine($"Activations trouvées avec includes: {activations.Count}");
                foreach (var activation in activations)
                {
                    Console.WriteLine($"- Activation: {activation.Nom} (Statut: {activation.Statut}, PreuvesValidees: {activation.PreuvesValidees})");
                }

                return View(activations);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERREUR ACTIVATIONS CLIENT ===");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return View("Error", new { Message = $"Erreur lors du chargement des activations: {ex.Message}" });
            }
        }

        // GET: ClientDashboard/Feedbacks
        public async Task<IActionResult> Feedbacks()
        {
            try
            {
                var userEmail = User.Identity.Name;
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur?.Client == null)
                {
                    return View("Error", new { Message = "Votre compte client n'est pas correctement configuré. Veuillez contacter l'administrateur." });
                }

                // Récupérer les feedbacks sur les campagnes
                var feedbacksCampagnes = await _context.Feedbacks
                    .Include(f => f.Campagne)
                    .Where(f => f.CampagneId != null && f.Campagne.ClientId == utilisateur.Client.Id)
                    .OrderByDescending(f => f.DateFeedback)
                    .ToListAsync();

                // Récupérer les feedbacks sur les activations
                var feedbacksActivations = await _context.Feedbacks
                    .Include(f => f.Activation)
                        .ThenInclude(a => a.Campagne)
                    .Where(f => f.ActivationId != null && f.Activation.Campagne.ClientId == utilisateur.Client.Id)
                    .OrderByDescending(f => f.DateFeedback)
                    .ToListAsync();

                // Combiner les deux listes
                var allFeedbacks = feedbacksCampagnes.Concat(feedbacksActivations)
                    .OrderByDescending(f => f.DateFeedback)
                    .ToList();

                return View(allFeedbacks);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des feedbacks: {ex.Message}" });
            }
        }

        // GET: ClientDashboard/CreateFeedback
        public async Task<IActionResult> CreateFeedback()
        {
            try
            {
                var userEmail = User.Identity.Name;
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur?.Client == null)
                {
                    return View("Error", new { Message = "Votre compte client n'est pas correctement configuré. Veuillez contacter l'administrateur." });
                }

                Console.WriteLine($"=== DEBUG CREATE FEEDBACK ===");
                Console.WriteLine($"Client ID: {utilisateur.Client.Id}");
                Console.WriteLine($"Client Email: {userEmail}");

                // Récupérer les campagnes terminées du client qui n'ont pas encore de feedback
                var campagnesTerminees = await _context.Campagnes
                    .Where(c => c.ClientId == utilisateur.Client.Id && 
                               c.Statut == DiversityPub.Models.enums.StatutCampagne.Terminee)
                    .ToListAsync();

                Console.WriteLine($"Campagnes terminées trouvées: {campagnesTerminees.Count}");

                // Récupérer les activations terminées du client qui n'ont pas encore de feedback
                var activationsTerminees = await _context.Activations
                    .Include(a => a.Campagne)
                    .Where(a => a.Campagne.ClientId == utilisateur.Client.Id && 
                               a.Statut == DiversityPub.Models.enums.StatutActivation.Terminee)
                    .ToListAsync();

                Console.WriteLine($"Activations terminées trouvées: {activationsTerminees.Count}");

                // Récupérer les campagnes avec feedback existant
                var campagnesAvecFeedback = await _context.Feedbacks
                    .Where(f => f.CampagneId != null && f.Campagne.ClientId == utilisateur.Client.Id)
                    .Select(f => f.CampagneId)
                    .ToListAsync();

                // Récupérer les activations avec feedback existant
                var activationsAvecFeedback = await _context.Feedbacks
                    .Where(f => f.ActivationId != null && f.Activation.Campagne.ClientId == utilisateur.Client.Id)
                    .Select(f => f.ActivationId)
                    .ToListAsync();

                var campagnesDisponibles = campagnesTerminees
                    .Where(c => !campagnesAvecFeedback.Contains(c.Id))
                    .ToList();

                var activationsDisponibles = activationsTerminees
                    .Where(a => !activationsAvecFeedback.Contains(a.Id))
                    .ToList();

                Console.WriteLine($"Campagnes disponibles pour feedback: {campagnesDisponibles.Count}");
                Console.WriteLine($"Activations disponibles pour feedback: {activationsDisponibles.Count}");

                if (!campagnesDisponibles.Any() && !activationsDisponibles.Any())
                {
                    Console.WriteLine("Aucune campagne ou activation disponible pour feedback - redirection vers Feedbacks");
                    return RedirectToAction("Feedbacks");
                }

                ViewBag.Campagnes = campagnesDisponibles;
                ViewBag.Activations = activationsDisponibles;
                return View(new Feedback());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERREUR CREATE FEEDBACK ===");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return View("Error", new { Message = $"Erreur lors du chargement du formulaire de feedback: {ex.Message}" });
            }
        }

        // POST: ClientDashboard/CreateFeedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFeedback([Bind("Note,Commentaire,CampagneId,ActivationId")] Feedback feedback)
        {
            try
            {
                var userEmail = User.Identity.Name;
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur?.Client == null)
                {
                    return View("Error", new { Message = "Votre compte client n'est pas correctement configuré. Veuillez contacter l'administrateur." });
                }

                Console.WriteLine($"=== DEBUG POST CREATE FEEDBACK ===");
                Console.WriteLine($"Client ID: {utilisateur.Client.Id}");
                Console.WriteLine($"Feedback CampagneId: {feedback.CampagneId}");
                Console.WriteLine($"Feedback ActivationId: {feedback.ActivationId}");
                Console.WriteLine($"Feedback Note: {feedback.Note}");
                Console.WriteLine($"Feedback Commentaire: {feedback.Commentaire}");
                Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("=== ERREURS DE VALIDATION ===");
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            Console.WriteLine($"- {error.ErrorMessage}");
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    // Vérifier qu'au moins une campagne ou activation est sélectionnée
                    if (feedback.CampagneId == null && feedback.ActivationId == null)
                    {
                        TempData["Error"] = "❌ Veuillez sélectionner une campagne ou une activation.";
                        return RedirectToAction("CreateFeedback");
                    }

                    // Vérifier qu'une seule option est sélectionnée
                    if (feedback.CampagneId != null && feedback.ActivationId != null)
                    {
                        TempData["Error"] = "❌ Veuillez sélectionner soit une campagne, soit une activation, pas les deux.";
                        return RedirectToAction("CreateFeedback");
                    }

                    if (feedback.CampagneId != null)
                    {
                        // Feedback sur campagne
                        var campagne = await _context.Campagnes
                            .FirstOrDefaultAsync(c => c.Id == feedback.CampagneId && c.ClientId == utilisateur.Client.Id);

                        Console.WriteLine($"Campagne trouvée: {(campagne != null ? campagne.Nom : "Non trouvée")}");

                        if (campagne == null)
                        {
                            return View("Error", new { Message = "Campagne non trouvée ou non autorisée." });
                        }

                        // Vérifier qu'il n'y a pas déjà un feedback pour cette campagne
                        var feedbackExistant = await _context.Feedbacks
                            .FirstOrDefaultAsync(f => f.CampagneId == feedback.CampagneId);

                        Console.WriteLine($"Feedback existant pour campagne: {(feedbackExistant != null ? "Oui" : "Non")}");

                        if (feedbackExistant != null)
                        {
                            TempData["Error"] = "❌ Un feedback existe déjà pour cette campagne.";
                            return RedirectToAction("Feedbacks");
                        }
                    }
                    else if (feedback.ActivationId != null)
                    {
                        // Feedback sur activation
                        var activation = await _context.Activations
                            .Include(a => a.Campagne)
                            .FirstOrDefaultAsync(a => a.Id == feedback.ActivationId && a.Campagne.ClientId == utilisateur.Client.Id);

                        Console.WriteLine($"Activation trouvée: {(activation != null ? activation.Nom : "Non trouvée")}");

                        if (activation == null)
                        {
                            return View("Error", new { Message = "Activation non trouvée ou non autorisée." });
                        }

                        // Vérifier qu'il n'y a pas déjà un feedback pour cette activation
                        var feedbackExistant = await _context.Feedbacks
                            .FirstOrDefaultAsync(f => f.ActivationId == feedback.ActivationId);

                        Console.WriteLine($"Feedback existant pour activation: {(feedbackExistant != null ? "Oui" : "Non")}");

                        if (feedbackExistant != null)
                        {
                            TempData["Error"] = "❌ Un feedback existe déjà pour cette activation.";
                            return RedirectToAction("Feedbacks");
                        }
                    }

                    feedback.Id = Guid.NewGuid();
                    feedback.DateFeedback = DateTime.Now;
                    
                    Console.WriteLine($"=== CRÉATION FEEDBACK ===");
                    Console.WriteLine($"ID: {feedback.Id}");
                    Console.WriteLine($"DateFeedback: {feedback.DateFeedback}");
                    
                    _context.Add(feedback);
                    await _context.SaveChangesAsync();

                    Console.WriteLine("Feedback sauvegardé avec succès!");

                    TempData["Success"] = "✅ Feedback créé avec succès !";
                    return RedirectToAction("Feedbacks");
                }

                // Si le modèle n'est pas valide, recharger les données pour la vue
                var campagnesTerminees = await _context.Campagnes
                    .Where(c => c.ClientId == utilisateur.Client.Id && 
                               c.Statut == DiversityPub.Models.enums.StatutCampagne.Terminee)
                    .ToListAsync();

                var activationsTerminees = await _context.Activations
                    .Include(a => a.Campagne)
                    .Where(a => a.Campagne.ClientId == utilisateur.Client.Id && 
                               a.Statut == DiversityPub.Models.enums.StatutActivation.Terminee)
                    .ToListAsync();

                var campagnesAvecFeedback = await _context.Feedbacks
                    .Where(f => f.CampagneId != null && f.Campagne.ClientId == utilisateur.Client.Id)
                    .Select(f => f.CampagneId)
                    .ToListAsync();

                var activationsAvecFeedback = await _context.Feedbacks
                    .Where(f => f.ActivationId != null && f.Activation.Campagne.ClientId == utilisateur.Client.Id)
                    .Select(f => f.ActivationId)
                    .ToListAsync();

                var campagnesDisponibles = campagnesTerminees
                    .Where(c => !campagnesAvecFeedback.Contains(c.Id))
                    .ToList();

                var activationsDisponibles = activationsTerminees
                    .Where(a => !activationsAvecFeedback.Contains(a.Id))
                    .ToList();

                ViewBag.Campagnes = campagnesDisponibles;
                ViewBag.Activations = activationsDisponibles;
                return View(feedback);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERREUR POST CREATE FEEDBACK ===");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return View("Error", new { Message = $"Erreur lors de la création du feedback: {ex.Message}" });
            }
        }

        // Action pour créer des données de test pour le client
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTestData()
        {
            try
            {
                var userEmail = User.Identity.Name;
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur?.Client == null)
                {
                    return Json(new { success = false, message = "Client non trouvé" });
                }

                // Créer un lieu de test
                var lieu = new Lieu
                {
                    Id = Guid.NewGuid(),
                    Nom = "Centre Commercial Abidjan",
                    Adresse = "123 Boulevard de la République, Abidjan",
                };

                // Créer une campagne de test
                var campagne = new Campagne
                {
                    Id = Guid.NewGuid(),
                    Nom = "Campagne Marketing Test",
                    Description = "Campagne de test pour démonstration",
                    DateDebut = DateTime.Today,
                    DateFin = DateTime.Today.AddDays(30),
                    Objectifs = "Tester l'interface client",
                    ClientId = utilisateur.Client.Id,
                    Statut = StatutCampagne.EnCours,
                    DateCreation = DateTime.Now
                };

                // Créer des agents de test
                var agent1 = new AgentTerrain
                {
                    Id = Guid.NewGuid(),
                    UtilisateurId = Guid.NewGuid(),
                    Telephone = "+225 0123456789",
                    Email = "agent1@test.ci",
                    EstConnecte = false
                };

                var agent2 = new AgentTerrain
                {
                    Id = Guid.NewGuid(),
                    UtilisateurId = Guid.NewGuid(),
                    Telephone = "+225 0987654321",
                    Email = "agent2@test.ci",
                    EstConnecte = false
                };

                // Créer les utilisateurs pour les agents
                var utilisateurAgent1 = new Utilisateur
                {
                    Id = agent1.UtilisateurId,
                    Nom = "Agent",
                    Prenom = "Test 1",
                    Email = agent1.Email,
                    MotDePasse = BCrypt.Net.BCrypt.HashPassword("Agent123!"),
                    Role = Role.AgentTerrain
                };

                var utilisateurAgent2 = new Utilisateur
                {
                    Id = agent2.UtilisateurId,
                    Nom = "Agent",
                    Prenom = "Test 2",
                    Email = agent2.Email,
                    MotDePasse = BCrypt.Net.BCrypt.HashPassword("Agent123!"),
                    Role = Role.AgentTerrain
                };

                // Créer des activations de test
                var activation1 = new Activation
                {
                    Id = Guid.NewGuid(),
                    Nom = "Activation Marketing Centre Commercial",
                    Description = "Distribution de flyers et promotion des produits",
                    Instructions = "Distribuer les flyers aux clients et collecter les retours",
                    DateActivation = DateTime.Today.AddDays(2),
                    HeureDebut = new TimeSpan(9, 0, 0),
                    HeureFin = new TimeSpan(17, 0, 0),
                    Statut = StatutActivation.Planifiee,
                    CampagneId = campagne.Id,
                    LieuId = lieu.Id,
                    ResponsableId = agent1.Id,
                    DateCreation = DateTime.Now
                };

                var activation2 = new Activation
                {
                    Id = Guid.NewGuid(),
                    Nom = "Activation Événementiel Place Félix Houphouët",
                    Description = "Organisation d'un événement promotionnel",
                    Instructions = "Installer les stands et animer l'événement",
                    DateActivation = DateTime.Today.AddDays(5),
                    HeureDebut = new TimeSpan(10, 0, 0),
                    HeureFin = new TimeSpan(18, 0, 0),
                    Statut = StatutActivation.EnCours,
                    CampagneId = campagne.Id,
                    LieuId = lieu.Id,
                    ResponsableId = agent2.Id,
                    DateCreation = DateTime.Now
                };

                var activation3 = new Activation
                {
                    Id = Guid.NewGuid(),
                    Nom = "Activation Digital Zone 4",
                    Description = "Promotion digitale et collecte de données",
                    Instructions = "Utiliser les tablettes pour collecter les données clients",
                    DateActivation = DateTime.Today.AddDays(-1),
                    HeureDebut = new TimeSpan(8, 0, 0),
                    HeureFin = new TimeSpan(16, 0, 0),
                    Statut = StatutActivation.Terminee,
                    CampagneId = campagne.Id,
                    LieuId = lieu.Id,
                    ResponsableId = agent1.Id,
                    DateCreation = DateTime.Now
                };

                // Ajouter les agents aux activations
                activation1.AgentsTerrain.Add(agent1);
                activation1.AgentsTerrain.Add(agent2);
                activation2.AgentsTerrain.Add(agent1);
                activation2.AgentsTerrain.Add(agent2);
                activation3.AgentsTerrain.Add(agent1);

                // Sauvegarder dans la base de données
                _context.Add(lieu);
                _context.Add(utilisateurAgent1);
                _context.Add(utilisateurAgent2);
                _context.Add(agent1);
                _context.Add(agent2);
                _context.Add(campagne);
                _context.Add(activation1);
                _context.Add(activation2);
                _context.Add(activation3);

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "✅ Données de test créées avec succès ! Vous pouvez maintenant voir vos activations.",
                    activationsCreated = 3,
                    campagneCreated = campagne.Nom
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"❌ Erreur lors de la création des données de test: {ex.Message}" 
                });
            }
        }

        // GET: ClientDashboard/DetailsCampagne
        public async Task<IActionResult> DetailsCampagne(Guid? id)
        {
            try
            {
                var userEmail = User.Identity.Name;
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (utilisateur?.Client == null)
                {
                    return View("Error", new { Message = "Votre compte client n'est pas correctement configuré. Veuillez contacter l'administrateur." });
                }

                if (id == null)
                {
                    return View("Error", new { Message = "ID de campagne non fourni." });
                }

                var campagne = await _context.Campagnes
                    .Include(c => c.Activations)
                        .ThenInclude(a => a.Lieu)
                    .Include(c => c.Activations)
                        .ThenInclude(a => a.AgentsTerrain)
                            .ThenInclude(at => at.Utilisateur)
                    .Include(c => c.Activations)
                        .ThenInclude(a => a.Responsable)
                            .ThenInclude(r => r.Utilisateur)
                    .Include(c => c.Feedbacks)
                    .FirstOrDefaultAsync(c => c.Id == id && c.ClientId == utilisateur.Client.Id);

                if (campagne == null)
                {
                    return View("Error", new { Message = "Campagne non trouvée ou non autorisée." });
                }

                return View(campagne);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des détails de la campagne: {ex.Message}" });
            }
        }
    }
} 