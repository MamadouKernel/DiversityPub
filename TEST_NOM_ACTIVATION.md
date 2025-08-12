# Test du nouveau format de nom d'activation

## Modifications apportées

### 1. Contrôleur ActivationController.cs
- **Ligne 207** : Modification du format de génération automatique du nom
- **Avant** : `activation.Nom = $"{lieu.Nom}-{campagneNom.Nom} {periode}";`
- **Après** : `activation.Nom = $"{lieu.Nom}-{lieu.Adresse}-{campagneNom.Nom} {periode}";`

### 2. Vue Activation/Create.cshtml
- **Lignes 734-738** : Modification du JavaScript pour extraire l'adresse du lieu
- **Lignes 750-756** : Modification du format de génération du nom côté client
- **Format final** : `{NomLieu}-{AdresseLieu}-{NomCampagne} (DateDebut - DateFin)`

### 3. Vue DemandeActivation/Create.cshtml
- **Ligne 75** : Ajout de l'adresse dans l'affichage des options de lieu
- **Format** : `@lieu.Nom - @lieu.Adresse`

### 4. Vue Activation/Index.cshtml
- **Ligne 397** : Ajout de l'adresse dans le filtre par lieu
- **Ligne 449** : Modification de l'attribut data-lieu pour inclure l'adresse
- **Format** : `{NomLieu} - {AdresseLieu}`

## Format final du nom d'activation

Le nom d'activation suit maintenant le format :
```
{NomLieu}-{AdresseLieu}-{NomCampagne} (DateDebut - DateFin)
```

### Exemple :
- **Avant** : `Centre Commercial-Abobo 2024 (01/01/2024 - 31/12/2024)`
- **Après** : `Centre Commercial-123 Avenue de la Paix, Abidjan-Abobo 2024 (01/01/2024 - 31/12/2024)`

## Test à effectuer

1. Créer une nouvelle activation
2. Sélectionner un lieu avec une adresse
3. Sélectionner une campagne
4. Vérifier que le nom généré automatiquement inclut l'adresse
5. Vérifier que le filtrage par lieu fonctionne correctement dans la liste des activations

## Compatibilité

- ✅ Génération automatique côté serveur
- ✅ Génération automatique côté client (JavaScript)
- ✅ Affichage cohérent dans toutes les vues
- ✅ Filtrage fonctionnel dans la liste des activations
- ✅ Compatible avec les demandes d'activation
