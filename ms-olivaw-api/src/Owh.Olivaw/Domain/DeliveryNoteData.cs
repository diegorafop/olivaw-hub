using System;
namespace Owh.Olivaw.Domain;
public sealed class DeliveryNoteData
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string InternalNote { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Category { get; set; } = "feature";
    public string Status { get; set; } = "draft";
    public string ApprovalReference { get; set; } = string.Empty;
    public string ServiceLabel { get; set; } = string.Empty;
    public DateTime? DeliveredAt { get; set; }
    public static bool IsCategory(string value) => value is "feature" or "improvement" or "fix";
    public static bool IsStatus(string value) => value is "draft" or "published" or "withdrawn";
    public void Validate(DateTime now)
    {
        if (Title == null || Title.Length > 120) throw new ArgumentException("Campo inválido.", nameof(Title));
        if (Summary == null || Summary.Length > 3000) throw new ArgumentException("Campo inválido.", nameof(Summary));
        if (InternalNote == null || InternalNote.Length > 3000) throw new ArgumentException("Campo inválido.", nameof(InternalNote));
        if (Version == null || Version.Length > 80) throw new ArgumentException("Campo inválido.", nameof(Version));
        if (Category == null || Category.Length > 20) throw new ArgumentException("Campo inválido.", nameof(Category));
        if (Status == null || Status.Length > 20) throw new ArgumentException("Campo inválido.", nameof(Status));
        if (ApprovalReference == null || ApprovalReference.Length > 300) throw new ArgumentException("Campo inválido.", nameof(ApprovalReference));
        if (ServiceLabel == null || ServiceLabel.Length > 120) throw new ArgumentException("Campo inválido.", nameof(ServiceLabel));
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Summary)) throw new ArgumentException("Título e resumo obrigatórios.");
        if (!IsCategory(Category) || !IsStatus(Status)) throw new ArgumentException("Classificação inválida.");
        if (DeliveredAt.HasValue && DeliveredAt.Value.Kind != DateTimeKind.Utc) throw new ArgumentException("Data da entrega deve estar em UTC.");
        if (Status == "published")
        {
            if (!DeliveredAt.HasValue || DeliveredAt > now) throw new ArgumentException("Confirme uma entrega já realizada.");
            if (string.IsNullOrWhiteSpace(ApprovalReference)) throw new ArgumentException("Informe a referência da confirmação da entrega.");
        }
        if (Status == "withdrawn" && string.IsNullOrWhiteSpace(InternalNote)) throw new ArgumentException("Registre o motivo da retirada da publicação.");
    }
}
