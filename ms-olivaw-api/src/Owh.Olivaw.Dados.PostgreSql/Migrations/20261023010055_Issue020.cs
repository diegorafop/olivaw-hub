using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Owh.Olivaw.Dados.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Issue020 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inventoryresource",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    technicalid = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    provideridentitykey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    verifiedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    purpose = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    accountreference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    region = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    providerresourceid = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    owner = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    payer = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    @operator = table.Column<string>(name: "operator", type: "character varying(120)", maxLength: 120, nullable: false),
                    scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    source = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    monitoring = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    referenceurl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    cpucapacity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    memorycapacitygib = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    storagecapacitygib = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("inventoryresource_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventoryrevision",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("inventoryrevision_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "managedservice",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    technicalid = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    owner = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("managedservice_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventoryresourcechange",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    inventoryresourceid = table.Column<int>(type: "integer", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    actorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    verifiedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    purpose = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    accountreference = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    region = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    providerresourceid = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    owner = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    payer = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    @operator = table.Column<string>(name: "operator", type: "character varying(120)", maxLength: 120, nullable: false),
                    scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    source = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    monitoring = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    referenceurl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    cpucapacity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    memorycapacitygib = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    storagecapacitygib = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("inventoryresourcechange_pk", x => x.id);
                    table.ForeignKey(
                        name: "inventoryresource_change_inventoryresourceid_fk",
                        column: x => x.inventoryresourceid,
                        principalTable: "inventoryresource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "resourcecustomer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    inventoryresourceid = table.Column<int>(type: "integer", nullable: false),
                    customerid = table.Column<int>(type: "integer", nullable: false),
                    purpose = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    startedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    endedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    startedbyuserid = table.Column<Guid>(type: "uuid", nullable: false),
                    endedbyuserid = table.Column<Guid>(type: "uuid", nullable: true),
                    startedbyname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    endedbyname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("resourcecustomer_pk", x => x.id);
                    table.ForeignKey(
                        name: "customer_resourcecustomer_customerid_fk",
                        column: x => x.customerid,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "inventoryresource_resourcecustomer_inventoryresourceid_fk",
                        column: x => x.inventoryresourceid,
                        principalTable: "inventoryresource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "resourcedependency",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sourceresourceid = table.Column<int>(type: "integer", nullable: false),
                    targetresourceid = table.Column<int>(type: "integer", nullable: false),
                    purpose = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    startedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    endedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    startedbyuserid = table.Column<Guid>(type: "uuid", nullable: false),
                    endedbyuserid = table.Column<Guid>(type: "uuid", nullable: true),
                    startedbyname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    endedbyname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("resourcedependency_pk", x => x.id);
                    table.ForeignKey(
                        name: "inventoryresource_resourcedependency_sourceresourceid_fk",
                        column: x => x.sourceresourceid,
                        principalTable: "inventoryresource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "inventoryresource_resourcedependency_targetresourceid_fk",
                        column: x => x.targetresourceid,
                        principalTable: "inventoryresource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "managedservicechange",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    managedserviceid = table.Column<int>(type: "integer", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    actorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    previousstatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    owner = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("managedservicechange_pk", x => x.id);
                    table.ForeignKey(
                        name: "managedservice_managedservicechange_managedserviceid_fk",
                        column: x => x.managedserviceid,
                        principalTable: "managedservice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "managedservicecustomer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    managedserviceid = table.Column<int>(type: "integer", nullable: false),
                    customerid = table.Column<int>(type: "integer", nullable: false),
                    startedbyuserid = table.Column<Guid>(type: "uuid", nullable: false),
                    endedbyuserid = table.Column<Guid>(type: "uuid", nullable: true),
                    startedbyname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    endedbyname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    startedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    endedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("managedservicecustomer_pk", x => x.id);
                    table.ForeignKey(
                        name: "customer_managedservicecustomer_customerid_fk",
                        column: x => x.customerid,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "managedservice_managedservicecustomer_managedserviceid_fk",
                        column: x => x.managedserviceid,
                        principalTable: "managedservice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "servicedeployment",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    managedserviceid = table.Column<int>(type: "integer", nullable: false),
                    environment = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    endpoint = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    endedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("servicedeployment_pk", x => x.id);
                    table.ForeignKey(
                        name: "managedservice_servicedeployment_managedserviceid_fk",
                        column: x => x.managedserviceid,
                        principalTable: "managedservice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "deploymentresource",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    inventoryresourceid = table.Column<int>(type: "integer", nullable: false),
                    servicedeploymentid = table.Column<int>(type: "integer", nullable: false),
                    purpose = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    reservedcpu = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    reservedmemorygib = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    reservedstoragegib = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    startedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    endedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    startedbyuserid = table.Column<Guid>(type: "uuid", nullable: false),
                    endedbyuserid = table.Column<Guid>(type: "uuid", nullable: true),
                    startedbyname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    endedbyname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("deploymentresource_pk", x => x.id);
                    table.ForeignKey(
                        name: "inventoryresource_deploymentresource_inventoryresourceid_fk",
                        column: x => x.inventoryresourceid,
                        principalTable: "inventoryresource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "servicedeployment_deploymentresource_servicedeploymentid_fk",
                        column: x => x.servicedeploymentid,
                        principalTable: "servicedeployment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "servicedeploymentchange",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    servicedeploymentid = table.Column<int>(type: "integer", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    actorname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    endpoint = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("servicedeploymentchange_pk", x => x.id);
                    table.ForeignKey(
                        name: "servicedeployment_change_servicedeploymentid_fk",
                        column: x => x.servicedeploymentid,
                        principalTable: "servicedeployment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "deploymentresource_externalid_ix",
                table: "deploymentresource",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "deploymentresource_inventoryresourceid_ix",
                table: "deploymentresource",
                columns: new[] { "inventoryresourceid", "servicedeploymentid" },
                unique: true,
                filter: "endedat IS NULL");

            migrationBuilder.CreateIndex(
                name: "deploymentresource_servicedeploymentid_ix",
                table: "deploymentresource",
                column: "servicedeploymentid");

            migrationBuilder.CreateIndex(
                name: "inventoryresource_externalid_ix",
                table: "inventoryresource",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "inventoryresource_provideridentitykey_ix",
                table: "inventoryresource",
                column: "provideridentitykey",
                unique: true,
                filter: "provideridentitykey IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "inventoryresource_technicalid_ix",
                table: "inventoryresource",
                column: "technicalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "inventoryresourcechange_inventoryresourceid_ix",
                table: "inventoryresourcechange",
                columns: new[] { "inventoryresourceid", "revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "inventoryrevision_key_ix",
                table: "inventoryrevision",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "managedservice_externalid_ix",
                table: "managedservice",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "managedservice_technicalid_ix",
                table: "managedservice",
                column: "technicalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "managedservicechange_managedserviceid_ix",
                table: "managedservicechange",
                columns: new[] { "managedserviceid", "revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "managedservicecustomer_customerid_ix",
                table: "managedservicecustomer",
                column: "customerid");

            migrationBuilder.CreateIndex(
                name: "managedservicecustomer_externalid_ix",
                table: "managedservicecustomer",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "managedservicecustomer_managedserviceid_ix",
                table: "managedservicecustomer",
                columns: new[] { "managedserviceid", "customerid" },
                unique: true,
                filter: "endedat IS NULL");

            migrationBuilder.CreateIndex(
                name: "resourcecustomer_customerid_ix",
                table: "resourcecustomer",
                column: "customerid");

            migrationBuilder.CreateIndex(
                name: "resourcecustomer_externalid_ix",
                table: "resourcecustomer",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "resourcecustomer_inventoryresourceid_ix",
                table: "resourcecustomer",
                columns: new[] { "inventoryresourceid", "customerid" },
                unique: true,
                filter: "endedat IS NULL");

            migrationBuilder.CreateIndex(
                name: "resourcedependency_externalid_ix",
                table: "resourcedependency",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "resourcedependency_sourceresourceid_ix",
                table: "resourcedependency",
                columns: new[] { "sourceresourceid", "targetresourceid", "kind" },
                unique: true,
                filter: "endedat IS NULL");

            migrationBuilder.CreateIndex(
                name: "resourcedependency_targetresourceid_ix",
                table: "resourcedependency",
                column: "targetresourceid");

            migrationBuilder.CreateIndex(
                name: "servicedeployment_externalid_ix",
                table: "servicedeployment",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "servicedeployment_managedserviceid_ix",
                table: "servicedeployment",
                columns: new[] { "managedserviceid", "environment" },
                unique: true,
                filter: "endedat IS NULL");

            migrationBuilder.CreateIndex(
                name: "servicedeploymentchange_servicedeploymentid_ix",
                table: "servicedeploymentchange",
                columns: new[] { "servicedeploymentid", "revision" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deploymentresource");

            migrationBuilder.DropTable(
                name: "inventoryresourcechange");

            migrationBuilder.DropTable(
                name: "inventoryrevision");

            migrationBuilder.DropTable(
                name: "managedservicechange");

            migrationBuilder.DropTable(
                name: "managedservicecustomer");

            migrationBuilder.DropTable(
                name: "resourcecustomer");

            migrationBuilder.DropTable(
                name: "resourcedependency");

            migrationBuilder.DropTable(
                name: "servicedeploymentchange");

            migrationBuilder.DropTable(
                name: "inventoryresource");

            migrationBuilder.DropTable(
                name: "servicedeployment");

            migrationBuilder.DropTable(
                name: "managedservice");
        }
    }
}
