using System;
using System.Linq.Expressions;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Owh.Olivaw.Api.Features.LeadFeature.Commands;
using Owh.Olivaw.Client.Features.LeadFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.LeadFeatureTest.Commands;

public class UpdateLeadContactDiscoveryTest
{
    [Theory]
    [InlineData("new", "conversation")]
    [InlineData("analysis", "analysis")]
    [InlineData("discontinued", "discontinued")]
    public async Task Handle_should_record_sem_email_once_preserve_human_data_and_never_regress_stage(string stage, string expected)
    {
        var lead = Lead.Create(Guid.NewGuid(), new string('a',64),1,"{\"email\":\"\",\"qualification\":{\"approach\":\"Texto humano\",\"unknown\":true},\"proposal\":{\"title\":\"Orçamento\"}}",DateTime.UtcNow);
        var actor = Guid.NewGuid();
        if (stage != "new") lead.UpdateFollowUp(stage,"Estado anterior",actor,"Operador",DateTime.UtcNow);
        var repository = new Mock<IRepository<Lead>>();
        var changes = new Mock<IRepository<LeadChange>>();
        repository.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Lead,bool>>>(),CancellationToken.None,It.IsAny<Expression<Func<Lead,object>>[]>())).ReturnsAsync(lead);
        var handler = new UpdateLeadContactDiscoveryHandler(repository.Object,changes.Object);
        var request = new UpdateLeadContactDiscoveryMediatorCommand { ExternalId=lead.ExternalId,ActorUserId=actor,ActorName="Trevize",Payload=new() { Revision=lead.Revision,FollowUpRevision=lead.FollowUpRevision,ResearchCompleted=true,Contacts=new() { ContactFormStatus="not-detected",ContactCheckedAt=DateTimeOffset.UtcNow } } };
        new UpdateLeadContactDiscoveryMediatorValidator().TestValidate(request).ShouldNotHaveAnyValidationErrors();
        Assert.Equal("not-found",(await handler.Handle(request,CancellationToken.None)).Status);
        request.Payload.Revision=lead.Revision;request.Payload.FollowUpRevision=lead.FollowUpRevision;
        await handler.Handle(request,CancellationToken.None);
        Assert.Equal(expected,lead.FollowUpStatus);
        changes.Verify(item => item.AddAsync(It.Is<LeadChange>(change => change.Note == "sem email"),CancellationToken.None),Times.Once);
        var saved=JsonNode.Parse(lead.Snapshot)!;
        Assert.Equal("Texto humano",saved["qualification"]!["approach"]!.GetValue<string>());
        Assert.True(saved["qualification"]!["unknown"]!.GetValue<bool>());
        Assert.Equal("Orçamento",saved["proposal"]!["title"]!.GetValue<string>());
    }
    [Theory]
    [InlineData("",1,"one@example.test","found")]
    [InlineData("manual@example.test",1,"manual@example.test","found")]
    [InlineData("",2,"","conflict")]
    [InlineData("",0,"","inconclusive")]
    public async Task Handle_should_promote_only_unambiguous_email_preserve_manual_email_and_distinguish_inconclusive(string primary,int count,string expected,string status)
    {
        var lead=Lead.Create(Guid.NewGuid(),new string('a',64),1,"{\"email\":\""+primary+"\",\"qualification\":{}}",DateTime.UtcNow);
        var repository=new Mock<IRepository<Lead>>();var changes=new Mock<IRepository<LeadChange>>();
        repository.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Lead,bool>>>(),CancellationToken.None,It.IsAny<Expression<Func<Lead,object>>[]>())).ReturnsAsync(lead);
        var contacts=new LeadQualification { ContactFormStatus="inconclusive",ContactCheckedAt=DateTimeOffset.UtcNow };
        if(count==1) contacts.PublicEmails=[new(){Email="one@example.test",SourceUrl="https://example.test"}];
        if(count==2) contacts.PublicEmails=[new(){Email="one@example.test",SourceUrl="https://example.test"},new(){Email="two@example.test",SourceUrl="https://example.test"}];
        var request=new UpdateLeadContactDiscoveryMediatorCommand { ExternalId=lead.ExternalId,ActorUserId=Guid.NewGuid(),ActorName="Trevize",Payload=new(){Revision=1,Contacts=contacts} };
        var result=await new UpdateLeadContactDiscoveryHandler(repository.Object,changes.Object).Handle(request,CancellationToken.None);
        Assert.Equal(status,result.Status);
        Assert.Equal(expected,JsonNode.Parse(lead.Snapshot)!["email"]!.GetValue<string>());
        Assert.Equal("new",lead.FollowUpStatus);
    }
    [Fact]
    public async Task Handle_should_report_stale_revision_and_database_conflict()
    {
        var lead=Lead.Create(Guid.NewGuid(),new string('a',64),2,"{}",DateTime.UtcNow);
        var repository=new Mock<IRepository<Lead>>();var changes=new Mock<IRepository<LeadChange>>();
        repository.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<Lead,bool>>>(),CancellationToken.None,It.IsAny<Expression<Func<Lead,object>>[]>())).ReturnsAsync(lead);
        var request=new UpdateLeadContactDiscoveryMediatorCommand { ActorUserId=Guid.NewGuid(),ActorName="Trevize",Payload=new(){Revision=1,Contacts=new(){ContactFormStatus="inconclusive",ContactCheckedAt=DateTimeOffset.UtcNow}} };
        var handler=new UpdateLeadContactDiscoveryHandler(repository.Object,changes.Object);
        Assert.True((await handler.Handle(request,CancellationToken.None)).Conflict);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()),Times.Never);
        request.Payload.Revision=2;
        repository.Setup(item => item.SaveChangesAsync(CancellationToken.None)).ThrowsAsync(new DbUpdateConcurrencyException());
        Assert.True((await handler.Handle(request,CancellationToken.None)).Conflict);
    }
}
