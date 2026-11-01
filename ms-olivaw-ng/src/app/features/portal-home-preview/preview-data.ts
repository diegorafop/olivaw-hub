// Dados exclusivamente ilustrativos. Não representam consumo, contrato ou cliente reais.
export type Period = 'day' | 'week' | 'month' | 'year';
export type Scenario = 'normal' | 'attention' | 'missing';
export interface Point {
  label: string;
  value: number;
}
export interface PreviewRequest {
  id: number;
  title: string;
  status: 'received' | 'working' | 'waiting' | 'done';
  date: string;
}
export interface PreviewImprovement {
  id: number;
  title: string;
  status: 'queued' | 'working' | 'done';
}
export const periods: { id: Period; label: string; caption: string }[] = [
  { id: 'day', label: 'Dia', caption: '25 de outubro de 2026 · por hora' },
  { id: 'week', label: 'Semana', caption: '19 a 25 de outubro de 2026 · por dia' },
  { id: 'month', label: 'Mês', caption: '26 de setembro a 25 de outubro de 2026 · por dia' },
  { id: 'year', label: 'Ano', caption: 'Novembro de 2025 a outubro de 2026 · por mês' },
];
const storage = [
  280, 287, 301, 310, 326, 330, 358, 364, 360, 379, 401, 425, 436, 448, 447, 459, 482, 501, 523,
  530, 552, 569, 578, 600, 624, 638, 662, 687, 698, 720,
];
const accesses = [
  104, 143, 167, 112, 89, 118, 175, 161, 194, 186, 224, 231, 178, 166, 192, 245, 267, 252, 283, 240,
  190, 210, 257, 283, 276, 312, 293, 341, 309, 328,
];
const monthLabels = storage.map((_, index) =>
  index < 5 ? `${index + 26} set` : `${index - 4} out`,
);
const sum = (values: number[]) => values.reduce((total, value) => total + value, 0);
export function series(period: Period, kind: 'storage' | 'accesses', usedMb: number): Point[] {
  const ratio = usedMb / 720;
  if (period === 'year') {
    const labels = [
      'Nov',
      'Dez',
      'Jan',
      'Fev',
      'Mar',
      'Abr',
      'Mai',
      'Jun',
      'Jul',
      'Ago',
      'Set',
      'Out',
    ];
    const values =
      kind === 'storage'
        ? [112, 132, 151, 198, 225, 260, 301, 346, 419, 480, 580, 720].map((value) =>
            Math.round(value * ratio),
          )
        : [1240, 1570, 1804, 2290, 2530, 2870, 3100, 3920, 4100, 4930, 5410, sum(accesses)];
    return labels.map((label, index) => ({ label, value: values[index] }));
  }
  if (period === 'day') {
    const weights = [
      2, 1, 1, 1, 2, 3, 5, 8, 11, 13, 14, 12, 10, 13, 15, 17, 15, 12, 11, 9, 7, 5, 4, 3,
    ];
    const hourly = weights.map((value) => Math.floor((value / sum(weights)) * accesses.at(-1)!));
    hourly[23] += accesses.at(-1)! - sum(hourly);
    return weights.map((_, index) => ({
      label: `${String(index).padStart(2, '0')}h`,
      value:
        kind === 'storage' ? Math.round((698 + ((720 - 698) * index) / 23) * ratio) : hourly[index],
    }));
  }
  const start = period === 'week' ? 23 : 0;
  return monthLabels.slice(start).map((label, index) => ({
    label,
    value:
      kind === 'storage' ? Math.round(storage[start + index] * ratio) : accesses[start + index],
  }));
}
export const initialRequests: PreviewRequest[] = [
  { id: 1042, title: 'Ajuste no formulário de contato', status: 'waiting', date: 'Hoje' },
  { id: 1038, title: 'Atualizar informações da empresa', status: 'working', date: '24 out' },
  { id: 1036, title: 'Ajuda com acesso ao painel', status: 'received', date: '23 out' },
  ...[
    'Ajuste de texto',
    'Atualização do telefone',
    'Imagem da página inicial',
    'Correção do menu',
    'Link de contato',
    'Exibição no celular',
    'Nova informação no rodapé',
    'Ajuste de endereço',
  ].map((title, index) => ({ id: 1030 - index, title, status: 'done' as const, date: 'Outubro' })),
];
export const initialImprovements: PreviewImprovement[] = [
  { id: 31, title: 'Nova página de serviços', status: 'working' },
  { id: 30, title: 'Galeria de projetos', status: 'queued' },
  { id: 29, title: 'Formulário de orçamento', status: 'queued' },
  ...[
    'Menu mais simples',
    'Contatos atualizados',
    'Melhor leitura no celular',
    'Nova apresentação da empresa',
  ].map((title, index) => ({ id: 28 - index, title, status: 'done' as const })),
];
