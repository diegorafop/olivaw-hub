import { PortfolioEvent, PUBLISHED_EVENTS } from './event-portfolio.data';

// Seleções de fotos reais para avaliação; agrupamento e categorias ainda não confirmados.
export const PORTFOLIO_SAMPLES: PortfolioEvent[] = [
  ...PUBLISHED_EVENTS,
  ...[
    {
      id: 'buffet-ao-ar-livre',
      title: 'Buffet e encontros ao ar livre',
      category: 'Confraternizações',
      description:
        'Mesas preparadas, buffet e ambiente acolhedor. Uma seleção do acervo para mostrar como os detalhes do encontro podem ganhar espaço no portfólio.',
      count: 4,
    },
    {
      id: 'churrasco-na-brasa',
      title: 'Churrasco na brasa',
      category: 'Confraternizações',
      description:
        'O preparo das carnes e a equipe junto à churrasqueira: registros dos sabores e dos bastidores do atendimento.',
      count: 3,
    },
    {
      id: 'bar-e-drinks',
      title: 'Bar e drinks para celebrar',
      category: 'Aniversários',
      description:
        'Frutas, bebidas e balcões preparados para receber. Uma seleção para apresentar o serviço de bar e o cuidado com cada detalhe.',
      count: 3,
    },
    {
      id: 'equipe-em-acao',
      title: 'Equipe em ação',
      category: 'Corporativos',
      description:
        'Da preparação ao atendimento, a equipe participa de cada etapa. Registros selecionados para apresentar quem faz a experiência acontecer.',
      count: 4,
    },
  ].map((sample) => ({
    id: sample.id,
    title: sample.title,
    category: sample.category as PortfolioEvent['category'],
    description: `${sample.description} Seleção demonstrativa: agrupamento e categoria sujeitos à confirmação pela LTMJ.`,
    services: [],
    preview: true,
    photos: Array.from({ length: sample.count }, (_, index) => ({
      src: `/assets/events/${sample.id}/foto${String(index + 1).padStart(2, '0')}.jpeg`,
      alt: `${sample.title} — registro do acervo LTMJ ${index + 1}`,
    })),
  })),
];
