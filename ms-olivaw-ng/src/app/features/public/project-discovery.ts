export interface DiscoveryOption {
  id: string;
  label: string;
  description?: string;
}

export interface DiscoveryPath extends DiscoveryOption {
  icon: string;
  question: string;
  choices: readonly DiscoveryOption[];
  contextQuestion: string;
  contexts: readonly DiscoveryOption[];
  recommendation: string;
  recommendationDescription: string;
  essentials: readonly string[];
}

const unsure: DiscoveryOption = {
  id: 'unsure',
  label: 'Ainda não sei',
  description: 'Quero ajuda para decidir.',
};

export const discoveryPaths: readonly DiscoveryPath[] = [
  {
    id: 'website',
    label: 'Quero um site',
    description: 'Um lugar para apresentar meu negócio.',
    icon: 'M3 5h18v14H3z M3 9h18 M7 7h.01 M10 7h.01',
    question: 'O que você gostaria que as pessoas fizessem no seu site?',
    choices: [
      { id: 'services', label: 'Conhecer meus serviços' },
      { id: 'portfolio', label: 'Ver meus trabalhos' },
      { id: 'quotes', label: 'Pedir um orçamento' },
      unsure,
    ],
    contextQuestion: 'Hoje, onde as pessoas encontram seu negócio?',
    contexts: [
      { id: 'social', label: 'Nas redes sociais' },
      { id: 'whatsapp', label: 'No WhatsApp ou por indicação' },
      { id: 'existing-site', label: 'Já tenho um site' },
      { id: 'starting', label: 'Estou começando agora' },
    ],
    recommendation: 'Um lugar para o seu negócio ser encontrado.',
    recommendationDescription:
      'Uma página clara pode apresentar o que você faz e facilitar o próximo passo de quem chega até você.',
    essentials: [
      'Apresentação do seu negócio e dos seus serviços',
      'Conteúdo fácil de consultar pelo celular',
      'Um caminho direto para entrar em contato',
    ],
  },
  {
    id: 'automation',
    label: 'Quero automatizar tarefas',
    description: 'Reduzir atividades repetidas e informações copiadas à mão.',
    icon: 'M20 7a8 8 0 0 0-14-1L3 9 M3 4v5h5 M4 17a8 8 0 0 0 14 1l3-3 M21 20v-5h-5',
    question: 'Qual tarefa mais se repete no seu dia?',
    choices: [
      { id: 'copying', label: 'Copiar informações de um lugar para outro' },
      { id: 'reminders', label: 'Enviar avisos e lembretes' },
      { id: 'reports', label: 'Atualizar controles e montar relatórios' },
      unsure,
    ],
    contextQuestion: 'Como essa tarefa acontece hoje?',
    contexts: [
      { id: 'manual', label: 'Faço tudo manualmente' },
      { id: 'spreadsheet', label: 'Uso planilhas para ajudar' },
      { id: 'system', label: 'Uso ferramentas que não conversam entre si' },
      { id: 'unsure', label: 'Preciso olhar melhor para essa rotina' },
    ],
    recommendation: 'Uma tarefa repetida pode ser um bom primeiro passo.',
    recommendationDescription:
      'Podemos entender como essa tarefa funciona e avaliar o que faz sentido automatizar, começando por uma parte pequena e fácil de acompanhar.',
    essentials: [
      'Mapear a tarefa e as informações envolvidas',
      'Conferir o que as ferramentas atuais permitem fazer',
      'Testar uma primeira automação e manter seu controle sobre o processo',
    ],
  },
  {
    id: 'tool',
    label: 'Quero uma ferramenta para minha rotina',
    description: 'Substituir uma planilha ou criar um app ou sistema para o que preciso.',
    icon: 'M4 4h6v6H4z M14 4h6v6h-6z M4 14h6v6H4z M14 14h6v6h-6z',
    question: 'O que você gostaria de cuidar com essa ferramenta?',
    choices: [
      { id: 'customers', label: 'Clientes e pedidos' },
      { id: 'finances', label: 'Entradas e despesas' },
      { id: 'tasks', label: 'Agenda e tarefas da equipe' },
      { id: 'app-idea', label: 'Tenho outra ideia de app ou sistema' },
      unsure,
    ],
    contextQuestion: 'Como você cuida disso hoje?',
    contexts: [
      { id: 'spreadsheet', label: 'Uso uma planilha' },
      { id: 'scattered', label: 'Anoto em mensagens e papéis' },
      { id: 'starting', label: 'Ainda não tenho um controle' },
      { id: 'beginning', label: 'É uma ideia nova que quero experimentar' },
    ],
    recommendation: 'Uma ferramenta que acompanhe seu jeito de trabalhar.',
    recommendationDescription:
      'Podemos partir da sua rotina ou da sua ideia e escolher o essencial para uma primeira versão. O formato vem depois de entender quem vai usar e para quê.',
    essentials: [
      'Entender quem vai usar e o que precisa resolver',
      'Escolher as funções mais importantes para começar',
      'Experimentar uma primeira versão antes de ampliar',
    ],
  },
  {
    id: 'improve',
    label: 'Quero melhorar algo que já existe',
    description: 'Evoluir meu site, sistema ou forma de trabalhar.',
    icon: 'M4 7h16 M4 17h16 M8 4v6 M16 14v6',
    question: 'O que você já tem e quer melhorar?',
    choices: [
      { id: 'website', label: 'Um site' },
      { id: 'system', label: 'Um app ou sistema' },
      { id: 'process', label: 'Uma forma de trabalhar' },
      unsure,
    ],
    contextQuestion: 'O que mais incomoda hoje?',
    contexts: [
      { id: 'outdated', label: 'Está desatualizado' },
      { id: 'difficult', label: 'É difícil ou demorado de usar' },
      { id: 'missing', label: 'Falta algo importante' },
      { id: 'unsure', label: 'Quero ajuda para identificar o que pode melhorar' },
    ],
    recommendation: 'Evoluir o que existe, começando pelo que mais faz falta.',
    recommendationDescription:
      'Podemos avaliar sua solução atual, entender o que funciona bem e identificar uma melhoria possível antes de propor uma troca ou reconstrução.',
    essentials: [
      'Revisar o que você já usa e gostaria de preservar',
      'Identificar a dificuldade que merece atenção primeiro',
      'Avaliar os ajustes possíveis e as dependências antes de definir o escopo',
    ],
  },
  {
    id: 'explore',
    label: 'Ainda não sei por onde começar',
    description: 'Vamos encontrar um caminho juntos.',
    icon: 'M12 3v4 M12 17v4 M3 12h4 M17 12h4 M8 8l8 8 M16 8l-8 8',
    question: 'O que mais ajudaria você neste momento?',
    choices: [
      { id: 'visibility', label: 'Divulgar melhor meu negócio' },
      { id: 'time', label: 'Gastar menos tempo com tarefas repetidas' },
      { id: 'clarity', label: 'Ter mais clareza sobre o que acontece' },
      { id: 'exploring', label: 'Só quero conhecer as possibilidades' },
    ],
    contextQuestion: 'Como você prefere começar?',
    contexts: [
      { id: 'small', label: 'Com uma melhoria pequena' },
      { id: 'examples', label: 'Vendo exemplos próximos da minha realidade' },
      { id: 'conversation', label: 'Conversando sobre minhas dificuldades' },
      { id: 'unsure', label: 'Quero ajuda para decidir' },
    ],
    recommendation: 'Encontrar uma oportunidade já é um bom começo.',
    recommendationDescription:
      'Você não precisa escolher uma tecnologia agora. Podemos olhar para uma dificuldade da sua rotina e identificar uma melhoria que faça sentido.',
    essentials: [
      'Partir de uma necessidade real do seu dia a dia',
      'Explorar possibilidades em linguagem simples',
      'Definir um próximo passo sem antecipar o escopo',
    ],
  },
];
