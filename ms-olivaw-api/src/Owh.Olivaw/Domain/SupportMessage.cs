using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class SupportMessage : Entity
{
    public int SupportRequestId { get; private set; }
    public SupportRequest SupportRequest { get; private set; } = null!;
    public Guid MessageKey { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public string AuthorName { get; private set; } = string.Empty;
    public bool IsPublic { get; private set; }
    public bool IsStaff { get; private set; }
    private SupportMessage() { }
    public static SupportMessage Create(SupportRequest request, Guid key, string body, bool isPublic, bool staff, Guid actor, string author, DateTime now)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (key == Guid.Empty || actor == Guid.Empty) throw new ArgumentException("Identificação obrigatória.");
        if (string.IsNullOrWhiteSpace(body) || body.Length > 3000) throw new ArgumentException("Mensagem inválida.");
        if (string.IsNullOrWhiteSpace(author) || author.Length > 120) throw new ArgumentException("Autor inválido.");
        return new() { ExternalId = Guid.NewGuid(), SupportRequest = request, SupportRequestId = request.Id, MessageKey = key, Body = body, IsPublic = isPublic, IsStaff = staff, ActorUserId = actor, AuthorName = author, CreatedAt = now };
    }
}
