using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mps.Ltmj.Dados.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentityAndPortfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "autorizacao",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    chave = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    idexterno = table.Column<Guid>(type: "uuid", nullable: false),
                    usuariologadoid = table.Column<Guid>(type: "uuid", nullable: false),
                    datacadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataatualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dataremocao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("autorizacao_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "perfilacesso",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nomenormalizado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    idexterno = table.Column<Guid>(type: "uuid", nullable: false),
                    usuariologadoid = table.Column<Guid>(type: "uuid", nullable: false),
                    datacadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataatualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dataremocao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("perfilacesso_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "portfolioevent",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    category = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    isvisible = table.Column<bool>(type: "boolean", nullable: false),
                    idexterno = table.Column<Guid>(type: "uuid", nullable: false),
                    usuariologadoid = table.Column<Guid>(type: "uuid", nullable: false),
                    datacadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataatualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dataremocao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("portfolioevent_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    login = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    loginnormalizado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    senhahash = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    falhaslogin = table.Column<int>(type: "integer", nullable: false),
                    bloqueadoate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    root = table.Column<bool>(type: "boolean", nullable: false),
                    versaoseguranca = table.Column<Guid>(type: "uuid", nullable: false),
                    idexterno = table.Column<Guid>(type: "uuid", nullable: false),
                    usuariologadoid = table.Column<Guid>(type: "uuid", nullable: false),
                    datacadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataatualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dataremocao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("usuario_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "perfilacessoautorizacao",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    perfilacessoid = table.Column<int>(type: "integer", nullable: false),
                    autorizacaoid = table.Column<int>(type: "integer", nullable: false),
                    idexterno = table.Column<Guid>(type: "uuid", nullable: false),
                    usuariologadoid = table.Column<Guid>(type: "uuid", nullable: false),
                    datacadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataatualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dataremocao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("perfilacessoautorizacao_pk", x => x.id);
                    table.ForeignKey(
                        name: "autorizacao_perfilacessoautorizacao_autorizacaoid_fk",
                        column: x => x.autorizacaoid,
                        principalTable: "autorizacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "perfilacesso_perfilacessoautorizacao_perfilacessoid_fk",
                        column: x => x.perfilacessoid,
                        principalTable: "perfilacesso",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "portfoliophoto",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portfolioeventid = table.Column<int>(type: "integer", nullable: false),
                    storagekey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    contenttype = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    idexterno = table.Column<Guid>(type: "uuid", nullable: false),
                    usuariologadoid = table.Column<Guid>(type: "uuid", nullable: false),
                    datacadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataatualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dataremocao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("portfoliophoto_pk", x => x.id);
                    table.ForeignKey(
                        name: "portfolioevent_portfoliophoto_portfolioeventid_fk",
                        column: x => x.portfolioeventid,
                        principalTable: "portfolioevent",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarioperfilacesso",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuarioid = table.Column<int>(type: "integer", nullable: false),
                    perfilacessoid = table.Column<int>(type: "integer", nullable: false),
                    idexterno = table.Column<Guid>(type: "uuid", nullable: false),
                    usuariologadoid = table.Column<Guid>(type: "uuid", nullable: false),
                    datacadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataatualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dataremocao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("usuarioperfilacesso_pk", x => x.id);
                    table.ForeignKey(
                        name: "perfilacesso_usuarioperfilacesso_perfilacessoid_fk",
                        column: x => x.perfilacessoid,
                        principalTable: "perfilacesso",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "usuario_usuarioperfilacesso_usuarioid_fk",
                        column: x => x.usuarioid,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "autorizacao_chave_ix",
                table: "autorizacao",
                column: "chave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "autorizacao_externalid_ix",
                table: "autorizacao",
                column: "idexterno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "perfilacesso_externalid_ix",
                table: "perfilacesso",
                column: "idexterno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "perfilacesso_nomenormalizado_ix",
                table: "perfilacesso",
                column: "nomenormalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "perfilacessoautorizacao_autorizacaoid_ix",
                table: "perfilacessoautorizacao",
                column: "autorizacaoid");

            migrationBuilder.CreateIndex(
                name: "perfilacessoautorizacao_externalid_ix",
                table: "perfilacessoautorizacao",
                column: "idexterno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "perfilacessoautorizacao_perfilacessoid_ix",
                table: "perfilacessoautorizacao",
                columns: new[] { "perfilacessoid", "autorizacaoid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portfolioevent_externalid_ix",
                table: "portfolioevent",
                column: "idexterno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portfoliophoto_externalid_ix",
                table: "portfoliophoto",
                column: "idexterno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "portfoliophoto_portfolioeventid_ix",
                table: "portfoliophoto",
                column: "portfolioeventid");

            migrationBuilder.CreateIndex(
                name: "usuario_externalid_ix",
                table: "usuario",
                column: "idexterno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "usuario_loginnormalizado_ix",
                table: "usuario",
                column: "loginnormalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "usuarioperfilacesso_externalid_ix",
                table: "usuarioperfilacesso",
                column: "idexterno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "usuarioperfilacesso_perfilacessoid_ix",
                table: "usuarioperfilacesso",
                column: "perfilacessoid");

            migrationBuilder.CreateIndex(
                name: "usuarioperfilacesso_usuarioid_ix",
                table: "usuarioperfilacesso",
                columns: new[] { "usuarioid", "perfilacessoid" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "perfilacessoautorizacao");

            migrationBuilder.DropTable(
                name: "portfoliophoto");

            migrationBuilder.DropTable(
                name: "usuarioperfilacesso");

            migrationBuilder.DropTable(
                name: "autorizacao");

            migrationBuilder.DropTable(
                name: "portfolioevent");

            migrationBuilder.DropTable(
                name: "perfilacesso");

            migrationBuilder.DropTable(
                name: "usuario");
        }
    }
}
