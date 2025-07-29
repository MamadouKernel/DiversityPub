using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiversityPub.Data;
using DiversityPub.Models;
using DiversityPub.Models.enums;
using Microsoft.AspNetCore.Authorization;

namespace DiversityPub.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly DiversityPubDbContext _context;

        public DashboardController(DiversityPubDbContext context)
        {
            _context = context;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            // Vérifier si l'utilisateur est un client
            if (User.IsInRole("Client"))
            {
                // Rediriger vers le dashboard client
                return RedirectToAction("Index", "ClientDashboard");
            }

            var isChefProjet = User.IsInRole("ChefProjet");
            
            var dashboardData = new
            {
                TotalCampagnes = await _context.Campagnes.CountAsync(),
                TotalActivations = await _context.Activations.CountAsync(),
                TotalClients = await _context.Clients.CountAsync(),
                TotalUtilisateurs = await _context.Utilisateurs.Where(u => u.Supprimer == 0).CountAsync(),
                TotalAgents = await _context.AgentsTerrain.CountAsync(),
                            ActivationsRecentes = await _context.Activations
                .Include(a => a.Campagne)
                .Include(a => a.Lieu)
                .OrderByDescending(a => a.DateCreation)
                .Take(5)
                .ToListAsync(),
                CampagnesActives = await _context.Campagnes
                    .Where(c => c.Statut == DiversityPub.Models.enums.StatutCampagne.EnCours)
                    .Include(c => c.Client)
                    .Take(5)
                    .ToListAsync(),
                ActivationsParStatut = await _context.Activations
                    .GroupBy(a => a.Statut)
                    .Select(g => new { Statut = g.Key, Count = g.Count() })
                    .ToListAsync(),
                EvolutionCampagnes = await _context.Campagnes
                    .GroupBy(c => c.Statut)
                    .Select(g => new { Statut = g.Key, Count = g.Count() })
                    .ToListAsync(),
                // Nouvelles données pour les graphiques
                ActivationsParLieu = await _context.Activations
                    .Include(a => a.Lieu)
                    .GroupBy(a => a.Lieu.Nom)
                    .Select(g => new { Lieu = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToListAsync(),
                CampagnesParClient = await _context.Campagnes
                    .Include(c => c.Client)
                    .GroupBy(c => c.Client.RaisonSociale)
                    .Select(g => new { Client = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToListAsync(),
                ActivationsParCampagne = await _context.Activations
                    .Include(a => a.Campagne)
                    .GroupBy(a => a.Campagne.Nom)
                    .Select(g => new { Campagne = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToListAsync(),
                IsChefProjet = isChefProjet
            };

            return View(dashboardData);
        }

        // GET: Dashboard/Statistiques
        public async Task<IActionResult> Statistiques()
        {
            var stats = new
            {
                ActivationsParStatut = await _context.Activations
                    .GroupBy(a => a.Statut)
                    .Select(g => new { Statut = g.Key, Count = g.Count() })
                    .ToListAsync(),
                EvolutionCampagnes = await _context.Campagnes
                    .GroupBy(c => c.Statut)
                    .Select(g => new { Statut = g.Key, Count = g.Count() })
                    .ToListAsync(),
                TopClients = await _context.Clients
                    .Include(c => c.Campagnes)
                    .OrderByDescending(c => c.Campagnes.Count)
                    .Take(5)
                    .Select(c => new { c.RaisonSociale, CampagnesCount = c.Campagnes.Count })
                    .ToListAsync()
            };

            return View(stats);
        }
    }
} 