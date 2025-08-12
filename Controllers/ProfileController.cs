using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DiversityPub.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public ProfileController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: Profile
        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity.Name;
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Client)
                .Include(u => u.AgentTerrain)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (utilisateur == null)
            {
                return NotFound();
            }

            return View(utilisateur);
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var userEmail = User.Identity.Name;
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Client)
                .Include(u => u.AgentTerrain)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (utilisateur == null)
            {
                return NotFound();
            }

            return View(utilisateur);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nom,Prenom,Email")] Utilisateur utilisateur, 
            string MotDePasse, string clientRaisonSociale, string clientTelephone, string clientAdresse,
            string agentTelephone, string agentEmail)
        {
            if (id != utilisateur.Id)
                return NotFound();

            // Vérifier que l'utilisateur modifie son propre profil
            var userEmail = User.Identity.Name;
            var currentUser = await _context.Utilisateurs
                .Include(u => u.Client)
                .Include(u => u.AgentTerrain)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (currentUser == null || currentUser.Id != id)
            {
                return Forbid();
            }

            try
            {
                // Mettre à jour les propriétés de base
                currentUser.Nom = utilisateur.Nom;
                currentUser.Prenom = utilisateur.Prenom;
                currentUser.Email = utilisateur.Email;

                // Gérer le changement de mot de passe
                if (!string.IsNullOrEmpty(MotDePasse))
                {
                    currentUser.MotDePasse = MotDePasse;
                }

                // Mettre à jour le profil Client si applicable
                if (currentUser.Role == Models.enums.Role.Client && currentUser.Client != null)
                {
                    currentUser.Client.RaisonSociale = clientRaisonSociale ?? currentUser.Client.RaisonSociale;
                    currentUser.Client.TelephoneContactPrincipal = clientTelephone ?? currentUser.Client.TelephoneContactPrincipal;
                    currentUser.Client.Adresse = clientAdresse ?? currentUser.Client.Adresse;
                    currentUser.Client.NomContactPrincipal = currentUser.Prenom + " " + currentUser.Nom;
                    currentUser.Client.EmailContactPrincipal = currentUser.Email;
                }

                // Mettre à jour le profil AgentTerrain si applicable
                if (currentUser.Role == Models.enums.Role.AgentTerrain && currentUser.AgentTerrain != null)
                {
                    currentUser.AgentTerrain.Telephone = agentTelephone ?? currentUser.AgentTerrain.Telephone;
                    currentUser.AgentTerrain.Email = agentEmail ?? currentUser.AgentTerrain.Email;
                }

                _context.Update(currentUser);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Votre profil a été mis à jour avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erreur lors de la modification : " + ex.Message);
                return View(currentUser);
            }
        }

        // GET: Profile/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Error = "Tous les champs sont obligatoires.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Les nouveaux mots de passe ne correspondent pas.";
                return View();
            }

            if (newPassword.Length < 6)
            {
                ViewBag.Error = "Le nouveau mot de passe doit contenir au moins 6 caractères.";
                return View();
            }

            var userEmail = User.Identity.Name;
            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (utilisateur == null)
            {
                return NotFound();
            }

            // Vérifier l'ancien mot de passe (gérer les mots de passe hashés et non hashés)
            bool currentPasswordValid = false;
            
            // D'abord essayer de vérifier si c'est un hash BCrypt
            if (utilisateur.MotDePasse.StartsWith("$2a$") || utilisateur.MotDePasse.StartsWith("$2b$"))
            {
                try
                {
                    currentPasswordValid = BCrypt.Net.BCrypt.Verify(currentPassword, utilisateur.MotDePasse);
                }
                catch
                {
                    currentPasswordValid = false;
                }
            }
            else
            {
                // Si ce n'est pas un hash BCrypt, comparer directement (pour les mots de passe en clair)
                currentPasswordValid = utilisateur.MotDePasse == currentPassword;
            }

            if (!currentPasswordValid)
            {
                ViewBag.Error = "L'ancien mot de passe est incorrect.";
                return View();
            }

            // Mettre à jour le mot de passe avec BCrypt
            utilisateur.MotDePasse = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.Update(utilisateur);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Votre mot de passe a été modifié avec succès.";
            return RedirectToAction(nameof(Index));
        }
    }
} 