using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Owh.Olivaw.Dados.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Issue020Panel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "backupintervalhours",
                table: "inventoryresourcechange",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "backupretentiondays",
                table: "inventoryresourcechange",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "certificateexpiresat",
                table: "inventoryresourcechange",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "logicalname",
                table: "inventoryresourcechange",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "renewalat",
                table: "inventoryresourcechange",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "technology",
                table: "inventoryresourcechange",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "technologyversion",
                table: "inventoryresourcechange",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "backupintervalhours",
                table: "inventoryresource",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "backupretentiondays",
                table: "inventoryresource",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "certificateexpiresat",
                table: "inventoryresource",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "logicalname",
                table: "inventoryresource",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "renewalat",
                table: "inventoryresource",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "technology",
                table: "inventoryresource",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "technologyversion",
                table: "inventoryresource",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "careevidence",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    inventoryresourceid = table.Column<int>(type: "integer", nullable: false),
                    samplekey = table.Column<Guid>(type: "uuid", nullable: false),
                    observedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    source = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    referenceurl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("careevidence_pk", x => x.id);
                    table.ForeignKey(
                        name: "inventoryresource_careevidence_inventoryresourceid_fk",
                        column: x => x.inventoryresourceid,
                        principalTable: "inventoryresource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "costreference",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    monthlycost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    source = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    effectiveat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("costreference_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customerplan",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customerid = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    monthlyprice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    memorylimitgib = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    storagelimitgib = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    conditions = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    approvalreference = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    actorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    startsat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("customerplan_pk", x => x.id);
                    table.ForeignKey(
                        name: "customer_customerplan_customerid_fk",
                        column: x => x.customerid,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "healthreading",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    inventoryresourceid = table.Column<int>(type: "integer", nullable: false),
                    samplekey = table.Column<Guid>(type: "uuid", nullable: false),
                    observedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    source = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    outcome = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    maxageminutes = table.Column<int>(type: "integer", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("healthreading_pk", x => x.id);
                    table.ForeignKey(
                        name: "inventoryresource_healthreading_inventoryresourceid_fk",
                        column: x => x.inventoryresourceid,
                        principalTable: "inventoryresource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "incident",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    resourceid = table.Column<int>(type: "integer", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    openkey = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    impact = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    owner = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    nextaction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    internalnote = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("incident_pk", x => x.id);
                    table.ForeignKey(
                        name: "inventoryresource_incident_resourceid_fk",
                        column: x => x.resourceid,
                        principalTable: "inventoryresource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planstudy",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    includedfeatures = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    storagelimitgb = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    memorylimitmib = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    supporthours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    hourlycost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    reservepercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    marginpercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    knownmonthlycost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    suggestedmonthlyprice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    unallocatedmonthlycost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    missingcosts = table.Column<int>(type: "integer", nullable: false),
                    actorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("planstudy_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "portalaccessaudit",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portaluserexternalid = table.Column<Guid>(type: "uuid", nullable: false),
                    customerid = table.Column<int>(type: "integer", nullable: true),
                    action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    details = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    actorauthority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    actorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("portalaccessaudit_pk", x => x.id);
                    table.ForeignKey(
                        name: "customer_portalaccessaudit_customerid_fk",
                        column: x => x.customerid,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "portalinvitation",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portaluserexternalid = table.Column<Guid>(type: "uuid", nullable: false),
                    customerid = table.Column<int>(type: "integer", nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tokenhash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    expiresat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    acceptedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    invitedbyuserid = table.Column<Guid>(type: "uuid", nullable: false),
                    acceptedbyuserid = table.Column<Guid>(type: "uuid", nullable: true),
                    revokedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    securitystampatissue = table.Column<Guid>(type: "uuid", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("portalinvitation_pk", x => x.id);
                    table.ForeignKey(
                        name: "customer_portalinvitation_customerid_fk",
                        column: x => x.customerid,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "portalmembership",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portaluserexternalid = table.Column<Guid>(type: "uuid", nullable: false),
                    customerid = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    actorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("portalmembership_pk", x => x.id);
                    table.ForeignKey(
                        name: "customer_portalmembership_customerid_fk",
                        column: x => x.customerid,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "portalservicelink",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customerid = table.Column<int>(type: "integer", nullable: false),
                    managedserviceid = table.Column<int>(type: "integer", nullable: false),
                    healthresourceid = table.Column<int>(type: "integer", nullable: true),
                    label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    publicurl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("portalservicelink_pk", x => x.id);
                    table.ForeignKey(
                        name: "customer_portalservicelink_customerid_fk",
                        column: x => x.customerid,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "inventoryresource_portalservicelink_healthresourceid_fk",
                        column: x => x.healthresourceid,
                        principalTable: "inventoryresource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "managedservice_portalservicelink_managedserviceid_fk",
                        column: x => x.managedserviceid,
                        principalTable: "managedservice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supportrequest",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customerid = table.Column<int>(type: "integer", nullable: false),
                    managedserviceid = table.Column<int>(type: "integer", nullable: true),
                    protocol = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    publicsummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    portalsubmissionkey = table.Column<Guid>(type: "uuid", nullable: true),
                    submissiondigest = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    requesttype = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    desiredstoragegb = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    desiredmemorymib = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    desiredplan = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    requester = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    impact = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    owner = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    nextaction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    internalnote = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("supportrequest_pk", x => x.id);
                    table.ForeignKey(
                        name: "customer_supportrequest_customerid_fk",
                        column: x => x.customerid,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "managedservice_supportrequest_managedserviceid_fk",
                        column: x => x.managedserviceid,
                        principalTable: "managedservice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usagereading",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    inventoryresourceid = table.Column<int>(type: "integer", nullable: false),
                    samplekey = table.Column<Guid>(type: "uuid", nullable: false),
                    observedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    source = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    metric = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    value = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    quality = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("usagereading_pk", x => x.id);
                    table.ForeignKey(
                        name: "inventoryresource_usagereading_inventoryresourceid_fk",
                        column: x => x.inventoryresourceid,
                        principalTable: "inventoryresource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "incidentchange",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    incidentid = table.Column<int>(type: "integer", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    actorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    impact = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    owner = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    nextaction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    internalnote = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("incidentchange_pk", x => x.id);
                    table.ForeignKey(
                        name: "incident_incidentchange_incidentid_fk",
                        column: x => x.incidentid,
                        principalTable: "incident",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planstudycost",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    planstudyid = table.Column<int>(type: "integer", nullable: false),
                    costreferenceid = table.Column<int>(type: "integer", nullable: false),
                    allocationpercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("planstudycost_pk", x => x.id);
                    table.ForeignKey(
                        name: "costreference_planstudycost_costreferenceid_fk",
                        column: x => x.costreferenceid,
                        principalTable: "costreference",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "planstudy_planstudycost_planstudyid_fk",
                        column: x => x.planstudyid,
                        principalTable: "planstudy",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "deliverynote",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customerid = table.Column<int>(type: "integer", nullable: false),
                    managedserviceid = table.Column<int>(type: "integer", nullable: true),
                    supportrequestid = table.Column<int>(type: "integer", nullable: true),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    publishedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    summary = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    internalnote = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    version = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    approvalreference = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    servicelabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    deliveredat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("deliverynote_pk", x => x.id);
                    table.ForeignKey(
                        name: "customer_deliverynote_customerid_fk",
                        column: x => x.customerid,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "managedservice_deliverynote_managedserviceid_fk",
                        column: x => x.managedserviceid,
                        principalTable: "managedservice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "supportrequest_deliverynote_supportrequestid_fk",
                        column: x => x.supportrequestid,
                        principalTable: "supportrequest",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supportmessage",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    supportrequestid = table.Column<int>(type: "integer", nullable: false),
                    messagekey = table.Column<Guid>(type: "uuid", nullable: false),
                    body = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    authorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ispublic = table.Column<bool>(type: "boolean", nullable: false),
                    isstaff = table.Column<bool>(type: "boolean", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("supportmessage_pk", x => x.id);
                    table.ForeignKey(
                        name: "supportrequest_supportmessage_supportrequestid_fk",
                        column: x => x.supportrequestid,
                        principalTable: "supportrequest",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supportrequestchange",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    supportrequestid = table.Column<int>(type: "integer", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    actorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    requester = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    impact = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    owner = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    nextaction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    publicsummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    servicelabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    internalnote = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("supportrequestchange_pk", x => x.id);
                    table.ForeignKey(
                        name: "supportrequest_supportrequestchange_supportrequestid_fk",
                        column: x => x.supportrequestid,
                        principalTable: "supportrequest",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "deliverynotechange",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    deliverynoteid = table.Column<int>(type: "integer", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    publishedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    authorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    summary = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    internalnote = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    version = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    approvalreference = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    servicelabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    deliveredat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("deliverynotechange_pk", x => x.id);
                    table.ForeignKey(
                        name: "deliverynote_deliverynotechange_deliverynoteid_fk",
                        column: x => x.deliverynoteid,
                        principalTable: "deliverynote",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "careevidence_externalid_ix",
                table: "careevidence",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "careevidence_inventoryresourceid_ix",
                table: "careevidence",
                columns: new[] { "inventoryresourceid", "samplekey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "costreference_externalid_ix",
                table: "costreference",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "costreference_name_ix",
                table: "costreference",
                columns: new[] { "name", "effectiveat" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "customerplan_customerid_ix",
                table: "customerplan",
                columns: new[] { "customerid", "startsat" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "customerplan_externalid_ix",
                table: "customerplan",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "deliverynote_customerid_ix",
                table: "deliverynote",
                columns: new[] { "customerid", "deliveredat", "id" });

            migrationBuilder.CreateIndex(
                name: "deliverynote_externalid_ix",
                table: "deliverynote",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "deliverynote_managedserviceid_ix",
                table: "deliverynote",
                column: "managedserviceid");

            migrationBuilder.CreateIndex(
                name: "deliverynote_supportrequestid_ix",
                table: "deliverynote",
                column: "supportrequestid");

            migrationBuilder.CreateIndex(
                name: "deliverynotechange_deliverynoteid_ix",
                table: "deliverynotechange",
                columns: new[] { "deliverynoteid", "revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "healthreading_externalid_ix",
                table: "healthreading",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "healthreading_inventoryresourceid_ix",
                table: "healthreading",
                columns: new[] { "inventoryresourceid", "samplekey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "healthreading_latest_ix",
                table: "healthreading",
                columns: new[] { "inventoryresourceid", "observedat", "id" });

            migrationBuilder.CreateIndex(
                name: "incident_externalid_ix",
                table: "incident",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "incident_openkey_ix",
                table: "incident",
                column: "openkey",
                unique: true,
                filter: "openkey IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "incident_resourceid_ix",
                table: "incident",
                column: "resourceid");

            migrationBuilder.CreateIndex(
                name: "incidentchange_incidentid_ix",
                table: "incidentchange",
                columns: new[] { "incidentid", "revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "planstudy_externalid_ix",
                table: "planstudy",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "planstudycost_costreferenceid_ix",
                table: "planstudycost",
                column: "costreferenceid");

            migrationBuilder.CreateIndex(
                name: "planstudycost_planstudyid_ix",
                table: "planstudycost",
                columns: new[] { "planstudyid", "costreferenceid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portalaccessaudit_customerid_ix",
                table: "portalaccessaudit",
                column: "customerid");

            migrationBuilder.CreateIndex(
                name: "portalaccessaudit_externalid_ix",
                table: "portalaccessaudit",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portalinvitation_customerid_ix",
                table: "portalinvitation",
                column: "customerid");

            migrationBuilder.CreateIndex(
                name: "portalinvitation_externalid_ix",
                table: "portalinvitation",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portalinvitation_tokenhash_ix",
                table: "portalinvitation",
                column: "tokenhash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portalmembership_customerid_ix",
                table: "portalmembership",
                column: "customerid");

            migrationBuilder.CreateIndex(
                name: "portalmembership_externalid_ix",
                table: "portalmembership",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portalmembership_portaluserexternalid_ix",
                table: "portalmembership",
                columns: new[] { "portaluserexternalid", "customerid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portalservicelink_customerid_ix",
                table: "portalservicelink",
                columns: new[] { "customerid", "managedserviceid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portalservicelink_externalid_ix",
                table: "portalservicelink",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portalservicelink_healthresourceid_ix",
                table: "portalservicelink",
                column: "healthresourceid");

            migrationBuilder.CreateIndex(
                name: "portalservicelink_managedserviceid_ix",
                table: "portalservicelink",
                column: "managedserviceid");

            migrationBuilder.CreateIndex(
                name: "supportmessage_externalid_ix",
                table: "supportmessage",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "supportmessage_supportrequestid_ix",
                table: "supportmessage",
                columns: new[] { "supportrequestid", "messagekey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "supportrequest_externalid_ix",
                table: "supportrequest",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "supportrequest_managedserviceid_ix",
                table: "supportrequest",
                column: "managedserviceid");

            migrationBuilder.CreateIndex(
                name: "supportrequest_protocol_ix",
                table: "supportrequest",
                column: "protocol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "supportrequest_submission_ix",
                table: "supportrequest",
                columns: new[] { "customerid", "portalsubmissionkey" },
                unique: true,
                filter: "portalsubmissionkey IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "supportrequestchange_supportrequestid_ix",
                table: "supportrequestchange",
                columns: new[] { "supportrequestid", "revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "usagereading_externalid_ix",
                table: "usagereading",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "usagereading_inventoryresourceid_ix",
                table: "usagereading",
                columns: new[] { "inventoryresourceid", "samplekey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "careevidence");

            migrationBuilder.DropTable(
                name: "customerplan");

            migrationBuilder.DropTable(
                name: "deliverynotechange");

            migrationBuilder.DropTable(
                name: "healthreading");

            migrationBuilder.DropTable(
                name: "incidentchange");

            migrationBuilder.DropTable(
                name: "planstudycost");

            migrationBuilder.DropTable(
                name: "portalaccessaudit");

            migrationBuilder.DropTable(
                name: "portalinvitation");

            migrationBuilder.DropTable(
                name: "portalmembership");

            migrationBuilder.DropTable(
                name: "portalservicelink");

            migrationBuilder.DropTable(
                name: "supportmessage");

            migrationBuilder.DropTable(
                name: "supportrequestchange");

            migrationBuilder.DropTable(
                name: "usagereading");

            migrationBuilder.DropTable(
                name: "deliverynote");

            migrationBuilder.DropTable(
                name: "incident");

            migrationBuilder.DropTable(
                name: "costreference");

            migrationBuilder.DropTable(
                name: "planstudy");

            migrationBuilder.DropTable(
                name: "supportrequest");

            migrationBuilder.DropColumn(
                name: "backupintervalhours",
                table: "inventoryresourcechange");

            migrationBuilder.DropColumn(
                name: "backupretentiondays",
                table: "inventoryresourcechange");

            migrationBuilder.DropColumn(
                name: "certificateexpiresat",
                table: "inventoryresourcechange");

            migrationBuilder.DropColumn(
                name: "logicalname",
                table: "inventoryresourcechange");

            migrationBuilder.DropColumn(
                name: "renewalat",
                table: "inventoryresourcechange");

            migrationBuilder.DropColumn(
                name: "technology",
                table: "inventoryresourcechange");

            migrationBuilder.DropColumn(
                name: "technologyversion",
                table: "inventoryresourcechange");

            migrationBuilder.DropColumn(
                name: "backupintervalhours",
                table: "inventoryresource");

            migrationBuilder.DropColumn(
                name: "backupretentiondays",
                table: "inventoryresource");

            migrationBuilder.DropColumn(
                name: "certificateexpiresat",
                table: "inventoryresource");

            migrationBuilder.DropColumn(
                name: "logicalname",
                table: "inventoryresource");

            migrationBuilder.DropColumn(
                name: "renewalat",
                table: "inventoryresource");

            migrationBuilder.DropColumn(
                name: "technology",
                table: "inventoryresource");

            migrationBuilder.DropColumn(
                name: "technologyversion",
                table: "inventoryresource");
        }
    }
}
