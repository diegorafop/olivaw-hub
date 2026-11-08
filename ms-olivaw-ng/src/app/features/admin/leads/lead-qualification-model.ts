export const WEBSITE_DIMENSIONS = {
  modernity: 'Modernidade',
  content: 'Conteúdo',
  ui: 'UI / identidade visual',
  ux: 'Usabilidade',
  management: 'Gestão',
} as const;
export interface WebsiteMetric {
  score: number | null;
  evidence: string;
  sourceUrl: string;
  confidence: 'unknown' | 'provisional' | 'confirmed';
}
export interface WebsiteAssessment {
  schemaVersion: number;
  assessedAt: string;
  basis: string;
  strategy: string;
  metrics: Record<keyof typeof WEBSITE_DIMENSIONS, WebsiteMetric>;
}
export interface LeadQualification {
  websiteAssessment?: WebsiteAssessment | null;
  commercialObservation?: string;
  restrictedAreaUrl?: string;
  schemaVersion: number;
  segment: string;
  region: string;
  websiteUrl: string;
  phone: string;
  whatsapp: string;
  contactFormUrl: string;
  publicEmails?: { email: string; sourceUrl: string }[];
  contactPageUrl?: string;
  contactFormStatus?: string;
  contactCheckedAt?: string | null;
  sourceUrl: string;
  presence: string;
  priority: string;
  confidence: string;
  tags: string[];
  portfolioReference: string;
  facts: string;
  positives: string;
  observations: string;
  questions: string;
  approach: string;
  approachStatus: string;
}
export const PRESENCE_LABELS: Record<string, string> = {
  unknown: 'A verificar',
  website: 'Site próprio',
  'social-only': 'Somente redes sociais',
  'not-located': 'Site não localizado',
};
export const PRIORITY_LABELS: Record<string, string> = {
  unassessed: 'A avaliar',
  high: 'Revisar primeiro',
  normal: 'Prioridade normal',
  low: 'Revisar depois',
};
export const CONFIDENCE_LABELS: Record<string, string> = {
  pending: 'Pendente de verificação',
  confirmed: 'Informações verificadas',
  inconclusive: 'Análise inconclusiva',
};
export function emptyQualification(): LeadQualification {
  return {
    schemaVersion: 1,
    segment: '',
    region: '',
    websiteUrl: '',
    phone: '',
    whatsapp: '',
    contactFormUrl: '',
    sourceUrl: '',
    presence: 'unknown',
    priority: 'unassessed',
    confidence: 'pending',
    tags: [],
    portfolioReference: '',
    facts: '',
    positives: '',
    observations: '',
    questions: '',
    approach: '',
    approachStatus: 'draft',
  };
}
export function webUrl(value: string | undefined): string | null {
  if (!value) return null;
  try {
    const url = new URL(value);
    if (!['http:', 'https:'].includes(url.protocol) || url.username || url.password) return null;
    return url.href;
  } catch {
    return null;
  }
}
export function assessmentStrategy(assessment?: WebsiteAssessment | null): string {
  const scored = Object.entries(assessment?.metrics || {}).filter(
    ([, item]) => item.score !== null,
  );
  scored.sort((a, b) => a[1].score! - b[1].score!);
  if (!scored.length) return 'discovery';
  return scored[0][1].score! < 8 ? scored[0][0] : 'operations';
}
export function approachSuggestion(
  business: string,
  qualification: LeadQualification,
  recipient = '',
): string {
  const greeting = business ? `Olá, pessoal da ${business}!` : 'Olá!';
  const observation = qualification.commercialObservation?.trim();
  const email = qualification.publicEmails?.find(
    (item) => item.email.toLowerCase() === recipient.toLowerCase(),
  );
  const source = webUrl(email?.sourceUrl);
  const questions: Record<string, string> = {
    modernity:
      'Vocês têm pensado em atualizar a apresentação do site para acompanhar a fase atual da empresa?',
    content:
      'Hoje vocês conseguem atualizar os serviços e novidades do site com a facilidade que gostariam?',
    ui: 'A apresentação do site hoje transmite a identidade que vocês querem passar aos clientes?',
    ux: 'Quem chega pelo celular consegue encontrar o que precisa e entrar em contato do jeito que vocês gostariam?',
    management:
      'Como vocês organizam as atualizações do site e os pedidos que chegam por ele hoje?',
    operations:
      'Há alguma rotina entre o contato pelo site e o atendimento que vocês gostariam de simplificar?',
    discovery: 'O site atende ao que vocês precisam hoje ou há algo que gostariam de facilitar?',
  };
  const strategy = assessmentStrategy(qualification.websiteAssessment);
  return [
    greeting,
    observation ? `Ao conhecer o site de vocês, chamou nossa atenção: ${observation}` : '',
    'Somos da Olivaw. Criamos sites e sistemas para facilitar a apresentação e a rotina dos negócios.',
    source ? `Encontramos este e-mail na página pública de contato: ${source}.` : '',
    questions[strategy] || questions['discovery'],
    'Abraços,\nEquipe Olivaw',
  ]
    .filter(Boolean)
    .join('\n\n');
}
