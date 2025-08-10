using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using DiversityPub.Models.enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DiversityPub.Controllers
{
    public class AuthController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public AuthController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: Auth/Login
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Redirection selon le rôle
                if (User.IsInRole("Client"))
                {
                    return RedirectToAction("Index", "ClientDashboard");
                }
                else if (User.IsInRole("AgentTerrain"))
                {
                    return RedirectToAction("Missions", "AgentTerrain");
                }
                else
                {
                    return RedirectToAction("Index", "Dashboard");
                }
            }
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email et mot de passe requis";
                return View();
            }

            try
            {
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Client)
                    .Include(u => u.AgentTerrain)
                    .FirstOrDefaultAsync(u => u.Email == email && u.Supprimer == 0);

                if (utilisateur == null)
                {
                    ViewBag.Error = "Email ou mot de passe incorrect";
                    return View();
                }

                // Vérifier le mot de passe (gérer les mots de passe en clair et hashés)
                bool passwordValid = false;
                
                // D'abord essayer de vérifier si c'est un hash BCrypt
                if (utilisateur.MotDePasse.StartsWith("$2a$") || utilisateur.MotDePasse.StartsWith("$2b$"))
                {
                    try
                    {
                        passwordValid = BCrypt.Net.BCrypt.Verify(password, utilisateur.MotDePasse);
                    }
                    catch
                    {
                        passwordValid = false;
                    }
                }
                else
                {
                    // Si ce n'est pas un hash BCrypt, comparer directement (pour les mots de passe en clair)
                    passwordValid = utilisateur.MotDePasse == password;
                }

                if (!passwordValid)
                {
                    ViewBag.Error = "Email ou mot de passe incorrect";
                    return View();
                }

                // Créer les claims pour l'authentification
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
                    new Claim(ClaimTypes.Name, utilisateur.Email),
                    new Claim(ClaimTypes.Role, utilisateur.Role.ToString()),
                    new Claim("UserId", utilisateur.Id.ToString())
                };

                // Créer l'identité et le ticket d'authentification
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) // Session de 8 heures
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                // Marquer l'agent comme connecté si c'est un AgentTerrain
                if (utilisateur.AgentTerrain != null)
                {
                    utilisateur.AgentTerrain.EstConnecte = true;
                    utilisateur.AgentTerrain.DerniereConnexion = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                // Redirection selon le rôle
                if (utilisateur.Role == Role.Client)
                {
                    return RedirectToAction("Index", "ClientDashboard");
                }
                else if (utilisateur.Role == Role.AgentTerrain)
                {
                    return RedirectToAction("Missions", "AgentTerrain");
                }
                else
                {
                    return RedirectToAction("Index", "Dashboard");
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                // Gérer les erreurs de base de données
                if (ex.Message.Contains("Invalid object name") || ex.Message.Contains("Cannot open database"))
                {
                    ViewBag.Error = "Erreur de configuration de la base de données. Veuillez contacter l'administrateur.";
                    return View();
                }
                else
                {
                    ViewBag.Error = "Erreur de connexion à la base de données. Veuillez réessayer.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Une erreur inattendue s'est produite. Veuillez réessayer.";
                return View();
            }
        }

        // POST: Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Récupérer l'email de l'utilisateur connecté
                var userEmail = User.Identity?.Name;
                
                if (!string.IsNullOrEmpty(userEmail))
                {
                    // Marquer l'agent comme déconnecté
                    var agentTerrain = await _context.AgentsTerrain
                        .Include(at => at.Utilisateur)
                        .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                    if (agentTerrain != null)
                    {
                        // Marquer l'agent comme déconnecté
                        agentTerrain.EstConnecte = false;
                        agentTerrain.DerniereDeconnexion = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }
                }

                // Déconnexion standard
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                
                // Nettoyer les cookies de session
                Response.Cookies.Delete(".AspNetCore.Identity.Application");
                Response.Cookies.Delete(".AspNetCore.Session");
                
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // En cas d'erreur, forcer quand même la déconnexion
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login");
            }
        }

        // GET: Auth/ForceLogout - Méthode pour forcer la déconnexion d'un agent
        [HttpGet]
        [Authorize(Roles = "Admin,ChefProjet")]
        public async Task<IActionResult> ForceLogout(Guid agentId)
        {
            try
            {
                var agentTerrain = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Id == agentId);

                if (agentTerrain == null)
                {
                    return Json(new { success = false, message = "Agent non trouvé." });
                }

                // Marquer l'agent comme déconnecté
                agentTerrain.EstConnecte = false;
                agentTerrain.DerniereDeconnexion = DateTime.Now;
                await _context.SaveChangesAsync();

                // Envoyer une notification SignalR pour déconnecter l'agent de sa tablette
                try
                {
                    var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<DiversityPub.Hubs.NotificationHub>>();
                    await hubContext.Clients.Group($"agent_{agentId}").SendAsync("ForceLogout");
                    Console.WriteLine($"📡 Notification SignalR envoyée au groupe agent_{agentId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur SignalR: {ex.Message}");
                }

                return Json(new { 
                    success = true, 
                    message = $"Agent {agentTerrain.Utilisateur.Prenom} {agentTerrain.Utilisateur.Nom} déconnecté avec succès." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la déconnexion forcée: {ex.Message}" });
            }
        }

        // GET: Auth/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: Auth/CheckConnectionStatus - Vérifier le statut de connexion d'un agent
        [HttpGet]
        [Authorize(Roles = "AgentTerrain,Admin,ChefProjet")]
        public async Task<IActionResult> CheckConnectionStatus()
        {
            try
            {
                var userEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Json(new { isConnected = false });
                }

                var agentTerrain = await _context.AgentsTerrain
                    .Include(at => at.Utilisateur)
                    .FirstOrDefaultAsync(at => at.Utilisateur.Email == userEmail);

                if (agentTerrain == null)
                {
                    return Json(new { isConnected = false });
                }

                return Json(new { isConnected = agentTerrain.EstConnecte });
            }
            catch (Exception ex)
            {
                return Json(new { isConnected = false, error = ex.Message });
            }
        }

        // GET: Auth/DebugLogin - Méthode de debug pour tester la connexion
        public async Task<IActionResult> DebugLogin(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return Content("Email et mot de passe requis");
            }

            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Client)
                .Include(u => u.AgentTerrain)
                .FirstOrDefaultAsync(u => u.Email == email && u.Supprimer == 0);

            if (utilisateur == null)
            {
                return Content($"❌ Utilisateur non trouvé pour l'email: {email}");
            }

            var result = new List<string>
            {
                $"✅ Utilisateur trouvé: {utilisateur.Email}",
                $"Nom: {utilisateur.Nom} {utilisateur.Prenom}",
                $"Rôle: {utilisateur.Role}",
                $"Mot de passe en DB: {utilisateur.MotDePasse}",
                $"Mot de passe fourni: {password}",
                $"Mot de passe hashé: {utilisateur.MotDePasse.StartsWith("$2a$") || utilisateur.MotDePasse.StartsWith("$2b$")}"
            };

            // Vérifier le mot de passe
            bool passwordValid = false;
            
            if (utilisateur.MotDePasse.StartsWith("$2a$") || utilisateur.MotDePasse.StartsWith("$2b$"))
            {
                try
                {
                    passwordValid = BCrypt.Net.BCrypt.Verify(password, utilisateur.MotDePasse);
                    result.Add($"Vérification BCrypt: {passwordValid}");
                }
                catch (Exception ex)
                {
                    result.Add($"Erreur BCrypt: {ex.Message}");
                }
            }
            else
            {
                passwordValid = utilisateur.MotDePasse == password;
                result.Add($"Comparaison directe: {passwordValid}");
            }

            if (utilisateur.Client != null)
            {
                result.Add($"✅ Client associé: {utilisateur.Client.RaisonSociale}");
            }
            else
            {
                result.Add("❌ Aucun client associé");
            }

            return Content(string.Join("\n", result));
        }
    }
} 