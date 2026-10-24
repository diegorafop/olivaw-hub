using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
namespace Owh.Olivaw.Dados.Extensions.Tables;
internal static class PlanExtensions
{
    internal static void SetupPlans(this ModelBuilder builder)
    {
        var study = builder.Entity<PlanStudy>(); study.ToTable("planstudy"); study.HasKey(item => item.Id);
        study.HasIndex(item => item.ExternalId).IsUnique();
        study.Property(item => item.Name).HasMaxLength(120).IsRequired();
        study.Property(item => item.IncludedFeatures).HasMaxLength(3000).IsRequired();
        study.Property(item => item.ActorName).HasMaxLength(120).IsRequired();
        study.Property(item => item.StorageLimitGB).HasPrecision(18, 3);
        study.Property(item => item.MemoryLimitMiB).HasPrecision(18, 3);
        study.Property(item => item.SupportHours).HasPrecision(18, 2);
        study.Property(item => item.HourlyCost).HasPrecision(18, 2);
        study.Property(item => item.ReservePercent).HasPrecision(5, 2);
        study.Property(item => item.MarginPercent).HasPrecision(5, 2);
        study.Property(item => item.KnownMonthlyCost).HasPrecision(18, 2);
        study.Property(item => item.UnallocatedMonthlyCost).HasPrecision(18, 2);
        study.Property(item => item.SuggestedMonthlyPrice).HasPrecision(18, 2);
        var line = builder.Entity<PlanStudyCost>(); line.ToTable("planstudycost"); line.HasKey(item => item.Id);
        line.HasIndex(item => new { item.PlanStudyId, item.CostReferenceId }).IsUnique();
        line.Property(item => item.AllocationPercent).HasPrecision(5, 2);
        line.HasOne(item => item.PlanStudy).WithMany(item => item.Costs).HasForeignKey(item => item.PlanStudyId).OnDelete(DeleteBehavior.Restrict);
        line.HasOne(item => item.CostReference).WithMany().HasForeignKey(item => item.CostReferenceId).OnDelete(DeleteBehavior.Restrict);
        var cost = builder.Entity<CostReference>();
        cost.ToTable("costreference"); cost.HasKey(item => item.Id);
        cost.HasIndex(item => item.ExternalId).IsUnique();
        cost.HasIndex(item => new { item.Name, item.EffectiveAt }).IsUnique();
        cost.Property(item => item.Name).HasMaxLength(120).IsRequired();
        cost.Property(item => item.Currency).HasMaxLength(3).IsRequired();
        cost.Property(item => item.Source).HasMaxLength(300).IsRequired();
        cost.Property(item => item.ActorName).HasMaxLength(120).IsRequired();
        cost.Property(item => item.MonthlyCost).HasPrecision(18, 2);
        var plan = builder.Entity<CustomerPlan>();
        plan.ToTable("customerplan"); plan.HasKey(item => item.Id);
        plan.HasIndex(item => item.ExternalId).IsUnique();
        plan.HasIndex(item => new { item.CustomerId, item.StartsAt }).IsUnique();
        plan.HasOne(item => item.Customer).WithMany().HasForeignKey(item => item.CustomerId).OnDelete(DeleteBehavior.Restrict);
        plan.Property(item => item.Name).HasMaxLength(120).IsRequired();
        plan.Property(item => item.Conditions).HasMaxLength(3000).IsRequired();
        plan.Property(item => item.ApprovalReference).HasMaxLength(300).IsRequired();
        plan.Property(item => item.ActorName).HasMaxLength(120).IsRequired();
        plan.Property(item => item.MonthlyPrice).HasPrecision(18, 2);
        plan.Property(item => item.MemoryLimitGiB).HasPrecision(18, 3);
        plan.Property(item => item.StorageLimitGiB).HasPrecision(18, 3);
    }
}
