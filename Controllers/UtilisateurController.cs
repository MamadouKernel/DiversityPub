using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using DiversityPub.Models.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.IO;

namespace DiversityPub.Controllers
{
    [Authorize(Roles = "Admin,ChefProjet")]
    public class UtilisateurController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public UtilisateurController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: Utilisateur
        public async Task<IActionResult> Index()
        {
            var utilisateurs = await _context.Utilisateurs
                .Include(u => u.Client)
                .Include(u => u.AgentTerrain)
                .OrderBy(u => u.Nom)
                .ToListAsync();
            
            return View(utilisateurs);
        }

        // GET: Utilisateur/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Client)
                .Include(u => u.AgentTerrain)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (utilisateur == null)
                return NotFound();

            return View(utilisateur);
        }

        // GET: Utilisateur/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Clients = await _context.Clients.ToListAsync();
            return View();
        }

        // POST: Utilisateur/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Nom,Prenom,Email,MotDePasse,Role")] Utilisateur utilisateur, Guid? clientId)
        {
            if (ModelState.IsValid)
            {
                utilisateur.Id = Guid.NewGuid();
                utilisateur.Supprimer = 0;
                
                // Hasher le mot de passe
                utilisateur.MotDePasse = BCrypt.Net.BCrypt.HashPassword(utilisateur.MotDePasse);

                // Créer le profil spécifique selon le rôle
                if (utilisateur.Role == Role.Client && clientId.HasValue)
                {
                    utilisateur.Client = await _context.Clients.FindAsync(clientId.Value);
                }
                else if (utilisateur.Role == Role.AgentTerrain)
                {
                    utilisateur.AgentTerrain = new AgentTerrain
                    {
                        Id = Guid.NewGuid(),
                        UtilisateurId = utilisateur.Id,
                        Email = utilisateur.Email,
                        Telephone = string.Empty
                    };
                }

                _context.Add(utilisateur);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Clients = await _context.Clients.ToListAsync();
            return View(utilisateur);
        }

        // GET: Utilisateur/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Client)
                .Include(u => u.AgentTerrain)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (utilisateur == null)
                return NotFound();

            return View(utilisateur);
        }

        // POST: Utilisateur/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nom,Prenom,Email,Role")] Utilisateur utilisateur, 
            string MotDePasse, string clientRaisonSociale, string clientTelephone, string clientAdresse,
            string agentTelephone, string agentEmail)
        {
            if (id != utilisateur.Id)
                return NotFound();

            try
            {
                var existingUser = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .Include(u => u.AgentTerrain)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (existingUser == null)
                    return NotFound();

                // Mettre à jour les propriétés de base
                existingUser.Nom = utilisateur.Nom;
                existingUser.Prenom = utilisateur.Prenom;
                existingUser.Email = utilisateur.Email;
                existingUser.Role = utilisateur.Role;

                // Gérer le changement de mot de passe
                if (!string.IsNullOrEmpty(MotDePasse))
                {
                    existingUser.MotDePasse = BCrypt.Net.BCrypt.HashPassword(MotDePasse);
                }

                // Gérer les profils spécifiques selon le nouveau rôle
                if (utilisateur.Role == Role.Client)
                {
                    if (existingUser.Client == null)
                    {
                        existingUser.Client = new Client
                        {
                            Id = Guid.NewGuid(),
                            UtilisateurId = existingUser.Id,
                            RaisonSociale = clientRaisonSociale ?? existingUser.Nom,
                            TelephoneContactPrincipal = clientTelephone ?? "",
                            Adresse = clientAdresse ?? "",
                            NomContactPrincipal = existingUser.Prenom + " " + existingUser.Nom,
                            EmailContactPrincipal = existingUser.Email,
                            NomDirigeant = existingUser.Prenom + " " + existingUser.Nom,
                            RegistreCommerce = ""
                        };
                        _context.Clients.Add(existingUser.Client);
                    }
                    else
                    {
                        existingUser.Client.RaisonSociale = clientRaisonSociale ?? existingUser.Client.RaisonSociale;
                        existingUser.Client.TelephoneContactPrincipal = clientTelephone ?? existingUser.Client.TelephoneContactPrincipal;
                        existingUser.Client.Adresse = clientAdresse ?? existingUser.Client.Adresse;
                    }

                    // Supprimer le profil AgentTerrain s'il existe
                    if (existingUser.AgentTerrain != null)
                    {
                        _context.AgentsTerrain.Remove(existingUser.AgentTerrain);
                        existingUser.AgentTerrain = null;
                    }
                }
                else if (utilisateur.Role == Role.AgentTerrain)
                {
                    if (existingUser.AgentTerrain == null)
                    {
                        existingUser.AgentTerrain = new AgentTerrain
                        {
                            Id = Guid.NewGuid(),
                            UtilisateurId = existingUser.Id,
                            Telephone = agentTelephone ?? "",
                            Email = agentEmail ?? existingUser.Email
                        };
                        _context.AgentsTerrain.Add(existingUser.AgentTerrain);
                    }
                    else
                    {
                        existingUser.AgentTerrain.Telephone = agentTelephone ?? existingUser.AgentTerrain.Telephone;
                        existingUser.AgentTerrain.Email = agentEmail ?? existingUser.AgentTerrain.Email;
                    }

                    // Supprimer le profil Client s'il existe
                    if (existingUser.Client != null)
                    {
                        _context.Clients.Remove(existingUser.Client);
                        existingUser.Client = null;
                    }
                }
                else
                {
                    // Pour Admin et ChefProjet, supprimer les profils spécifiques
                    if (existingUser.Client != null)
                    {
                        _context.Clients.Remove(existingUser.Client);
                        existingUser.Client = null;
                    }
                    if (existingUser.AgentTerrain != null)
                    {
                        _context.AgentsTerrain.Remove(existingUser.AgentTerrain);
                        existingUser.AgentTerrain = null;
                    }
                }

                _context.Update(existingUser);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Utilisateur modifié avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erreur lors de la modification : " + ex.Message);
                return View(utilisateur);
            }
        }

        // GET: Utilisateur/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Client)
                .Include(u => u.AgentTerrain)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (utilisateur == null)
                return NotFound();

            return View(utilisateur);
        }

        // POST: Utilisateur/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur != null)
            {
                utilisateur.Supprimer = 1; // Soft delete
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Utilisateur/Import
        [Authorize(Roles = "Admin")]
        public IActionResult Import()
        {
            return View();
        }

        // GET: Utilisateur/DownloadTemplate
        [Authorize(Roles = "Admin")]
        public IActionResult DownloadTemplate()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Utilisateurs");
                    
                    // En-têtes
                    var headers = new[]
                    {
                        "Nom", "Prenom", "Email", "Role", "MotDePasse", "Telephone", "Adresse", "RaisonSociale", "EmailPro"
                    };
                    
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    }
                    
                    // Données d'exemple
                    var sampleData = new[]
                    {
                        new { Nom = "Dupont", Prenom = "Jean", Email = "jean.dupont@exemple.com", Role = "AgentTerrain", MotDePasse = "MotDePasse123!", Telephone = "0123456789", Adresse = "", RaisonSociale = "", EmailPro = "jean.pro@exemple.com" },
                        new { Nom = "Martin", Prenom = "Marie", Email = "marie.martin@exemple.com", Role = "Client", MotDePasse = "MotDePasse123!", Telephone = "0987654321", Adresse = "123 Rue de la Paix, 75001 Paris", RaisonSociale = "Entreprise Martin SARL", EmailPro = "" },
                        new { Nom = "Durand", Prenom = "Pierre", Email = "pierre.durand@exemple.com", Role = "ChefProjet", MotDePasse = "MotDePasse123!", Telephone = "0555666777", Adresse = "", RaisonSociale = "", EmailPro = "" },
                        new { Nom = "Admin", Prenom = "Administrateur", Email = "admin@exemple.com", Role = "Admin", MotDePasse = "MotDePasse123!", Telephone = "", Adresse = "", RaisonSociale = "", EmailPro = "" },
                        new { Nom = "Leroy", Prenom = "Sophie", Email = "sophie.leroy@exemple.com", Role = "AgentTerrain", MotDePasse = "MotDePasse123!", Telephone = "0111222333", Adresse = "", RaisonSociale = "", EmailPro = "sophie.terrain@exemple.com" },
                        new { Nom = "Moreau", Prenom = "Thomas", Email = "thomas.moreau@exemple.com", Role = "Client", MotDePasse = "MotDePasse123!", Telephone = "0444555666", Adresse = "456 Avenue des Champs, 69000 Lyon", RaisonSociale = "Société Moreau & Associés", EmailPro = "" }
                    };
                    
                    for (int row = 0; row < sampleData.Length; row++)
                    {
                        var data = sampleData[row];
                        worksheet.Cells[row + 2, 1].Value = data.Nom;
                        worksheet.Cells[row + 2, 2].Value = data.Prenom;
                        worksheet.Cells[row + 2, 3].Value = data.Email;
                        worksheet.Cells[row + 2, 4].Value = data.Role;
                        worksheet.Cells[row + 2, 5].Value = data.MotDePasse;
                        worksheet.Cells[row + 2, 6].Value = data.Telephone;
                        worksheet.Cells[row + 2, 7].Value = data.Adresse;
                        worksheet.Cells[row + 2, 8].Value = data.RaisonSociale;
                        worksheet.Cells[row + 2, 9].Value = data.EmailPro;
                    }
                    
                    // Ajouter une feuille d'instructions
                    var instructionsSheet = package.Workbook.Worksheets.Add("Instructions");
                    
                    instructionsSheet.Cells[1, 1].Value = "INSTRUCTIONS POUR L'IMPORT D'UTILISATEURS";
                    instructionsSheet.Cells[1, 1].Style.Font.Bold = true;
                    instructionsSheet.Cells[1, 1].Style.Font.Size = 16;
                    
                    instructionsSheet.Cells[3, 1].Value = "COLONNES OBLIGATOIRES :";
                    instructionsSheet.Cells[3, 1].Style.Font.Bold = true;
                    instructionsSheet.Cells[4, 1].Value = "• Nom : Nom de famille de l'utilisateur";
                    instructionsSheet.Cells[5, 1].Value = "• Prenom : Prénom de l'utilisateur";
                    instructionsSheet.Cells[6, 1].Value = "• Email : Adresse email unique (servira d'identifiant)";
                    instructionsSheet.Cells[7, 1].Value = "• Role : Rôle de l'utilisateur (Admin, ChefProjet, Client, AgentTerrain)";
                    
                    instructionsSheet.Cells[9, 1].Value = "COLONNES OPTIONNELLES :";
                    instructionsSheet.Cells[9, 1].Style.Font.Bold = true;
                    instructionsSheet.Cells[10, 1].Value = "• MotDePasse : Mot de passe (défaut: MotDePasse123!)";
                    instructionsSheet.Cells[11, 1].Value = "• Telephone : Numéro de téléphone";
                    instructionsSheet.Cells[12, 1].Value = "• Adresse : Adresse complète (pour les clients)";
                    instructionsSheet.Cells[13, 1].Value = "• RaisonSociale : Nom de l'entreprise (pour les clients)";
                    instructionsSheet.Cells[14, 1].Value = "• EmailPro : Email professionnel (pour les agents terrain)";
                    
                    instructionsSheet.Cells[16, 1].Value = "RÔLES DISPONIBLES :";
                    instructionsSheet.Cells[16, 1].Style.Font.Bold = true;
                    instructionsSheet.Cells[17, 1].Value = "• Admin : Administrateur système (accès complet)";
                    instructionsSheet.Cells[18, 1].Value = "• ChefProjet : Chef de projet (gestion opérationnelle)";
                    instructionsSheet.Cells[19, 1].Value = "• Client : Client final (consultation campagnes)";
                    instructionsSheet.Cells[20, 1].Value = "• AgentTerrain : Agent terrain (exécution missions)";
                    
                    instructionsSheet.Cells[22, 1].Value = "IMPORTANT :";
                    instructionsSheet.Cells[22, 1].Style.Font.Bold = true;
                    instructionsSheet.Cells[23, 1].Value = "• Un utilisateur ne peut avoir qu'UN SEUL rôle";
                    instructionsSheet.Cells[24, 1].Value = "• Les emails doivent être uniques";
                    instructionsSheet.Cells[25, 1].Value = "• Les mots de passe doivent contenir au moins 8 caractères";
                    instructionsSheet.Cells[26, 1].Value = "• Supprimez les lignes d'exemple avant l'import";
                    
                    // Ajuster la largeur des colonnes
                    worksheet.Cells.AutoFitColumns();
                    instructionsSheet.Cells.AutoFitColumns();
                    
                    // Convertir en bytes
                    var bytes = package.GetAsByteArray();
                    
                    return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Utilisateurs.xlsx");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Erreur lors de la création du template : {ex.Message}");
            }
        }

        // POST: Utilisateur/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "Veuillez sélectionner un fichier Excel.";
                return View();
            }

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            {
                ViewBag.Error = "Veuillez sélectionner un fichier Excel (.xlsx ou .xls).";
                return View();
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var importResults = new List<string>();
                var successCount = 0;
                var errorCount = 0;

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension?.Rows ?? 0;
                        var colCount = worksheet.Dimension?.Columns ?? 0;

                        // Vérifier les en-têtes
                        var headers = new List<string>();
                        for (int col = 1; col <= colCount; col++)
                        {
                            headers.Add(worksheet.Cells[1, col].Value?.ToString() ?? "");
                        }

                        // Vérifier les colonnes requises
                        var requiredColumns = new[] { "Nom", "Prenom", "Email", "Role" };
                        var missingColumns = requiredColumns.Where(col => !headers.Contains(col)).ToList();

                        if (missingColumns.Any())
                        {
                            ViewBag.Error = $"Colonnes manquantes : {string.Join(", ", missingColumns)}";
                            return View();
                        }

                        // Traiter chaque ligne (en commençant par la ligne 2 pour ignorer les en-têtes)
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var nom = worksheet.Cells[row, headers.IndexOf("Nom") + 1].Value?.ToString();
                                var prenom = worksheet.Cells[row, headers.IndexOf("Prenom") + 1].Value?.ToString();
                                var email = worksheet.Cells[row, headers.IndexOf("Email") + 1].Value?.ToString();
                                var roleStr = worksheet.Cells[row, headers.IndexOf("Role") + 1].Value?.ToString();
                                var motDePasse = worksheet.Cells[row, headers.IndexOf("MotDePasse") + 1].Value?.ToString() ?? "MotDePasse123!";

                                // Validation des données
                                if (string.IsNullOrEmpty(nom) || string.IsNullOrEmpty(prenom) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(roleStr))
                                {
                                    importResults.Add($"Ligne {row}: Données manquantes");
                                    errorCount++;
                                    continue;
                                }

                                // Vérifier si l'email existe déjà
                                if (await _context.Utilisateurs.AnyAsync(u => u.Email == email))
                                {
                                    importResults.Add($"Ligne {row}: Email '{email}' existe déjà");
                                    errorCount++;
                                    continue;
                                }

                                // Parser le rôle
                                if (!Enum.TryParse<Role>(roleStr, true, out var role))
                                {
                                    importResults.Add($"Ligne {row}: Rôle '{roleStr}' invalide");
                                    errorCount++;
                                    continue;
                                }

                                // Créer l'utilisateur
                                var utilisateur = new Utilisateur
                                {
                                    Id = Guid.NewGuid(),
                                    Nom = nom,
                                    Prenom = prenom,
                                    Email = email,
                                    MotDePasse = BCrypt.Net.BCrypt.HashPassword(motDePasse),
                                    Role = role,
                                    Supprimer = 0
                                };

                                // Créer les profils spécifiques selon le rôle
                                if (role == Role.Client)
                                {
                                    var raisonSociale = worksheet.Cells[row, headers.IndexOf("RaisonSociale") + 1].Value?.ToString() ?? nom;
                                    var telephone = worksheet.Cells[row, headers.IndexOf("Telephone") + 1].Value?.ToString() ?? "";
                                    var adresse = worksheet.Cells[row, headers.IndexOf("Adresse") + 1].Value?.ToString() ?? "";

                                    utilisateur.Client = new Client
                                    {
                                        Id = Guid.NewGuid(),
                                        UtilisateurId = utilisateur.Id,
                                        RaisonSociale = raisonSociale,
                                        TelephoneContactPrincipal = telephone,
                                        Adresse = adresse
                                    };
                                }
                                else if (role == Role.AgentTerrain)
                                {
                                    var telephone = worksheet.Cells[row, headers.IndexOf("Telephone") + 1].Value?.ToString() ?? "";
                                    var emailPro = worksheet.Cells[row, headers.IndexOf("EmailPro") + 1].Value?.ToString() ?? email;

                                    utilisateur.AgentTerrain = new AgentTerrain
                                    {
                                        Id = Guid.NewGuid(),
                                        UtilisateurId = utilisateur.Id,
                                        Telephone = telephone,
                                        Email = emailPro
                                    };
                                }

                                _context.Utilisateurs.Add(utilisateur);
                                successCount++;
                                importResults.Add($"Ligne {row}: Utilisateur '{prenom} {nom}' créé avec succès");
                            }
                            catch (Exception ex)
                            {
                                importResults.Add($"Ligne {row}: Erreur - {ex.Message}");
                                errorCount++;
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                ViewBag.SuccessCount = successCount;
                ViewBag.ErrorCount = errorCount;
                ViewBag.ImportResults = importResults;
                ViewBag.Success = $"Import terminé : {successCount} utilisateurs créés, {errorCount} erreurs";

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Erreur lors de l'import : {ex.Message}";
                return View();
            }
        }

        // POST: Utilisateur/MigrerMotsDePasse
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MigrerMotsDePasse()
        {
            try
            {
                var utilisateurs = await _context.Utilisateurs.ToListAsync();
                var migrations = new List<string>();
                var erreurs = new List<string>();

                foreach (var utilisateur in utilisateurs)
                {
                    try
                    {
                        // Vérifier si le mot de passe est déjà hashé
                        if (utilisateur.MotDePasse.StartsWith("$2a$") || utilisateur.MotDePasse.StartsWith("$2b$"))
                        {
                            migrations.Add($"✅ {utilisateur.Email}: Mot de passe déjà hashé");
                            continue;
                        }

                        // Si le mot de passe est vide ou trop court, générer un mot de passe par défaut
                        if (string.IsNullOrEmpty(utilisateur.MotDePasse) || utilisateur.MotDePasse.Length < 6)
                        {
                            string defaultPassword;
                            switch (utilisateur.Role)
                            {
                                case Role.Admin:
                                    defaultPassword = "Admin123!";
                                    break;
                                case Role.ChefProjet:
                                    defaultPassword = "ChefProjet123!";
                                    break;
                                case Role.AgentTerrain:
                                    defaultPassword = "AgentTerrain123!";
                                    break;
                                case Role.Client:
                                    defaultPassword = "Client123!";
                                    break;
                                default:
                                    defaultPassword = "User123!";
                                    break;
                            }
                            
                            utilisateur.MotDePasse = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
                            migrations.Add($"✅ {utilisateur.Email}: Mot de passe par défaut généré ({defaultPassword})");
                        }
                        else
                        {
                            // Hasher le mot de passe existant
                            var motDePasseOriginal = utilisateur.MotDePasse;
                            utilisateur.MotDePasse = BCrypt.Net.BCrypt.HashPassword(motDePasseOriginal);
                            migrations.Add($"✅ {utilisateur.Email}: Mot de passe hashé");
                        }
                    }
                    catch (Exception ex)
                    {
                        erreurs.Add($"❌ {utilisateur.Email}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                var result = new List<string>
                {
                    $"=== MIGRATION DES MOTS DE PASSE ===",
                    $"Total utilisateurs traités: {utilisateurs.Count}",
                    $"Migrations réussies: {migrations.Count}",
                    $"Erreurs: {erreurs.Count}"
                };

                result.AddRange(migrations);
                if (erreurs.Any())
                {
                    result.Add("=== ERREURS ===");
                    result.AddRange(erreurs);
                }

                TempData["MigrationResult"] = string.Join("\n", result);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la migration: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool UtilisateurExists(Guid id)
        {
            return _context.Utilisateurs.Any(e => e.Id == id);
        }
    }
} 