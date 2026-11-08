export type FinancePeriod = 'day' | 'week' | 'month' | 'year';
export function financeRange(period: FinancePeriod, anchor: string) {
  const start = new Date(anchor + 'T00:00:00Z');
  if (
    !/^\d{4}-\d{2}-\d{2}$/.test(anchor) ||
    !Number.isFinite(start.getTime()) ||
    start.toISOString().slice(0, 10) !== anchor ||
    start.getUTCFullYear() < 2000 ||
    start.getUTCFullYear() > 9998
  )
    throw new RangeError('Data inválida.');
  if (period === 'week') start.setUTCDate(start.getUTCDate() - ((start.getUTCDay() + 6) % 7));
  if (period === 'month') start.setUTCDate(1);
  if (period === 'year') start.setUTCMonth(0, 1);
  const end = new Date(start);
  if (period === 'year') end.setUTCFullYear(end.getUTCFullYear() + 1);
  else if (period === 'month') end.setUTCMonth(end.getUTCMonth() + 1);
  else end.setUTCDate(end.getUTCDate() + (period === 'week' ? 7 : 1));
  const last = new Date(end);
  last.setUTCDate(last.getUTCDate() - 1);
  return {
    from: start.toISOString(),
    to: end.toISOString(),
    first: start.toISOString().slice(0, 10),
    last: last.toISOString().slice(0, 10),
    groupBy: period === 'year' ? 'month' : 'day',
  };
}
