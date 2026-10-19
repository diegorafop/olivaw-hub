export type SiteBlock = 'services' | 'gallery' | 'products' | 'questions' | 'contact' | 'spreadsheet';
export interface SitePreset {
  id: string;
  label: string;
  subtitle: string;
  eyebrow: string;
  headline: string;
  description: string;
  cta: string;
  blocks: readonly SiteBlock[];
}
export const sitePresets: readonly SitePreset[] = [
  {
    id: 'business',
    label: 'Apresentar meu negócio',
    subtitle: 'Uma página para começar bem',
    eyebrow: 'FEITO PARA QUEM QUER IR ALÉM',
    headline: 'Seu trabalho merece ser encontrado.',
    description:
      'Um espaço para apresentar o que você faz, mostrar seu diferencial e aproximar quem precisa de você.',
    cta: 'Conheça nossos serviços',
    blocks: ['services', 'contact'],
  },
  {
    id: 'portfolio',
    label: 'Mostrar meus trabalhos',
    subtitle: 'Seu portfólio em destaque',
    eyebrow: 'IDEIAS QUE GANHAM VIDA',
    headline: 'O que você faz fala por você.',
    description:
      'Cada trabalho tem uma história. Aqui, suas melhores entregas encontram o espaço que merecem.',
    cta: 'Explore os trabalhos',
    blocks: ['gallery', 'contact'],
  },
  {
    id: 'catalog',
    label: 'Divulgar produtos',
    subtitle: 'Um catálogo para explorar',
    eyebrow: 'ESCOLHAS COM PERSONALIDADE',
    headline: 'Encontre algo que combina com você.',
    description:
      'Uma seleção dos seus produtos, com detalhes claros e um caminho simples para saber mais.',
    cta: 'Conheça a seleção',
    blocks: ['products', 'contact'],
  },
];
export const siteBlocks: readonly { id: SiteBlock; label: string; description: string }[] = [
  { id: 'spreadsheet', label: 'Uma planilha', description: 'Transforme linhas em uma tabela interativa.' },
  { id: 'services', label: 'Meus serviços', description: 'Explique o que você oferece.' },
  { id: 'gallery', label: 'Meus trabalhos', description: 'Dê espaço ao que você já fez.' },
  { id: 'products', label: 'Meus produtos', description: 'Apresente uma seleção de produtos.' },
  {
    id: 'questions',
    label: 'Dúvidas frequentes',
    description: 'Ajude quem está conhecendo seu negócio.',
  },
  { id: 'contact', label: 'Um jeito de conversar', description: 'Facilite o próximo contato.' },
];
