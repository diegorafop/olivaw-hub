export interface LeadAcquisition {
  initiative: 'inbound' | 'outbound';
  origin: string;
  externalReference: string;
  sourceUrl: string;
}
export interface LeadCreateData {
  externalId: string;
  initiative: 'inbound' | 'outbound';
  origin: string;
  externalReference: string;
  sourceUrl: string;
  business: string;
  title: string;
  requirements: string;
  email: string;
  whatsapp: string;
}
export interface ProposalRow {
  description: string;
  role: string;
  solution: string;
  hours: number;
  hourlyRate: number;
  optional: boolean;
}
export interface LeadProposal {
  title: string;
  status: string;
  platform: string;
  requirementsAnalysis: string;
  technicalApproach: string;
  assumptions: string;
  externalCosts: string;
  deadline: string;
  terms: string;
  continuity: string;
  reservePercent: number;
  discountPercent: number;
  installments: number;
  rows: ProposalRow[];
}
export const LEAD_ENTRIES: Record<string, string> = {
  studio: 'Estúdio',
  discovery: 'Descoberta / contato',
  prospecting: 'Prospecção ativa',
  manual: 'Contato recebido / cadastro manual',
};
const cents = (amount: number) => Math.round((amount + Number.EPSILON) * 100);
export function proposalTotals(proposal: LeadProposal) {
  const included = proposal.rows.filter((row) => !row.optional);
  const base = included.reduce((sum, row) => sum + cents(row.hours * row.hourlyRate), 0);
  const optional = proposal.rows
    .filter((row) => row.optional)
    .reduce((sum, row) => sum + cents(row.hours * row.hourlyRate), 0);
  const reserve = Math.round((base * proposal.reservePercent) / 100);
  const total = base + reserve;
  const discount = Math.round((total * proposal.discountPercent) / 100);
  const negotiated = total - discount;
  const count = Math.max(1, proposal.installments);
  const installment = Math.floor(negotiated / count);
  return {
    hours: included.reduce((sum, row) => sum + row.hours, 0),
    base: base / 100,
    reserve: reserve / 100,
    total: total / 100,
    optional: optional / 100,
    discount: discount / 100,
    negotiated: negotiated / 100,
    installment: installment / 100,
    finalInstallment: (negotiated - installment * (count - 1)) / 100,
  };
}
export function emptyProposal(title: string): LeadProposal {
  return {
    title,
    status: 'draft',
    platform: '',
    requirementsAnalysis: '',
    technicalApproach: '',
    assumptions: '',
    externalCosts: '',
    deadline: '',
    terms: '',
    continuity: '',
    reservePercent: 20,
    discountPercent: 0,
    installments: 1,
    rows: [
      {
        description: 'Análise dos requisitos e da plataforma',
        role: 'Analista',
        solution: 'analysis',
        hours: 4,
        hourlyRate: 100,
        optional: false,
      },
    ],
  };
}
export function proposalHtml(proposal: LeadProposal): string {
  const escape = (value: string) =>
    value.replace(
      /[&<>"']/g,
      (char) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[char]!,
    );
  const money = (value: number) =>
    value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  const total = proposalTotals(proposal);
  const section = (title: string, content: string) =>
    `<section><h2>${title}</h2><p>${escape(content || 'A confirmar')}</p></section>`;
  return `<!doctype html><html lang="pt-BR"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Olivaw — ${escape(proposal.title)}</title><style>
  :root{font-family:Arial,sans-serif;color:#162c39;background:#fafbf8}*{box-sizing:border-box}body{margin:0;padding:32px 16px}main{max-width:1000px;margin:auto}header{border-top:6px solid #69c7b1;padding:28px 0}h1{font-size:clamp(26px,5vw,44px);letter-spacing:-.04em}h2{font-size:19px}section{padding:20px;background:white;border:1px solid #dce6e0;border-radius:12px;margin:16px 0}p{white-space:pre-wrap;line-height:1.6}small{color:#536a5b}.table{overflow:auto}table{width:100%;border-collapse:collapse}th,td{text-align:left;padding:14px;border-bottom:1px solid #dce6e0;min-width:90px}th{background:#edf5f2}.total{font-size:26px;color:#193e34}footer{padding:24px 0}@media print{body{padding:0}section{break-inside:avoid}header{padding:8px 0}}
  </style></head><body><main><header><strong>OLIVAW</strong><p>Proposta de desenvolvimento · ${escape(proposal.status === 'draft' ? 'Rascunho para avaliação' : 'Condições sujeitas ao aceite')}</p><h1>${escape(proposal.title)}</h1></header>
  ${section('Plataforma e requisitos', proposal.platform + '\n' + proposal.requirementsAnalysis)}
  ${section('Solução proposta', proposal.technicalApproach)}
  <section><h2>Etapas e investimento</h2><div class="table"><table><thead><tr><th>Entrega</th><th>Profissional</th><th>Horas</th><th>Investimento</th></tr></thead><tbody>${proposal.rows.map((row) => `<tr><td>${escape(row.description)}${row.optional ? ' (opcional, fora do total)' : ''}</td><td>${escape(row.role)}</td><td>${row.hours}</td><td>${money(row.hours * row.hourlyRate)}</td></tr>`).join('')}</tbody></table></div><p>${total.hours} horas base · Reserva (${proposal.reservePercent}%): ${money(total.reserve)}</p><p class="total">Implementação avulsa: ${money(total.total)}</p><p>Complementos opcionais: ${money(total.optional)} (sem reserva; dependem de avaliação).</p></section>
  <section><h2>Implementação com continuidade Olivaw</h2><p>${escape(proposal.continuity || 'Plano e condições a negociar.')}</p><p>Desconto proposto: ${proposal.discountPercent}% (${money(total.discount)}). Implementação: ${money(total.negotiated)}.</p><p>${proposal.installments} parcela(s): ${proposal.installments > 1 ? `${proposal.installments - 1} de ${money(total.installment)} e última de ${money(total.finalInstallment)}` : money(total.negotiated)}. Mensalidade do plano e custos externos são separados.</p><small>Desconto e distribuição mensal dependem de aprovação das condições, permanência e cancelamento. Não representam contratação automática de plano.</small></section>
  ${section('Premissas e exclusões', proposal.assumptions)}${section('Custos externos', proposal.externalCosts)}${section('Prazo', proposal.deadline)}${section('Condições e validade', proposal.terms)}<footer>Olivaw · Escopo, solução e condições sujeitos à confirmação e ao aceite. Este documento não confirma contratação ou disponibilidade de plano.</footer></main></body></html>`;
}
