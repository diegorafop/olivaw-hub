using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Owh.Olivaw.Dados.PostgreSql.Migrations.PortalIdentity
{
    /// <inheritdoc />
    public partial class Issue020PortalIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "portalaccessprofile",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    normalizedname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("portalaccessprofile_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "portalauthorization",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("portalauthorization_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "portaluserprofile",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    login = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    normalizedlogin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    passwordhash = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    failedloginattempts = table.Column<int>(type: "integer", nullable: false),
                    lockeduntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    isserviceaccount = table.Column<bool>(type: "boolean", nullable: false),
                    isroot = table.Column<bool>(type: "boolean", nullable: false),
                    securitystamp = table.Column<Guid>(type: "uuid", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("portaluserprofile_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "portalaccessprofileauthorization",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    accessprofileid = table.Column<int>(type: "integer", nullable: false),
                    authorizationid = table.Column<int>(type: "integer", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("portalaccessprofileauthorization_pk", x => x.id);
                    table.ForeignKey(
                        name: "portalaccessprofile_authorization_accessprofileid_fk",
                        column: x => x.accessprofileid,
                        principalTable: "portalaccessprofile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "portalauthorization_portalaccessprofileautho_authorizationid_fk",
                        column: x => x.authorizationid,
                        principalTable: "portalauthorization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "portaluserprofileaccessprofile",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userprofileid = table.Column<int>(type: "integer", nullable: false),
                    accessprofileid = table.Column<int>(type: "integer", nullable: false),
                    externalid = table.Column<Guid>(type: "uuid", nullable: false),
                    actoruserid = table.Column<Guid>(type: "uuid", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("portaluserprofileaccessprofile_pk", x => x.id);
                    table.ForeignKey(
                        name: "portalaccessprofile_portaluserprofileaccessp_accessprofileid_fk",
                        column: x => x.accessprofileid,
                        principalTable: "portalaccessprofile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "portaluserprofile_accessprofile_userprofileid_fk",
                        column: x => x.userprofileid,
                        principalTable: "portaluserprofile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "portalaccessprofile_externalid_ix",
                table: "portalaccessprofile",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portalaccessprofile_normalizedname_ix",
                table: "portalaccessprofile",
                column: "normalizedname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portalaccessprofileauthorization_accessprofileid_ix",
                table: "portalaccessprofileauthorization",
                columns: new[] { "accessprofileid", "authorizationid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portalaccessprofileauthorization_authorizationid_ix",
                table: "portalaccessprofileauthorization",
                column: "authorizationid");

            migrationBuilder.CreateIndex(
                name: "portalaccessprofileauthorization_externalid_ix",
                table: "portalaccessprofileauthorization",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portalauthorization_externalid_ix",
                table: "portalauthorization",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portalauthorization_key_ix",
                table: "portalauthorization",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portaluserprofile_externalid_ix",
                table: "portaluserprofile",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portaluserprofile_normalizedlogin_ix",
                table: "portaluserprofile",
                column: "normalizedlogin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portaluserprofileaccessprofile_accessprofileid_ix",
                table: "portaluserprofileaccessprofile",
                column: "accessprofileid");

            migrationBuilder.CreateIndex(
                name: "portaluserprofileaccessprofile_externalid_ix",
                table: "portaluserprofileaccessprofile",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portaluserprofileaccessprofile_userprofileid_ix",
                table: "portaluserprofileaccessprofile",
                columns: new[] { "userprofileid", "accessprofileid" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "portalaccessprofileauthorization");

            migrationBuilder.DropTable(
                name: "portaluserprofileaccessprofile");

            migrationBuilder.DropTable(
                name: "portalauthorization");

            migrationBuilder.DropTable(
                name: "portalaccessprofile");

            migrationBuilder.DropTable(
                name: "portaluserprofile");
        }
    }
}
