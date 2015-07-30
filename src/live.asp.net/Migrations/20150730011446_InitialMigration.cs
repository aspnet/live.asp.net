using System.Collections.Generic;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.Operations;

namespace live.asp.net.Migrations
{
    public partial class InitialMigration : Migration
    {
        public override void Up(MigrationBuilder migration)
        {
            migration.CreateSequence(
                name: "DefaultSequence",
                type: "bigint",
                startWith: 1L,
                incrementBy: 10);
            migration.CreateTable(
                name: "LiveShowDetails",
                columns: table => new
                {
                    Id = table.Column(type: "int", nullable: false),
                    AdminMessage = table.Column(type: "nvarchar(max)", nullable: true),
                    LiveShowEmbedUrl = table.Column(type: "nvarchar(max)", nullable: true),
                    NextShowDateUtc = table.Column(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveShowDetails", x => x.Id);
                });
        }
        
        public override void Down(MigrationBuilder migration)
        {
            migration.DropSequence("DefaultSequence");
            migration.DropTable("LiveShowDetails");
        }
    }
}
