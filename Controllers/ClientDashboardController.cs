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
                            .ThenInclude(c => c.Client)
                        .Include(f => f.Activation)
                            .ThenInclude(a => a.Campagne)
                                .ThenInclude(c => c.Client)
                        .Where(f => (f.CampagneId != null && f.Campagne.ClientId == clientId) || 
                                   (f.ActivationId != null && f.Activation.Campagne.ClientId == clientId))
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

                // Mettre à jour automatiquement le statut des campagnes selon leurs activations
                foreach (var campagne in campagnes)
                {
                    await UpdateCampaignStatus(campagne);
                }

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

                // Récupérer les feedbacks sur les campagnes avec les données de réponse
                var feedbacksCampagnes = await _context.Feedbacks
                    .Include(f => f.Campagne)
                        .ThenInclude(c => c.Client)
                    .Where(f => f.CampagneId != null && f.Campagne.ClientId == utilisateur.Client.Id)
                    .OrderByDescending(f => f.DateFeedback)
                    .ToListAsync();

                // Récupérer les feedbacks sur les activations avec les données de réponse
                var feedbacksActivations = await _context.Feedbacks
                    .Include(f => f.Activation)
                        .ThenInclude(a => a.Campagne)
                            .ThenInclude(c => c.Client)
                    .Where(f => f.ActivationId != null && f.Activation.Campagne.ClientId == utilisateur.Client.Id)
                    .OrderByDescending(f => f.DateFeedback)
                    .ToListAsync();

                // Combiner les deux listes et prendre seulement les 5 derniers
                var allFeedbacks = feedbacksCampagnes.Concat(feedbacksActivations)
                    .OrderByDescending(f => f.DateFeedback)
                    .Take(5)
                    .ToList();

                // Passer le nombre total de feedbacks à la vue
                var totalFeedbacks = feedbacksCampagnes.Concat(feedbacksActivations).Count();
                ViewBag.TotalFeedbacks = totalFeedbacks;
                ViewBag.ShowMoreButton = totalFeedbacks > 5;

                return View(allFeedbacks);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des feedbacks: {ex.Message}" });
            }
        }

        // GET: ClientDashboard/FeedbacksAll
        public async Task<IActionResult> FeedbacksAll()
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

                // Récupérer les feedbacks sur les campagnes avec les données de réponse
                var feedbacksCampagnes = await _context.Feedbacks
                    .Include(f => f.Campagne)
                        .ThenInclude(c => c.Client)
                    .Where(f => f.CampagneId != null && f.Campagne.ClientId == utilisateur.Client.Id)
                    .OrderByDescending(f => f.DateFeedback)
                    .ToListAsync();

                // Récupérer les feedbacks sur les activations avec les données de réponse
                var feedbacksActivations = await _context.Feedbacks
                    .Include(f => f.Activation)
                        .ThenInclude(a => a.Campagne)
                            .ThenInclude(c => c.Client)
                    .Where(f => f.ActivationId != null && f.Activation.Campagne.ClientId == utilisateur.Client.Id)
                    .OrderByDescending(f => f.DateFeedback)
                    .ToListAsync();

                // Combiner les deux listes
                var allFeedbacks = feedbacksCampagnes.Concat(feedbacksActivations)
                    .OrderByDescending(f => f.DateFeedback)
                    .ToList();

                return View("Feedbacks", allFeedbacks);
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
                    Console.WriteLine("=== ERREUR: Client non trouvé ===");
                    return View("Error", new { Message = "Votre compte client n'est pas correctement configuré. Veuillez contacter l'administrateur." });
                }

                Console.WriteLine($"=== DEBUG CREATE FEEDBACK ===");
                Console.WriteLine($"Client ID: {utilisateur.Client.Id}");
                Console.WriteLine($"Client Email: {userEmail}");

                // Récupérer TOUTES les campagnes du client (pas seulement terminées)
                var toutesLesCampagnes = await _context.Campagnes
                    .Include(c => c.Client)
                    .Where(c => c.ClientId == utilisateur.Client.Id)
                    .ToListAsync();

                Console.WriteLine($"Toutes les campagnes trouvées: {toutesLesCampagnes.Count}");
                foreach (var c in toutesLesCampagnes)
                {
                    Console.WriteLine($"  - {c.Nom} (ID: {c.Id}) - Statut: {c.Statut}");
                }

                // Récupérer TOUTES les activations du client avec leurs campagnes
                var toutesLesActivations = await _context.Activations
                    .Include(a => a.Campagne)
                        .ThenInclude(c => c.Client)
                    .Include(a => a.Lieu)
                    .Where(a => a.Campagne.ClientId == utilisateur.Client.Id)
                    .ToListAsync();

                Console.WriteLine($"Toutes les activations trouvées: {toutesLesActivations.Count}");
                foreach (var a in toutesLesActivations)
                {
                    Console.WriteLine($"  - {a.Nom} (ID: {a.Id}) - Statut: {a.Statut} - Campagne: {a.Campagne?.Nom}");
                }

                // Permettre plusieurs feedbacks - montrer toutes les campagnes et activations
                var campagnesDisponibles = toutesLesCampagnes.ToList();
                var activationsDisponibles = toutesLesActivations.ToList();

                Console.WriteLine($"Campagnes disponibles pour feedback: {campagnesDisponibles.Count}");
                Console.WriteLine($"Activations disponibles pour feedback: {activationsDisponibles.Count}");

                if (!campagnesDisponibles.Any() && !activationsDisponibles.Any())
                {
                    Console.WriteLine("Aucune campagne ou activation disponible pour feedback - affichage du formulaire vide avec message");
                    TempData["Info"] = "Aucune campagne ou activation disponible pour feedback. Utilisez le bouton 'Créer Données Test' pour créer des données de test.";
                    ViewBag.Campagnes = new List<Campagne>();
                    ViewBag.Activations = new List<Activation>();
                    ViewBag.ActivationsJson = "[]";
                    return View(new Feedback());
                }

                ViewBag.Campagnes = campagnesDisponibles;
                ViewBag.Activations = activationsDisponibles;
                
                Console.WriteLine($"=== DEBUG VIEWBAG ===");
                Console.WriteLine($"ViewBag.Campagnes count: {campagnesDisponibles.Count}");
                Console.WriteLine($"ViewBag.Activations count: {activationsDisponibles.Count}");
                
                if (campagnesDisponibles.Any())
                {
                    var firstCampagne = campagnesDisponibles.First();
                    Console.WriteLine($"Première campagne - Nom: {firstCampagne.Nom}, ID: {firstCampagne.Id}, ClientId: {firstCampagne.ClientId}");
                }
                
                if (activationsDisponibles.Any())
                {
                    var firstActivation = activationsDisponibles.First();
                    Console.WriteLine($"Première activation - Nom: {firstActivation.Nom}, ID: {firstActivation.Id}, CampagneId: {firstActivation.CampagneId}");
                }
                
                // Préparer les données JSON pour JavaScript
                var activationsJson = activationsDisponibles.Select(a => new
                {
                    id = a.Id.ToString(),
                    nom = a.Nom,
                    campagneId = a.CampagneId.ToString(),
                    dateActivation = a.DateActivation.ToString("dd/MM/yyyy"),
                    statut = a.Statut.ToString()
                }).ToList();
                
                Console.WriteLine($"=== DEBUG JSON ACTIVATIONS ===");
                Console.WriteLine($"Activations JSON créé: {System.Text.Json.JsonSerializer.Serialize(activationsJson)}");
                
                ViewBag.ActivationsJson = System.Text.Json.JsonSerializer.Serialize(activationsJson);
                
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

                        // Permettre plusieurs feedbacks pour une même campagne
                        Console.WriteLine($"Feedback existant pour campagne: Permis (plusieurs feedbacks autorisés)");
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

                        // Permettre plusieurs feedbacks pour une même activation
                        Console.WriteLine($"Feedback existant pour activation: Permis (plusieurs feedbacks autorisés)");
                    }

                    feedback.Id = Guid.NewGuid();
                    feedback.DateFeedback = DateTime.Now;
                    
                    Console.WriteLine($"=== CRÉATION FEEDBACK ===");
                    Console.WriteLine($"ID: {feedback.Id}");
                    Console.WriteLine($"DateFeedback: {feedback.DateFeedback}");
                    
                    _context.Add(feedback);
                    await _context.SaveChangesAsync();

                    Console.WriteLine("Feedback sauvegardé avec succès!");

                    TempData["Success"] = "✅ Feedback créé avec succès ! Vous pouvez créer plusieurs feedbacks pour la même campagne ou activation.";
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

                // Permettre plusieurs feedbacks - montrer toutes les campagnes et activations
                var campagnesDisponibles = campagnesTerminees.ToList();
                var activationsDisponibles = activationsTerminees.ToList();

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

        // Action pour créer des feedbacks groupés
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroupedFeedback([FromBody] GroupedFeedbackRequest request)
        {
            try
            {
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

                if (utilisateur?.Client == null)
                {
                    return Json(new { success = false, message = "Utilisateur non autorisé." });
                }

                if (request.Feedbacks == null || !request.Feedbacks.Any())
                {
                    return Json(new { success = false, message = "Aucun feedback à créer." });
                }

                var createdFeedbacks = new List<object>();

                foreach (var feedbackData in request.Feedbacks)
                {
                    var feedback = new Feedback
                    {
                        Id = Guid.NewGuid(),
                        Note = feedbackData.Note,
                        Commentaire = feedbackData.Commentaire,
                        DateFeedback = DateTime.Now,
                        CampagneId = feedbackData.CampagneId,
                        ActivationId = feedbackData.ActivationId
                    };

                    // Validation de sécurité
                    if (feedback.CampagneId.HasValue)
                    {
                        var campagne = await _context.Campagnes
                            .FirstOrDefaultAsync(c => c.Id == feedback.CampagneId && c.ClientId == utilisateur.Client.Id);
                        
                        if (campagne == null)
                        {
                            return Json(new { success = false, message = $"Campagne non autorisée: {feedback.CampagneId}" });
                        }
                    }

                    if (feedback.ActivationId.HasValue)
                    {
                        var activation = await _context.Activations
                            .Include(a => a.Campagne)
                            .FirstOrDefaultAsync(a => a.Id == feedback.ActivationId && a.Campagne.ClientId == utilisateur.Client.Id);
                        
                        if (activation == null)
                        {
                            return Json(new { success = false, message = $"Activation non autorisée: {feedback.ActivationId}" });
                        }
                    }

                    _context.Add(feedback);
                    createdFeedbacks.Add(new { 
                        id = feedback.Id, 
                        type = feedback.CampagneId.HasValue ? "campagne" : "activation",
                        targetId = feedback.CampagneId ?? feedback.ActivationId
                    });
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"✅ {createdFeedbacks.Count} feedback(s) créé(s) avec succès !",
                    feedbacks = createdFeedbacks
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la création des feedbacks: {ex.Message}" });
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

                // Créer une campagne TERMINÉE de test
                var campagneTerminee = new Campagne
                {
                    Id = Guid.NewGuid(),
                    Nom = "Campagne Marketing Terminée",
                    Description = "Campagne de test terminée pour démonstration des feedbacks",
                    DateDebut = DateTime.Today.AddDays(-30),
                    DateFin = DateTime.Today.AddDays(-1),
                    Objectifs = "Tester l'interface client avec feedbacks",
                    ClientId = utilisateur.Client.Id,
                    Statut = StatutCampagne.Terminee,
                    DateCreation = DateTime.Now.AddDays(-35)
                };

                // Créer une campagne EN COURS de test
                var campagneEnCours = new Campagne
                {
                    Id = Guid.NewGuid(),
                    Nom = "Campagne Marketing En Cours",
                    Description = "Campagne de test en cours pour démonstration",
                    DateDebut = DateTime.Today.AddDays(-15),
                    DateFin = DateTime.Today.AddDays(15),
                    Objectifs = "Tester l'interface client",
                    ClientId = utilisateur.Client.Id,
                    Statut = StatutCampagne.EnCours,
                    DateCreation = DateTime.Now.AddDays(-20)
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

                // Créer une activation TERMINÉE de test
                var activationTerminee = new Activation
                {
                    Id = Guid.NewGuid(),
                    Nom = "Activation Marketing Terminée",
                    Description = "Distribution de flyers et promotion des produits - TERMINÉE",
                    Instructions = "Distribuer les flyers aux clients et collecter les retours",
                    DateActivation = DateTime.Today.AddDays(-5),
                    HeureDebut = new TimeSpan(9, 0, 0),
                    HeureFin = new TimeSpan(17, 0, 0),
                    Statut = StatutActivation.Terminee,
                    CampagneId = campagneTerminee.Id,
                    LieuId = lieu.Id,
                    ResponsableId = agent1.Id,
                    DateCreation = DateTime.Now.AddDays(-10)
                };

                // Créer une activation EN COURS de test
                var activationEnCours = new Activation
                {
                    Id = Guid.NewGuid(),
                    Nom = "Activation Événementiel En Cours",
                    Description = "Organisation d'un événement promotionnel - EN COURS",
                    Instructions = "Organiser l'événement et gérer les participants",
                    DateActivation = DateTime.Today.AddDays(2),
                    HeureDebut = new TimeSpan(10, 0, 0),
                    HeureFin = new TimeSpan(18, 0, 0),
                    Statut = StatutActivation.EnCours,
                    CampagneId = campagneEnCours.Id,
                    LieuId = lieu.Id,
                    ResponsableId = agent2.Id,
                    DateCreation = DateTime.Now.AddDays(-5)
                };

                // Sauvegarder les données
                _context.Lieux.Add(lieu);
                _context.Campagnes.Add(campagneTerminee);
                _context.Campagnes.Add(campagneEnCours);
                _context.Utilisateurs.Add(utilisateurAgent1);
                _context.Utilisateurs.Add(utilisateurAgent2);
                _context.AgentsTerrain.Add(agent1);
                _context.AgentsTerrain.Add(agent2);
                _context.Activations.Add(activationTerminee);
                _context.Activations.Add(activationEnCours);

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Données de test créées avec succès ! Campagnes et activations terminées disponibles pour les feedbacks." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la création des données de test: {ex.Message}" });
            }
        }

        // GET: ClientDashboard/DetailsActivation
        public async Task<IActionResult> DetailsActivation(Guid? id)
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
                    return View("Error", new { Message = "ID d'activation non fourni." });
                }

                var activation = await _context.Activations
                    .Include(a => a.Campagne)
                        .ThenInclude(c => c.Client)
                    .Include(a => a.Lieu)
                    .Include(a => a.Responsable)
                        .ThenInclude(r => r.Utilisateur)
                    .Include(a => a.AgentsTerrain)
                        .ThenInclude(at => at.Utilisateur)
                    .Include(a => a.Medias.Where(m => m.Valide)) // Filtrer uniquement les médias validés
                    .Include(a => a.Incidents)
                    .Include(a => a.Feedbacks)
                    .FirstOrDefaultAsync(a => a.Id == id && a.Campagne.ClientId == utilisateur.Client.Id);

                if (activation == null)
                {
                    return View("Error", new { Message = "Activation non trouvée ou non autorisée." });
                }

                return View(activation);
            }
            catch (Exception ex)
            {
                return View("Error", new { Message = $"Erreur lors du chargement des détails de l'activation: {ex.Message}" });
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

        // Méthode privée pour mettre à jour automatiquement le statut d'une campagne
        private async Task UpdateCampaignStatus(Campagne campagne)
        {
            try
            {
                if (campagne.Activations == null || !campagne.Activations.Any())
                {
                    // Pas d'activations : rester en préparation
                    if (campagne.Statut != StatutCampagne.EnPreparation && campagne.Statut != StatutCampagne.Annulee)
                    {
                        campagne.Statut = StatutCampagne.EnPreparation;
                        _context.Update(campagne);
                        await _context.SaveChangesAsync();
                    }
                    return;
                }

                // Vérifier si la campagne est passée (toutes les activations terminées et après DateFin)
                var aujourdhui = DateTime.Today;
                var toutesActivationsTerminees = campagne.Activations.All(a => a.Statut == StatutActivation.Terminee);
                var campagnePassee = aujourdhui > campagne.DateFin;

                // Vérifier s'il y a des activations en cours
                var activationsEnCours = campagne.Activations.Any(a => a.Statut == StatutActivation.EnCours);

                // Vérifier s'il y a des activations planifiées pour aujourd'hui ou dans le futur
                var activationsPlanifiees = campagne.Activations.Any(a => 
                    a.Statut == StatutActivation.Planifiee && a.DateActivation >= aujourdhui);

                StatutCampagne nouveauStatut;

                if (campagne.Statut == StatutCampagne.Annulee)
                {
                    // Ne pas changer le statut des campagnes annulées
                    return;
                }
                else if (activationsEnCours)
                {
                    // Au moins une activation en cours → Campagne en cours
                    nouveauStatut = StatutCampagne.EnCours;
                }
                else if (toutesActivationsTerminees && campagnePassee)
                {
                    // Toutes les activations terminées et campagne passée → Terminée
                    nouveauStatut = StatutCampagne.Terminee;
                }
                else if (activationsPlanifiees || !campagnePassee)
                {
                    // Activations planifiées ou campagne pas encore finie → En cours ou en préparation
                    var campagneCommencee = aujourdhui >= campagne.DateDebut;
                    nouveauStatut = campagneCommencee ? StatutCampagne.EnCours : StatutCampagne.EnPreparation;
                }
                else
                {
                    // Cas par défaut : toutes les activations terminées mais dans la période
                    nouveauStatut = StatutCampagne.Terminee;
                }

                // Mettre à jour uniquement si le statut a changé
                if (campagne.Statut != nouveauStatut)
                {
                    var ancienStatut = campagne.Statut;
                    campagne.Statut = nouveauStatut;
                    _context.Update(campagne);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"=== MISE À JOUR STATUT CAMPAGNE ===");
                    Console.WriteLine($"Campagne: {campagne.Nom}");
                    Console.WriteLine($"Ancien statut: {ancienStatut} → Nouveau statut: {nouveauStatut}");
                    Console.WriteLine($"Activations en cours: {activationsEnCours}");
                    Console.WriteLine($"Activations planifiées: {activationsPlanifiees}");
                    Console.WriteLine($"Toutes terminées: {toutesActivationsTerminees}");
                    Console.WriteLine($"Campagne passée: {campagnePassee}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise à jour du statut de la campagne {campagne.Nom}: {ex.Message}");
            }
        }


    }
} 