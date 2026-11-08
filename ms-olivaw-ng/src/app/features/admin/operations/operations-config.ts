export interface OperationField {
  key: string;
  label: string;
  type?: 'number' | 'datetime-local' | 'textarea';
  required?: boolean;
  max?: number;
  source?: 'services' | 'requests';
  options?: Record<string, string>;
}
export interface OperationConfig {
  title: string;
  endpoint: string;
  anchor: 'resourceExternalId' | 'customerExternalId';
  workflow: boolean;
  fields: OperationField[];
  notice: string;
}
const common: OperationField[] = [
  { key: 'title', label: 'Título', required: true, max: 120 },
  { key: 'impact', label: 'Impacto', type: 'textarea', max: 1000 },
  { key: 'owner', label: 'Responsável', required: true, max: 120 },
  { key: 'nextAction', label: 'Próxima ação', required: true, max: 500 },
  { key: 'internalNote', label: 'Nota interna / justificativa', type: 'textarea', max: 3000 },
];
const observation: OperationField[] = [
  { key: 'source', label: 'Fonte da informação', required: true, max: 300 },
  { key: 'observedAt', label: 'Instante da observação', required: true, type: 'datetime-local' },
  { key: 'note', label: 'Observações internas', type: 'textarea', max: 1000 },
];
export const operationConfigs: Record<string, OperationConfig> = {
  deliveries: {
    title: 'Entregas e notas de versão',
    endpoint: 'delivery-notes',
    anchor: 'customerExternalId',
    workflow: true,
    notice:
      'Sugestão de fluxo: salve primeiro o rascunho, confirme a entrega e publique para o cliente. Publicar esta nota não executa deploy. Rascunhos, referências de confirmação e notas internas ficam restritos à equipe.',
    fields: [
      { key: 'serviceExternalId', label: 'Serviço relacionado', source: 'services' },
      { key: 'requestExternalId', label: 'Pedido relacionado', source: 'requests' },
      { key: 'title', label: 'Título público', required: true, max: 120 },
      {
        key: 'summary',
        label: 'Resumo público da entrega',
        type: 'textarea',
        required: true,
        max: 3000,
      },
      { key: 'serviceLabel', label: 'Nome público do sistema nesta nota', max: 120 },
      { key: 'version', label: 'Versão, se houver', max: 80 },
      {
        key: 'category',
        label: 'Tipo da entrega',
        required: true,
        options: { feature: 'Funcionalidade', improvement: 'Melhoria', fix: 'Correção' },
      },
      { key: 'deliveredAt', label: 'Data da entrega confirmada', type: 'datetime-local' },
      { key: 'approvalReference', label: 'Referência da confirmação de entrega', max: 300 },
      {
        key: 'internalNote',
        label: 'Nota interna / motivo da retirada',
        type: 'textarea',
        max: 3000,
      },
      {
        key: 'status',
        label: 'Publicação',
        required: true,
        options: {
          draft: 'Rascunho',
          published: 'Publicada para o cliente',
          withdrawn: 'Retirada da publicação',
        },
      },
    ],
  },
  health: {
    title: 'Saúde',
    endpoint: 'health-readings',
    anchor: 'resourceExternalId',
    workflow: false,
    notice:
      'Registre uma verificação pontual com fonte e horário. Sem amostras recentes, a saúde permanece sem confirmação. A coleta automática ainda depende de definição e autorização da fonte.',
    fields: [
      ...observation,
      {
        key: 'outcome',
        label: 'Resultado observado',
        required: true,
        options: {
          success: 'Verificação pontual OK',
          failure: 'Falha observada',
          timeout: 'Tempo esgotado',
          'source-error': 'Erro da fonte',
        },
      },
      {
        key: 'maxAgeMinutes',
        label: 'Validade da informação (minutos, até 1440)',
        required: true,
        type: 'number',
      },
    ],
  },
  incidents: {
    title: 'Incidentes',
    endpoint: 'incidents',
    anchor: 'resourceExternalId',
    workflow: true,
    notice:
      'Uma ocorrência aberta por recurso de origem. Compartilhamento não cria incidentes duplicados por cliente. Resolver exige observação e justificativa de recuperação. Notificações externas não estão configuradas.',
    fields: [
      ...common.map((field) => ({ ...field, required: field.required || field.key === 'impact' })),
      {
        key: 'severity',
        label: 'Gravidade',
        required: true,
        options: { attention: 'Atenção', major: 'Alta', critical: 'Crítica' },
      },
      {
        key: 'status',
        label: 'Estado',
        required: true,
        options: {
          detected: 'Detectado',
          assessment: 'Em avaliação',
          handling: 'Em atendimento',
          observation: 'Em observação',
          resolved: 'Resolvido',
        },
      },
    ],
  },
  usage: {
    title: 'Consumo e capacidade',
    endpoint: 'usage-readings',
    anchor: 'resourceExternalId',
    workflow: false,
    notice:
      'Registre valores pontuais em GiB, sem somar amostras do mesmo contador. Uso total da VM não representa uso individual por cliente. Limites e reservas do inventário são informações distintas.',
    fields: [
      ...observation,
      {
        key: 'metric',
        label: 'Métrica',
        required: true,
        options: { 'storage-gib': 'Armazenamento (GiB)', 'memory-gib': 'Memória (GiB)' },
      },
      { key: 'value', label: 'Valor (GiB)', required: true, type: 'number' },
      {
        key: 'quality',
        label: 'Qualidade da informação',
        required: true,
        options: {
          manual: 'Informada manualmente',
          measured: 'Medida na fonte indicada',
          estimated: 'Estimada',
        },
      },
    ],
  },
  care: {
    title: 'Publicações e cuidados',
    endpoint: 'care-evidence',
    anchor: 'resourceExternalId',
    workflow: false,
    notice:
      'Evidências declaradas com fonte. Publicação concluída não comprova saúde; backup concluído não comprova restauração. O registro não executa comandos ou acessa o endereço informado.',
    fields: [
      ...observation,
      {
        key: 'kind',
        label: 'Tipo de evidência',
        required: true,
        options: { deployment: 'Publicação', backup: 'Backup', restore: 'Teste de restauração' },
      },
      {
        key: 'outcome',
        label: 'Resultado',
        required: true,
        options: {
          success: 'Concluído segundo a fonte',
          failure: 'Falha',
          unknown: 'Sem confirmação',
        },
      },
      { key: 'referenceUrl', label: 'Referência HTTP(S), sem parâmetros ou credenciais', max: 300 },
    ],
  },
  support: {
    title: 'Atendimento interno',
    endpoint: 'support-requests',
    anchor: 'customerExternalId',
    workflow: true,
    notice:
      'Atendimento interno por empresa. Solicitar melhoria não aprova escopo, preço ou prazo e não consome franquia automaticamente. Notas e histórico permanecem internos.',
    fields: [
      { key: 'serviceExternalId', label: 'Serviço relacionado', source: 'services' },
      { key: 'publicSummary', label: 'Resumo visível ao cliente', type: 'textarea', max: 1000 },
      ...common,
      { key: 'requester', label: 'Solicitante', required: true, max: 120 },
      {
        key: 'category',
        label: 'Tipo do pedido',
        required: true,
        options: { question: 'Dúvida', problem: 'Problema', improvement: 'Melhoria' },
      },
      {
        key: 'status',
        label: 'Estado',
        required: true,
        options: {
          received: 'Recebido',
          analysis: 'Em análise',
          waiting: 'Aguardando resposta',
          planned: 'Planejado',
          working: 'Em execução',
          validation: 'Em validação',
          delivered: 'Entregue',
          closed: 'Encerrado',
          cancelled: 'Cancelado',
        },
      },
    ],
  },
};
