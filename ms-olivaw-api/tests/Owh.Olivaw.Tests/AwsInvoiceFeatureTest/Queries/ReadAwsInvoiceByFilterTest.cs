using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Tests.Mocks;
using Owh.Olivaw.Api.Features.AwsInvoiceFeature.Queries;
using Xunit;
namespace Owh.Olivaw.Tests.AwsInvoiceFeatureTest.Queries;
public sealed class ReadAwsInvoiceByFilterTest
{
    [Fact]public async Task Handle_should_page_without_raw_json_and_apply_month_year_cursor()
    {
        var rows=Enumerable.Range(1,52).Select(AwsInvoiceFakerBuilder.Create).ToArray();var h=new ReadAwsInvoiceByFilterHandler(InventoryFakerBuilder.Repository(rows).Object);
        var q=new ReadAwsInvoiceByFilterMediatorQuery{Payload=new(){BillingYear=2026,BillingMonth=10}};
        var page=await h.Handle(q,CancellationToken.None);Assert.Equal(50,page.Items.Count);Assert.Equal(3,page.NextBeforeId);
        q.Payload.BeforeId=3;Assert.Equal(2,(await h.Handle(q,CancellationToken.None)).Items.Count);
        q.Payload.BillingMonth=9;Assert.Empty((await h.Handle(q,CancellationToken.None)).Items);
        q.Payload.BillingMonth=13;Assert.False(new ReadAwsInvoiceByFilterValidator().Validate(q.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(()=>h.Handle(null!,CancellationToken.None));
    }
}
