using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Owh.Cavalcante.Dados.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accessprofile",
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
                    table.PrimaryKey("accessprofile_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "authorization",
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
                    table.PrimaryKey("authorization_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "userprofile",
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
                    table.PrimaryKey("userprofile_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "accessprofileauthorization",
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
                    table.PrimaryKey("accessprofileauthorization_pk", x => x.id);
                    table.ForeignKey(
                        name: "accessprofile_accessprofileauthorization_accessprofileid_fk",
                        column: x => x.accessprofileid,
                        principalTable: "accessprofile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "authorization_accessprofileauthorization_authorizationid_fk",
                        column: x => x.authorizationid,
                        principalTable: "authorization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "userprofileaccessprofile",
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
                    table.PrimaryKey("userprofileaccessprofile_pk", x => x.id);
                    table.ForeignKey(
                        name: "accessprofile_userprofileaccessprofile_accessprofileid_fk",
                        column: x => x.accessprofileid,
                        principalTable: "accessprofile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "userprofile_userprofileaccessprofile_userprofileid_fk",
                        column: x => x.userprofileid,
                        principalTable: "userprofile",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "accessprofile_externalid_ix",
                table: "accessprofile",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "accessprofile_normalizedname_ix",
                table: "accessprofile",
                column: "normalizedname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "accessprofileauthorization_accessprofileid_ix",
                table: "accessprofileauthorization",
                columns: new[] { "accessprofileid", "authorizationid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "accessprofileauthorization_authorizationid_ix",
                table: "accessprofileauthorization",
                column: "authorizationid");

            migrationBuilder.CreateIndex(
                name: "accessprofileauthorization_externalid_ix",
                table: "accessprofileauthorization",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "authorization_externalid_ix",
                table: "authorization",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "authorization_key_ix",
                table: "authorization",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "userprofile_externalid_ix",
                table: "userprofile",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "userprofile_normalizedlogin_ix",
                table: "userprofile",
                column: "normalizedlogin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "userprofileaccessprofile_accessprofileid_ix",
                table: "userprofileaccessprofile",
                column: "accessprofileid");

            migrationBuilder.CreateIndex(
                name: "userprofileaccessprofile_externalid_ix",
                table: "userprofileaccessprofile",
                column: "externalid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "userprofileaccessprofile_userprofileid_ix",
                table: "userprofileaccessprofile",
                columns: new[] { "userprofileid", "accessprofileid" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accessprofileauthorization");

            migrationBuilder.DropTable(
                name: "userprofileaccessprofile");

            migrationBuilder.DropTable(
                name: "authorization");

            migrationBuilder.DropTable(
                name: "accessprofile");

            migrationBuilder.DropTable(
                name: "userprofile");
        }
    }
}
