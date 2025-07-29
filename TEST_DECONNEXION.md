# Test de Déconnexion des Agents Terrain

## Problème résolu
Les agents terrain ne pouvaient pas se déconnecter car la page Missions n'avait pas de bouton de déconnexion visible.

## Solution implémentée
Ajout d'un bouton de déconnexion dans l'en-tête de la page Missions des agents terrain.

## Comment tester

### 1. Connexion d'un agent terrain
1. Accédez à l'application DiversityPub
2. Connectez-vous avec un compte agent terrain
3. Vous devriez être redirigé vers la page Missions

### 2. Vérification du bouton de déconnexion
1. Sur la page Missions, vérifiez que le bouton "Déconnexion" est visible dans l'en-tête
2. Le bouton doit être à droite de l'heure et de la date
3. Le bouton doit avoir l'icône "sign-out-alt" et le texte "Déconnexion"

### 3. Test de la déconnexion
1. Cliquez sur le bouton "Déconnexion"
2. Vous devriez être redirigé vers la page de connexion
3. Vérifiez que vous ne pouvez plus accéder aux pages protégées

### 4. Test de sécurité
1. Essayez d'accéder directement à `/AgentTerrain/Missions` sans être connecté
2. Vous devriez être redirigé vers la page de connexion

## Fichiers modifiés
- `Views/AgentTerrain/Missions.cshtml` : Ajout du bouton de déconnexion dans l'en-tête

## Fichiers déjà corrects
- `Controllers/AuthController.cs` : Méthode Logout déjà implémentée
- `Views/AgentTerrain/Error.cshtml` : Bouton de déconnexion déjà présent
- `Views/Shared/_Layout.cshtml` : Bouton de déconnexion dans le menu utilisateur
- `Program.cs` : Configuration de l'authentification par cookies

## Notes techniques
- La déconnexion utilise la méthode POST pour la sécurité
- Le token anti-falsification est inclus automatiquement par ASP.NET Core
- La session cookie est supprimée lors de la déconnexion
- Redirection automatique vers la page de connexion après déconnexion 