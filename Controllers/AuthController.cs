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
                return RedirectToAction("Index", "Dashboard");
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
                new Claim(ClaimTypes.Name, utilisateur.Email), // Utiliser l'email comme nom d'utilisateur
                new Claim(ClaimTypes.GivenName, utilisateur.Prenom),
                new Claim(ClaimTypes.Surname, utilisateur.Nom),
                new Claim(ClaimTypes.Email, utilisateur.Email),
                new Claim(ClaimTypes.Role, utilisateur.Role.ToString())
            };

            // Ajouter des claims spécifiques selon le rôle
            if (utilisateur.Role == Role.Client && utilisateur.Client != null)
            {
                claims.Add(new Claim("ClientId", utilisateur.Client.Id.ToString()));
                claims.Add(new Claim("RaisonSociale", utilisateur.Client.RaisonSociale));
            }
            else if (utilisateur.Role == Role.AgentTerrain && utilisateur.AgentTerrain != null)
            {
                claims.Add(new Claim("AgentTerrainId", utilisateur.AgentTerrain.Id.ToString()));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Dashboard");
        }

        // POST: Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: Auth/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
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