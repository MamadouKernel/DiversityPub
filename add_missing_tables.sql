-- Script pour ajouter les tables manquantes
USE DB_Diversity;
GO

-- Créer la table Medias
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Medias')
BEGIN
    CREATE TABLE Medias (
        Id uniqueidentifier PRIMARY KEY,
        Nom nvarchar(max) NOT NULL,
        Url nvarchar(max) NOT NULL,
        Type int NOT NULL,
        DateUpload datetime2 NOT NULL,
        Description nvarchar(max) NOT NULL DEFAULT '',
        Valide bit NOT NULL DEFAULT 0,
        DateValidation datetime2 NULL,
        ValideParId uniqueidentifier NULL,
        CommentaireValidation nvarchar(max) NULL,
        AgentTerrainId uniqueidentifier NOT NULL,
        ActivationId uniqueidentifier NULL,
        FOREIGN KEY (AgentTerrainId) REFERENCES AgentsTerrain(Id) ON DELETE NO ACTION,
        FOREIGN KEY (ActivationId) REFERENCES Activations(Id) ON DELETE NO ACTION
    );
    PRINT 'Table Medias créée avec succès';
END

-- Créer la table Incidents
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Incidents')
BEGIN
    CREATE TABLE Incidents (
        Id uniqueidentifier PRIMARY KEY,
        Titre nvarchar(max) NOT NULL,
        Description nvarchar(max) NOT NULL,
        Priorite nvarchar(max) NOT NULL DEFAULT 'Normale',
        Statut nvarchar(max) NOT NULL DEFAULT 'Ouvert',
        DateCreation datetime2 NOT NULL,
        DateResolution datetime2 NULL,
        CommentaireResolution nvarchar(max) NULL,
        AgentTerrainId uniqueidentifier NOT NULL,
        ActivationId uniqueidentifier NULL,
        FOREIGN KEY (AgentTerrainId) REFERENCES AgentsTerrain(Id) ON DELETE NO ACTION,
        FOREIGN KEY (ActivationId) REFERENCES Activations(Id) ON DELETE NO ACTION
    );
    PRINT 'Table Incidents créée avec succès';
END

-- Créer la table Feedbacks
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Feedbacks')
BEGIN
    CREATE TABLE Feedbacks (
        Id uniqueidentifier PRIMARY KEY,
        Titre nvarchar(max) NOT NULL,
        Description nvarchar(max) NOT NULL,
        Note int NOT NULL,
        DateCreation datetime2 NOT NULL,
        CampagneId uniqueidentifier NOT NULL,
        FOREIGN KEY (CampagneId) REFERENCES Campagnes(Id) ON DELETE CASCADE
    );
    PRINT 'Table Feedbacks créée avec succès';
END

-- Créer la table DemandesActivation avec des contraintes Restrict
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DemandesActivation')
BEGIN
    CREATE TABLE DemandesActivation (
        Id uniqueidentifier PRIMARY KEY,
        Titre nvarchar(max) NOT NULL,
        Description nvarchar(max) NOT NULL,
        DateDemande datetime2 NOT NULL,
        Statut int NOT NULL,
        CampagneId uniqueidentifier NOT NULL,
        ClientId uniqueidentifier NOT NULL,
        LieuId uniqueidentifier NOT NULL,
        ReponduParId uniqueidentifier NULL,
        FOREIGN KEY (CampagneId) REFERENCES Campagnes(Id) ON DELETE NO ACTION,
        FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE NO ACTION,
        FOREIGN KEY (LieuId) REFERENCES Lieux(Id) ON DELETE NO ACTION,
        FOREIGN KEY (ReponduParId) REFERENCES Utilisateurs(Id) ON DELETE NO ACTION
    );
    PRINT 'Table DemandesActivation créée avec succès';
END

-- Créer la table de liaison ActivationAgentTerrain
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ActivationAgentTerrain')
BEGIN
    CREATE TABLE ActivationAgentTerrain (
        ActivationsId uniqueidentifier NOT NULL,
        AgentsTerrainId uniqueidentifier NOT NULL,
        PRIMARY KEY (ActivationsId, AgentsTerrainId),
        FOREIGN KEY (ActivationsId) REFERENCES Activations(Id) ON DELETE CASCADE,
        FOREIGN KEY (AgentsTerrainId) REFERENCES AgentsTerrain(Id) ON DELETE NO ACTION
    );
    PRINT 'Table ActivationAgentTerrain créée avec succès';
END

-- Insérer l'utilisateur admin par défaut s'il n'existe pas
IF NOT EXISTS (SELECT * FROM Utilisateurs WHERE Email = 'admin@diversitypub.ci')
BEGIN
    INSERT INTO Utilisateurs (Id, Nom, Prenom, Email, MotDePasse, Supprimer, Role)
    VALUES ('11111111-1111-1111-1111-111111111111', 'Super', 'Admin', 'admin@diversitypub.ci', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 0, 0);
    PRINT 'Utilisateur admin créé avec succès';
END

PRINT 'Toutes les tables manquantes ont été créées avec succès !';
GO 