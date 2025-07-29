# 🗺️ Carte Interactive OpenStreetMap

## ✅ **Carte des Positions en Temps Réel**

### **🎯 Objectif**
Afficher les **positions GPS réelles** des agents connectés sur une carte interactive OpenStreetMap avec Leaflet.js.

### **🔧 Fonctionnalités**

#### **1. Carte Interactive**
```javascript
// Initialisation de la carte OpenStreetMap
map = L.map('map').setView([48.8566, 2.3522], 10);

// Couche OpenStreetMap
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '© OpenStreetMap contributors'
}).addTo(map);
```

#### **2. Marqueurs des Agents**
- **Marqueurs colorés** : Vert pour les agents en ligne, gris pour hors ligne
- **Popups informatives** : Détails de l'agent au clic
- **Mise à jour automatique** : Toutes les 30 secondes

#### **3. Données Affichées**
```javascript
// Structure des données d'agent
{
    agentId: "guid",
    agentName: "Prénom Nom",
    agentEmail: "email@example.com",
    agentPhone: "+33123456789",
    lastPosition: {
        latitude: 48.8566,
        longitude: 2.3522,
        precision: 10,
        horodatage: "2024-01-01T12:00:00"
    },
    isOnline: true,
    activeActivations: 2,
    openIncidents: 0
}
```

## 🚀 **Interface Utilisateur**

### **Section Carte**
```html
<!-- Carte des positions en temps réel -->
<div class="card mb-4">
    <div class="card-header">
        <h5 class="mb-0">
            <i class="fas fa-map-marker-alt"></i> Positions en Temps Réel
        </h5>
    </div>
    <div class="card-body">
        <div id="map" style="height: 400px; width: 100%;"></div>
    </div>
</div>
```

### **Marqueurs Personnalisés**
```css
.marker-icon.online {
    background: linear-gradient(135deg, #28a745, #20c997);
}

.marker-icon.offline {
    background: linear-gradient(135deg, #6c757d, #495057);
}
```

### **Popups Informatives**
```html
<div class="agent-popup">
    <h6><i class="fas fa-user"></i> Nom de l'Agent</h6>
    <p><strong>Email:</strong> email@example.com</p>
    <p><strong>Téléphone:</strong> +33123456789</p>
    <p><strong>Statut:</strong> 
        <span class="badge bg-success">En ligne</span>
    </p>
    <p><strong>Position:</strong> 48.856600, 2.352200</p>
    <p><strong>Précision:</strong> 10 m</p>
    <p><strong>Horodatage:</strong> 01/01/2024 12:00:00</p>
</div>
```

## 📊 **Mise à Jour Automatique**

### **Récupération des Données**
```javascript
// Mise à jour toutes les 30 secondes
setInterval(updatePositions, 30000);

async function updatePositions() {
    const response = await fetch('/AgentSurveillance/GetPositions');
    const result = await response.json();
    
    if (result.success) {
        agentsData = result.data;
        updateMapMarkers();
        updateStatistics();
    }
}
```

### **API Endpoint**
```csharp
// GET: AgentSurveillance/GetPositions
[HttpGet]
public async Task<IActionResult> GetPositions()
{
    var positions = await _context.AgentsTerrain
        .Include(at => at.Utilisateur)
        .Include(at => at.PositionsGPS.OrderByDescending(p => p.Horodatage).Take(1))
        .Select(at => new
        {
            agentId = at.Id,
            agentName = $"{at.Utilisateur.Prenom} {at.Utilisateur.Nom}",
            lastPosition = at.PositionsGPS.FirstOrDefault() != null ? new
            {
                latitude = at.PositionsGPS.FirstOrDefault().Latitude,
                longitude = at.PositionsGPS.FirstOrDefault().Longitude,
                precision = at.PositionsGPS.FirstOrDefault().Precision,
                horodatage = at.PositionsGPS.FirstOrDefault().Horodatage
            } : null,
            isOnline = at.EstConnecte
        })
        .ToListAsync();

    return Json(new { success = true, data = positions });
}
```

## 🎨 **Design et UX**

### **Couleurs et Thème**
- **Marqueurs en ligne** : Dégradé vert (#28a745 → #20c997)
- **Marqueurs hors ligne** : Dégradé gris (#6c757d → #495057)
- **Popups** : Design moderne avec badges Bootstrap
- **Carte** : OpenStreetMap avec attribution

### **Responsive Design**
- **Hauteur fixe** : 400px pour la carte
- **Largeur adaptative** : 100% du conteneur
- **Marqueurs** : 30px de diamètre avec icônes FontAwesome

### **Interactions**
- **Clic sur marqueur** : Affiche les détails de l'agent
- **Bouton centrer** : Centre la carte sur un agent spécifique
- **Zoom automatique** : Ajuste la vue pour tous les agents visibles

## ✅ **Résultat**

### **Carte Fonctionnelle**
- **OpenStreetMap** : Carte gratuite et fiable
- **Marqueurs dynamiques** : Statut en temps réel
- **Informations détaillées** : Popups avec toutes les données
- **Mise à jour automatique** : Positions actualisées toutes les 30s

### **Interface Intuitive**
- **Visualisation claire** : Agents facilement identifiables
- **Statut visuel** : Couleurs pour distinguer en ligne/hors ligne
- **Navigation simple** : Clic pour plus d'informations
- **Performance optimisée** : Chargement rapide et fluide

### **Données Réelles**
- **Positions GPS** : Coordonnées exactes du device
- **Statut synchrone** : Connexion/déconnexion en temps réel
- **Informations complètes** : Nom, email, téléphone, activations, incidents

La carte affiche maintenant les **positions réelles** des agents connectés sur OpenStreetMap ! 🗺️📍 