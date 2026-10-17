using Microsoft.EntityFrameworkCore.Migrations;
using Mps.Auth.Dados.Extensions.Tables;

#nullable disable

namespace Mps.Auth.Dados.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Task2635Seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SeedAutorizacaoTask2635Data();
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}