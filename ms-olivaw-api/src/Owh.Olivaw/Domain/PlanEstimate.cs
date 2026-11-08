using System;
using System.Collections.Generic;
using System.Linq;

namespace Owh.Olivaw.Domain;

public sealed class PlanCostLine
{
    public string Name { get; set; } = string.Empty;
    public decimal? MonthlyCost { get; set; }
    public decimal AllocationPercent { get; set; }
}

public sealed class PlanEstimate
{
    public decimal KnownMonthlyCost { get; private set; }
    public decimal? SuggestedMonthlyPrice { get; private set; }
    public decimal UnallocatedMonthlyCost { get; private set; }
    public int MissingCosts { get; private set; }
    public static PlanEstimate Calculate(IReadOnlyCollection<PlanCostLine> lines, decimal supportHours, decimal hourlyCost, decimal reservePercent, decimal marginPercent, decimal commercialPercent = 0)
    {
        if (lines == null) throw new ArgumentNullException(nameof(lines));
        if (lines.Count > 100) throw new ArgumentException("Limite de 100 itens.");
        if (supportHours < 0 || supportHours > 1000) throw new ArgumentException("Horas inválidas.");
        if (hourlyCost < 0 || hourlyCost > 1000000) throw new ArgumentException("Custo por hora inválido.");
        if (reservePercent < 0 || reservePercent > 100) throw new ArgumentException("Reserva inválida.");
        if (marginPercent < 0 || marginPercent >= 100) throw new ArgumentException("Margem deve ser menor que 100%.");
        if (commercialPercent < 0 || marginPercent + commercialPercent >= 100) throw new ArgumentException("Margem e despesas comerciais precisam somar menos que 100%.");
        foreach (var number in new[] { supportHours, hourlyCost, reservePercent, marginPercent, commercialPercent })
            if (decimal.Round(number, 2) != number) throw new ArgumentException("Use no máximo duas casas decimais.");
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line.Name) || line.Name.Length > 120) throw new ArgumentException("Identifique cada custo.");
            if (line.MonthlyCost < 0 || line.MonthlyCost > 1000000000) throw new ArgumentException("Custo inválido.");
            if (line.MonthlyCost.HasValue && decimal.Round(line.MonthlyCost.Value, 2) != line.MonthlyCost.Value) throw new ArgumentException("Custo admite duas casas decimais.");
            if (decimal.Round(line.AllocationPercent, 2) != line.AllocationPercent) throw new ArgumentException("Atribuição admite duas casas decimais.");
            if (line.AllocationPercent < 0 || line.AllocationPercent > 100) throw new ArgumentException("Atribuição inválida.");
        }
        var known = lines.Where(item => item.MonthlyCost.HasValue).Sum(item => item.MonthlyCost!.Value * item.AllocationPercent / 100);
        var unallocated = lines.Where(item => item.MonthlyCost.HasValue).Sum(item => item.MonthlyCost!.Value * (100 - item.AllocationPercent) / 100);
        var result = new PlanEstimate { KnownMonthlyCost = decimal.Round(known, 2, MidpointRounding.AwayFromZero), UnallocatedMonthlyCost = decimal.Round(unallocated, 2, MidpointRounding.AwayFromZero), MissingCosts = lines.Count(item => !item.MonthlyCost.HasValue) };
        if (result.MissingCosts > 0 || lines.Count == 0) return result;
        var costWithReserve = decimal.Round((result.KnownMonthlyCost + supportHours * hourlyCost) * (1 + reservePercent / 100), 2, MidpointRounding.AwayFromZero);
        result.SuggestedMonthlyPrice = decimal.Round(costWithReserve / (1 - (marginPercent + commercialPercent) / 100), 2, MidpointRounding.AwayFromZero);
        return result;
    }
}
