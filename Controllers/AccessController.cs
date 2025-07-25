using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DiversityPub.Data;

namespace flotteEngin.Controllers
{
    public class AccessController: Controller
    {
        private readonly DiversityPubDbContext _context;

        public AccessController(DiversityPubDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            if (claimUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Dtos.LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                ViewData["MessagedeWarning"] = "Veuillez entrer des informations valides.";
                return View(loginDto);
            }

            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (utilisateur != null)
            {
                if (utilisateur.Supprimer == 1)
                {
                    ViewData["Messagedevalidation"] = "Votre compte a été désactivé.";
                    return View(loginDto);
                }

                if (BCrypt.Net.BCrypt.Verify(loginDto.MotDePasse, utilisateur.MotDePasse))
                {
                    List<Claim> claims = new List<Claim>
                    {
                        new Claim("Id", utilisateur?.Id.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, utilisateur.Nom),
                        new Claim("Prenoms", utilisateur.Prenom),
                        new Claim(ClaimTypes.Role, utilisateur.Role.ToString()),
                        new Claim(ClaimTypes.Email, utilisateur.Email),

                    };

                    // Création de l'identité et des propriétés d'authentification
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    AuthenticationProperties properties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                    };

                    // Connexion de l'utilisateur
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

                    return RedirectToAction("Index", "Home");
                }

                // Message d'avertissement si le mot de passe est incorrect
                ViewData["Messagedevalidation"] = "Matricule ou Mot de passe incorrect";
                return View(loginDto); // Retourne les informations pour une meilleure UX
            }

            // Message d'avertissement si le matricule est incorrect
            ViewData["Messagedevalidation"] = "Matricule ou Mot de passe incorrect";
            return View(loginDto); // Retourne les informations pour une meilleure UX

        }

        private string HashPassword(string password)
        {
            // Utiliser BCrypt pour le hachage du mot de passe
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Méthode pour vérifier un mot de passe haché
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

    }

}