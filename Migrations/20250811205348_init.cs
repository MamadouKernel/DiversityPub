using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiversityPub.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lieux",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adresse = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lieux", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotDePasse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supprimer = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentsTerrain",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstConnecte = table.Column<bool>(type: "bit", nullable: false),
                    DerniereConnexion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DerniereDeconnexion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentsTerrain", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentsTerrain_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RaisonSociale = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adresse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistreCommerce = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomDirigeant = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomContactPrincipal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TelephoneContactPrincipal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailContactPrincipal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clients_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateUpload = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgentTerrainId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_AgentsTerrain_AgentTerrainId",
                        column: x => x.AgentTerrainId,
                        principalTable: "AgentsTerrain",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PositionsGPS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Horodatage = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Precision = table.Column<double>(type: "float", nullable: false),
                    AgentTerrainId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionsGPS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PositionsGPS_AgentsTerrain_AgentTerrainId",
                        column: x => x.AgentTerrainId,
                        principalTable: "AgentsTerrain",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Campagnes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Objectifs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campagnes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campagnes_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Activations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateActivation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HeureDebut = table.Column<TimeSpan>(type: "time", nullable: false),
                    HeureFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MotifSuspension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateSuspension = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreuvesValidees = table.Column<bool>(type: "bit", nullable: false),
                    DateValidationPreuves = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValideParId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CampagneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activations_AgentsTerrain_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "AgentsTerrain",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Activations_Campagnes_CampagneId",
                        column: x => x.CampagneId,
                        principalTable: "Campagnes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Activations_Lieux_LieuId",
                        column: x => x.LieuId,
                        principalTable: "Lieux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Activations_Utilisateurs_ValideParId",
                        column: x => x.ValideParId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DemandesActivation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateActivation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HeureDebut = table.Column<TimeSpan>(type: "time", nullable: false),
                    HeureFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampagneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateDemande = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false),
                    MotifRefus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateReponse = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReponduParId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandesActivation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemandesActivation_Campagnes_CampagneId",
                        column: x => x.CampagneId,
                        principalTable: "Campagnes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DemandesActivation_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DemandesActivation_Lieux_LieuId",
                        column: x => x.LieuId,
                        principalTable: "Lieux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DemandesActivation_Utilisateurs_ReponduParId",
                        column: x => x.ReponduParId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivationAgentTerrain",
                columns: table => new
                {
                    ActivationsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentsTerrainId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivationAgentTerrain", x => new { x.ActivationsId, x.AgentsTerrainId });
                    table.ForeignKey(
                        name: "FK_ActivationAgentTerrain_Activations_ActivationsId",
                        column: x => x.ActivationsId,
                        principalTable: "Activations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivationAgentTerrain_AgentsTerrain_AgentsTerrainId",
                        column: x => x.AgentsTerrainId,
                        principalTable: "AgentsTerrain",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<int>(type: "int", nullable: false),
                    Commentaire = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateFeedback = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CampagneId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActivationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReponseAdmin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateReponseAdmin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminRepondant = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstMasque = table.Column<bool>(type: "bit", nullable: false),
                    DateMasquage = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminMasquant = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Activations_ActivationId",
                        column: x => x.ActivationId,
                        principalTable: "Activations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Campagnes_CampagneId",
                        column: x => x.CampagneId,
                        principalTable: "Campagnes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priorite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateResolution = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CommentaireResolution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgentTerrainId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActivationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_Activations_ActivationId",
                        column: x => x.ActivationId,
                        principalTable: "Activations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Incidents_AgentsTerrain_AgentTerrainId",
                        column: x => x.AgentTerrainId,
                        principalTable: "AgentsTerrain",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Medias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateUpload = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Valide = table.Column<bool>(type: "bit", nullable: false),
                    DateValidation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValideParId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CommentaireValidation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgentTerrainId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medias_Activations_ActivationId",
                        column: x => x.ActivationId,
                        principalTable: "Activations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Medias_AgentsTerrain_AgentTerrainId",
                        column: x => x.AgentTerrainId,
                        principalTable: "AgentsTerrain",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Medias_Utilisateurs_ValideParId",
                        column: x => x.ValideParId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Utilisateurs",
                columns: new[] { "Id", "Email", "MotDePasse", "Nom", "Prenom", "Role", "Supprimer" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "admin@diversitypub.ci", "$2a$11$3EuiBTa8JYTS1DmxK1oA3.TX3wpuLZjDH2Dz2I.0r5U3.SQO4HNKW", "Super", "Admin", 1, 0 });

            migrationBuilder.CreateIndex(
                name: "IX_ActivationAgentTerrain_AgentsTerrainId",
                table: "ActivationAgentTerrain",
                column: "AgentsTerrainId");

            migrationBuilder.CreateIndex(
                name: "IX_Activations_CampagneId",
                table: "Activations",
                column: "CampagneId");

            migrationBuilder.CreateIndex(
                name: "IX_Activations_LieuId",
                table: "Activations",
                column: "LieuId");

            migrationBuilder.CreateIndex(
                name: "IX_Activations_ResponsableId",
                table: "Activations",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Activations_ValideParId",
                table: "Activations",
                column: "ValideParId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentsTerrain_UtilisateurId",
                table: "AgentsTerrain",
                column: "UtilisateurId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Campagnes_ClientId",
                table: "Campagnes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_UtilisateurId",
                table: "Clients",
                column: "UtilisateurId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DemandesActivation_CampagneId",
                table: "DemandesActivation",
                column: "CampagneId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesActivation_ClientId",
                table: "DemandesActivation",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesActivation_LieuId",
                table: "DemandesActivation",
                column: "LieuId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandesActivation_ReponduParId",
                table: "DemandesActivation",
                column: "ReponduParId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_AgentTerrainId",
                table: "Documents",
                column: "AgentTerrainId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_ActivationId",
                table: "Feedbacks",
                column: "ActivationId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_CampagneId",
                table: "Feedbacks",
                column: "CampagneId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_ActivationId",
                table: "Incidents",
                column: "ActivationId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_AgentTerrainId",
                table: "Incidents",
                column: "AgentTerrainId");

            migrationBuilder.CreateIndex(
                name: "IX_Medias_ActivationId",
                table: "Medias",
                column: "ActivationId");

            migrationBuilder.CreateIndex(
                name: "IX_Medias_AgentTerrainId",
                table: "Medias",
                column: "AgentTerrainId");

            migrationBuilder.CreateIndex(
                name: "IX_Medias_ValideParId",
                table: "Medias",
                column: "ValideParId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionsGPS_AgentTerrainId",
                table: "PositionsGPS",
                column: "AgentTerrainId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivationAgentTerrain");

            migrationBuilder.DropTable(
                name: "DemandesActivation");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "Medias");

            migrationBuilder.DropTable(
                name: "PositionsGPS");

            migrationBuilder.DropTable(
                name: "Activations");

            migrationBuilder.DropTable(
                name: "AgentsTerrain");

            migrationBuilder.DropTable(
                name: "Campagnes");

            migrationBuilder.DropTable(
                name: "Lieux");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Utilisateurs");
        }
    }
}
