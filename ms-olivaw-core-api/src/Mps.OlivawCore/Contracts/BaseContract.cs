using System;

namespace Mps.OlivawCore.Contracts;

public abstract class BaseContract
{
    public Guid Id { get; set; }
    public Guid TraceId { get; set; }
}