using System;
using System.Linq;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Xunit;

namespace Owh.Olivaw.Tests.InventoryFeatureTest;

public sealed class InventoryResourceDetailsTest
{
    [Theory]
    [InlineData("database")]
    [InlineData("domain")]
    [InlineData("certificate")]
    [InlineData("backup")]
    public void Apply_should_preserve_previous_details_in_revision_snapshot(string type)
    {
        var data = Data(type);
        var resource = InventoryResource.Create("resource-details", data, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var first = resource.Changes.Single();
        data.TechnologyVersion = "";
        data.LogicalName = "";
        data.RenewalAt = null;
        data.CertificateExpiresAt = null;
        data.BackupIntervalHours = null;
        data.BackupRetentionDays = null;
        var second = resource.Apply(data, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now.AddDays(1));
        Assert.Equal(2, resource.Revision);
        Assert.Equal("", second.LogicalName);
        Assert.Null(second.RenewalAt);
        Assert.Null(second.BackupRetentionDays);
        switch (type)
        {
            case "database": Assert.Equal("PostgreSQL", first.Technology); Assert.Equal("18", first.TechnologyVersion); Assert.Equal("application", first.LogicalName); break;
            case "domain": Assert.Equal(Date(), first.RenewalAt); break;
            case "certificate": Assert.Equal(Date(), first.CertificateExpiresAt); break;
            case "backup": Assert.Equal(24, first.BackupIntervalHours); Assert.Equal(30, first.BackupRetentionDays); break;
        }
    }

    [Theory]
    [InlineData("technology-type")]
    [InlineData("version-without-technology")]
    [InlineData("logical-type")]
    [InlineData("renewal-type")]
    [InlineData("expiry-type")]
    [InlineData("backup-type")]
    [InlineData("interval-zero")]
    [InlineData("retention-limit")]
    [InlineData("date-kind")]
    [InlineData("date-time")]
    public void Validate_should_reject_incompatible_or_invalid_details(string scenario)
    {
        var data = Data("compute");
        switch (scenario)
        {
            case "technology-type": data.Type = "volume"; data.Technology = "PostgreSQL"; break;
            case "version-without-technology": data.TechnologyVersion = "18"; break;
            case "logical-type": data.LogicalName = "application"; break;
            case "renewal-type": data.RenewalAt = Date(); break;
            case "expiry-type": data.CertificateExpiresAt = Date(); break;
            case "backup-type": data.BackupIntervalHours = 24; break;
            case "interval-zero": data = Data("backup"); data.BackupIntervalHours = 0; break;
            case "retention-limit": data = Data("backup"); data.BackupRetentionDays = 36501; break;
            case "date-kind": data = Data("domain"); data.RenewalAt = DateTime.SpecifyKind(Date(), DateTimeKind.Unspecified); break;
            case "date-time": data = Data("certificate"); data.CertificateExpiresAt = Date().AddHours(1); break;
        }
        Assert.Throws<ArgumentException>(() => data.Validate());
    }

    internal static DateTime Date() => new(2027, 1, 10, 0, 0, 0, DateTimeKind.Utc);
    internal static InventoryResourceData Data(string type)
    {
        var data = new InventoryResourceData { Name = "Recurso de teste", Type = type };
        switch (type)
        {
            case "database": data.Technology = "PostgreSQL"; data.TechnologyVersion = "18"; data.LogicalName = "application"; break;
            case "domain": data.RenewalAt = Date(); break;
            case "certificate": data.CertificateExpiresAt = Date(); break;
            case "backup": data.BackupIntervalHours = 24; data.BackupRetentionDays = 30; break;
        }
        return data;
    }
}
