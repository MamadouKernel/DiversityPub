-- Script pour ajouter les nouvelles colonnes au modèle Feedback
-- Exécutez ce script dans votre base de données

-- Ajouter les colonnes pour la réponse admin
ALTER TABLE Feedbacks ADD ReponseAdmin NVARCHAR(MAX) NULL;
ALTER TABLE Feedbacks ADD DateReponseAdmin DATETIME2 NULL;
ALTER TABLE Feedbacks ADD AdminRepondant NVARCHAR(255) NULL;

-- Ajouter les colonnes pour le masquage
ALTER TABLE Feedbacks ADD EstMasque BIT NOT NULL DEFAULT 0;
ALTER TABLE Feedbacks ADD DateMasquage DATETIME2 NULL;
ALTER TABLE Feedbacks ADD AdminMasquant NVARCHAR(255) NULL;

-- Ajouter la colonne ActivationId si elle n'existe pas déjà
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Feedbacks' AND COLUMN_NAME = 'ActivationId')
BEGIN
    ALTER TABLE Feedbacks ADD ActivationId UNIQUEIDENTIFIER NULL;
END

-- Ajouter la contrainte de clé étrangère pour ActivationId si elle n'existe pas déjà
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = 'Feedbacks' AND COLUMN_NAME = 'ActivationId')
BEGIN
    ALTER TABLE Feedbacks ADD CONSTRAINT FK_Feedbacks_Activations FOREIGN KEY (ActivationId) REFERENCES Activations(Id) ON DELETE CASCADE;
END

PRINT 'Colonnes ajoutées avec succès au modèle Feedback'; 