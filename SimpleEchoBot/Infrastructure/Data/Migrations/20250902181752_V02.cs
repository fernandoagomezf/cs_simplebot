using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleBot.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class V02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Text",
                table: "Training",
                newName: "Utterance");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Utterance",
                table: "Training",
                newName: "Text");
        }
    }
}
