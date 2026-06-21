using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gschatbot.api.Migrations
{
    /// <inheritdoc />
    public partial class PlanoAssistenciaERemoveEnderecoEspecialista : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EspecialistasEnderecos");

            migrationBuilder.AddColumn<int>(
                name: "PlanoAssistenciaId",
                table: "Agendamentos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoPagamento",
                table: "Agendamentos",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PlanosAssistencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosAssistencia", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClientePlanos",
                columns: table => new
                {
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    PlanoAssistenciaId = table.Column<int>(type: "int", nullable: false),
                    NumeroCarteirinha = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientePlanos", x => new { x.ClienteId, x.PlanoAssistenciaId });
                    table.ForeignKey(
                        name: "FK_ClientePlanos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientePlanos_PlanosAssistencia_PlanoAssistenciaId",
                        column: x => x.PlanoAssistenciaId,
                        principalTable: "PlanosAssistencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EspecialistaPlanos",
                columns: table => new
                {
                    EspecialistaId = table.Column<int>(type: "int", nullable: false),
                    PlanoAssistenciaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EspecialistaPlanos", x => new { x.EspecialistaId, x.PlanoAssistenciaId });
                    table.ForeignKey(
                        name: "FK_EspecialistaPlanos_Especialistas_EspecialistaId",
                        column: x => x.EspecialistaId,
                        principalTable: "Especialistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EspecialistaPlanos_PlanosAssistencia_PlanoAssistenciaId",
                        column: x => x.PlanoAssistenciaId,
                        principalTable: "PlanosAssistencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_PlanoAssistenciaId",
                table: "Agendamentos",
                column: "PlanoAssistenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientePlanos_PlanoAssistenciaId",
                table: "ClientePlanos",
                column: "PlanoAssistenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_EspecialistaPlanos_PlanoAssistenciaId",
                table: "EspecialistaPlanos",
                column: "PlanoAssistenciaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamentos_PlanosAssistencia_PlanoAssistenciaId",
                table: "Agendamentos",
                column: "PlanoAssistenciaId",
                principalTable: "PlanosAssistencia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agendamentos_PlanosAssistencia_PlanoAssistenciaId",
                table: "Agendamentos");

            migrationBuilder.DropTable(
                name: "ClientePlanos");

            migrationBuilder.DropTable(
                name: "EspecialistaPlanos");

            migrationBuilder.DropTable(
                name: "PlanosAssistencia");

            migrationBuilder.DropIndex(
                name: "IX_Agendamentos_PlanoAssistenciaId",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "PlanoAssistenciaId",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "TipoPagamento",
                table: "Agendamentos");

            migrationBuilder.CreateTable(
                name: "EspecialistasEnderecos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EnderecoId = table.Column<int>(type: "int", nullable: false),
                    EspecialistaId = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EspecialistasEnderecos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EspecialistasEnderecos_Enderecos_EnderecoId",
                        column: x => x.EnderecoId,
                        principalTable: "Enderecos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EspecialistasEnderecos_Especialistas_EspecialistaId",
                        column: x => x.EspecialistaId,
                        principalTable: "Especialistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EspecialistasEnderecos_EnderecoId",
                table: "EspecialistasEnderecos",
                column: "EnderecoId");

            migrationBuilder.CreateIndex(
                name: "IX_EspecialistasEnderecos_EspecialistaId",
                table: "EspecialistasEnderecos",
                column: "EspecialistaId");
        }
    }
}
