namespace Mps.Auth.Domain.Enum;

public enum StatusIntegracaoEnum
{
    None,
    NaoIntegrar,
    // TODO: Renomear, deixando mais claro que se refere à integração unitária acada 30s.  
    Pendente,
    Integrado,
    Processando,
    PendenteProcessoAdmissao
}