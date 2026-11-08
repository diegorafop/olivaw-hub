import { describe, expect, it } from 'vitest';
import { emptyProposal, proposalTotals, proposalHtml } from './lead-proposal-model';
describe('Orçamento', () => {
  it('separa opcionais, aplica reserva e desconto e fecha as parcelas em centavos', () => {
    const p = emptyProposal('Teste');
    p.rows[0].hours = 10;
    p.rows[0].hourlyRate = 100;
    p.rows.push({
      description: 'Lacuna',
      role: 'Programador',
      solution: 'custom',
      hours: 5,
      hourlyRate: 100,
      optional: true,
    });
    p.discountPercent = 10;
    p.installments = 7;
    const t = proposalTotals(p);
    expect(t.total).toBe(1200);
    expect(t.optional).toBe(500);
    expect(t.hours).toBe(10);
    expect(t.negotiated).toBe(1080);
    expect(Math.round((t.installment * 6 + t.finalInstallment) * 100)).toBe(108000);
  });
  it('escapa conteúdo do lead no HTML compartilhável', () => {
    const p = emptyProposal('<script>alert(1)</script>');
    p.technicalApproach = '<img src=x onerror=alert(1)>';
    const html = proposalHtml(p);
    expect(html).not.toContain('<script>');
    expect(html).not.toContain('<img');
    expect(html).toContain('&lt;script&gt;');
    expect(html).toContain('Mensalidade do plano');
  });
});
