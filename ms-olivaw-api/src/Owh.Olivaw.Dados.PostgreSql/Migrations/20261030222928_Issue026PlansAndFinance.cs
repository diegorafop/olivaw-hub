using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Owh.Olivaw.Dados.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Issue026PlansAndFinance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "commercialpercent",
                table: "planstudy",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "costsheetdata",
                table: "planstudy",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "scenariodigest",
                table: "planstudy",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "scenariokey",
                table: "planstudy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isscheduled",
                table: "healthreading",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "planversionid",
                table: "customerplan",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "awsinvoice",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    submissionkey = table.Column<Guid>(type: "uuid", nullable: false),
                    digest = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    issuer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    accountreference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    invoicenumber = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    billingyear = table.Column<int>(type: "integer", nullable: false),
                    billingmonth = table.Column<int>(type: "integer", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    documentstatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    completeness = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    grossamount = table.Column<decimal>(type: "numeric(24,10)", precision: 24, scale: 10, nullable: true),
                    creditamount = table.Column<decimal>(type: "numeric(24,10)", precision: 24, scale: 10, nullable: true),
                    taxamount = table.Column<decimal>(type: "numeric(24,10)", precision: 24, scale: 10, nullable: true),
                    netamount = table.Column<decimal>(type: "numeric(24,10)", precision: 24, scale: 10, nullable: true),
                    exchangeratetobrl = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    exchangeratenature = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    source = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    rawdata = table.Column<string>(type: "text", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("awsinvoice_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "financialaccount",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    accountkey = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    openingbalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    openedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("financialaccount_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "financialentry",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entrykey = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    settledamount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    dueat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    source = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cancellationreason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    customerid = table.Column<int>(type: "integer", nullable: true),
                    expenseid = table.Column<int>(type: "integer", nullable: true),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("financialentry_pk", x => x.id);
                    table.ForeignKey(
                        name: "customer_financialentry_customerid_fk",
                        column: x => x.customerid,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "expense_financialentry_expenseid_fk",
                        column: x => x.expenseid,
                        principalTable: "expense",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planversion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    publicationkey = table.Column<Guid>(type: "uuid", nullable: false),
                    digest = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    plankey = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    monthlyprice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    storagelimitgib = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    memorylimitmib = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    cpulimit = table.Column<decimal>(type: "numeric(7,3)", precision: 7, scale: 3, nullable: true),
                    conditions = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    approvalreference = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    effectiveat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    planstudyid = table.Column<int>(type: "integer", nullable: true),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("planversion_pk", x => x.id);
                    table.ForeignKey(
                        name: "planstudy_planversion_planstudyid_fk",
                        column: x => x.planstudyid,
                        principalTable: "planstudy",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "awsinvoiceexpense",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    awsinvoiceid = table.Column<int>(type: "integer", nullable: false),
                    expenseid = table.Column<int>(type: "integer", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("awsinvoiceexpense_pk", x => x.id);
                    table.ForeignKey(
                        name: "awsinvoice_awsinvoiceexpense_awsinvoiceid_fk",
                        column: x => x.awsinvoiceid,
                        principalTable: "awsinvoice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "expense_awsinvoiceexpense_expenseid_fk",
                        column: x => x.expenseid,
                        principalTable: "expense",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "financialtransfer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transferkey = table.Column<Guid>(type: "uuid", nullable: false),
                    fromaccountid = table.Column<int>(type: "integer", nullable: false),
                    toaccountid = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    transferredat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    source = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("financialtransfer_pk", x => x.id);
                    table.ForeignKey(
                        name: "financialaccount_financialtransfer_fromaccountid_fk",
                        column: x => x.fromaccountid,
                        principalTable: "financialaccount",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "financialaccount_financialtransfer_toaccountid_fk",
                        column: x => x.toaccountid,
                        principalTable: "financialaccount",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "financialsettlement",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    settlementkey = table.Column<Guid>(type: "uuid", nullable: false),
                    financialentryid = table.Column<int>(type: "integer", nullable: false),
                    financialaccountid = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    isrefund = table.Column<bool>(type: "boolean", nullable: false),
                    paidat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    source = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("financialsettlement_pk", x => x.id);
                    table.ForeignKey(
                        name: "financialaccount_financialsettlement_financialaccountid_fk",
                        column: x => x.financialaccountid,
                        principalTable: "financialaccount",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "financialentry_financialsettlement_financialentryid_fk",
                        column: x => x.financialentryid,
                        principalTable: "financialentry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "planstudy_scenariokey_ix",
                table: "planstudy",
                column: "scenariokey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "customerplan_planversionid_ix",
                table: "customerplan",
                column: "planversionid");

            migrationBuilder.CreateIndex(
                name: "awsinvoice_billingyear_ix",
                table: "awsinvoice",
                columns: new[] { "billingyear", "billingmonth" });

            migrationBuilder.CreateIndex(
                name: "awsinvoice_digest_ix",
                table: "awsinvoice",
                column: "digest",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "awsinvoice_externalid_ix",
                table: "awsinvoice",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "awsinvoice_submissionkey_ix",
                table: "awsinvoice",
                column: "submissionkey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "awsinvoiceexpense_awsinvoiceid_ix",
                table: "awsinvoiceexpense",
                column: "awsinvoiceid");

            migrationBuilder.CreateIndex(
                name: "awsinvoiceexpense_expenseid_ix",
                table: "awsinvoiceexpense",
                column: "expenseid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "awsinvoiceexpense_externalid_ix",
                table: "awsinvoiceexpense",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "financialaccount_accountkey_ix",
                table: "financialaccount",
                column: "accountkey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "financialaccount_externalid_ix",
                table: "financialaccount",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "financialentry_customerid_ix",
                table: "financialentry",
                column: "customerid");

            migrationBuilder.CreateIndex(
                name: "financialentry_dueat_ix",
                table: "financialentry",
                columns: new[] { "dueat", "customerid" });

            migrationBuilder.CreateIndex(
                name: "financialentry_entrykey_ix",
                table: "financialentry",
                column: "entrykey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "financialentry_expenseid_ix",
                table: "financialentry",
                column: "expenseid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "financialentry_externalid_ix",
                table: "financialentry",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "financialsettlement_externalid_ix",
                table: "financialsettlement",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "financialsettlement_financialaccountid_ix",
                table: "financialsettlement",
                column: "financialaccountid");

            migrationBuilder.CreateIndex(
                name: "financialsettlement_financialentryid_ix",
                table: "financialsettlement",
                column: "financialentryid");

            migrationBuilder.CreateIndex(
                name: "financialsettlement_settlementkey_ix",
                table: "financialsettlement",
                column: "settlementkey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "financialtransfer_externalid_ix",
                table: "financialtransfer",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "financialtransfer_fromaccountid_ix",
                table: "financialtransfer",
                column: "fromaccountid");

            migrationBuilder.CreateIndex(
                name: "financialtransfer_toaccountid_ix",
                table: "financialtransfer",
                column: "toaccountid");

            migrationBuilder.CreateIndex(
                name: "financialtransfer_transferkey_ix",
                table: "financialtransfer",
                column: "transferkey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "planversion_effective_ix",
                table: "planversion",
                columns: new[] { "plankey", "effectiveat" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "planversion_externalid_ix",
                table: "planversion",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "planversion_planstudyid_ix",
                table: "planversion",
                column: "planstudyid");

            migrationBuilder.CreateIndex(
                name: "planversion_publicationkey_ix",
                table: "planversion",
                column: "publicationkey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "planversion_version_ix",
                table: "planversion",
                columns: new[] { "plankey", "version" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "planversion_customerplan_planversionid_fk",
                table: "customerplan",
                column: "planversionid",
                principalTable: "planversion",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "planversion_customerplan_planversionid_fk",
                table: "customerplan");

            migrationBuilder.DropTable(
                name: "awsinvoiceexpense");

            migrationBuilder.DropTable(
                name: "financialsettlement");

            migrationBuilder.DropTable(
                name: "financialtransfer");

            migrationBuilder.DropTable(
                name: "planversion");

            migrationBuilder.DropTable(
                name: "awsinvoice");

            migrationBuilder.DropTable(
                name: "financialentry");

            migrationBuilder.DropTable(
                name: "financialaccount");

            migrationBuilder.DropIndex(
                name: "planstudy_scenariokey_ix",
                table: "planstudy");

            migrationBuilder.DropIndex(
                name: "customerplan_planversionid_ix",
                table: "customerplan");

            migrationBuilder.DropColumn(
                name: "commercialpercent",
                table: "planstudy");

            migrationBuilder.DropColumn(
                name: "costsheetdata",
                table: "planstudy");

            migrationBuilder.DropColumn(
                name: "scenariodigest",
                table: "planstudy");

            migrationBuilder.DropColumn(
                name: "scenariokey",
                table: "planstudy");

            migrationBuilder.DropColumn(
                name: "isscheduled",
                table: "healthreading");

            migrationBuilder.DropColumn(
                name: "planversionid",
                table: "customerplan");
        }
    }
}
