export type SiteBlock =
  | 'services'
  | 'gallery'
  | 'testimonials'
  | 'contact'
  | 'categories'
  | 'details'
  | 'cart'
  | 'orders'
  | 'spreadsheet'
  | 'dashboard'
  | 'payables'
  | 'inventory';
export interface SitePreset {
  id: string;
  label: string;
  subtitle: string;
  eyebrow: string;
  headline: string;
  description: string;
  blocks: readonly SiteBlock[];
}
export const sitePresets: readonly SitePreset[] = [
  {
    id: 'business',
    label: 'Apresentar minha empresa',
    subtitle: 'Uma página para sua história',
    eyebrow: 'Prazer, somos seu negócio',
    headline: 'Seu trabalho merece ser encontrado.',
    description: 'Uma boa apresentação abre novas conversas.',
    blocks: ['services', 'gallery', 'testimonials', 'contact'],
  },
  {
    id: 'catalog',
    label: 'Vender meus produtos',
    subtitle: 'Uma loja para suas ideias',
    eyebrow: 'Coleção essencial',
    headline: 'Escolhas que fazem parte do seu dia.',
    description: 'Uma pequena vitrine. Muitas possibilidades.',
    blocks: ['categories', 'details', 'cart', 'orders'],
  },
  {
    id: 'management',
    label: 'Organizar meu negócio',
    subtitle: 'Um sistema para sua rotina',
    eyebrow: 'Seu espaço de trabalho',
    headline: 'Tudo encontra seu lugar.',
    description: 'Escolha o que ajudaria no seu dia a dia.',
    blocks: ['spreadsheet', 'dashboard', 'payables', 'inventory'],
  },
];
export const siteBlocks: readonly { id: SiteBlock; label: string; description: string }[] = [
  { id: 'services', label: 'Meus serviços', description: 'Mostre como você pode ajudar.' },
  { id: 'gallery', label: 'Meus trabalhos', description: 'Dê espaço ao que você já fez.' },
  { id: 'testimonials', label: 'Depoimentos', description: 'Histórias de quem já confia em você.' },
  { id: 'contact', label: 'Fale comigo', description: 'Abra caminho para uma conversa.' },
  { id: 'categories', label: 'Categorias', description: 'Organize a vitrine por interesses.' },
  {
    id: 'details',
    label: 'Detalhes do produto',
    description: 'Mostre o que torna cada peça especial.',
  },
  { id: 'cart', label: 'Carrinho', description: 'Uma escolha vira uma compra possível.' },
  { id: 'orders', label: 'Meus pedidos', description: 'Acompanhe cada etapa da compra.' },
  { id: 'spreadsheet', label: 'Uma planilha', description: 'Linhas e colunas ganham vida.' },
  { id: 'dashboard', label: 'Um dashboard', description: 'Veja os números de um jeito claro.' },
  { id: 'payables', label: 'Contas a pagar', description: 'Organize valores e vencimentos.' },
  {
    id: 'inventory',
    label: 'Controle de estoque',
    description: 'Saiba o que tem e o que precisa repor.',
  },
];
