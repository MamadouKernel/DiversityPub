-- Script de création de la base de données DiversityPub
USE master;
GO

-- Supprimer la base de données si elle existe
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'DB_Diversity')
BEGIN
    DROP DATABASE DB_Diversity;
END
GO

-- Créer la nouvelle base de données
CREATE DATABASE DB_Diversity;
GO

USE DB_Diversity;
GO

-- Créer la table Utilisateurs
CREATE TABLE Utilisateurs (
    Id uniqueidentifier PRIMARY KEY,
    Nom nvarchar(max) NOT NULL,
    Prenom nvarchar(max) NOT NULL,
    Email nvarchar(max) NOT NULL,
    MotDePasse nvarchar(max) NOT NULL,
    Supprimer int NOT NULL DEFAULT 0,
    Role int NOT NULL
);

-- Créer la table AgentsTerrain
CREATE TABLE AgentsTerrain (
    Id uniqueidentifier PRIMARY KEY,
    UtilisateurId uniqueidentifier NOT NULL,
    Telephone nvarchar(max) NOT NULL,
    Email nvarchar(max) NOT NULL,
    FOREIGN KEY (UtilisateurId) REFERENCES Utilisateurs(Id) ON DELETE CASCADE
);

-- Créer la table Clients
CREATE TABLE Clients (
    Id uniqueidentifier PRIMARY KEY,
    UtilisateurId uniqueidentifier NOT NULL,
    RaisonSociale nvarchar(max) NOT NULL,
    Adresse nvarchar(max) NOT NULL,
    RegistreCommerce nvarchar(max) NOT NULL,
    NomDirigeant nvarchar(max) NOT NULL,
    NomContactPrincipal nvarchar(max) NOT NULL,
    TelephoneContactPrincipal nvarchar(max) NOT NULL,
    EmailContactPrincipal nvarchar(max) NOT NULL,
    FOREIGN KEY (UtilisateurId) REFERENCES Utilisateurs(Id) ON DELETE CASCADE
);

-- Créer la table Lieux
CREATE TABLE Lieux (
    Id uniqueidentifier PRIMARY KEY,
    Nom nvarchar(max) NOT NULL,
    Adresse nvarchar(max) NOT NULL
);

-- Créer la table Campagnes
CREATE TABLE Campagnes (
    Id uniqueidentifier PRIMARY KEY,
    Nom nvarchar(max) NOT NULL,
    Description nvarchar(max) NULL,
    DateDebut datetime2 NOT NULL,
    DateFin datetime2 NOT NULL,
    Statut int NOT NULL,
    ClientId uniqueidentifier NOT NULL,
    FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE CASCADE
);

-- Créer la table Activations
CREATE TABLE Activations (
    Id uniqueidentifier PRIMARY KEY,
    Nom nvarchar(max) NOT NULL,
    Description nvarchar(max) NULL,
    Instructions nvarchar(max) NULL,
    DateActivation datetime2 NOT NULL,
    HeureDebut time NOT NULL,
    HeureFin time NOT NULL,
    Statut int NOT NULL,
    PreuvesValidees bit NOT NULL DEFAULT 0,
    DateValidationPreuves datetime2 NULL,
    ValideParId uniqueidentifier NULL,
    CampagneId uniqueidentifier NOT NULL,
    LieuId uniqueidentifier NOT NULL,
    ResponsableId uniqueidentifier NULL,
    FOREIGN KEY (CampagneId) REFERENCES Campagnes(Id) ON DELETE CASCADE,
    FOREIGN KEY (LieuId) REFERENCES Lieux(Id) ON DELETE CASCADE,
    FOREIGN KEY (ResponsableId) REFERENCES AgentsTerrain(Id) ON DELETE NO ACTION
);

-- Créer la table de liaison ActivationAgentTerrain
CREATE TABLE ActivationAgentTerrain (
    ActivationsId uniqueidentifier NOT NULL,
    AgentsTerrainId uniqueidentifier NOT NULL,
    PRIMARY KEY (ActivationsId, AgentsTerrainId),
    FOREIGN KEY (ActivationsId) REFERENCES Activations(Id) ON DELETE CASCADE,
    FOREIGN KEY (AgentsTerrainId) REFERENCES AgentsTerrain(Id) ON DELETE NO ACTION
);

-- Créer la table Documents
CREATE TABLE Documents (
    Id uniqueidentifier PRIMARY KEY,
    Nom nvarchar(max) NOT NULL,
    Url nvarchar(max) NOT NULL,
    DateUpload datetime2 NOT NULL,
    AgentTerrainId uniqueidentifier NOT NULL,
    FOREIGN KEY (AgentTerrainId) REFERENCES AgentsTerrain(Id) ON DELETE NO ACTION
);

-- Créer la table Incidents
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

-- Créer la table PositionsGPS
CREATE TABLE PositionsGPS (
    Id uniqueidentifier PRIMARY KEY,
    AgentTerrainId uniqueidentifier NOT NULL,
    Latitude float NOT NULL,
    Longitude float NOT NULL,
    Horodatage datetime2 NOT NULL,
    Precision float NOT NULL DEFAULT 0.0,
    FOREIGN KEY (AgentTerrainId) REFERENCES AgentsTerrain(Id) ON DELETE NO ACTION
);

-- Créer la table Medias
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

-- Créer la table Feedbacks
CREATE TABLE Feedbacks (
    Id uniqueidentifier PRIMARY KEY,
    Titre nvarchar(max) NOT NULL,
    Description nvarchar(max) NOT NULL,
    Note int NOT NULL,
    DateCreation datetime2 NOT NULL,
    CampagneId uniqueidentifier NOT NULL,
    FOREIGN KEY (CampagneId) REFERENCES Campagnes(Id) ON DELETE CASCADE
);

-- Créer la table DemandesActivation
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
    FOREIGN KEY (CampagneId) REFERENCES Campagnes(Id) ON DELETE CASCADE,
    FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE CASCADE,
    FOREIGN KEY (LieuId) REFERENCES Lieux(Id) ON DELETE CASCADE,
    FOREIGN KEY (ReponduParId) REFERENCES Utilisateurs(Id) ON DELETE NO ACTION
);

-- Créer la table __EFMigrationsHistory
CREATE TABLE __EFMigrationsHistory (
    MigrationId nvarchar(150) PRIMARY KEY,
    ProductVersion nvarchar(32) NOT NULL
);

-- Insérer l'utilisateur admin par défaut
INSERT INTO Utilisateurs (Id, Nom, Prenom, Email, MotDePasse, Supprimer, Role)
VALUES ('11111111-1111-1111-1111-111111111111', 'Super', 'Admin', 'admin@diversitypub.ci', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 0, 0);

-- Insérer l'enregistrement de migration
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20250727074215_AddValidationProperties', '8.0.7');

PRINT 'Base de données créée avec succès !';
GO 