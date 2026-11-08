using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Owh.Olivaw.Dados.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Issue025Remediation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "securityremediation",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    securityreportid = table.Column<int>(type: "integer", nullable: false),
                    findingindex = table.Column<int>(type: "integer", nullable: false),
                    executoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    hostid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    activehostkey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    planjson = table.Column<string>(type: "text", nullable: false),
                    resultjson = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    leasetoken = table.Column<Guid>(type: "uuid", nullable: true),
                    leaseexpiresat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approvalexpiresat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("securityremediation_pk", x => x.id);
                    table.ForeignKey(
                        name: "securityreport_securityremediation_securityreportid_fk",
                        column: x => x.securityreportid,
                        principalTable: "securityreport",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "securityremediation_activehostkey_ix",
                table: "securityremediation",
                column: "activehostkey",
                unique: true,
                filter: "activehostkey IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "securityremediation_executoruserid_ix",
                table: "securityremediation",
                columns: new[] { "executoruserid", "hostid", "status" });

            migrationBuilder.CreateIndex(
                name: "securityremediation_externalid_ix",
                table: "securityremediation",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "securityremediation_securityreportid_ix",
                table: "securityremediation",
                columns: new[] { "securityreportid", "findingindex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "securityremediation");
        }
    }
}
