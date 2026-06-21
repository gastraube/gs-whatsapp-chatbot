using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gschatbot.api.Migrations
{
    /// <inheritdoc />
    public partial class ClienteNumerosECpf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_NumeroWhatsApp",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NumeroWhatsApp",
                table: "Clientes");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Clientes",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Cpf",
                table: "Clientes",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClienteNumeros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Numero = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Principal = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClienteNumeros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClienteNumeros_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Cpf",
                table: "Clientes",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClienteNumeros_ClienteId",
                table: "ClienteNumeros",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClienteNumeros_Numero",
                table: "ClienteNumeros",
                column: "Numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClienteNumeros");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Cpf",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Cpf",
                table: "Clientes");

            migrationBuilder.UpdateData(
                table: "Clientes",
                keyColumn: "Nome",
                keyValue: null,
                column: "Nome",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Clientes",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NumeroWhatsApp",
                table: "Clientes",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_NumeroWhatsApp",
                table: "Clientes",
                column: "NumeroWhatsApp",
                unique: true);
        }
    }
}
