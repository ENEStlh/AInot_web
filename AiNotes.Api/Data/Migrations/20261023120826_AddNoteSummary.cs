using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiNotes.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Notes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Notes");
        }
    }
}
