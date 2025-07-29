-- Script pour ajouter les champs DateCreation aux tables
-- Exécutez ce script directement sur votre base de données

-- Ajouter DateCreation à la table Clients
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Clients' AND COLUMN_NAME = 'DateCreation')
BEGIN
    ALTER TABLE Clients ADD DateCreation DATETIME2 NOT NULL DEFAULT GETUTCDATE();
END

-- Ajouter DateCreation à la table Campagnes
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Campagnes' AND COLUMN_NAME = 'DateCreation')
BEGIN
    ALTER TABLE Campagnes ADD DateCreation DATETIME2 NOT NULL DEFAULT GETUTCDATE();
END

-- Ajouter DateCreation à la table Activations
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Activations' AND COLUMN_NAME = 'DateCreation')
BEGIN
    ALTER TABLE Activations ADD DateCreation DATETIME2 NOT NULL DEFAULT GETUTCDATE();
END

-- Mettre à jour les enregistrements existants avec une date de création par défaut
-- pour les clients qui n'ont pas de DateCreation
UPDATE Clients SET DateCreation = GETUTCDATE() WHERE DateCreation IS NULL;

-- pour les campagnes qui n'ont pas de DateCreation
UPDATE Campagnes SET DateCreation = GETUTCDATE() WHERE DateCreation IS NULL;

-- pour les activations qui n'ont pas de DateCreation
UPDATE Activations SET DateCreation = GETUTCDATE() WHERE DateCreation IS NULL; 