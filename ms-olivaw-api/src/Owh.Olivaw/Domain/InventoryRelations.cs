using System;
using System.Collections.Generic;
using System.Linq;
namespace Owh.Olivaw.Domain;

public static class InventoryRelations
{
    public static bool IsKind(string value) => value is "part-of" or "runs-on" or "stored-on" or "exposed-by" or "depends-on";
    public static bool IsCompatible(string source, string target, string kind)
    {
        if (kind == "depends-on") return true;
        if (kind == "part-of") return (source, target) is ("database", "database-instance") or ("message-namespace", "message-broker") or ("cache-namespace", "cache-instance") or ("subdomain", "domain") or ("dns-record", "dns-zone") or ("compute", "compute");
        if (kind == "runs-on") return target == "compute" && source is "database-instance" or "message-broker" or "cache-instance" or "proxy" or "observability" or "registry" or "other";
        if (kind == "stored-on") return target == "volume" && source is "compute" or "database" or "database-instance" or "message-broker" or "cache-instance" or "backup" or "observability" or "registry" or "other";
        if (kind == "exposed-by") return target is "proxy" or "public-ip" or "dns-record" or "dns-zone" or "domain" or "subdomain" or "certificate" or "external-service";
        return false;
    }
    public static bool CreatesCompositionCycle(int source, int target, IEnumerable<InventoryEdge> edges)
    {
        var adjacency = edges.Where(item => item.Kind == "part-of").ToLookup(item => item.SourceId, item => item.TargetId);
        var visited = new HashSet<int>();
        var pending = new Stack<int>(); pending.Push(target);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (current == source) return true;
            if (!visited.Add(current)) continue;
            foreach (var next in adjacency[current]) pending.Push(next);
        }
        return false;
    }
}
public sealed class InventoryEdge
{
    public int SourceId { get; set; }
    public int TargetId { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
}
public sealed class InventoryOrigin
{
    public int ResourceId { get; set; }
    public int CustomerId { get; set; }
    public string Environment { get; set; } = string.Empty;
    public Guid? DeploymentExternalId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
}
public sealed class InventoryReachability
{
    public Dictionary<int, HashSet<int>> Customers { get; } = new();
    public Dictionary<int, HashSet<string>> Environments { get; } = new();
    public bool LimitExceeded { get; private set; }
    public static InventoryReachability Build(IEnumerable<InventoryEdge> edges, IEnumerable<InventoryOrigin> origins, int maxPairs = 100000)
    {
        var result = new InventoryReachability();
        var adjacency = edges.ToLookup(item => item.SourceId, item => item.TargetId);
        var queue = new Queue<(int Resource, int Customer, string Environment)>();
        var seen = new HashSet<(int, int, string)>();
        bool Enqueue(int resource, int customer, string environment)
        {
            if (!seen.Add((resource, customer, environment))) return true;
            if (seen.Count > maxPairs) { result.LimitExceeded = true; return false; }
            queue.Enqueue((resource, customer, environment));
            return true;
        }
        foreach (var root in origins)
        {
            if (!Enqueue(root.ResourceId, root.CustomerId, root.Environment)) return result;
        }
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!result.Customers.ContainsKey(current.Resource)) result.Customers[current.Resource] = new();
            if (current.Customer > 0) result.Customers[current.Resource].Add(current.Customer);
            if (!result.Environments.ContainsKey(current.Resource)) result.Environments[current.Resource] = new();
            if (current.Environment.Length > 0) result.Environments[current.Resource].Add(current.Environment);
            foreach (var next in adjacency[current.Resource])
            {
                if (!Enqueue(next, current.Customer, current.Environment)) return result;
            }
        }
        return result;
    }
}
