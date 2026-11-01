using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Owh.Olivaw.Tests.Mocks;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Api.Features.AwsInvoiceFeature.Queries;
using Xunit;
namespace Owh.Olivaw.Tests.AwsInvoiceFeatureTest.Queries;
public sealed class ReadAwsInvoiceByIdTest
{
    [Fact]public async Task Handle_should_preserve_original_and_distinguish_missing_document()
    {
        var entity=AwsInvoiceFakerBuilder.Create();var h=new ReadAwsInvoiceByIdHandler(InventoryFakerBuilder.Repository(entity).Object,InventoryFakerBuilder.Repository<AwsInvoiceExpense>().Object);
        var q=new ReadAwsInvoiceByIdMediatorQuery{Payload=new(){ExternalId=entity.ExternalId}};
        var result=await h.Handle(q,CancellationToken.None);Assert.Equal(entity.RawData,result.RawData);Assert.Equal(entity.ExternalId,result.Invoice.ExternalId);Assert.Empty(result.Expenses);
        q.Payload.ExternalId=Guid.NewGuid();await Assert.ThrowsAsync<ValidationException>(()=>h.Handle(q,CancellationToken.None));
        q.Payload.ExternalId=Guid.Empty;Assert.False(new ReadAwsInvoiceByIdValidator().Validate(q.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(()=>h.Handle(null!,CancellationToken.None));
    }
}
