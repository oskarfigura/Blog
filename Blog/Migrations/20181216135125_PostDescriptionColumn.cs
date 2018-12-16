using Microsoft.EntityFrameworkCore.Migrations;

namespace Blog.Migrations
{
    public partial class PostDescriptionColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Posts",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Posts");
        }
    }
}
