using Microsoft.EntityFrameworkCore;
using Mps.HolosCore.Domain;

namespace Mps.Limpo.Dados.Extensions.Tables
{
    internal static class ExampleExtensions
    {
        internal static void SetupExample
        (
            this ModelBuilder modelBuilder
        )
        {
            modelBuilder
                .Entity<Example>()
                .HasIndex(item => item.IdExterno);

            modelBuilder
                .Entity<Example>()
                .Property(item => item.Name)
                .HasMaxLength(50);

            modelBuilder
                .Entity<Example>()
                .Property(item => item.Description)
                .HasMaxLength(200);
        }
    }
}