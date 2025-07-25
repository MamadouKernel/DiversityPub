using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiversityPub.Migrations
{
    /// <inheritdoc />
    public partial class km02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Supprimer",
                table: "Utilisateurs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Utilisateurs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "MotDePasse", "Supprimer" },
                values: new object[] { "$2a$11$aDIrirmLYUWqV/NTu7fIa.0ODpVkWXzN7JHvsaYwB/jqjvXpe7hOy", 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Supprimer",
                table: "Utilisateurs");

            migrationBuilder.UpdateData(
                table: "Utilisateurs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "MotDePasse",
                value: "$2a$11$sFOF2Rh9vaUbmtJaw07e2etMUOxAApCLxD54jpf5BhO0J0zRr7Spm");
        }
    }
}
