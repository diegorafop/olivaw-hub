using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mps.Auth.Dados.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class DbCreate : Migration
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
                    tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    valor = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    perfil = table.Column<bool>(type: "boolean", nullable: false),
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
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    login = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    senha = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    email = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    integracaoid = table.Column<string>(type: "text", nullable: true),
                    statusintegracaoenum = table.Column<int>(type: "integer", nullable: false),
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
                    autorizacaoid = table.Column<int>(type: "integer", nullable: false),
                    perfilacessoid = table.Column<int>(type: "integer", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "perfilacesso_perfilacessoautorizacao_perfilacessoid_fk",
                        column: x => x.perfilacessoid,
                        principalTable: "perfilacesso",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarioautenticacao",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuarioid = table.Column<int>(type: "integer", nullable: false),
                    token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    idexterno = table.Column<Guid>(type: "uuid", nullable: false),
                    usuariologadoid = table.Column<Guid>(type: "uuid", nullable: false),
                    datacadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataatualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dataremocao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("usuarioautenticacao_pk", x => x.id);
                    table.ForeignKey(
                        name: "usuario_usuarioautenticacao_usuarioid_fk",
                        column: x => x.usuarioid,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarioautorizacao",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuarioid = table.Column<int>(type: "integer", nullable: false),
                    autorizacaoid = table.Column<int>(type: "integer", nullable: false),
                    idexterno = table.Column<Guid>(type: "uuid", nullable: false),
                    usuariologadoid = table.Column<Guid>(type: "uuid", nullable: false),
                    datacadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dataatualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dataremocao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("usuarioautorizacao_pk", x => x.id);
                    table.ForeignKey(
                        name: "autorizacao_usuarioautorizacao_autorizacaoid_fk",
                        column: x => x.autorizacaoid,
                        principalTable: "autorizacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "usuario_usuarioautorizacao_usuarioid_fk",
                        column: x => x.usuarioid,
                        principalTable: "usuario",
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "usuario_usuarioperfilacesso_usuarioid_fk",
                        column: x => x.usuarioid,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_perfilacesso_idexterno",
                table: "perfilacesso",
                column: "idexterno");

            migrationBuilder.CreateIndex(
                name: "ix_perfilacessoautorizacao_idexterno",
                table: "perfilacessoautorizacao",
                column: "idexterno");

            migrationBuilder.CreateIndex(
                name: "perfilacessoautorizaca_perfilacessoid_ix",
                table: "perfilacessoautorizacao",
                column: "perfilacessoid");

            migrationBuilder.CreateIndex(
                name: "perfilacessoautorizacaoperfilacessoautorizacao_autorizacaoid_ix",
                table: "perfilacessoautorizacao",
                column: "autorizacaoid");

            migrationBuilder.CreateIndex(
                name: "usuarioautenticacaousuarioautenticacao_usuarioid_ix",
                table: "usuarioautenticacao",
                column: "usuarioid");

            migrationBuilder.CreateIndex(
                name: "usuarioautorizacaousuarioautorizacao_autorizacaoid_ix",
                table: "usuarioautorizacao",
                column: "autorizacaoid");

            migrationBuilder.CreateIndex(
                name: "usuarioautorizacaousuarioautorizacao_usuarioid_ix",
                table: "usuarioautorizacao",
                column: "usuarioid");

            migrationBuilder.CreateIndex(
                name: "ix_usuarioperfilacesso_idexterno",
                table: "usuarioperfilacesso",
                column: "idexterno");

            migrationBuilder.CreateIndex(
                name: "usuarioperfilacessousuarioperfilacesso_perfilacessoid_ix",
                table: "usuarioperfilacesso",
                column: "perfilacessoid");

            migrationBuilder.CreateIndex(
                name: "usuarioperfilacessousuarioperfilacesso_usuarioid_ix",
                table: "usuarioperfilacesso",
                column: "usuarioid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "perfilacessoautorizacao");

            migrationBuilder.DropTable(
                name: "usuarioautenticacao");

            migrationBuilder.DropTable(
                name: "usuarioautorizacao");

            migrationBuilder.DropTable(
                name: "usuarioperfilacesso");

            migrationBuilder.DropTable(
                name: "autorizacao");

            migrationBuilder.DropTable(
                name: "perfilacesso");

            migrationBuilder.DropTable(
                name: "usuario");
        }
    }
}