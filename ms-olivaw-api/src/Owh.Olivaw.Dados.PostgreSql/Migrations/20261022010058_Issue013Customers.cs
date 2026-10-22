using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Owh.Olivaw.Dados.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Issue013Customers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    business = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    legalname = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    document = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    owner = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    notes = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    sourceleadid = table.Column<int>(type: "integer", nullable: true),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("customer_pk", x => x.id);
                    table.ForeignKey(
                        name: "lead_customer_sourceleadid_fk",
                        column: x => x.sourceleadid,
                        principalTable: "lead",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customerchange",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customerid = table.Column<int>(type: "integer", nullable: false),
                    actorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    previousstatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("customerchange_pk", x => x.id);
                    table.ForeignKey(
                        name: "customer_customerchange_customerid_fk",
                        column: x => x.customerid,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customercontact",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customerid = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    role = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    whatsapp = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("customercontact_pk", x => x.id);
                    table.ForeignKey(
                        name: "customer_customercontact_customerid_fk",
                        column: x => x.customerid,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "customer_document_ix",
                table: "customer",
                column: "document",
                unique: true,
                filter: "document IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "customer_externalid_ix",
                table: "customer",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "customer_sourceleadid_ix",
                table: "customer",
                column: "sourceleadid",
                unique: true,
                filter: "sourceleadid IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "customer_status_ix",
                table: "customer",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "customerchange_customerid_ix",
                table: "customerchange",
                columns: new[] { "customerid", "createdat" });

            migrationBuilder.CreateIndex(
                name: "customercontact_customerid_ix",
                table: "customercontact",
                column: "customerid");

            migrationBuilder.CreateIndex(
                name: "customercontact_externalid_ix",
                table: "customercontact",
                column: "externalid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customerchange");

            migrationBuilder.DropTable(
                name: "customercontact");

            migrationBuilder.DropTable(
                name: "customer");
        }
    }
}
