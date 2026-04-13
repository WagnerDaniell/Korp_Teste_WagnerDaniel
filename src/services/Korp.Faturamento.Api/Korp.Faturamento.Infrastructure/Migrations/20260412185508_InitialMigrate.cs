using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Korp.Faturamento.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "notas_fiscais_numero_seq");

            migrationBuilder.CreateTable(
                name: "notas_fiscais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('notas_fiscais_numero_seq')"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ImpressaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notas_fiscais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "itens_nota_fiscal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotaFiscalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoProduto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_nota_fiscal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_itens_nota_fiscal_notas_fiscais_NotaFiscalId",
                        column: x => x.NotaFiscalId,
                        principalTable: "notas_fiscais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_itens_nota_fiscal_NotaFiscalId",
                table: "itens_nota_fiscal",
                column: "NotaFiscalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "itens_nota_fiscal");

            migrationBuilder.DropTable(
                name: "notas_fiscais");

            migrationBuilder.DropSequence(
                name: "notas_fiscais_numero_seq");
        }
    }
}
