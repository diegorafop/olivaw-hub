using System;
namespace Owh.Olivaw.Domain;
public sealed class IncidentData
{
    public string Title { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string NextAction { get; set; } = string.Empty;
    public string InternalNote { get; set; } = string.Empty;
    public string Severity { get; set; } = "attention";
    public string Status { get; set; } = "detected";
    public static bool IsStatus(string value) => value is "detected" or "assessment" or "handling" or "observation" or "resolved";
    public static bool IsSeverity(string value) => value is "attention" or "major" or "critical";
    public void Validate()
    {
        if (Title == null || Title.Length > 120) throw new ArgumentException("Campo inválido.", nameof(Title));
        if (Impact == null || Impact.Length > 1000) throw new ArgumentException("Campo inválido.", nameof(Impact));
        if (Owner == null || Owner.Length > 120) throw new ArgumentException("Campo inválido.", nameof(Owner));
        if (NextAction == null || NextAction.Length > 500) throw new ArgumentException("Campo inválido.", nameof(NextAction));
        if (InternalNote == null || InternalNote.Length > 3000) throw new ArgumentException("Campo inválido.", nameof(InternalNote));
        if (string.IsNullOrWhiteSpace(Title)) throw new ArgumentException("Campo obrigatório.", nameof(Title));
        if (string.IsNullOrWhiteSpace(Owner)) throw new ArgumentException("Campo obrigatório.", nameof(Owner));
        if (string.IsNullOrWhiteSpace(NextAction)) throw new ArgumentException("Campo obrigatório.", nameof(NextAction));
        if (string.IsNullOrWhiteSpace(Impact)) throw new ArgumentException("Campo obrigatório.", nameof(Impact));
        if (!IsStatus(Status)) throw new ArgumentException("Estado inválido.");
        if (!IsSeverity(Severity)) throw new ArgumentException("Classificação inválida.");
        if (Status is "resolved" or "closed" or "cancelled")
        {
            if (string.IsNullOrWhiteSpace(InternalNote)) throw new ArgumentException("Registre a justificativa do encerramento.");
        }
    }
}
