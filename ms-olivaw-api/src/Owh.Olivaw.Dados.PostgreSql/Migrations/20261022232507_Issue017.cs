using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Owh.Olivaw.Dados.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Issue017 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "followuprevision",
                table: "lead",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "followupstatus",
                table: "lead",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "new");

            migrationBuilder.AddColumn<DateTime>(
                name: "followupupdatedat",
                table: "lead",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "leadchange",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    leadid = table.Column<int>(type: "integer", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    previousstatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    note = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    actorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("leadchange_pk", x => x.id);
                    table.ForeignKey(
                        name: "lead_leadchange_leadid_fk",
                        column: x => x.leadid,
                        principalTable: "lead",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "lead_followupstatus_ix",
                table: "lead",
                column: "followupstatus");

            migrationBuilder.CreateIndex(
                name: "leadchange_externalid_ix",
                table: "leadchange",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "leadchange_leadid_ix",
                table: "leadchange",
                columns: new[] { "leadid", "revision" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "leadchange");

            migrationBuilder.DropIndex(
                name: "lead_followupstatus_ix",
                table: "lead");

            migrationBuilder.DropColumn(
                name: "followuprevision",
                table: "lead");

            migrationBuilder.DropColumn(
                name: "followupstatus",
                table: "lead");

            migrationBuilder.DropColumn(
                name: "followupupdatedat",
                table: "lead");
        }
    }
}
