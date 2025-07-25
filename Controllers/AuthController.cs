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

            if (utilisateur == null || utilisateur.MotDePasse != password)
            {
                ViewBag.Error = "Email ou mot de passe incorrect";
                return View();
            }

            // Créer les claims pour l'authentification
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{utilisateur.Prenom} {utilisateur.Nom}"),
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
    }
} 