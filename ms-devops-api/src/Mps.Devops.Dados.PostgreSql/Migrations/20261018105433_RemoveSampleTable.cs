using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mps.Devops.Dados.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSampleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "example");

            migrationBuilder.RenameIndex(
                name: "usuarioperfilacesso_idexterno_ix",
                table: "usuarioperfilacesso",
                newName: "usuarioperfilacesso_externalid_ix");

            migrationBuilder.RenameIndex(
                name: "usuario_idexterno_ix",
                table: "usuario",
                newName: "usuario_externalid_ix");

            migrationBuilder.RenameIndex(
                name: "perfilacessoautorizacao_idexterno_ix",
                table: "perfilacessoautorizacao",
                newName: "perfilacessoautorizacao_externalid_ix");

            migrationBuilder.RenameIndex(
                name: "perfilacesso_idexterno_ix",
                table: "perfilacesso",
                newName: "perfilacesso_externalid_ix");

            migrationBuilder.RenameIndex(
                name: "autorizacao_idexterno_ix",
                table: "autorizacao",
                newName: "autorizacao_externalid_ix");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "usuarioperfilacesso_externalid_ix",
                table: "usuarioperfilacesso",
                newName: "usuarioperfilacesso_idexterno_ix");

            migrationBuilder.RenameIndex(
                name: "usuario_externalid_ix",
                table: "usuario",
                newName: "usuario_idexterno_ix");

            migrationBuilder.RenameIndex(
                name: "perfilacessoautorizacao_externalid_ix",
                table: "perfilacessoautorizacao",
                newName: "perfilacessoautorizacao_idexterno_ix");

            migrationBuilder.RenameIndex(
                name: "perfilacesso_externalid_ix",
                table: "perfilacesso",
                newName: "perfilacesso_idexterno_ix");

            migrationBuilder.RenameIndex(
                name: "autorizacao_externalid_ix",
                table: "autorizacao",
                newName: "autorizacao_idexterno_ix");

            migrationBuilder.CreateTable(
                name: "example",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dataatualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    datacadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataremocao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    idexterno = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    permission = table.Column<bool>(type: "boolean", nullable: false),
                    usuariologadoid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("example_pk", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "example_idexterno_ix",
                table: "example",
                column: "idexterno");
        }
    }
}
