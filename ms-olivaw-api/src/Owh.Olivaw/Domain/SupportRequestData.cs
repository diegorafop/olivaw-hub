using System;
namespace Owh.Olivaw.Domain;
public sealed class SupportRequestData
{
    public string Title { get; set; } = string.Empty;
    public string Requester { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string NextAction { get; set; } = string.Empty;
    public string PublicSummary { get; set; } = string.Empty;
    public string InternalNote { get; set; } = string.Empty;
    public string Category { get; set; } = "question";
    public string Status { get; set; } = "received";
    public static bool IsStatus(string value) => value is "received" or "analysis" or "waiting" or "planned" or "working" or "validation" or "delivered" or "closed" or "cancelled";
    public static bool IsCategory(string value) => value is "problem" or "question" or "improvement";
    public void Validate()
    {
        if (PublicSummary == null || PublicSummary.Length > 1000) throw new ArgumentException("Resumo público inválido.");
        if (Title == null || Title.Length > 120) throw new ArgumentException("Campo inválido.", nameof(Title));
        if (Requester == null || Requester.Length > 120) throw new ArgumentException("Campo inválido.", nameof(Requester));
        if (Impact == null || Impact.Length > 1000) throw new ArgumentException("Campo inválido.", nameof(Impact));
        if (Owner == null || Owner.Length > 120) throw new ArgumentException("Campo inválido.", nameof(Owner));
        if (NextAction == null || NextAction.Length > 500) throw new ArgumentException("Campo inválido.", nameof(NextAction));
        if (InternalNote == null || InternalNote.Length > 3000) throw new ArgumentException("Campo inválido.", nameof(InternalNote));
        if (string.IsNullOrWhiteSpace(Title)) throw new ArgumentException("Campo obrigatório.", nameof(Title));
        if (string.IsNullOrWhiteSpace(Owner)) throw new ArgumentException("Campo obrigatório.", nameof(Owner));
        if (string.IsNullOrWhiteSpace(NextAction)) throw new ArgumentException("Campo obrigatório.", nameof(NextAction));
        if (string.IsNullOrWhiteSpace(Requester)) throw new ArgumentException("Campo obrigatório.", nameof(Requester));
        if (!IsStatus(Status)) throw new ArgumentException("Estado inválido.");
        if (!IsCategory(Category)) throw new ArgumentException("Classificação inválida.");
        if (Status is "resolved" or "closed" or "cancelled")
        {
            if (string.IsNullOrWhiteSpace(InternalNote)) throw new ArgumentException("Registre a justificativa do encerramento.");
        }
    }
}
