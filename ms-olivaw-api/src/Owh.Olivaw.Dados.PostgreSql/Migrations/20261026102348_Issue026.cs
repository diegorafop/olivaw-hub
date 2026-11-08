using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Owh.Olivaw.Dados.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Issue026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "expense",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entrykey = table.Column<Guid>(type: "uuid", nullable: false),
                    digest = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    description = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    kind = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    exchangeratetobrl = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    amountbrl = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    occurredat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    source = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    customerid = table.Column<int>(type: "integer", nullable: true),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("expense_pk", x => x.id);
                    table.ForeignKey(
                        name: "customer_expense_customerid_fk",
                        column: x => x.customerid,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "portalusagesample",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customerid = table.Column<int>(type: "integer", nullable: false),
                    samplekey = table.Column<Guid>(type: "uuid", nullable: false),
                    day = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    storageusedmib = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    pageviews = table.Column<long>(type: "bigint", nullable: true),
                    source = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("portalusagesample_pk", x => x.id);
                    table.ForeignKey(
                        name: "customer_portalusagesample_customerid_fk",
                        column: x => x.customerid,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "expense_customerid_ix",
                table: "expense",
                column: "customerid");

            migrationBuilder.CreateIndex(
                name: "expense_entrykey_ix",
                table: "expense",
                column: "entrykey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "expense_externalid_ix",
                table: "expense",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "expense_occurredat_ix",
                table: "expense",
                columns: new[] { "occurredat", "customerid" });

            migrationBuilder.CreateIndex(
                name: "portalusagesample_customerid_ix",
                table: "portalusagesample",
                columns: new[] { "customerid", "day" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portalusagesample_externalid_ix",
                table: "portalusagesample",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portalusagesample_samplekey_ix",
                table: "portalusagesample",
                column: "samplekey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expense");

            migrationBuilder.DropTable(
                name: "portalusagesample");
        }
    }
}
