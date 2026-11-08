using System;
using System.Collections.Generic;
using Owh.OlivawCore.Domain;

namespace Owh.Ltmj.Domain;

public class PortfolioEvent : Entity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public bool IsVisible { get; private set; }
    public List<PortfolioPhoto> Photos { get; private set; } = new();

    public static PortfolioEvent Create(string title, string description, string category, Guid userId)
    {
        var item = new PortfolioEvent { ExternalId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        item.Update(title, description, category, userId);
        return item;
    }

    public void Update(string title, string description, string category, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Informe o título.", nameof(title));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Informe a descrição.", nameof(description));
        if (!IsCategory(category)) throw new ArgumentException("Categoria inválida.", nameof(category));
        Title = title.Trim();
        Description = description.Trim();
        Category = category;
        ActorUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetVisibility(bool visible, Guid userId)
    {
        IsVisible = visible;
        ActorUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public static bool IsCategory(string category)
    {
        return category is "Casamentos" or "Aniversários" or "Confraternizações" or "Corporativos";
    }
}
