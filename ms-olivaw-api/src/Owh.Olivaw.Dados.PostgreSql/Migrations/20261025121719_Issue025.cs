using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Owh.Olivaw.Dados.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Issue025 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "securityreport",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    runid = table.Column<Guid>(type: "uuid", nullable: false),
                    agentid = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    hostid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    schemaversion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    startedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    executionstatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    rawdata = table.Column<string>(type: "text", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("securityreport_pk", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "securityreport_agentid_ix",
                table: "securityreport",
                columns: new[] { "agentid", "runid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "securityreport_externalid_ix",
                table: "securityreport",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "securityreport_hostid_ix",
                table: "securityreport",
                columns: new[] { "hostid", "completedat" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "securityreport");
        }
    }
}
