using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mps.Auth.Dados.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Task10062 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "usuarioperfilacessousuarioperfilacesso_usuarioid_ix",
                table: "usuarioperfilacesso",
                newName: "usuarioperfilacesso_usuarioid_ix");

            migrationBuilder.RenameIndex(
                name: "usuarioperfilacessousuarioperfilacesso_perfilacessoid_ix",
                table: "usuarioperfilacesso",
                newName: "usuarioperfilacesso_perfilacessoid_ix");

            migrationBuilder.RenameIndex(
                name: "usuarioautorizacaousuarioautorizacao_usuarioid_ix",
                table: "usuarioautorizacao",
                newName: "usuarioautorizacao_usuarioid_ix");

            migrationBuilder.RenameIndex(
                name: "usuarioautorizacaousuarioautorizacao_autorizacaoid_ix",
                table: "usuarioautorizacao",
                newName: "usuarioautorizacao_autorizacaoid_ix");

            migrationBuilder.RenameIndex(
                name: "usuarioautenticacaousuarioautenticacao_usuarioid_ix",
                table: "usuarioautenticacao",
                newName: "usuarioautenticacao_usuarioid_ix");

            migrationBuilder.RenameIndex(
                name: "perfilacessoautorizacaoperfilacessoautorizacao_autorizacaoid_ix",
                table: "perfilacessoautorizacao",
                newName: "perfilacessoautorizacao_autorizacaoid_ix");

            migrationBuilder.RenameIndex(
                name: "perfilacessoautorizaca_perfilacessoid_ix",
                table: "perfilacessoautorizacao",
                newName: "perfilacessoautorizacao_perfilacessoid_ix");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "usuarioperfilacesso_usuarioid_ix",
                table: "usuarioperfilacesso",
                newName: "usuarioperfilacessousuarioperfilacesso_usuarioid_ix");

            migrationBuilder.RenameIndex(
                name: "usuarioperfilacesso_perfilacessoid_ix",
                table: "usuarioperfilacesso",
                newName: "usuarioperfilacessousuarioperfilacesso_perfilacessoid_ix");

            migrationBuilder.RenameIndex(
                name: "usuarioautorizacao_usuarioid_ix",
                table: "usuarioautorizacao",
                newName: "usuarioautorizacaousuarioautorizacao_usuarioid_ix");

            migrationBuilder.RenameIndex(
                name: "usuarioautorizacao_autorizacaoid_ix",
                table: "usuarioautorizacao",
                newName: "usuarioautorizacaousuarioautorizacao_autorizacaoid_ix");

            migrationBuilder.RenameIndex(
                name: "usuarioautenticacao_usuarioid_ix",
                table: "usuarioautenticacao",
                newName: "usuarioautenticacaousuarioautenticacao_usuarioid_ix");

            migrationBuilder.RenameIndex(
                name: "perfilacessoautorizacao_perfilacessoid_ix",
                table: "perfilacessoautorizacao",
                newName: "perfilacessoautorizaca_perfilacessoid_ix");

            migrationBuilder.RenameIndex(
                name: "perfilacessoautorizacao_autorizacaoid_ix",
                table: "perfilacessoautorizacao",
                newName: "perfilacessoautorizacaoperfilacessoautorizacao_autorizacaoid_ix");
        }
    }
}