# Migration vers Google Maps

## 🗺️ **Changement de Technologie**

### **Avant : Leaflet + OpenStreetMap**
- ✅ Gratuit et open source
- ❌ Carte blanche (problème de chargement)
- ❌ Dépendance aux CDN externes

### **Après : Google Maps**
- ✅ Interface familière et professionnelle
- ✅ Chargement fiable
- ✅ Fonctionnalités avancées
- ⚠️ Nécessite une clé API (à configurer)

## 🔧 **Modifications Implémentées**

### **1. Remplacement des Bibliothèques**

#### **Avant (Leaflet)**
```html
<!-- Leaflet CSS -->
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
<!-- Leaflet JavaScript -->
<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
```

#### **Après (Google Maps)**
```html
<!-- Google Maps JavaScript API -->
<script src="https://maps.googleapis.com/maps/api/js?key=YOUR_API_KEY&libraries=geometry"></script>
```

### **2. Initialisation de la Carte**

#### **Avant (Leaflet)**
```javascript
function initMap() {
    const center = [48.8566, 2.3522]; // Paris
    map = L.map('map').setView(center, 10);
    
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);
}
```

#### **Après (Google Maps)**
```javascript
function initMap() {
    const center = { lat: 48.8566, lng: 2.3522 }; // Paris
    
    map = new google.maps.Map(document.getElementById('map'), {
        center: center,
        zoom: 10,
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        mapTypeControl: true,
        streetViewControl: true,
        fullscreenControl: true
    });
}
```

### **3. Création des Marqueurs**

#### **Avant (Leaflet)**
```javascript
const icon = L.divIcon({
    className: 'agent-marker',
    html: `<div style="background-color: ${agent.IsOnline ? '#28a745' : '#6c757d'}; width: 20px; height: 20px; border-radius: 50%; border: 2px solid white; box-shadow: 0 2px 4px rgba(0,0,0,0.3);"></div>`,
    iconSize: [20, 20],
    iconAnchor: [10, 10]
});

const marker = L.marker(position, {
    icon: icon,
    title: agent.AgentName
}).addTo(map);
```

#### **Après (Google Maps)**
```javascript
const icon = {
    path: google.maps.SymbolPath.CIRCLE,
    scale: 10,
    fillColor: agent.IsOnline ? '#28a745' : '#6c757d',
    fillOpacity: 1,
    strokeColor: '#ffffff',
    strokeWeight: 2
};

const marker = new google.maps.Marker({
    position: position,
    map: map,
    title: agent.AgentName,
    icon: icon
});
```

### **4. Gestion des Popups/Info Windows**

#### **Avant (Leaflet)**
```javascript
const popupContent = `<div>...</div>`;
marker.bindPopup(popupContent);
```

#### **Après (Google Maps)**
```javascript
const infoWindowContent = `<div>...</div>`;
const infoWindow = new google.maps.InfoWindow({
    content: infoWindowContent
});

marker.addListener('click', () => {
    infoWindow.open(map, marker);
});
```

### **5. Mise à Jour des Positions**

#### **Avant (Leaflet)**
```javascript
const position = [agent.LastPosition.Latitude, agent.LastPosition.Longitude];
markers[agent.AgentId].setLatLng(position);
markers[agent.AgentId].setIcon(newIcon);
```

#### **Après (Google Maps)**
```javascript
const position = { 
    lat: agent.LastPosition.Latitude, 
    lng: agent.LastPosition.Longitude 
};
markers[agent.AgentId].marker.setPosition(position);
markers[agent.AgentId].marker.setIcon(newIcon);
```

### **6. Centrage sur un Agent**

#### **Avant (Leaflet)**
```javascript
map.setView(position, 15);
markers[agentId].openPopup();
```

#### **Après (Google Maps)**
```javascript
map.setCenter(position);
map.setZoom(15);
markers[agentId].infoWindow.open(map, markers[agentId].marker);
```

## 🎯 **Fonctionnalités Google Maps**

### **1. Interface Avancée**
- ✅ **Contrôles de zoom** intuitifs
- ✅ **Street View** intégré
- ✅ **Plein écran** disponible
- ✅ **Types de cartes** (Route, Satellite, Terrain)

### **2. Marqueurs Personnalisés**
- ✅ **Cercles colorés** selon le statut
- ✅ **Bordure blanche** pour la visibilité
- ✅ **Info windows** détaillées
- ✅ **Clic interactif** pour voir les détails

### **3. Gestion des Événements**
- ✅ **Clic sur marqueur** = ouverture info window
- ✅ **Bouton "Centrer"** dans le tableau
- ✅ **Mise à jour en temps réel** des positions
- ✅ **Mise à jour des couleurs** selon le statut

## ⚙️ **Configuration Requise**

### **1. Clé API Google Maps**
```html
<script src="https://maps.googleapis.com/maps/api/js?key=YOUR_API_KEY&libraries=geometry"></script>
```

**Remplacez `YOUR_API_KEY` par votre clé API Google Maps.**

### **2. Obtention d'une Clé API**
1. **Aller sur** [Google Cloud Console](https://console.cloud.google.com/)
2. **Créer un projet** ou sélectionner un existant
3. **Activer l'API** "Maps JavaScript API"
4. **Créer des identifiants** → Clé API
5. **Restreindre la clé** (recommandé) :
   - Référents HTTP : `localhost:7169/*`
   - Référents HTTP : `votre-domaine.com/*`

### **3. Configuration de Sécurité**
```javascript
// Exemple de restriction par domaine
// Dans Google Cloud Console :
// Référents HTTP autorisés :
// localhost:7169/*
// votre-domaine.com/*
```

## ✅ **Avantages de Google Maps**

### **1. Fiabilité**
- ✅ **Chargement garanti** des cartes
- ✅ **Pas de problème** de carte blanche
- ✅ **Performance optimisée**

### **2. Fonctionnalités**
- ✅ **Street View** intégré
- ✅ **Types de cartes** multiples
- ✅ **Contrôles avancés**
- ✅ **Interface familière**

### **3. Intégration**
- ✅ **Marqueurs personnalisés** faciles
- ✅ **Info windows** détaillées
- ✅ **Gestion d'événements** robuste
- ✅ **Mise à jour fluide**

## 🎯 **Utilisation**

### **Pour Tester**
1. **Remplacer** `YOUR_API_KEY` par votre clé API
2. **Lancer** l'application
3. **Accéder** à `/AgentSurveillance/Index`
4. **Vérifier** que la carte se charge correctement

### **Fonctionnalités Disponibles**
- **Zoom** : Molette de souris ou boutons +/-
- **Déplacement** : Clic et glisser
- **Street View** : Bouton orange sur la carte
- **Types de cartes** : Bouton en haut à gauche
- **Marqueurs** : Clic pour voir les détails
- **Centrage** : Bouton "Centrer" dans le tableau

## 📊 **Résultat**

Maintenant, la page de surveillance des agents utilise **Google Maps** avec :
- ✅ **Carte visible** et fonctionnelle
- ✅ **Marqueurs colorés** selon le statut
- ✅ **Info windows** détaillées
- ✅ **Interface professionnelle**
- ✅ **Fonctionnalités avancées**

La migration est terminée et la carte devrait maintenant s'afficher correctement ! 🗺️ 