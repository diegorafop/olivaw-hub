import { describe, expect, it } from 'vitest';
import { dailySeries, leadMetrics, securityMetrics, withinPeriod } from './agent-metrics';
import type { LeadItem } from '../leads/leads-api';
import type { SecurityReport } from './agents-api';
const now = new Date(2026, 10, 1, 12);
function report(status: string, rawData: string): SecurityReport {
  return {
    externalId: 'one',
    runId: 'run',
    agentId: 'baley',
    hostId: 'host',
    executionStatus: status,
    completedAt: now.toISOString(),
    createdAt: now.toISOString(),
    rawData,
  };
}
describe('Métricas gerenciais dos agentes', () => {
  it('inclui o dia inicial inteiro e exclui datas inválidas e futuras', () => {
    expect(withinPeriod(new Date(2026, 9, 26).toISOString(), 7, now)).toBe(true);
    expect(withinPeriod(new Date(2026, 9, 25, 23, 59).toISOString(), 7, now)).toBe(false);
    expect(withinPeriod(new Date(2026, 10, 2).toISOString(), 7, now)).toBe(false);
    expect(withinPeriod('invalid', 7, now)).toBe(false);
  });
  it('preenche os dias sem atividade sem carregar eventos de fora da janela', () => {
    const series = dailySeries(
      [now.toISOString(), now.toISOString(), 'invalid', new Date(2026, 9, 1).toISOString()],
      7,
      now,
    );
    expect(series).toHaveLength(7);
    expect(series.at(-1)?.value).toBe(2);
    expect(series.slice(0, -1).every((point) => point.value === 0)).toBe(true);
  });
  it('não considera cadastro novo ou desistência como avanço comercial', () => {
    const items = ['new', 'conversation', 'delivered', 'discontinued', 'future-status'].map(
      (followUpStatus) => ({ followUpStatus }) as LeadItem,
    );
    const metrics = leadMetrics(items);
    expect(metrics.active).toBe(2);
    expect(metrics.pending).toBe(1);
    expect(metrics.delivered).toBe(1);
  });
  it('mantém falhas e conteúdo inválido visíveis sem inventar achados', () => {
    const metrics = securityMetrics([
      report('failed', '{}'),
      report('partial', 'invalid'),
      report(
        'completed',
        JSON.stringify({
          result: {
            findings: [
              { title: 'Socket', severity: 'high' },
              { title: 'New', severity: 'future' },
            ],
          },
        }),
      ),
    ]);
    expect(metrics.incomplete).toBe(2);
    expect(metrics.hosts).toBe(1);
    expect(metrics.elevated).toBe(1);
    expect(metrics.severity.find((group) => group.label === 'Não classificados')?.value).toBe(1);
  });
});
