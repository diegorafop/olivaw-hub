import { financeRange } from './finance-period';
describe('Períodos financeiros UTC', () => {
  it('separa dia, semana de segunda a domingo e mês de calendário', () => {
    expect(financeRange('day', '2026-09-30')).toMatchObject({
      first: '2026-09-30',
      last: '2026-09-30',
      to: '2026-10-01T00:00:00.000Z',
    });
    expect(financeRange('week', '2026-09-30')).toMatchObject({
      first: '2026-09-28',
      last: '2026-10-04',
      to: '2026-10-05T00:00:00.000Z',
    });
    expect(financeRange('month', '2026-09-30')).toMatchObject({
      first: '2026-09-01',
      last: '2026-09-30',
    });
  });
  it('preserva ano bissexto sem ultrapassar o limite de 366 dias', () => {
    const leap = financeRange('year', '2024-02-29');
    expect((Date.parse(leap.to) - Date.parse(leap.from)) / 86400000).toBe(366);
    expect(leap.groupBy).toBe('month');
    expect(financeRange('week', '2026-01-01')).toMatchObject({
      first: '2025-12-29',
      last: '2026-01-04',
    });
  });
  it('recusa datas ausentes, normalizadas silenciosamente ou fora do intervalo suportado', () => {
    for (const date of ['', '2026-02-30', '2026-13-01', 'bad', '1999-01-01', '9999-01-01'])
      expect(() => financeRange('month', date)).toThrow();
  });
});
