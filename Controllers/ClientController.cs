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
                    .OrderBy(c => c.RaisonSociale)
                    .ToListAsync();

                // Dictionnaire : clé = ID client, valeur = nombre de campagnes
                var nombreCampagnesParClient = clients.ToDictionary(
                    c => c.Id,
                    c => c.Campagnes?.Count ?? 0
                );

                // On le passe à la vue
                ViewBag.NombreCampagnes = nombreCampagnesParClient;

                if (clients.Count == 0)
                {
                    TempData["Info"] = "🏢 Aucun client trouvé. Créez votre premier client !";
                }
                else
                {
                    TempData["Info"] = $"🏢 {clients.Count} client(s) trouvé(s)";
                }

                return View(clients);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Erreur lors du chargement des clients: {ex.Message}";
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
            TempData["Info"] = "🏢 Prêt à créer un nouveau client. Un compte utilisateur sera automatiquement créé.";
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

                TempData["Success"] = "Client de test créé avec succès !";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la création du client de test: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 