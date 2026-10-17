export type EventCategory = 'Casamentos' | 'Aniversários' | 'Confraternizações' | 'Corporativos';
export interface EventPhoto {
  src: string;
  alt: string;
  caption?: string;
}
export interface PortfolioEvent {
  id: string;
  title: string;
  category: EventCategory;
  description: string;
  services: string[];
  photos: EventPhoto[];
  location?: string;
  dateLabel?: string;
  preview?: boolean;
}

// Somente eventos reais, aprovados pelo cliente, com fotos autorizadas.
export const PUBLISHED_EVENTS: PortfolioEvent[] = [];

// Modelos de apresentação; não são eventos realizados ou clientes atendidos.
export const PREVIEW_EVENTS: PortfolioEvent[] = [
  {
    id: 'preview-wedding',
    title: 'Uma celebração a dois',
    category: 'Casamentos',
    description:
      'Aqui entra a história de um casamento atendido pela LTMJ: o estilo da celebração, os serviços realizados e os detalhes que merecem ser lembrados.',
    services: ['Gastronomia', 'Bar e drinks', 'Equipe de atendimento'],
    photos: [],
    preview: true,
  },
  {
    id: 'preview-birthday',
    title: 'Mais um motivo para brindar',
    category: 'Aniversários',
    description:
      'Este espaço poderá reunir as fotos e o relato de um aniversário, destacando o cardápio escolhido e o cuidado com cada convidado.',
    services: ['Buffet', 'Bar e drinks'],
    photos: [],
    preview: true,
  },
  {
    id: 'preview-gathering',
    title: 'Bons encontros ao redor da mesa',
    category: 'Confraternizações',
    description:
      'Um futuro registro de confraternização: ambiente, sabores e equipe. O conteúdo será substituído por um evento real aprovado pela LTMJ.',
    services: ['Churrasco', 'Equipe de atendimento'],
    photos: [],
    preview: true,
  },
  {
    id: 'preview-corporate',
    title: 'Conexões que merecem um encontro',
    category: 'Corporativos',
    description:
      'Modelo para apresentar uma ocasião corporativa, o formato de atendimento e a estrutura efetivamente contratada.',
    services: ['Gastronomia', 'Equipe de atendimento'],
    photos: [],
    preview: true,
  },
];
