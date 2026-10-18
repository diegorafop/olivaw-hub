namespace Owh.OlivawCore.Requests;

public class ReadVersionQuery { }

public class ReadVersionQueryResponse
{
    public string Titulo { get; set; }
    public string Versao { get; set; }
    public string UltimaAtualizacao { get; set; }
}