export interface LeadQualification {
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
export function approachSuggestion(business: string, qualification: LeadQualification): string {
  const greeting = business ? `Olá, pessoal da ${business}.` : 'Olá!';
  const context = qualification.segment ? ` do segmento de ${qualification.segment}` : '';
  const location = qualification.region ? ` em ${qualification.region}` : '';
  const positive = qualification.positives.trim();
  const acknowledgement = positive
    ? `\n\nAo conhecer a apresentação de vocês, destacamos: ${positive}`
    : '';
  let question =
    'Hoje, a tecnologia atende bem à rotina de vocês ou há algo que gostariam de facilitar no negócio?';
  if (qualification.presence === 'website')
    question =
      'O site atual atende bem às necessidades de vocês ou existe algo que gostariam de melhorar, seja na apresentação ou na gestão do negócio?';
  return `${greeting} Somos da Olivaw, uma nova empresa de desenvolvimento de sistemas. Estamos conhecendo empresas${context}${location} para construir parcerias.${acknowledgement}\n\nGostaríamos de entender como podemos apoiar a operação de vocês. ${question}`;
}
