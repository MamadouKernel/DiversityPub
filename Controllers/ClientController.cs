using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using Microsoft.AspNetCore.Authorization;

namespace DiversityPub.Controllers
{
    [Authorize(Roles = "Admin,ChefProjet")]
    public class ClientController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public ClientController(DiversityPubDbContext context)
        {
            _context = context;
        }

        //// GET: Client
        //public async Task<IActionResult> Index()
        //{
        //    var clients = await _context.Clients
        //        .Include(c => c.Utilisateur)
        //        .Include(c => c.)
        //        .OrderBy(c => c.RaisonSociale)
        //        .ToListAsync();

        //    return View(clients);
        //}

        // GET: Client
        public async Task<IActionResult> Index()
        {
            try
            {
                            var clients = await _context.Clients
                .Include(c => c.Utilisateur)
                .Include(c => c.Campagnes)
                .OrderByDescending(c => c.DateCreation)
                .ToListAsync();

                // Dictionnaire : clé = ID client, valeur = nombre de campagnes
                var nombreCampagnesParClient = clients.ToDictionary(
                    c => c.Id,
                    c => c.Campagnes?.Count ?? 0
                );

                // On le passe à la vue
                ViewBag.NombreCampagnes = nombreCampagnesParClient;



                return View(clients);
            }
            catch (Exception ex)
            {
                return View(new List<Client>());
            }
        }


        // GET: Client/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var client = await _context.Clients
                .Include(c => c.Utilisateur)
                .Include(c => c.Campagnes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
                return NotFound();

            return View(client);
        }

        // GET: Client/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Client/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RaisonSociale,Adresse,RegistreCommerce,NomDirigeant,NomContactPrincipal,TelephoneContactPrincipal,EmailContactPrincipal")] Client client)
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
                    client.Id = Guid.NewGuid();
                    client.DateCreation = DateTime.Now;
                    
                    // Créer un utilisateur associé au client
                    var utilisateur = new Utilisateur
                    {
                        Id = Guid.NewGuid(),
                        Nom = client.NomDirigeant,
                        Prenom = "",
                        Email = client.EmailContactPrincipal,
                        MotDePasse = BCrypt.Net.BCrypt.HashPassword("Client123!"), // Mot de passe par défaut
                        Role = Models.enums.Role.Client
                    };

                    client.UtilisateurId = utilisateur.Id;
                    client.Utilisateur = utilisateur;

                    _context.Add(utilisateur);
                    _context.Add(client);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"✅ Client '{client.RaisonSociale}' créé avec succès ! Un compte utilisateur a été créé avec l'email '{client.EmailContactPrincipal}' et le mot de passe par défaut 'Client123!'.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"❌ Erreur lors de la création du client: {ex.Message}";
                }
            }
            
            return View(client);
        }

        // GET: Client/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var client = await _context.Clients
                .Include(c => c.Utilisateur)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
                return NotFound();

            return View(client);
        }

        // POST: Client/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,RaisonSociale,Adresse,RegistreCommerce,NomDirigeant,NomContactPrincipal,TelephoneContactPrincipal,EmailContactPrincipal,UtilisateurId")] Client client)
        {
            if (id != client.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingClient = await _context.Clients
                        .Include(c => c.Utilisateur)
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (existingClient == null)
                        return NotFound();

                    // Mettre à jour les propriétés du client
                    existingClient.RaisonSociale = client.RaisonSociale;
                    existingClient.Adresse = client.Adresse;
                    existingClient.RegistreCommerce = client.RegistreCommerce;
                    existingClient.NomDirigeant = client.NomDirigeant;
                    existingClient.NomContactPrincipal = client.NomContactPrincipal;
                    existingClient.TelephoneContactPrincipal = client.TelephoneContactPrincipal;
                    existingClient.EmailContactPrincipal = client.EmailContactPrincipal;

                    // Mettre à jour l'utilisateur associé
                    if (existingClient.Utilisateur != null)
                    {
                        existingClient.Utilisateur.Nom = client.NomDirigeant;
                        existingClient.Utilisateur.Email = client.EmailContactPrincipal;
                    }

                    _context.Update(existingClient);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Client mis à jour avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            
            return View(client);
        }

        // GET: Client/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var client = await _context.Clients
                .Include(c => c.Utilisateur)
                .Include(c => c.Campagnes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
                return NotFound();

            return View(client);
        }

        // POST: Client/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var client = await _context.Clients
                .Include(c => c.Utilisateur)
                .Include(c => c.Campagnes)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            if (client != null)
            {
                if (client.Campagnes.Any())
                {
                    TempData["Error"] = "Impossible de supprimer ce client car il a des campagnes associées.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Clients.Remove(client);
                if (client.Utilisateur != null)
                {
                    _context.Utilisateurs.Remove(client.Utilisateur);
                }
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Client supprimé avec succès.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(Guid id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }

        // Action pour créer un client de test
        public async Task<IActionResult> CreateTestClient()
        {
            try
            {
                var testClient = new Client
                {
                    Id = Guid.NewGuid(),
                    RaisonSociale = "Entreprise Test SARL",
                    Adresse = "123 Rue Test, Abidjan",
                    RegistreCommerce = "CI-ABJ-2025-001",
                    NomDirigeant = "Test Dirigeant",
                    NomContactPrincipal = "Test Contact",
                    TelephoneContactPrincipal = "+225 0123456789",
                    EmailContactPrincipal = "test@entreprise.ci"
                };

                var utilisateur = new Utilisateur
                {
                    Id = Guid.NewGuid(),
                    Nom = testClient.NomDirigeant,
                    Prenom = "",
                    Email = testClient.EmailContactPrincipal,
                    MotDePasse = BCrypt.Net.BCrypt.HashPassword("Client123!"),
                    Role = Models.enums.Role.Client
                };

                testClient.UtilisateurId = utilisateur.Id;
                testClient.Utilisateur = utilisateur;

                _context.Add(utilisateur);
                _context.Add(testClient);
                await _context.SaveChangesAsync();


                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // Action temporaire pour corriger les utilisateurs clients existants
        public async Task<IActionResult> FixExistingClients()
        {
            try
            {
                var clientEmails = new[]
                {
                    "bile.rene@sunu.ci",
                    "kkn@gmail.com", 
                    "toto.toto@toto.com"
                };

                var results = new List<string>();

                foreach (var email in clientEmails)
                {
                    // Vérifier si l'utilisateur existe
                    var utilisateur = await _context.Utilisateurs
                        .Include(u => u.Client)
                        .FirstOrDefaultAsync(u => u.Email == email);

                    if (utilisateur == null)
                    {
                        // Créer l'utilisateur s'il n'existe pas
                        utilisateur = new Utilisateur
                        {
                            Id = Guid.NewGuid(),
                            Nom = "Client",
                            Prenom = "",
                            Email = email,
                            MotDePasse = BCrypt.Net.BCrypt.HashPassword("Client123!"),
                            Role = Models.enums.Role.Client,
                            Supprimer = 0
                        };
                        _context.Utilisateurs.Add(utilisateur);
                        results.Add($"✅ Utilisateur créé pour {email}");
                    }
                    else
                    {
                        // Mettre à jour le rôle si nécessaire
                        if (utilisateur.Role != Models.enums.Role.Client)
                        {
                            utilisateur.Role = Models.enums.Role.Client;
                            results.Add($"✅ Rôle mis à jour pour {email}");
                        }
                        else
                        {
                            results.Add($"ℹ️ Utilisateur {email} déjà configuré");
                        }
                    }

                    // Vérifier si le client existe
                    var client = await _context.Clients
                        .FirstOrDefaultAsync(c => c.UtilisateurId == utilisateur.Id);

                    if (client == null)
                    {
                        // Créer le client s'il n'existe pas
                        client = new Client
                        {
                            Id = Guid.NewGuid(),
                            UtilisateurId = utilisateur.Id,
                            RaisonSociale = $"Client {email.Split('@')[0]}",
                            Adresse = "Adresse à compléter",
                            RegistreCommerce = "RC à compléter",
                            NomDirigeant = "Dirigeant à compléter",
                            NomContactPrincipal = "Contact à compléter",
                            TelephoneContactPrincipal = "+225 000000000",
                            EmailContactPrincipal = email
                        };
                        _context.Clients.Add(client);
                        results.Add($"✅ Client créé pour {email}");
                    }
                    else
                    {
                        results.Add($"ℹ️ Client déjà existant pour {email}");
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // Action pour tester la connexion des utilisateurs clients
        public async Task<IActionResult> TestClientLogin()
        {
            try
            {
                var results = new List<string>();
                
                // Récupérer tous les utilisateurs clients
                var utilisateursClients = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .Where(u => u.Role == Models.enums.Role.Client && u.Supprimer == 0)
                    .ToListAsync();

                if (!utilisateursClients.Any())
                {
                    results.Add("❌ Aucun utilisateur client trouvé dans la base de données");
                }
                else
                {
                    results.Add($"✅ {utilisateursClients.Count} utilisateur(s) client(s) trouvé(s)");
                    
                    foreach (var utilisateur in utilisateursClients)
                    {
                        var status = "✅";
                        var details = new List<string>();
                        
                        if (utilisateur.Client == null)
                        {
                            status = "❌";
                            details.Add("Pas de profil Client associé");
                        }
                        else
                        {
                            details.Add($"Client: {utilisateur.Client.RaisonSociale}");
                        }
                        
                        if (string.IsNullOrEmpty(utilisateur.MotDePasse))
                        {
                            status = "❌";
                            details.Add("Mot de passe manquant");
                        }
                        
                        results.Add($"{status} {utilisateur.Email} - {string.Join(", ", details)}");
                    }
                }


                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // Action pour créer un utilisateur client de test avec mot de passe en clair
        public async Task<IActionResult> CreateTestClientWithPlainPassword()
        {
            try
            {
                var testEmail = "test.client@diversitypub.ci";
                
                // Vérifier si l'utilisateur existe déjà
                var existingUser = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.Email == testEmail);
                
                if (existingUser != null)
                {
    
                    return RedirectToAction(nameof(Index));
                }

                var testClient = new Client
                {
                    Id = Guid.NewGuid(),
                    RaisonSociale = "Entreprise Test SARL",
                    Adresse = "123 Rue Test, Abidjan",
                    RegistreCommerce = "CI-ABJ-2025-001",
                    NomDirigeant = "Test Dirigeant",
                    NomContactPrincipal = "Test Contact",
                    TelephoneContactPrincipal = "+225 0123456789",
                    EmailContactPrincipal = testEmail
                };

                var utilisateur = new Utilisateur
                {
                    Id = Guid.NewGuid(),
                    Nom = testClient.NomDirigeant,
                    Prenom = "",
                    Email = testClient.EmailContactPrincipal,
                    MotDePasse = "Client123!", // Mot de passe en clair pour le test
                    Role = Models.enums.Role.Client,
                    Supprimer = 0
                };

                testClient.UtilisateurId = utilisateur.Id;
                testClient.Utilisateur = utilisateur;

                _context.Add(utilisateur);
                _context.Add(testClient);
                await _context.SaveChangesAsync();


                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // Action pour analyser et corriger tous les utilisateurs clients existants
        public async Task<IActionResult> AnalyzeExistingClients()
        {
            try
            {
                var results = new List<string>();
                var corrections = new List<string>();
                
                // Récupérer tous les utilisateurs
                var allUtilisateurs = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .Include(u => u.AgentTerrain)
                    .Where(u => u.Supprimer == 0)
                    .ToListAsync();

                results.Add($"📊 Analyse de {allUtilisateurs.Count} utilisateurs actifs");
                results.Add("");

                // Analyser les utilisateurs par rôle
                var usersByRole = allUtilisateurs.GroupBy(u => u.Role).ToList();
                
                foreach (var roleGroup in usersByRole)
                {
                    results.Add($"👥 Rôle {roleGroup.Key}: {roleGroup.Count()} utilisateur(s)");
                    
                    foreach (var user in roleGroup)
                    {
                        var status = "✅";
                        var issues = new List<string>();
                        
                        // Vérifier le mot de passe
                        if (string.IsNullOrEmpty(user.MotDePasse))
                        {
                            status = "❌";
                            issues.Add("Mot de passe manquant");
                        }
                        else if (user.MotDePasse.Length < 6)
                        {
                            status = "⚠️";
                            issues.Add("Mot de passe trop court");
                        }
                        
                        // Vérifier les profils spécifiques selon le rôle
                        if (user.Role == Models.enums.Role.Client)
                        {
                            if (user.Client == null)
                            {
                                status = "❌";
                                issues.Add("Pas de profil Client");
                            }
                            else
                            {
                                issues.Add($"Client: {user.Client.RaisonSociale}");
                            }
                        }
                        else if (user.Role == Models.enums.Role.AgentTerrain)
                        {
                            if (user.AgentTerrain == null)
                            {
                                status = "❌";
                                issues.Add("Pas de profil AgentTerrain");
                            }
                        }
                        
                        results.Add($"  {status} {user.Email} - {string.Join(", ", issues)}");
                    }
                    results.Add("");
                }

                // Proposer des corrections automatiques
                var clientsWithoutProfile = allUtilisateurs
                    .Where(u => u.Role == Models.enums.Role.Client && u.Client == null)
                    .ToList();

                if (clientsWithoutProfile.Any())
                {
                    results.Add("🔧 CORRECTIONS PROPOSÉES:");
                    results.Add("");
                    
                    foreach (var user in clientsWithoutProfile)
                    {
                        // Créer un profil client manquant
                        var client = new Client
                        {
                            Id = Guid.NewGuid(),
                            UtilisateurId = user.Id,
                            RaisonSociale = $"Client {user.Email.Split('@')[0]}",
                            Adresse = "À compléter",
                            RegistreCommerce = "À compléter",
                            NomDirigeant = user.Nom,
                            NomContactPrincipal = $"{user.Prenom} {user.Nom}",
                            TelephoneContactPrincipal = "+225 000000000",
                            EmailContactPrincipal = user.Email
                        };
                        
                        _context.Clients.Add(client);
                        corrections.Add($"✅ Profil Client créé pour {user.Email}");
                    }
                }

                // Corriger les mots de passe manquants ou trop courts
                var usersWithPasswordIssues = allUtilisateurs
                    .Where(u => string.IsNullOrEmpty(u.MotDePasse) || u.MotDePasse.Length < 6)
                    .ToList();

                if (usersWithPasswordIssues.Any())
                {
                    foreach (var user in usersWithPasswordIssues)
                    {
                        var defaultPassword = "Client123!";
                        if (user.Role == Models.enums.Role.Admin)
                            defaultPassword = "Admin123!";
                        else if (user.Role == Models.enums.Role.ChefProjet)
                            defaultPassword = "ChefProjet123!";
                        else if (user.Role == Models.enums.Role.AgentTerrain)
                            defaultPassword = "AgentTerrain123!";
                        
                        user.MotDePasse = defaultPassword;
                        corrections.Add($"✅ Mot de passe corrigé pour {user.Email}: {defaultPassword}");
                    }
                }

                if (corrections.Any())
                {
                    await _context.SaveChangesAsync();
                    results.Add("✅ Corrections appliquées avec succès !");
                    results.AddRange(corrections);
                }
                else
                {
                    results.Add("✅ Aucune correction nécessaire");
                }


                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // Action pour réinitialiser les mots de passe de tous les utilisateurs clients
        public async Task<IActionResult> ResetClientPasswords()
        {
            try
            {
                var results = new List<string>();
                
                var clientUsers = await _context.Utilisateurs
                    .Where(u => u.Role == Models.enums.Role.Client && u.Supprimer == 0)
                    .ToListAsync();

                if (!clientUsers.Any())
                {
    
                    return RedirectToAction(nameof(Index));
                }

                foreach (var user in clientUsers)
                {
                    user.MotDePasse = "Client123!";
                    results.Add($"✅ Mot de passe réinitialisé pour {user.Email}: Client123!");
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 