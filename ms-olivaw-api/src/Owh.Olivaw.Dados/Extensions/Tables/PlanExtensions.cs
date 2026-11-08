using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
namespace Owh.Olivaw.Dados.Extensions.Tables;
internal static class PlanExtensions
{
    internal static void SetupPlans(this ModelBuilder builder)
    {
        var version=builder.Entity<PlanVersion>();version.ToTable("planversion");version.HasKey(x=>x.Id);
        version.HasIndex(x=>x.ExternalId).IsUnique();version.HasIndex(x=>x.PublicationKey).IsUnique();version.HasIndex(x=>new{x.PlanKey,x.Version}).IsUnique();version.HasIndex(x=>new{x.PlanKey,x.EffectiveAt}).IsUnique();
        version.Property(x=>x.Digest).HasMaxLength(64).IsRequired();version.Property(x=>x.PlanKey).HasMaxLength(20).IsRequired();version.Property(x=>x.Conditions).HasMaxLength(3000).IsRequired();version.Property(x=>x.ApprovalReference).HasMaxLength(300).IsRequired();
        version.Property(x=>x.MonthlyPrice).HasPrecision(18,2);version.Property(x=>x.StorageLimitGiB).HasPrecision(18,3);version.Property(x=>x.MemoryLimitMiB).HasPrecision(18,3);version.Property(x=>x.CpuLimit).HasPrecision(7,3);
        version.HasOne(x=>x.PlanStudy).WithMany().HasForeignKey(x=>x.PlanStudyId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<CustomerPlan>().HasOne(x=>x.PlanVersion).WithMany().HasForeignKey(x=>x.PlanVersionId).OnDelete(DeleteBehavior.Restrict);
        var study = builder.Entity<PlanStudy>(); study.ToTable("planstudy"); study.HasKey(item => item.Id);
        study.HasIndex(item => item.ExternalId).IsUnique();
        study.HasIndex(item => item.ScenarioKey).IsUnique();
        study.Property(item => item.ScenarioDigest).HasMaxLength(64);
        study.Property(item => item.CommercialPercent).HasPrecision(5,2);
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
