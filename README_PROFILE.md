# 📋 Guide d'Utilisation - Profil Utilisateur

## 🎯 Fonctionnalité Ajoutée

Tous les utilisateurs de l'application **DiversityPub** peuvent maintenant accéder à leur profil personnel pour voir et modifier leurs informations.

## 🔐 Accès au Profil

### Comment y accéder ?
1. **Connectez-vous** à l'application
2. **Cliquez sur votre nom** dans la barre de navigation (en haut à droite)
3. **Sélectionnez "Mon Profil"** dans le menu déroulant

### Interface du Profil

#### 📊 **Page d'accueil du profil**
- **Informations générales** : Nom, prénom, email, rôle, statut du compte
- **Profil spécifique** : 
  - **Client** : Raison sociale, téléphone, adresse, contact principal
  - **Agent Terrain** : Téléphone, email professionnel
- **Actions rapides** : Liens vers les fonctionnalités principales selon le rôle
- **Conseils de sécurité** : Bonnes pratiques pour la sécurité du compte

#### ✏️ **Modification du profil**
- **Informations personnelles** : Nom, prénom, email
- **Mot de passe** : Changement optionnel
- **Profil spécifique** : 
  - **Client** : Raison sociale, téléphone, adresse
  - **Agent Terrain** : Téléphone, email professionnel

#### 🔑 **Changement de mot de passe**
- **Validation** : Ancien mot de passe requis
- **Sécurité** : Nouveau mot de passe avec confirmation
- **Indicateur de force** : Barre de progression en temps réel
- **Conseils** : Bonnes pratiques pour un mot de passe sécurisé

## 🛡️ Sécurité

### Contrôles d'accès
- ✅ **Authentification requise** : Seuls les utilisateurs connectés peuvent accéder
- ✅ **Profil personnel uniquement** : Chaque utilisateur ne peut voir/modifier que son propre profil
- ✅ **Validation des données** : Vérification côté client et serveur
- ✅ **Protection CSRF** : Protection contre les attaques cross-site

### Bonnes pratiques
- 🔒 **Changement régulier** : Modifiez votre mot de passe périodiquement
- 🔒 **Mot de passe fort** : Utilisez au moins 8 caractères avec majuscules, minuscules, chiffres et symboles
- 🔒 **Confidentialité** : Ne partagez jamais vos identifiants
- 🔒 **Déconnexion** : Déconnectez-vous après chaque session

## 👥 Fonctionnalités par Rôle

### 👑 **Administrateur**
- Accès complet au profil personnel
- Gestion des utilisateurs (création, modification, suppression)
- Accès à toutes les fonctionnalités de l'application

### 👨‍💼 **Chef de Projet**
- Accès complet au profil personnel
- Gestion des campagnes et activations
- Suivi des agents terrain et clients

### 🏢 **Client**
- Accès complet au profil personnel
- Modification des informations client (raison sociale, téléphone, adresse)
- Accès au tableau de bord client
- Suivi des campagnes et activations

### 🚶‍♂️ **Agent Terrain**
- Accès complet au profil personnel
- Modification des informations agent (téléphone, email professionnel)
- Accès à l'interface mobile pour les activations
- Gestion des médias et incidents

## 🎨 Interface Utilisateur

### Design Responsive
- 📱 **Mobile-friendly** : Interface adaptée aux tablettes et smartphones
- 🎨 **Design cohérent** : Utilisation des couleurs de la charte graphique
- ⚡ **Performance** : Interface fluide et réactive

### Navigation Intuitive
- 🧭 **Menu déroulant** : Accès rapide depuis la barre de navigation
- 🔗 **Liens contextuels** : Actions adaptées au rôle de l'utilisateur
- 📍 **Breadcrumbs** : Navigation claire et intuitive

## 🔧 Fonctionnalités Techniques

### Validation des Données
- ✅ **Email** : Format valide requis
- ✅ **Mot de passe** : Minimum 6 caractères
- ✅ **Champs obligatoires** : Validation côté client et serveur
- ✅ **Confirmation** : Double vérification pour les mots de passe

### Gestion des Erreurs
- ⚠️ **Messages d'erreur** : Affichage clair des problèmes
- 🔄 **Récupération** : Possibilité de corriger les erreurs
- 📝 **Logs** : Traçabilité des modifications

### Performance
- ⚡ **Chargement rapide** : Optimisation des requêtes
- 🔄 **Mise à jour en temps réel** : Indicateurs dynamiques
- 📱 **Responsive** : Adaptation automatique à tous les écrans

## 📞 Support

### En cas de problème
1. **Vérifiez votre connexion** : Assurez-vous d'être connecté
2. **Contactez l'administrateur** : Pour les problèmes techniques
3. **Consultez les logs** : Pour le diagnostic des erreurs

### Améliorations futures
- 🔐 **Authentification à deux facteurs** : Sécurité renforcée
- 📧 **Notifications par email** : Confirmation des modifications
- 📊 **Historique des modifications** : Traçabilité complète
- 🎨 **Personnalisation** : Thèmes et préférences utilisateur

---

**DiversityPub** - Système de gestion des campagnes de terrain  
*Version 1.0 - Profil Utilisateur*  
*Développé avec ASP.NET Core MVC* 