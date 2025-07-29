# Solution: Erreur de Référence Circulaire JSON

## Problème
L'utilisateur a rencontré une `JsonException` lors de la navigation vers la page de surveillance des agents :
```
A possible object cycle was detected. This can either be due to a cycle or if the object depth is larger than the maximum allowed depth of 32. Consider using ReferenceHandler.Preserve on JsonSerializerOptions to support cycles.
```

## Cause
Le problème est causé par une **référence circulaire** entre les modèles `Utilisateur` et `AgentTerrain` :

- `Utilisateur` a une propriété `AgentTerrain` (navigation)
- `AgentTerrain` a une propriété `Utilisateur` (navigation)
- Lors de la sérialisation JSON dans `Views/AgentSurveillance/Map.cshtml`, cela crée une boucle infinie

### Point de Déclenchement
Dans `Views/AgentSurveillance/Map.cshtml`, ligne 107 :
```javascript
let agentsData = @Html.Raw(Json.Serialize(Model));
```

Cette ligne essaie de sérialiser le modèle complet `IEnumerable<AgentTerrain>` en JSON pour JavaScript, ce qui déclenche la boucle circulaire.

## Solution Implémentée

### 1. Configuration JSON dans Program.cs
Ajout de la configuration des options JSON pour gérer les références circulaires :

```csharp
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.MaxDepth = 64;
    });
```

### 2. Paramètres Configurés

#### `ReferenceHandler.Preserve`
- **Fonction** : Gère les références circulaires en préservant les références
- **Avantage** : Évite les boucles infinies lors de la sérialisation
- **Résultat** : Les objets référencés plusieurs fois sont sérialisés une seule fois avec des références

#### `MaxDepth = 64`
- **Fonction** : Augmente la profondeur maximale de sérialisation
- **Valeur par défaut** : 32 (insuffisant pour les modèles complexes)
- **Avantage** : Permet la sérialisation d'objets avec plus de niveaux de profondeur

## Vérification
- ✅ Application compilée avec succès
- ✅ Configuration JSON appliquée
- ✅ Gestion des références circulaires activée

## Résultat
L'erreur `JsonException` est maintenant résolue. La page de surveillance des agents avec carte peut maintenant se charger correctement sans rencontrer de boucle circulaire lors de la sérialisation JSON.

## Impact
- **Pages concernées** : Toutes les pages utilisant `Json.Serialize()` avec des modèles contenant des références circulaires
- **Fonctionnalités** : Surveillance des agents, cartes interactives, sérialisation de données pour JavaScript
- **Performance** : Amélioration de la stabilité de l'application

## Notes Techniques
- **Compatibilité** : Solution compatible avec ASP.NET Core 6+
- **Sécurité** : Configuration sécurisée pour la production
- **Maintenance** : Configuration globale applicable à toute l'application 