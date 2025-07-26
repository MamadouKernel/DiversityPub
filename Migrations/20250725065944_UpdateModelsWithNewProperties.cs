using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiversityPub.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelsWithNewProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Campagnes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Objectifs",
                table: "Campagnes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Statut",
                table: "Campagnes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Activations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "Activations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nom",
                table: "Activations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ResponsableId",
                table: "Activations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Utilisateurs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "MotDePasse",
                value: "$2a$11$A0ifypBFjqDd9YtBWi6d4OYGlzjlamIDTowRqGAcpzGe2fOXUxnd2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Campagnes");

            migrationBuilder.DropColumn(
                name: "Objectifs",
                table: "Campagnes");

            migrationBuilder.DropColumn(
                name: "Statut",
                table: "Campagnes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Activations");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "Activations");

            migrationBuilder.DropColumn(
                name: "Nom",
                table: "Activations");

            migrationBuilder.DropColumn(
                name: "ResponsableId",
                table: "Activations");

            migrationBuilder.UpdateData(
                table: "Utilisateurs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "MotDePasse",
                value: "$2a$11$aDIrirmLYUWqV/NTu7fIa.0ODpVkWXzN7JHvsaYwB/jqjvXpe7hOy");
        }
    }
}
