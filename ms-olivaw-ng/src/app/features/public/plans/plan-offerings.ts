export interface PlanOffering {
  id: string;
  name: string;
  note: string;
  text: string;
  monthlyPrice: number | null;
  monthlyImprovementPoints: number | null;
  features: readonly string[];
  creationOffer: string | null;
}
export const planOfferings: readonly PlanOffering[] = [
  {
    id: 'bronze',
    name: 'Bronze',
    note: 'Sua presença na internet',
    monthlyPrice: 49,
    monthlyImprovementPoints: 1,
    text: 'Um site estático para apresentar seu negócio e facilitar o primeiro contato.',
    features: [
      '2 GB de armazenamento',
      'Endereço Olivaw',
      'Site estático, sem API ou banco do negócio',
      'Pequenos ajustes de conteúdo com o ponto mensal incluído',
    ],
    creationOffer:
      'Criação e implantação incluídas para projetos Essenciais em assinaturas com mais de 12 meses.',
  },
  {
    id: 'prata',
    name: 'Prata',
    note: 'Seu conteúdo, nas suas mãos',
    monthlyPrice: null,
    monthlyImprovementPoints: 4,
    text: 'Publique conteúdos e use as funcionalidades administrativas combinadas para o seu site.',
    features: [
      '5 GB de armazenamento total',
      '2 contas administrativas do seu site',
      'Domínio próprio permitido',
      'API e PostgreSQL para as funcionalidades contratadas',
    ],
    creationOffer:
      'Criação e implantação incluídas para projetos Essenciais ou Intermediários em assinaturas com mais de 12 meses.',
  },
  {
    id: 'ouro',
    name: 'Ouro',
    note: 'Gestão para o seu negócio',
    monthlyPrice: null,
    monthlyImprovementPoints: 12,
    text: 'Um sistema de gestão com processos, usuários e permissões definidos para a sua operação.',
    features: [
      'Funcionalidades de gestão personalizadas',
      'Múltiplos usuários e perfis conforme proposta',
      'Capacidade dimensionada para o projeto',
      'Criação e evolução com escopo aprovado',
    ],
    creationOffer: null,
  },
  {
    id: 'diamante',
    name: 'Diamante',
    note: 'Uma parceria sob medida',
    monthlyPrice: null,
    monthlyImprovementPoints: 36,
    text: 'Para necessidades específicas de integração, capacidade e acompanhamento.',
    features: [
      'Contrato personalizado',
      'Integrações e capacidade sob proposta',
      'Entregas e atendimento negociados',
      'Evolução planejada com a equipe',
    ],
    creationOffer: null,
  },
];
export const projectScopes = [
  {
    name: 'Essencial',
    description: 'Uma primeira entrega com poucos fluxos e funcionalidades conhecidas.',
    example: 'Exemplo: site institucional estático com páginas de apresentação e contato.',
    bronze: true,
    prata: true,
  },
  {
    name: 'Intermediário',
    description: 'Conteúdo gerenciável e rotinas administrativas que pedem mais análise e testes.',
    example: 'Exemplo: catálogo com fotos, cadastro de conteúdo e duas contas administrativas.',
    bronze: false,
    prata: true,
  },
  {
    name: 'Avançado',
    description:
      'Regras específicas, integrações ou processos mais extensos, com esforço e riscos maiores.',
    example: 'Exemplo: gestão com vários perfis, aprovações e integração com outros sistemas.',
    bronze: false,
    prata: false,
  },
] as const;
export const creationOfferTerms =
  'Oferta para novas contratações com prazo superior a 12 meses. A Olivaw avalia complexidade e esforço estimado e confirma o enquadramento antes da assinatura. A proposta registra funcionalidades, etapas, horas previstas, critérios de aceite e rodadas de aprovação incluídas. A mensalidade continua sendo cobrada. Domínio próprio e serviços externos são tratados separadamente quando aplicáveis. Melhorias posteriores podem usar os pontos mensais, após análise; demandas acima da franquia ou fora do plano recebem orçamento separado. Permanência, cancelamento e demais condições são apresentados no contrato, antes da contratação.';

export const improvementPointRules =
  'Pontos de melhoria renovados a cada ciclo mensal, sem acúmulo. A equipe avalia escopo, complexidade, esforço e testes e informa os pontos antes de executar. O cliente aprova a estimativa; o consumo é confirmado após entrega e aceite. Ajustes no escopo precisam de nova aprovação. Correções de defeitos do escopo entregue não consomem pontos. A criação inicial incluída na oferta também não consome a franquia mensal. Pedidos acima da franquia ou incompatíveis com o plano recebem orçamento separado; qualquer custo adicional depende de orçamento e aprovação.';
export const improvementExamples = [
  {
    points: '1 ponto',
    task: 'Trocar uma foto ou um texto curto',
    detail:
      'Arquivo ou texto pronto, em um espaço já existente, sem edição de imagem, novo layout ou mudança de regra. Referência sujeita à análise.',
  },
  {
    points: '3 pontos',
    task: 'Uma pequena seção de evento',
    detail:
      'Reutilizando um bloco já existente, com conteúdo pronto e sem integração nova. Layout personalizado ou comportamento novo pode exigir mais pontos ou orçamento.',
  },
  {
    points: 'Sob análise',
    task: 'Nova funcionalidade ou integração',
    detail:
      'Regras, telas e testes são avaliados antes de informar pontos e prazo. Podemos dividir o pedido em entregas menores.',
  },
] as const;
export function improvementAllowanceLabel(id: string): string {
  const points = planOfferings.find((plan) => plan.id === id)?.monthlyImprovementPoints;
  return points === null || points === undefined
    ? 'Pontos de melhoria definidos em contrato'
    : `${points} ${points === 1 ? 'ponto' : 'pontos'} de melhoria por ciclo mensal`;
}
