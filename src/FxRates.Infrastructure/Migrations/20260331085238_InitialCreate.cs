using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FxRates.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FromCurrency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    ToCurrency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    BidPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    AskPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_CurrencyPair",
                table: "ExchangeRates",
                columns: new[] { "FromCurrency", "ToCurrency" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeRates");
        }
    }
}
