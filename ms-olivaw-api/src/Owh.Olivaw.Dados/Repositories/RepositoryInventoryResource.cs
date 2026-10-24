using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
namespace Owh.Olivaw.Dados.Repositories;

public sealed class RepositoryInventoryResource(WriteDbContext context) : Repository<InventoryResource>(context), IRepositoryInventoryResource
{
    private Task<InventoryGraphData>? _graph;
    public Task<InventoryGraphData> ReadGraphAsync(CancellationToken token) => _graph ??= LoadGraphAsync(token);
    private async Task<InventoryGraphData> LoadGraphAsync(CancellationToken token)
    {
        const int nodeLimit = 5000, edgeLimit = 20000;
        var graph = new InventoryGraphData();
        graph.Nodes = await _context.Set<InventoryResource>().AsNoTracking().Where(item => item.DeletedAt == null)
            .OrderBy(item => item.Id).Take(nodeLimit + 1).Select(item => new InventoryNode { Id = item.Id, ExternalId = item.ExternalId, Name = item.Name }).ToListAsync(token);
        if (graph.Nodes.Count > nodeLimit) { graph.LimitExceeded = true; return graph; }
        graph.Edges = await _context.Set<ResourceDependency>().AsNoTracking().Where(item => item.EndedAt == null && item.DeletedAt == null && item.SourceResource.DeletedAt == null && item.TargetResource.DeletedAt == null)
            .OrderBy(item => item.Id).Take(edgeLimit + 1).Select(item => new InventoryEdge { SourceId = item.SourceResourceId, TargetId = item.TargetResourceId, Kind = item.Kind, Purpose = item.Purpose }).ToListAsync(token);
        var direct = await _context.Set<ResourceCustomer>().AsNoTracking().Where(item => item.EndedAt == null && item.DeletedAt == null && item.Customer.DeletedAt == null && item.InventoryResource.DeletedAt == null)
            .OrderBy(item => item.Id).Take(edgeLimit + 1).Select(item => new { ResourceId = item.InventoryResourceId, item.CustomerId, item.Customer.ExternalId, item.Customer.Business, item.Purpose }).ToListAsync(token);
        graph.Uses = await _context.Set<DeploymentResource>().AsNoTracking().Where(item => item.EndedAt == null && item.DeletedAt == null && item.ServiceDeployment.EndedAt == null && item.ServiceDeployment.DeletedAt == null && item.ServiceDeployment.ManagedService.DeletedAt == null && item.ServiceDeployment.ManagedService.Status != "closed" && item.InventoryResource.DeletedAt == null)
            .OrderBy(item => item.Id).Take(edgeLimit + 1).Select(item => new InventoryDeclaredUse
            {
                ResourceId = item.InventoryResourceId, ServiceId = item.ServiceDeployment.ManagedServiceId,
                DeploymentExternalId = item.ServiceDeployment.ExternalId, Environment = item.ServiceDeployment.Environment,
                ServiceName = item.ServiceDeployment.ManagedService.Name, Purpose = item.Purpose,
                ReservedCpu = item.ReservedCpu, ReservedMemoryGiB = item.ReservedMemoryGiB, ReservedStorageGiB = item.ReservedStorageGiB
            }).ToListAsync(token);
        var serviceCustomers = await _context.Set<ManagedServiceCustomer>().AsNoTracking().Where(item => item.EndedAt == null && item.DeletedAt == null && item.Customer.DeletedAt == null && item.ManagedService.DeletedAt == null && item.ManagedService.Status != "closed")
            .OrderBy(item => item.Id).Take(edgeLimit + 1).Select(item => new { item.ManagedServiceId, item.CustomerId, item.Customer.ExternalId, item.Customer.Business }).ToListAsync(token);
        if (graph.Edges.Count > edgeLimit || direct.Count > edgeLimit || graph.Uses.Count > edgeLimit || serviceCustomers.Count > edgeLimit) { graph.LimitExceeded = true; return graph; }
        graph.Customers = direct.Select(item => new InventoryBeneficiary { Id = item.CustomerId, ExternalId = item.ExternalId, Business = item.Business })
            .Concat(serviceCustomers.Select(item => new InventoryBeneficiary { Id = item.CustomerId, ExternalId = item.ExternalId, Business = item.Business })).GroupBy(item => item.Id).Select(group => group.First()).ToList();
        graph.Origins.AddRange(direct.Select(item => new InventoryOrigin { ResourceId = item.ResourceId, CustomerId = item.CustomerId, Purpose = item.Purpose }));
        var byService = serviceCustomers.ToLookup(item => item.ManagedServiceId);
        foreach (var use in graph.Uses)
        {
            var beneficiaries = byService[use.ServiceId].Select(item => item.CustomerId).ToList();
            if (beneficiaries.Count == 0) beneficiaries.Add(0);
            foreach (var beneficiary in beneficiaries) graph.Origins.Add(new InventoryOrigin { ResourceId = use.ResourceId, CustomerId = beneficiary, DeploymentExternalId = use.DeploymentExternalId, Environment = use.Environment, ServiceName = use.ServiceName, Purpose = use.Purpose });
            if (graph.Origins.Count > 100000) { graph.LimitExceeded = true; return graph; }
        }
        return graph;
    }
    public async Task<InventoryLinkPage> ReadLinksAsync(InventoryLinkFilter filter, CancellationToken token)
    {
        var query = ReadLinkRows(filter);
        return new() { Total = await query.LongCountAsync(token), Items = await query.OrderByDescending(item => item.StartedAt).ThenBy(item => item.ExternalId).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(token) };
    }
    public IQueryable<InventoryLinkReadItem> ReadLinkRows(InventoryLinkFilter filter)
    {
        if (filter.Kind == "customer")
        {
            var rows = _context.Set<ResourceCustomer>().AsNoTracking().Where(item => item.DeletedAt == null);
            if (!filter.IncludeEnded) rows = rows.Where(item => item.EndedAt == null);
            if (filter.ResourceExternalId.HasValue) rows = rows.Where(item => item.InventoryResource.ExternalId == filter.ResourceExternalId.Value);
            return rows.Select(item => new InventoryLinkReadItem
            {
                ExternalId = item.ExternalId, ResourceExternalId = item.InventoryResource.ExternalId, ResourceName = item.InventoryResource.Name, TargetExternalId = item.Customer.ExternalId, TargetName = item.Customer.Business,
                Purpose = item.Purpose, Revision = item.Revision, StartedAt = item.StartedAt, EndedAt = item.EndedAt, StartedByName = item.StartedByName, EndedByName = item.EndedByName,
            });
        }
        if (filter.Kind == "dependency")
        {
            var rows = _context.Set<ResourceDependency>().AsNoTracking().Where(item => item.DeletedAt == null);
            if (!filter.IncludeEnded) rows = rows.Where(item => item.EndedAt == null);
            if (filter.ResourceExternalId.HasValue) rows = rows.Where(item => item.SourceResource.ExternalId == filter.ResourceExternalId.Value || item.TargetResource.ExternalId == filter.ResourceExternalId.Value);
            return rows.Select(item => new InventoryLinkReadItem
            {
                ExternalId = item.ExternalId, ResourceExternalId = item.SourceResource.ExternalId, ResourceName = item.SourceResource.Name, TargetExternalId = item.TargetResource.ExternalId, TargetName = item.TargetResource.Name,
                Purpose = item.Purpose, Revision = item.Revision, StartedAt = item.StartedAt, EndedAt = item.EndedAt, StartedByName = item.StartedByName, EndedByName = item.EndedByName,
                RelationshipKind = item.Kind,
            });
        }
        if (filter.Kind == "deployment")
        {
            var rows = _context.Set<DeploymentResource>().AsNoTracking().Where(item => item.DeletedAt == null);
            if (!filter.IncludeEnded) rows = rows.Where(item => item.EndedAt == null);
            if (filter.ResourceExternalId.HasValue) rows = rows.Where(item => item.InventoryResource.ExternalId == filter.ResourceExternalId.Value);
            if (filter.DeploymentExternalId.HasValue) rows = rows.Where(item => item.ServiceDeployment.ExternalId == filter.DeploymentExternalId.Value);
            return rows.Select(item => new InventoryLinkReadItem
            {
                ExternalId = item.ExternalId, ResourceExternalId = item.InventoryResource.ExternalId, ResourceName = item.InventoryResource.Name, TargetExternalId = item.ServiceDeployment.ExternalId, TargetName = item.ServiceDeployment.Name,
                Purpose = item.Purpose, Revision = item.Revision, StartedAt = item.StartedAt, EndedAt = item.EndedAt, StartedByName = item.StartedByName, EndedByName = item.EndedByName,
                Environment = item.ServiceDeployment.Environment, ReservedCpu = item.ReservedCpu, ReservedMemoryGiB = item.ReservedMemoryGiB, ReservedStorageGiB = item.ReservedStorageGiB,
            });
        }
        throw new ArgumentException("Tipo de vínculo inválido.", nameof(filter));
    }
    public IQueryable<InventoryResource> ReadRows(InventoryResourceFilter filter)
    {
        var query = _context.Set<InventoryResource>().AsNoTracking().Where(item => item.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.ToLowerInvariant();
            query = query.Where(item => item.Name.ToLower().Contains(term) || item.TechnicalId.Contains(term));
        }
        if (!string.IsNullOrEmpty(filter.Type)) query = query.Where(item => item.Type == filter.Type);
        if (!string.IsNullOrEmpty(filter.Provider)) query = query.Where(item => item.Provider == filter.Provider);
        if (!string.IsNullOrEmpty(filter.Status)) query = query.Where(item => item.Status == filter.Status);
        return query;
    }
    public async Task<InventoryResourcePage> ReadPageAsync(InventoryResourceFilter filter, CancellationToken token)
    {
        var graph = await ReadGraphAsync(token);
        if (graph.LimitExceeded) return new() { LimitExceeded = true };
        var reachability = InventoryReachability.Build(graph.Edges, graph.Origins);
        if (reachability.LimitExceeded) return new() { LimitExceeded = true };
        var query = ReadRows(filter);
        InventoryReachability? selected = null;
        if (filter.CustomerExternalId.HasValue || filter.Environment.Length > 0)
        {
            IEnumerable<InventoryOrigin> roots = graph.Origins;
            if (filter.CustomerExternalId.HasValue)
            {
                var customer = graph.Customers.FirstOrDefault(item => item.ExternalId == filter.CustomerExternalId.Value);
                if (customer == null) return new();
                roots = roots.Where(item => item.CustomerId == customer.Id);
            }
            if (filter.Environment.Length > 0)
            {
                roots = roots.Where(item => item.Environment == filter.Environment || (filter.CustomerExternalId.HasValue && !item.DeploymentExternalId.HasValue));
            }
            selected = InventoryReachability.Build(graph.Edges, roots);
            if (selected.LimitExceeded) return new() { LimitExceeded = true };
            var ids = selected.Customers.Keys.ToArray();
            query = query.Where(item => ids.Contains(item.Id));
        }
        if (filter.Shared.HasValue)
        {
            var ids = reachability.Customers.Where(item => item.Value.Count > 1).Select(item => item.Key).ToArray();
            if (filter.Shared.Value) query = query.Where(item => ids.Contains(item.Id));
            else query = query.Where(item => !ids.Contains(item.Id));
        }
        var total = await query.LongCountAsync(token);
        var items = await query.OrderBy(item => item.Name).ThenBy(item => item.Id).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(token);
        return new() { Total = total, Items = items.Select(item => new InventoryResourceReadItem { Resource = item, Context = BuildContext(item.Id, graph, reachability, selected, filter) }).ToList() };
    }
    public async Task<InventoryResourceContext> ReadContextAsync(int resourceId, CancellationToken token)
    {
        var graph = await ReadGraphAsync(token);
        if (graph.LimitExceeded) return new() { LimitExceeded = true };
        var reachability = InventoryReachability.Build(graph.Edges, graph.Origins);
        if (reachability.LimitExceeded) return new() { LimitExceeded = true };
        return BuildContext(resourceId, graph, reachability, null, new());
    }
    private static InventoryResourceContext BuildContext(int resourceId, InventoryGraphData graph, InventoryReachability all, InventoryReachability? selected, InventoryResourceFilter filter)
    {
        all.Customers.TryGetValue(resourceId, out var customerIds);
        customerIds ??= new();
        var result = new InventoryResourceContext { CustomerCount = customerIds.Count, Customers = graph.Customers.Where(item => customerIds.Contains(item.Id)).OrderBy(item => item.Business).Take(100).ToList() };
        var uses = graph.Uses.Where(item => item.ResourceId == resourceId).ToList();
        var cpu = uses.Where(item => item.ReservedCpu.HasValue).ToList();
        var memory = uses.Where(item => item.ReservedMemoryGiB.HasValue).ToList();
        var storage = uses.Where(item => item.ReservedStorageGiB.HasValue).ToList();
        if (cpu.Count > 0) result.ReservedCpu = cpu.Sum(item => item.ReservedCpu!.Value);
        if (memory.Count > 0) result.ReservedMemoryGiB = memory.Sum(item => item.ReservedMemoryGiB!.Value);
        if (storage.Count > 0) result.ReservedStorageGiB = storage.Sum(item => item.ReservedStorageGiB!.Value);
        result.IncompleteReservations = uses.Count(item => !item.ReservedCpu.HasValue || !item.ReservedMemoryGiB.HasValue || !item.ReservedStorageGiB.HasValue);
        var customerId = graph.Customers.FirstOrDefault(item => item.ExternalId == filter.CustomerExternalId)?.Id;
        foreach (var origin in graph.Origins.Where(item => item.ResourceId == resourceId))
        {
            if (customerId.HasValue && origin.CustomerId != customerId.Value) continue;
            if (filter.Environment.Length > 0 && origin.DeploymentExternalId.HasValue && origin.Environment != filter.Environment) continue;
            result.Origins.Add(new() { Kind = "direct", DeploymentExternalId = origin.DeploymentExternalId, ServiceName = origin.ServiceName, Environment = origin.Environment, Purpose = origin.Purpose });
            if (origin.DeploymentExternalId.HasValue) result.Origins[^1].Kind = "deployment";
        }
        var nodes = graph.Nodes.ToDictionary(item => item.Id);
        foreach (var edge in graph.Edges.Where(item => item.TargetId == resourceId))
        {
            if (selected != null && !selected.Customers.ContainsKey(edge.SourceId)) continue;
            if (!nodes.TryGetValue(edge.SourceId, out var source)) continue;
            result.Origins.Add(new() { Kind = "dependency", FromResourceExternalId = source.ExternalId, FromResourceName = source.Name, RelationshipKind = edge.Kind, Purpose = edge.Purpose });
        }
        result.Origins = result.Origins.GroupBy(item => new { item.Kind, item.FromResourceExternalId, item.DeploymentExternalId, item.RelationshipKind, item.Purpose }).Select(group => group.First()).ToList();
        result.OriginCount = result.Origins.Count;
        result.Origins = result.Origins.Take(100).ToList();
        return result;
    }
}
