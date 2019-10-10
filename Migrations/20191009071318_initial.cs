using Microsoft.EntityFrameworkCore.Migrations;

namespace BattleGrounds.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Armies",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    NumberOfUnits = table.Column<int>(nullable: false),
                    AttackingStrategy = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Armies", x => x.Name);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Armies");
        }
    }
}
