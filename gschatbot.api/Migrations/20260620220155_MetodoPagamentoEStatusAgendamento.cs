using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gschatbot.api.Migrations
{
    /// <inheritdoc />
    public partial class MetodoPagamentoEStatusAgendamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Agendamentos",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MetodosPagamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodosPagamento", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EspecialistaMetodosPagamento",
                columns: table => new
                {
                    EspecialistaId = table.Column<int>(type: "int", nullable: false),
                    MetodoPagamentoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EspecialistaMetodosPagamento", x => new { x.EspecialistaId, x.MetodoPagamentoId });
                    table.ForeignKey(
                        name: "FK_EspecialistaMetodosPagamento_Especialistas_EspecialistaId",
                        column: x => x.EspecialistaId,
                        principalTable: "Especialistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EspecialistaMetodosPagamento_MetodosPagamento_MetodoPagament~",
                        column: x => x.MetodoPagamentoId,
                        principalTable: "MetodosPagamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EspecialistaMetodosPagamento_MetodoPagamentoId",
                table: "EspecialistaMetodosPagamento",
                column: "MetodoPagamentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EspecialistaMetodosPagamento");

            migrationBuilder.DropTable(
                name: "MetodosPagamento");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Agendamentos",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
