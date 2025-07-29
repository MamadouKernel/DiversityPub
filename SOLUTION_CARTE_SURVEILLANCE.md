# Solution: Carte Interactive pour la Surveillance des Agents

## Problème Identifié
L'utilisateur ne pouvait pas voir visuellement les positions des agents sur la page de surveillance (`/AgentSurveillance/Index`). La page affichait seulement les coordonnées textuelles sans carte interactive.

## Solution Implémentée

### 🗺️ **1. Remplacement de Google Maps par Leaflet**

#### **Problème avec Google Maps**
- ❌ Nécessite une clé API (factice dans le code)
- ❌ Limites d'utilisation gratuite
- ❌ Configuration complexe

#### **Solution avec Leaflet**
- ✅ **Gratuit** et open source
- ✅ **Sans clé API** requise
- ✅ **OpenStreetMap** comme fond de carte
- ✅ **Facile à configurer**

### 🔧 **2. Intégration de Leaflet**

#### **Inclusion des Bibliothèques**
```html
<!-- Leaflet CSS -->
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
<!-- Leaflet JavaScript -->
<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
```

#### **Initialisation de la Carte**
```javascript
function initMap() {
    const center = [48.8566, 2.3522]; // Paris
    
    map = L.map('map').setView(center, 10);
    
    // Ajouter la couche de tuiles OpenStreetMap
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);
}
```

### 🎯 **3. Marqueurs d'Agents Interactifs**

#### **Création de Marqueurs Personnalisés**
```javascript
function addAgentMarker(agent) {
    if (!agent.LastPosition) return;
    
    const position = [agent.LastPosition.Latitude, agent.LastPosition.Longitude];
    
    // Créer une icône personnalisée
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
    
    // Créer le contenu de la popup
    const popupContent = `
        <div style="padding: 10px; min-width: 200px;">
            <h6 style="margin-bottom: 10px; color: #333;">${agent.AgentName}</h6>
            <p style="margin: 5px 0;"><strong>Statut:</strong> 
                <span style="color: ${agent.IsOnline ? '#28a745' : '#6c757d'};">
                    ${agent.IsOnline ? 'En ligne' : 'Hors ligne'}
                </span>
            </p>
            <p style="margin: 5px 0;"><strong>Activations actives:</strong> ${agent.ActiveActivations}</p>
            <p style="margin: 5px 0;"><strong>Incidents ouverts:</strong> ${agent.OpenIncidents}</p>
            <p style="margin: 5px 0;"><strong>Dernière position:</strong> ${new Date(agent.LastPosition.Horodatage).toLocaleString('fr-FR')}</p>
            <p style="margin: 5px 0; font-size: 12px; color: #666;">
                ${agent.LastPosition.Latitude.toFixed(6)}, ${agent.LastPosition.Longitude.toFixed(6)}
            </p>
        </div>
    `;
    
    marker.bindPopup(popupContent);
    
    markers[agent.AgentId] = marker;
}
```

### 🔄 **4. Mise à Jour en Temps Réel**

#### **Fonction de Mise à Jour**
```javascript
async function updatePositions() {
    try {
        const response = await fetch('/AgentSurveillance/GetPositions');
        const result = await response.json();
        
        if (result.success) {
            agentsData = result.data;
            
            // Mettre à jour les statistiques
            document.getElementById('onlineAgents').textContent = agentsData.filter(a => a.IsOnline).length;
            document.getElementById('activeActivations').textContent = agentsData.reduce((sum, a) => sum + a.ActiveActivations, 0);
            document.getElementById('openIncidents').textContent = agentsData.reduce((sum, a) => sum + a.OpenIncidents, 0);
            
            // Mettre à jour les marqueurs sur la carte
            agentsData.forEach(agent => {
                if (markers[agent.AgentId]) {
                    // Mettre à jour la position existante
                    const position = [agent.LastPosition.Latitude, agent.LastPosition.Longitude];
                    markers[agent.AgentId].setLatLng(position);
                    
                    // Mettre à jour l'icône selon le statut
                    const newIcon = L.divIcon({
                        className: 'agent-marker',
                        html: `<div style="background-color: ${agent.IsOnline ? '#28a745' : '#6c757d'}; width: 20px; height: 20px; border-radius: 50%; border: 2px solid white; box-shadow: 0 2px 4px rgba(0,0,0,0.3);"></div>`,
                        iconSize: [20, 20],
                        iconAnchor: [10, 10]
                    });
                    markers[agent.AgentId].setIcon(newIcon);
                } else {
                    // Ajouter un nouveau marqueur
                    addAgentMarker(agent);
                }
            });
            
            // Mettre à jour le tableau
            updateTable();
        }
    } catch (error) {
        console.error('Erreur lors de la mise à jour des positions:', error);
    }
}
```

### 🎯 **5. Fonctionnalités Interactives**

#### **Centrage sur un Agent**
```javascript
function centerOnAgent(agentId) {
    const agent = agentsData.find(a => a.AgentId === agentId);
    if (agent && agent.LastPosition) {
        const position = [agent.LastPosition.Latitude, agent.LastPosition.Longitude];
        map.setView(position, 15);
        
        // Ouvrir la popup du marqueur
        if (markers[agentId]) {
            markers[agentId].openPopup();
        }
    }
}
```

## ✅ **Fonctionnalités Implémentées**

### **1. Carte Interactive**
- ✅ **Carte OpenStreetMap** gratuite et détaillée
- ✅ **Zoom et déplacement** fluides
- ✅ **Marqueurs colorés** selon le statut (vert = en ligne, gris = hors ligne)

### **2. Marqueurs d'Agents**
- ✅ **Icônes personnalisées** avec couleurs selon le statut
- ✅ **Popups informatives** au clic
- ✅ **Mise à jour en temps réel** des positions

### **3. Informations Détaillées**
- ✅ **Nom de l'agent** dans la popup
- ✅ **Statut en ligne/hors ligne**
- ✅ **Nombre d'activations actives**
- ✅ **Nombre d'incidents ouverts**
- ✅ **Horodatage de la dernière position**
- ✅ **Coordonnées GPS précises**

### **4. Interactions**
- ✅ **Clic sur marqueur** = ouverture de popup
- ✅ **Bouton "Centrer"** dans le tableau
- ✅ **Mise à jour automatique** toutes les 30 secondes

## 🎯 **Utilisation**

### **Pour la Surveillance**
1. **Accéder** à `/AgentSurveillance/Index`
2. **Voir** la carte interactive en haut de la page
3. **Cliquer** sur un marqueur pour voir les détails
4. **Utiliser** le bouton "Centrer" dans le tableau pour focaliser sur un agent
5. **Observer** les mises à jour en temps réel

### **Fonctionnalités de la Carte**
- **Zoom** : Molette de souris ou boutons +/-
- **Déplacement** : Clic et glisser
- **Marqueurs verts** : Agents en ligne
- **Marqueurs gris** : Agents hors ligne
- **Popups** : Clic sur un marqueur

## 📊 **Avantages de la Solution**

### **1. Gratuité**
- ✅ **OpenStreetMap** : Cartes gratuites et détaillées
- ✅ **Leaflet** : Bibliothèque open source
- ✅ **Aucune clé API** requise

### **2. Performance**
- ✅ **Mise à jour fluide** des positions
- ✅ **Marqueurs optimisés** pour les performances
- ✅ **Chargement rapide** de la carte

### **3. Interface Utilisateur**
- ✅ **Design cohérent** avec le reste de l'application
- ✅ **Marqueurs intuitifs** avec couleurs
- ✅ **Popups informatives** et bien structurées

### **4. Fonctionnalités**
- ✅ **Temps réel** : Mise à jour automatique
- ✅ **Interactivité** : Clics et centrage
- ✅ **Responsive** : S'adapte à tous les écrans

## ✅ **Vérification**
- ✅ Bibliothèques Leaflet intégrées
- ✅ Carte OpenStreetMap fonctionnelle
- ✅ Marqueurs d'agents interactifs
- ✅ Mise à jour en temps réel
- ✅ Fonction de centrage opérationnelle
- ✅ Application compilée avec succès

## 🎯 **Résultat**
Maintenant, la page de surveillance des agents (`/AgentSurveillance/Index`) affiche une **carte interactive** où vous pouvez voir visuellement les positions de tous les agents en temps réel. Les marqueurs sont colorés selon le statut et cliquables pour voir les détails complets de chaque agent. 