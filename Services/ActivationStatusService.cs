using DiversityPub.Models.enums;

namespace DiversityPub.Services
{
    public class ActivationStatusService
    {
        /// <summary>
        /// Vérifie si une transition de statut est autorisée
        /// </summary>
        /// <param name="currentStatus">Statut actuel</param>
        /// <param name="newStatus">Nouveau statut</param>
        /// <returns>True si la transition est autorisée</returns>
        public static bool IsTransitionAllowed(StatutActivation currentStatus, StatutActivation newStatus)
        {
            // Règles de transition autorisées
            return (currentStatus, newStatus) switch
            {
                // Planifiée peut devenir EnCours, Suspendue ou Terminée
                (StatutActivation.Planifiee, StatutActivation.EnCours) => true,
                (StatutActivation.Planifiee, StatutActivation.Suspendue) => true,
                (StatutActivation.Planifiee, StatutActivation.Terminee) => true,
                
                // EnCours peut devenir Suspendue ou Terminée (mais pas Planifiée)
                (StatutActivation.EnCours, StatutActivation.Suspendue) => true,
                (StatutActivation.EnCours, StatutActivation.Terminee) => true,
                
                // Suspendue peut redevenir EnCours ou devenir Terminée
                (StatutActivation.Suspendue, StatutActivation.EnCours) => true,
                (StatutActivation.Suspendue, StatutActivation.Terminee) => true,
                
                // Terminée ne peut plus changer de statut
                (StatutActivation.Terminee, _) => false,
                
                // Par défaut, pas de transition autorisée
                _ => false
            };
        }

        /// <summary>
        /// Vérifie si une activation peut démarrer (a des agents terrain)
        /// </summary>
        /// <param name="agentsCount">Nombre d'agents terrain assignés</param>
        /// <returns>True si l'activation peut démarrer</returns>
        public static bool CanStartActivation(int agentsCount)
        {
            return agentsCount > 0;
        }

        /// <summary>
        /// Obtient le message d'erreur pour une transition non autorisée
        /// </summary>
        /// <param name="currentStatus">Statut actuel</param>
        /// <param name="newStatus">Nouveau statut</param>
        /// <returns>Message d'erreur explicatif</returns>
        public static string GetTransitionErrorMessage(StatutActivation currentStatus, StatutActivation newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                (StatutActivation.EnCours, StatutActivation.Planifiee) => 
                    "Une activation en cours ne peut pas revenir au statut 'Planifiée'.",
                
                (StatutActivation.Terminee, _) => 
                    "Une activation terminée ne peut plus changer de statut.",
                
                (StatutActivation.Suspendue, StatutActivation.Planifiee) => 
                    "Une activation suspendue ne peut pas revenir au statut 'Planifiée'.",
                
                _ => $"Transition non autorisée : de '{GetStatusDisplayName(currentStatus)}' vers '{GetStatusDisplayName(newStatus)}'."
            };
        }

        /// <summary>
        /// Obtient le nom d'affichage d'un statut
        /// </summary>
        /// <param name="status">Statut</param>
        /// <returns>Nom d'affichage</returns>
        public static string GetStatusDisplayName(StatutActivation status)
        {
            return status switch
            {
                StatutActivation.Planifiee => "Planifiée",
                StatutActivation.EnCours => "En Cours",
                StatutActivation.Suspendue => "Suspendue",
                StatutActivation.Terminee => "Terminée",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Obtient la classe CSS pour l'affichage du badge de statut
        /// </summary>
        /// <param name="status">Statut</param>
        /// <returns>Classe CSS</returns>
        public static string GetStatusBadgeClass(StatutActivation status)
        {
            return status switch
            {
                StatutActivation.Planifiee => "bg-warning",
                StatutActivation.EnCours => "bg-success",
                StatutActivation.Suspendue => "bg-warning",
                StatutActivation.Terminee => "bg-info",
                _ => "bg-secondary"
            };
        }
    }
} 