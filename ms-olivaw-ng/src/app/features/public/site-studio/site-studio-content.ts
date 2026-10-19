export type SiteBlock =
  'services' | 'gallery' | 'products' | 'questions' | 'contact' | 'spreadsheet';
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
    eyebrow: 'Feito para quem quer ir além',
    headline: 'Seu trabalho merece ser encontrado.',
    description: 'Um espaço para mostrar o que você faz.',
    cta: 'Conheça nossos serviços',
    blocks: ['services'],
  },
  {
    id: 'portfolio',
    label: 'Mostrar meus trabalhos',
    subtitle: 'Seu portfólio em destaque',
    eyebrow: 'Ideias que ganham vida',
    headline: 'O que você faz fala por você.',
    description: 'Seus trabalhos em um espaço com a sua identidade.',
    cta: 'Explore os trabalhos',
    blocks: ['gallery'],
  },
  {
    id: 'catalog',
    label: 'Divulgar produtos',
    subtitle: 'Um catálogo para explorar',
    eyebrow: 'Escolhas com personalidade',
    headline: 'Encontre algo que combina com você.',
    description: 'Seus produtos, fáceis de conhecer.',
    cta: 'Conheça a seleção',
    blocks: ['products'],
  },
];
export const siteBlocks: readonly { id: SiteBlock; label: string; description: string }[] = [
  {
    id: 'spreadsheet',
    label: 'Uma planilha',
    description: 'Transforme linhas em uma tabela interativa.',
  },
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
