using Microsoft.AspNetCore.Mvc;
using Mps.OlivawCore.Requests;

namespace Mps.OlivawCore.Api.Extensions.Serializations;

public static class FileExtensions
{
    public static FileStreamResult ToFileStreamResult
    (
        this ConteudoArquivoQueryResponse request
    )
    {
        return new FileStreamResult
        (
            new MemoryStream(request.Content),
            request.ContentType
        )
        {
            FileDownloadName = request.FileName
        };
    }
}