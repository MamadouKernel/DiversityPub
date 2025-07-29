# 📍 Format Coordonnées GPS

## ✅ **Format International Standard**

### **🎯 Format Correct**
Les coordonnées GPS utilisent le **format décimal** avec des **points** comme séparateur décimal :

```
Latitude: 5.360000
Longitude: -4.008300
```

### **❌ Format Incorrect**
Ne pas utiliser des virgules comme séparateur décimal :
```
❌ 5,360000, -4,008300
```

## 🗺️ **Exemples pour la Côte d'Ivoire**

### **Abidjan**
- **Latitude** : `5.360000`
- **Longitude** : `-4.008300`
- **Format affiché** : `5.360000 / -4.008300`

### **Man**
- **Latitude** : `6.850000`
- **Longitude** : `-7.350000`
- **Format affiché** : `6.850000 / -7.350000`

### **Bouaké**
- **Latitude** : `7.690000`
- **Longitude** : `-5.030000`
- **Format affiché** : `7.690000 / -5.030000`

## 🔧 **Implémentation Technique**

### **1. Affichage dans les Popups**
```javascript
// Format correct avec points décimaux
<p><strong>Position:</strong> ${agent.lastPosition.latitude.toFixed(6)} / ${agent.lastPosition.longitude.toFixed(6)}</p>
```

### **2. Affichage dans les Tableaux**
```csharp
// Format correct avec points décimaux
@lastPosition.Latitude.ToString("F6") / @lastPosition.Longitude.ToString("F6")
```

### **3. Format de Base de Données**
```sql
-- Stockage en base avec points décimaux
Latitude float NOT NULL,  -- 5.360000
Longitude float NOT NULL, -- -4.008300
```

## 📊 **Précision et Formatage**

### **Précision à 6 Décimales**
- **Format** : `F6` (6 décimales après le point)
- **Exemple** : `5.360000` (pas `5.36`)
- **Raison** : Précision GPS maximale

### **Séparateur de Coordonnées**
- **Ancien** : Virgule `,` (confus avec séparateur décimal)
- **Nouveau** : Barre oblique `/` (plus clair)
- **Exemple** : `5.360000 / -4.008300`

## 🌍 **Conventions Internationales**

### **ISO 6709 Standard**
- **Format décimal** : Points comme séparateur décimal
- **Précision** : 6 décimales recommandées
- **Séparateur** : Espace ou barre oblique entre lat/lng

### **Google Maps Format**
- **Exemple** : `5.360000, -4.008300`
- **Note** : Google Maps accepte les virgules comme séparateur de coordonnées

## ✅ **Résultat**

### **Affichage Cohérent**
- **Popups carte** : `5.360000 / -4.008300`
- **Tableaux** : `5.360000 / -4.008300`
- **Détails** : `5.360000` et `-4.008300` (colonnes séparées)

### **Format International**
- **Points décimaux** : Conforme aux standards internationaux
- **Précision** : 6 décimales pour la précision GPS
- **Lisibilité** : Barre oblique pour séparer lat/lng

Les coordonnées GPS sont maintenant affichées avec le **format correct** utilisant des points décimaux ! 📍 