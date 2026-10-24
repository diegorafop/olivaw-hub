using Microsoft.AspNetCore.Mvc;
using Owh.OlivawCore.Requests;

namespace Owh.OlivawCore.Api.Extensions.Serializations;

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