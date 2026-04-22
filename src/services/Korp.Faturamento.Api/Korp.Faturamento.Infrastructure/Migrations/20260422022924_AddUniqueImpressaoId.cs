using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Korp.Faturamento.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueImpressaoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_notas_fiscais_ImpressaoId",
                table: "notas_fiscais",
                column: "ImpressaoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_notas_fiscais_ImpressaoId",
                table: "notas_fiscais");
        }
    }
}
