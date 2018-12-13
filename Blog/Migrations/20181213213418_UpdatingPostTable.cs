using Microsoft.EntityFrameworkCore.Migrations;

namespace Blog.Migrations
{
    public partial class UpdatingPostTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SlugUrl",
                table: "Posts",
                newName: "Slug");

            migrationBuilder.AddColumn<string>(
                name: "AuthorUserName",
                table: "Posts",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorUserName",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "Posts",
                newName: "SlugUrl");
        }
    }
}
