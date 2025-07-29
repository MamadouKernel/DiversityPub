# Guide de Configuration Google Maps

## 🚨 **Problème Actuel**
La carte est blanche car Google Maps nécessite une clé API valide pour fonctionner.

## 🔧 **Solution Étape par Étape**

### **Étape 1: Obtenir une Clé API Google Maps**

#### **1. Aller sur Google Cloud Console**
- Ouvrez [Google Cloud Console](https://console.cloud.google.com/)
- Connectez-vous avec votre compte Google

#### **2. Créer un Projet**
- Cliquez sur le sélecteur de projet en haut
- Cliquez sur "Nouveau projet"
- Nommez-le "DiversityPub" ou similaire
- Cliquez sur "Créer"

#### **3. Activer l'API Maps JavaScript**
- Dans le menu, allez à "APIs et services" → "Bibliothèque"
- Recherchez "Maps JavaScript API"
- Cliquez dessus et cliquez sur "Activer"

#### **4. Créer une Clé API**
- Dans le menu, allez à "APIs et services" → "Identifiants"
- Cliquez sur "Créer des identifiants" → "Clé API"
- Copiez la clé générée

### **Étape 2: Configurer la Clé API**

#### **1. Remplacer dans le Code**
Dans `Views/AgentSurveillance/Index.cshtml`, remplacez :

```html
<!-- Remplacer cette ligne -->
<script src="https://maps.googleapis.com/maps/api/js?libraries=geometry"></script>

<!-- Par celle-ci (avec votre clé) -->
<script src="https://maps.googleapis.com/maps/api/js?key=VOTRE_CLE_API&libraries=geometry"></script>
```

#### **2. Exemple avec une Vraie Clé**
```html
<script src="https://maps.googleapis.com/maps/api/js?key=AIzaSyBxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx&libraries=geometry"></script>
```

### **Étape 3: Sécuriser la Clé API (Recommandé)**

#### **1. Restreindre la Clé**
- Dans Google Cloud Console, allez à "Identifiants"
- Cliquez sur votre clé API
- Dans "Restrictions d'application", sélectionnez "Référents HTTP"
- Ajoutez ces domaines :
  ```
  localhost:7169/*
  localhost:5000/*
  votre-domaine.com/*
  ```

#### **2. Restreindre les APIs**
- Dans "Restrictions d'API", sélectionnez "Restreindre la clé"
- Sélectionnez uniquement "Maps JavaScript API"

### **Étape 4: Tester la Configuration**

#### **1. Vérifier la Console**
- Ouvrez F12 dans votre navigateur
- Allez à l'onglet "Console"
- Vérifiez les messages :
  ```
  DOM chargé, initialisation de Google Maps...
  Google disponible: true
  Google Maps disponible: true
  Initialisation de Google Maps...
  Carte Google Maps créée avec succès
  ```

#### **2. Vérifier les Erreurs**
Si vous voyez des erreurs comme :
- `Google Maps n'est pas chargé !`
- `Google Maps non disponible après chargement`

Cela signifie que la clé API n'est pas correcte ou que l'API n'est pas activée.

## 🔍 **Diagnostic des Problèmes**

### **Problème 1: Carte Blanche**
**Cause** : Clé API manquante ou invalide
**Solution** : Vérifiez que la clé API est correcte et que l'API est activée

### **Problème 2: Erreur "Quota Exceeded"**
**Cause** : Limite d'utilisation dépassée
**Solution** : 
- Vérifiez l'utilisation dans Google Cloud Console
- Activez la facturation si nécessaire
- Les 200$ de crédit gratuit devraient suffire pour les tests

### **Problème 3: Erreur "Referer Not Allowed"**
**Cause** : Restrictions de domaine trop strictes
**Solution** : 
- Vérifiez les restrictions dans Google Cloud Console
- Ajoutez `localhost:7169/*` aux domaines autorisés

### **Problème 4: Pas d'Agents Visibles**
**Cause** : Problème avec l'API de positions
**Solution** :
- Vérifiez la console pour les erreurs
- Testez l'API `/AgentSurveillance/GetPositions` directement

## 🎯 **Test Rapide**

### **1. Test de l'API de Positions**
Ouvrez dans votre navigateur :
```
http://localhost:7169/AgentSurveillance/GetPositions
```

Vous devriez voir quelque chose comme :
```json
{
  "success": true,
  "data": [
    {
      "AgentId": "...",
      "AgentName": "Sephora AGBASSI",
      "IsOnline": true,
      "LastPosition": {
        "Latitude": 48.861596,
        "Longitude": 2.357195
      }
    }
  ]
}
```

### **2. Test de Google Maps**
Ouvrez la console (F12) et tapez :
```javascript
console.log('Google disponible:', typeof google !== 'undefined');
console.log('Google Maps disponible:', typeof google !== 'undefined' && typeof google.maps !== 'undefined');
```

## ✅ **Résultat Attendu**

Une fois configuré correctement, vous devriez voir :
- ✅ **Carte Google Maps** visible avec les contrôles
- ✅ **Marqueurs colorés** des agents (vert = en ligne, gris = hors ligne)
- ✅ **Info windows** au clic sur les marqueurs
- ✅ **Mise à jour en temps réel** des positions

## 🆘 **Solution Alternative Temporaire**

Si vous ne pouvez pas configurer Google Maps immédiatement, la carte affichera un placeholder avec un bouton "Réessayer" qui vous permettra de tester les autres fonctionnalités.

## 📞 **Aide Supplémentaire**

Si vous avez des problèmes :
1. **Vérifiez la console** (F12) pour les erreurs
2. **Testez l'API** de positions directement
3. **Vérifiez la clé API** dans Google Cloud Console
4. **Assurez-vous** que l'API Maps JavaScript est activée

La configuration de Google Maps est nécessaire pour voir la carte, mais le reste de l'application (tableau des agents, statuts, etc.) fonctionne déjà ! 🗺️ 