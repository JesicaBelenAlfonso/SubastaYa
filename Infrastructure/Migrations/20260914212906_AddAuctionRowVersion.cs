using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubastaYa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuctionRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Auctions");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Auctions",
                type: "rowversion",
                rowVersion: true,
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Auctions");

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Auctions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
