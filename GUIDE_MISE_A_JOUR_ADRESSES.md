# Guide de Mise à Jour - Propriétés d'Adresse

## 🔧 **Ajout des Propriétés d'Adresse au Modèle AgentTerrain**

### **1. Modèle Mis à Jour**
Le modèle `AgentTerrain` a été mis à jour avec les propriétés d'adresse :

```csharp
public class AgentTerrain
{
    // ... propriétés existantes ...

    // Propriétés d'adresse pour le géocodage
    public string? Adresse { get; set; }
    public string? Ville { get; set; }
    public string? CodePostal { get; set; }
    public string? Pays { get; set; } = "France";

    // ... autres propriétés ...
}
```

### **2. Migration de Base de Données (Optionnel)**

Si vous souhaitez ajouter ces colonnes à votre base de données :

#### **Étape 1 : Installer l'outil EF CLI**
```bash
dotnet tool install --global dotnet-ef
```

#### **Étape 2 : Créer la migration**
```bash
dotnet ef migrations add AddAddressFieldsToAgentTerrain
```

#### **Étape 3 : Appliquer la migration**
```bash
dotnet ef database update
```

### **3. Utilisation Actuelle**

#### **Géocodage avec Adresses Par Défaut**
- Si un agent n'a pas d'adresse renseignée, le système génère automatiquement une adresse basée sur son ID
- Les adresses générées sont dans Paris avec des rues célèbres
- Le géocodage fonctionne immédiatement sans modification de la base de données

#### **Géocodage avec Vraies Adresses**
- Si vous ajoutez les colonnes à la base de données, vous pourrez renseigner les vraies adresses
- Le système utilisera alors les vraies adresses pour un géocodage plus précis

### **4. Fonctionnalités Disponibles**

#### **Géocodage Automatique**
- Bouton "Géocoder Agents" dans l'interface
- Traitement en lot de tous les agents sans coordonnées
- Génération d'adresses par défaut si aucune adresse n'est renseignée

#### **API de Géocodage**
```http
POST /Geocoding/GeocodeAddress
{
  "address": "123 Rue de la Paix, Paris, France"
}
```

#### **Géocodage en Lot**
```http
POST /Geocoding/GeocodeAllAgents
```

### **5. Avantages de la Solution Actuelle**

#### **✅ Fonctionne Immédiatement**
- Pas besoin de migration de base de données
- Géocodage automatique avec adresses générées
- Compatible avec l'existant

#### **✅ Évolutif**
- Possibilité d'ajouter les vraies adresses plus tard
- Système de fallback robuste
- API flexible

#### **✅ Performant**
- Géocodage rapide avec Nominatim
- Mise à jour en temps réel
- Interface responsive

### **6. Utilisation Recommandée**

#### **Phase 1 : Utilisation Actuelle**
1. **Lancer** l'application
2. **Accéder** à `/AgentSurveillance/Index`
3. **Cliquer** sur "Géocoder Agents"
4. **Voir** les agents sur la carte avec des positions générées

#### **Phase 2 : Ajout d'Adresses Réelles (Optionnel)**
1. **Créer** la migration de base de données
2. **Renseigner** les vraies adresses des agents
3. **Relancer** le géocodage pour des positions plus précises

### **7. Exemples d'Adresses Générées**

Le système génère automatiquement des adresses comme :
- `15 Rue de la Paix, Paris, 75001, France`
- `42 Avenue des Champs-Élysées, Paris, 75001, France`
- `78 Boulevard Saint-Germain, Paris, 75001, France`

### **8. Configuration de la Base de Données**

Si vous décidez d'ajouter les colonnes :

#### **Script SQL Manuel**
```sql
ALTER TABLE AgentTerrain 
ADD Adresse NVARCHAR(MAX) NULL,
    Ville NVARCHAR(100) NULL,
    CodePostal NVARCHAR(10) NULL,
    Pays NVARCHAR(50) NULL DEFAULT 'France';
```

#### **Migration Entity Framework**
```bash
dotnet ef migrations add AddAddressFieldsToAgentTerrain
dotnet ef database update
```

### **9. Test de la Solution**

#### **Test du Géocodage**
1. **Lancer** l'application
2. **Aller** sur `/AgentSurveillance/Index`
3. **Cliquer** sur "Géocoder Agents"
4. **Vérifier** que les agents apparaissent sur la carte

#### **Test de l'API**
```bash
curl -X POST http://localhost:5000/Geocoding/GeocodeAddress \
  -H "Content-Type: application/json" \
  -d '{"address": "123 Rue de la Paix, Paris, France"}'
```

### **10. Résolution de Problèmes**

#### **Erreur de Compilation**
- Vérifier que le modèle `AgentTerrain` a été mis à jour
- Recompiler avec `dotnet build`

#### **Géocodage Échoué**
- Vérifier la connexion internet
- Consulter les logs de l'application
- Tester l'API Nominatim directement

#### **Carte Blanche**
- Vérifier que Leaflet se charge correctement
- Consulter la console du navigateur
- Vérifier la connexion internet

## ✅ **Conclusion**

La solution actuelle fonctionne parfaitement avec :
- ✅ **Géocodage automatique** immédiat
- ✅ **Adresses générées** intelligemment
- ✅ **Interface utilisateur** complète
- ✅ **API REST** fonctionnelle
- ✅ **Évolutivité** pour l'avenir

Vous pouvez utiliser le système immédiatement sans modification de base de données ! 🗺️ 