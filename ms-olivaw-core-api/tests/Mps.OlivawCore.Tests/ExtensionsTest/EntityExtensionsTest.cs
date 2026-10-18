using MediatR;
using Mps.OlivawCore.Api.Extensions.Entities;
using Mps.OlivawCore.Contracts;
using Mps.OlivawCore.Domain;
using Xunit;

namespace Mps.OlivawCore.Tests.ExtensionsTest;

public class EntityExtensionsTest
{
    [Fact]
    public void Map_should_use_external_identity_and_skip_collections_and_entity_navigation()
    {
        var source = new SampleEntity
        {
            Id = 7,
            ExternalId = Guid.NewGuid(),
            Name = "shared",
            Items = [1, 2],
            Child = new SampleEntity()
        };
        var result = source.Map<SampleResponse>();
        Assert.Equal(source.ExternalId, result.Id);
        Assert.Equal(source.Name, result.Name);
        Assert.Empty(result.Items);
        Assert.Null(result.Child);
        Assert.Equal("default", result.Unmatched);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ToDomain_should_initialize_new_identity_and_utc_audit(int overload)
    {
        var userId = Guid.NewGuid();
        var request = new SampleRequest { Name = "new" };
        var before = DateTime.UtcNow;
        var result = overload switch
        {
            0 => request.ToDomain<SampleEntity>(),
            1 => EntityExtensions.ToDomain<SampleRequest, SampleEntity>(request),
            2 => EntityExtensions.ToDomain<SampleRequest, SampleEntity>(request, userId),
            _ => EntityExtensions.ToDomain<SampleEntity>(userId)
        };
        Assert.NotEqual(Guid.Empty, result.ExternalId);
        Assert.InRange(result.CreatedAt, before, DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, result.CreatedAt.Kind);
        Assert.Null(result.UpdatedAt);
        Assert.Equal(overload >= 2 ? userId : Guid.Empty, result.UsuarioLogadoId);
        if (overload < 3)
            Assert.Equal("new", result.Name);
    }

    [Fact]
    public void Replicate_should_copy_data_with_new_identity_and_audit()
    {
        var source = new SampleEntity { Id = 4, ExternalId = Guid.NewGuid(), Name = "copy", UpdatedAt = DateTime.UtcNow };
        var userId = Guid.NewGuid();
        var result = source.Replicate<SampleEntity>(userId);
        Assert.NotSame(source, result);
        Assert.Equal(source.Name, result.Name);
        Assert.Equal(0, result.Id);
        Assert.NotEqual(source.ExternalId, result.ExternalId);
        Assert.Equal(userId, result.UsuarioLogadoId);
        Assert.Null(result.UpdatedAt);
        Assert.NotNull(source.UpdatedAt);
    }

    [Fact]
    public void Update_should_preserve_identity_and_update_audit_and_fields()
    {
        var entity = new SampleEntity { ExternalId = Guid.NewGuid(), Name = "old" };
        var identity = entity.ExternalId;
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow;
        entity.Update(userId);
        Assert.Equal(userId, entity.UsuarioLogadoId);
        Assert.InRange(entity.UpdatedAt!.Value, before, DateTime.UtcNow);
        entity.Update(new SampleRequest { Name = "updated" });
        Assert.Equal("updated", entity.Name);
        entity.Update(new { Name = "final" }, userId);
        Assert.Equal("final", entity.Name);
        Assert.Equal(identity, entity.ExternalId);
    }

    [Fact]
    public void Toggle_should_remove_and_reactivate_an_entity()
    {
        var entity = new SampleEntity();
        var userId = Guid.NewGuid();
        entity.Toggle(false, userId);
        Assert.NotNull(entity.DeletedAt);
        Assert.Equal(entity.DeletedAt, entity.UpdatedAt);
        Assert.Equal(userId, entity.UsuarioLogadoId);
        entity.Toggle(true, userId);
        Assert.Null(entity.DeletedAt);
    }

    [Fact]
    public void Toggle_should_apply_to_every_entity_without_requiring_enumeration_by_the_caller()
    {
        Entity[] entities = [new SampleEntity(), new SampleEntity()];
        var userId = Guid.NewGuid();
        entities.Toggle(false, userId);
        Assert.All(entities, item =>
        {
            Assert.NotNull(item.DeletedAt);
            Assert.Equal(userId, item.UsuarioLogadoId);
        });
    }

    [Fact]
    public void Convert_should_map_nullable_and_nonnullable_values_without_overwriting_unmapped_types()
    {
        var source = new NullableSource { Number = 9, Required = 3, Identifier = null, EmptyIdentifier = Guid.Empty };
        var result = EntityExtensions.Convert<NullableSource, NullableTarget>(source);
        Assert.Equal(9, result.Number);
        Assert.Equal(3, result.Required);
        Assert.Equal(Guid.Empty, result.Identifier);
        Assert.Null(result.EmptyIdentifier);
        Assert.Equal("unchanged", result.UnmatchedType);
        source.Number = null;
        Assert.Equal(42, EntityExtensions.Convert<NullableSource, NullableTarget>(source).Number);
    }

    [Fact]
    public void Convert_should_assign_new_trace_id_to_contracts_and_support_conversion_interfaces()
    {
        var request = new SampleRequest { Name = "contract" };
        var contract = request.Convert<SampleContract>();
        Assert.Equal("contract", contract.Name);
        Assert.NotEqual(Guid.Empty, contract.TraceId);
        var result = contract.Map<SampleResponse>();
        Assert.Equal(contract.Name, result.Name);
        IConversivel convertible = new Convertible { Name = "convertible" };
        Assert.Equal("convertible", convertible.Convert<SampleResponse>().Name);
        Assert.Equal("default", EntityExtensions.Convert<object, SampleResponse>(null!).Unmatched);
    }

    public class SampleEntity : Entity
    {
        public string Name { get; set; } = "";
        public List<int> Items { get; set; } = [];
        public SampleEntity? Child { get; set; }
    }
    public class SampleResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public List<int> Items { get; set; } = [];
        public SampleEntity? Child { get; set; }
        public string Unmatched { get; set; } = "default";
    }
    public class SampleRequest : IRequest<string>
    {
        public string Name { get; set; } = "";
    }
    public class SampleContract : BaseContract
    {
        public string Name { get; set; } = "";
    }
    public class Convertible : IConversivel
    {
        public string Name { get; set; } = "";
    }
    public class NullableSource
    {
        public int? Number { get; set; }
        public int Required { get; set; }
        public Guid? Identifier { get; set; }
        public Guid EmptyIdentifier { get; set; }
        public int UnmatchedType { get; set; }
    }
    public class NullableTarget
    {
        public int Number { get; set; } = 42;
        public int? Required { get; set; }
        public Guid Identifier { get; set; } = Guid.NewGuid();
        public Guid? EmptyIdentifier { get; set; }
        public string UnmatchedType { get; set; } = "unchanged";
    }
}
