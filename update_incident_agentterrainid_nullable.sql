-- Script pour rendre AgentTerrainId nullable dans la table Incidents
-- Ce script permet aux Admin/ChefProjet de créer des incidents sans agent terrain

-- 1. Supprimer la contrainte de clé étrangère existante
ALTER TABLE Incidents DROP CONSTRAINT FK_Incidents_AgentsTerrain_AgentTerrainId;

-- 2. Modifier la colonne pour la rendre nullable
ALTER TABLE Incidents ALTER COLUMN AgentTerrainId uniqueidentifier NULL;

-- 3. Recréer la contrainte de clé étrangère avec ON DELETE SET NULL
ALTER TABLE Incidents ADD CONSTRAINT FK_Incidents_AgentsTerrain_AgentTerrainId 
    FOREIGN KEY (AgentTerrainId) REFERENCES AgentsTerrain(Id) ON DELETE SET NULL;

-- 4. Mettre à jour l'index si nécessaire
-- (L'index existant devrait fonctionner avec les valeurs NULL) 