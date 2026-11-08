using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.PortalFeature.Queries;
public sealed class ReadPortalHomeQuery { public Guid CustomerExternalId {get;set;} public string Period {get;set;}="month"; }
public sealed class ReadPortalHomeResponse
{
    public int OpenRequests {get;set;} public int WaitingRequests {get;set;} public int CompletedRequests {get;set;}
    public int WorkingImprovements {get;set;} public int QueuedImprovements {get;set;} public int CompletedImprovements {get;set;}
    public decimal? StorageUsedMiB {get;set;} public decimal? StorageLimitMiB {get;set;} public decimal? StoragePercent {get;set;}
    public DateTime? ObservedAt {get;set;}
    public DateTime From {get;set;} public DateTime To {get;set;}
    public int ObservedDays {get;set;} public int ExpectedDays {get;set;} public long? PageViews {get;set;}
    public List<PortalHomePoint> Series {get;set;}=new();
}
public sealed class PortalHomePoint { public DateTime Date {get;set;} public decimal? StorageUsedMiB {get;set;} public long? PageViews {get;set;} }
