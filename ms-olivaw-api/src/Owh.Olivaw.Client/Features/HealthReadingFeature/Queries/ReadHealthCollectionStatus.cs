using System;
namespace Owh.Olivaw.Client.Features.HealthReadingFeature.Queries;
public sealed class ReadHealthCollectionStatusResponse
{
    public bool Enabled { get; set; }
    public int IntervalMinutes { get; set; }
    public int RetentionDays { get; set; }
    public int Resources { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int Attempted { get; set; }
    public int Failures { get; set; }
}
