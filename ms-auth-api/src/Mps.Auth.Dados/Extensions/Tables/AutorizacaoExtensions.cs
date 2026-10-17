using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Mps.Auth.Domain;

namespace Mps.Auth.Dados.Extensions.Tables;

public static class AutorizacaoExtensions
{
    internal static void SetupAutorizacao
    (
        this ModelBuilder modelBuilder
    )
    {
        modelBuilder
            .Entity<Autorizacao>()
            .Property(item => item.Tipo)
            .HasMaxLength(40);

        modelBuilder
            .Entity<Autorizacao>()
            .Property(item => item.Valor)
            .HasMaxLength(400);

        modelBuilder
            .Entity<Autorizacao>()
            .Property(item => item.Descricao)
            .HasMaxLength(200);

        modelBuilder
            .Entity<Autorizacao>()
            .Property(item => item.Nome)
            .HasMaxLength(100);
    }

    public static void SeedAutorizacaoDbCreateDataFacade
    (
        this MigrationBuilder migrationBuilder
    )
    {
        InsertDataAutorizacaoRoot(migrationBuilder);
        InsertDataAutorizacaoDashboard(migrationBuilder);
        InsertDataAutorizacaoFluxo(migrationBuilder);
        InsertDataAutorizacaoLocalFisico(migrationBuilder);
        InsertDataAutorizacaoSolicitacao(migrationBuilder);
        InsertDataAutorizacaoQuestionario(migrationBuilder);
        InsertDataAutorizacaoModeloTexto(migrationBuilder);
        InsertDataAutorizacaoUsuario(migrationBuilder);
        InsertDataAutorizacaoCargo(migrationBuilder);
        InsertDataAutorizacaoSituacao(migrationBuilder);
        InsertDataAutorizacaoPerfilAcesso(migrationBuilder);
        InsertDataAutorizacaoTipoArquivo(migrationBuilder);
        InsertDataAutorizacaoTipoMovimento(migrationBuilder);
        InsertDataAutorizacaoProtocolo(migrationBuilder);
        InsertDataAutorizacaoIntegracaoRest(migrationBuilder);
        InsertDataAutorizacaoAutenticacaoIntegracaoRest(migrationBuilder);
        InsertDataAutorizacaoPessoa(migrationBuilder);
        InsertDataAutorizacaoAutuacao(migrationBuilder);
        InsertDataAutorizacaoRelatorioRequisicaoMaterial(migrationBuilder);
        InsertDataAutorizacaoQualificacaoInteressado(migrationBuilder);
        InsertDataAutorizacaoMesaVirtual(migrationBuilder);
        InsertDataAutorizacaoRascunho(migrationBuilder);
        InsertDataAutorizacaoDocumentoVisualizacaoVisibilidade(migrationBuilder);
    }

    private static string[] GetAutorizacaoColunas()
    {
        return new string[]
        {
            "idexterno",
            "usuariologadoid",
            "datacadastro",
            "dataatualizacao",
            "dataremocao",
            "tipo",
            "valor",
            "nome",
            "descricao",
            "perfil"
        };
    }

    private static void InsertDataAutorizacaoDashboard
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Dashboard",
                "Home - dashboard principal",
                "Tarefas a realizar, meus pedidos, meu histórico e visão geral do setor.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoAutuacao
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Autuacao",
                "Fila de autuação",
                "Autuar requisitórios que estão na fila.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoRelatorioRequisicaoMaterial
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "RelatorioRequisicaoMaterial",
                "Relatório requisição de materiais",
                "Relatório para análise e estatística dos pedidos referentes a requisição de materiais.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoAutenticacaoIntegracaoRest
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "AutenticacaoIntegracaoRest",
                "Painel de autenticação para as integrações de sistemas.",
                "Acesso ao painel de autenticações utilizadas em integrações de sistemas.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoIntegracaoRest
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "IntegracaoRest",
                "Painel de integração de sistemas",
                "Acesso ao painel de integrações de sistemas.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoProtocolo
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Protocolo",
                "Pesquisa de protocolos",
                "Acesso ao painel de protocolos.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoTipoMovimento
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "TipoMovimento",
                "Painel de tipos de movimento",
                "Acesso ao painel de tipos de movimento.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoTipoArquivo
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "TipoArquivo",
                "Painel de tipos de arquivo",
                "Acesso ao painel de tipos de arquivo.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoPerfilAcesso
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "PerfilAcesso",
                "Painel de perfis acesso",
                "Acesso ao painel de perfis de acesso.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoSituacao
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Situacao",
                "Painel de situações",
                "Acesso ao painel de situações de documentos em fluxos.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoCargo
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Cargo",
                "Painel de cargos",
                "Acesso ao painel de cargos de funcionáros.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoUsuario
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Usuario",
                "Painel de usuários",
                "Acesso ao painel de usuários do sistema.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoModeloTexto
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "ModeloTexto",
                "Painel de modelo texto",
                "Acesso ao painel de modelos de texto.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoQuestionario
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Questionario",
                "Painel de questionários",
                "Acesso ao painel de questionários.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoSolicitacao
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Solicitacao",
                "Painel de solicitações",
                "Acesso ao painel de solicitações.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoLocalFisico
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "LocalFisico",
                "Painel de setores",
                "Acesso ao painel de setores.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoFluxo
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Fluxo",
                "Painel de fluxos",
                "Acesso ao painel de fluxos - BPM.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoRoot
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null, null,
                "Role",
                "root",
                "Usuário root",
                "Usuário master do sistema.",
                false
            }
        );
    }

    private static void InsertDataAutorizacaoQualificacaoInteressado
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "QualificacaoInteressado",
                "Qualificação de interessados",
                "Acesso ao painel de qualificação de interessados.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoPessoa
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Pessoa",
                "Painel de pessoas",
                "Acesso ao painel de pessoas.",
                true
            }
        );
    }


    private static void InsertDataAutorizacaoRascunho
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Rascunho",
                "Painel de rascunhos",
                "Acesso ao painel de rascunhos.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoMesaVirtual
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "MesaVirtual",
                "Mesa virtual",
                "Acesso ao painel da mesa virtual.",
                true
            }
        );
    }

    private static void InsertDataAutorizacaoDocumentoVisualizacaoVisibilidade
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "DocumentoVisualizacaoVisibilidade",
                "Permissão de acesso a visibilidade de documentos.",
                "Usuário terá acesso as visibilidades dos documentos, podendo alterá-las.",
                true
            }
        );
    }

    public static void SeedAutorizacaoTask1805Data
    (
        this MigrationBuilder migrationBuilder
    )
    {
        var roleValor = "Relatorio";

        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                roleValor,
                "Painel de relatórios",
                "Relatórios gerados pelo sistema.",
                true
            }
        );
    }

    public static void SeedAutorizacaoTask2129Data
    (
        this MigrationBuilder migrationBuilder
    )
    {
        InsertDataRelatorioDocumentoLocal(migrationBuilder);
        InsertDataDocumentoVisualizacaoVisibilidade(migrationBuilder);
        InsertDataRelatorioLocalFisico(migrationBuilder);
    }

    public static void SeedAutorizacaoTask2167Data
    (
        this MigrationBuilder migrationBuilder
    )
    {
        InsertDataPesquisaDocumento(migrationBuilder);
    }

    public static void SeedAutorizacaoTask2416Data
    (
        this MigrationBuilder migrationBuilder
    )
    {
        InsertDataGerenciamentoEtapaFalha(migrationBuilder);
    }

    public static void SeedAutorizacaoTask2619Data
    (
        this MigrationBuilder migrationBuilder
    )
    {
        InsertDataImpersonate(migrationBuilder);
    }

    public static void SeedAutorizacaoTask2635Data
    (
        this MigrationBuilder migrationBuilder
    )
    {
        InsertDataDocumentoMovimentoArquivar(migrationBuilder);
        InsertDataDocumentoMovimentoDesarquivar(migrationBuilder);
    }

    public static void SeedAutorizacaoTask4329Data
    (
        this MigrationBuilder migrationBuilder
    )
    {
        InsertDataUsuarioBronzeGov(migrationBuilder);
        InsertDataUsuarioSilverGov(migrationBuilder);
        InsertDataUsuarioGoldGov(migrationBuilder);
    }

    public static void SeedAutorizacaoTask2667Data
    (
        this MigrationBuilder migrationBuilder
    )
    {
        InsertDataPesquisarDocumentoGerencia(migrationBuilder);
    }

    public static void SeedAutorizacaoTask5471Data
    (
        this MigrationBuilder migrationBuilder
    )
    {
        InsertDataAuthorityOverride(migrationBuilder);
    }

    public static void SeedAutorizacaoTask6001Data
    (
        this MigrationBuilder migrationBuilder
    )
    {
        InsertDataHolosDashboard(migrationBuilder);
    }

    public static void SeedAutorizacaoTask7348Data
    (
        this MigrationBuilder migrationBuilder
    )
    {
        InsertDataBeneficiario(migrationBuilder);
        InsertDataProcurador(migrationBuilder);
    }

    public static void SeedAutorizacaoTask8257Data
    (
        this MigrationBuilder migrationBuilder
    )
    {
        InsertDataAuthorityOverrideBeneficiario(migrationBuilder);
        InsertDataAuthorityOverrideAdvogado(migrationBuilder);
        InsertDataAuthorityOverrideProcurador(migrationBuilder);
    }

    private static void InsertDataRelatorioDocumentoLocal
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "RelatorioDocumentoLocal",
                "Relátorio de documentos por local.",
                "Usuário terá permissão de acesso ao relatório dos documentos presentes num determinado setor.",
                true
            }
        );
    }

    private static void InsertDataDocumentoVisualizacaoVisibilidade
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "DocumentoVisualizacaoVisibilidade",
                "Permissão de acesso a visibilidade de documentos.",
                "Usuário terá acesso as visibilidades dos documentos, podendo alterá-las.",
                true
            }
        );
    }

    private static void InsertDataRelatorioLocalFisico
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "RelatorioLocalFisico",
                "Relátorio de setores.",
                "Acesso ao relatório de setores.",
                true
             }
        );
    }

    private static void InsertDataPesquisaDocumento
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Documento",
                "Painel de pesquisa de documentos.",
                "Acesso à tela de pesquisa de documentos.",
                true
            }
         );
    }

    private static void InsertDataGerenciamentoEtapaFalha
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "GerenciamentoEtapaFalha",
                "Painel de gerenciamento de etapas com falha.",
                "Acesso à tela de gerenciamento de etapas com falha.",
                true
             }
         );
    }

    private static void InsertDataImpersonate
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Impersonate",
                "Role básica para impersonate.",
                "Permite acesso a outros usuários via impersonate.",
                false
             }
         );
    }

    private static void InsertDataDocumentoMovimentoArquivar
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "DocumentoMovimentoArquivar",
                "Role para executar movimentos de arquivamento.",
                "Permite que o usuário possa executar qualquer movimento que esteja configurado como arquivar.",
                false
            }
        );
    }

    private static void InsertDataDocumentoMovimentoDesarquivar
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "DocumentoMovimentoDesarquivar",
                "Role para executar o movimento padrão 'desarquivar'.",
                "Permite que o usuário possa executar o movimento desarquivamento/restaurar algum documento excluído.",
                false
            }
        );
    }

    private static void InsertDataUsuarioSilverGov
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Silver",
                "Nível prata da Conta Gov-Br",
                "Nível da conta gov-br do cidadão.",
                false
             }
         );
    }

    private static void InsertDataUsuarioGoldGov
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Gold",
                "Nível ouro da Conta Gov-Br",
                "Nível da conta gov-br do cidadão.",
                false
             }
         );
    }

    private static void InsertDataUsuarioBronzeGov
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Bronze",
                "Nível bronze da Conta Gov-Br",
                "Nível da conta gov-br do cidadão.",
                false
             }
         );
    }

    private static void InsertDataPesquisarDocumentoGerencia
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "DocumentoGestor",
                "Autorização para habilitar filtros em pesquisar documento.",
                "Habilita campos específicos como pesquisar usuário interessado.",
                true
             }
         );
    }

    private static void InsertDataAuthorityOverride
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "AuthorityOverride",
                "Authority Override",
                "Permissão usada para contornar validações de acesso.",
                false
             }
         );
    }

    private static void InsertDataHolosDashboard
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "HolosDashboard",
                "Holos Dashboard",
                "Acesso aos dashboards do holos BI.",
                false
             }
         );
    }

    private static void InsertDataBeneficiario
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Beneficiario",
                "Beneficiario",
                "Role de perfil de acesso ao portal precatorio",
                false
             }
         );
    }

    private static void InsertDataProcurador
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "Procurador",
                "Procurador",
                "Role de perfil de acesso ao portal precatorio",
                false
             }
         );
    }

    private static void InsertDataAuthorityOverrideBeneficiario
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "AuthorityOverrideBeneficiario",
                "Authority Override Beneficiario",
                "Permissão usada para contornar validações de acesso e identificação de perfil.",
                false
             }
         );
    }

    private static void InsertDataAuthorityOverrideAdvogado
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "AuthorityOverrideAdvogado",
                "Authority Override Advogado",
                "Permissão usada para contornar validações de acesso e identificação de perfil.",
                false
             }
         );
    }

    private static void InsertDataAuthorityOverrideProcurador
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "autorizacao",
            GetAutorizacaoColunas(),
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Role",
                "AuthorityOverrideProcurador",
                "Authority Override Procurador",
                "Permissão usada para contornar validações de acesso e identificação de perfil.",
                false
             }
         );
    }
}