import type { LeadItem } from '../leads/leads-api';
import { LEAD_STATUSES } from '../leads/lead-status';
import { reportContent, reportRisk, type SecurityReport } from './agents-api';

export interface ChartPoint {
  label: string;
  value: number;
}
export function dailySeries(dates: string[], days: number, now = new Date()): ChartPoint[] {
  const end = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const counts = new Map<string, number>();
  const key = (date: Date) => `${date.getFullYear()}-${date.getMonth()}-${date.getDate()}`;
  for (const value of dates) {
    const date = new Date(value);
    if (Number.isFinite(date.getTime())) counts.set(key(date), (counts.get(key(date)) ?? 0) + 1);
  }
  return Array.from({ length: days }, (_, i) => {
    const date = new Date(end);
    date.setDate(end.getDate() - days + i + 1);
    return {
      label: date.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' }),
      value: counts.get(key(date)) ?? 0,
    };
  });
}
export function withinPeriod(date: string, days: number, now = new Date()): boolean {
  const start = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  start.setDate(start.getDate() - days + 1);
  const timestamp = new Date(date).getTime();
  return Number.isFinite(timestamp) && timestamp >= start.getTime() && timestamp <= now.getTime();
}
export function leadMetrics(items: LeadItem[]) {
  const active = items.filter((item) =>
    ['conversation', 'analysis', 'development', 'validation', 'delivered'].includes(
      item.followUpStatus,
    ),
  ).length;
  return {
    active,
    delivered: items.filter((item) => item.followUpStatus === 'delivered').length,
    pending: items.filter((item) => item.followUpStatus === 'new').length,
    stages: Object.entries(LEAD_STATUSES).map(([id, label]) => ({
      label,
      value: items.filter((item) => item.followUpStatus === id).length,
    })),
  };
}
export function securityMetrics(reports: SecurityReport[]) {
  return {
    elevated: reports.filter((report) => reportRisk(report).tone === 'danger').length,
    incomplete: reports.filter(
      (report) => report.executionStatus !== 'completed' || !reportContent(report).valid,
    ).length,
    hosts: new Set(reports.map((report) => report.hostId)).size,
    severity: [
      { label: 'Elevados', values: ['high', 'critical'] },
      { label: 'Moderados', values: ['medium'] },
      { label: 'Baixos', values: ['low'] },
      { label: 'Informativos', values: ['info'] },
      { label: 'Não classificados', values: [] },
    ].map((group) => ({
      label: group.label,
      value: reports.reduce(
        (sum, report) =>
          sum +
          reportContent(report).findings.filter((finding) =>
            group.values.length
              ? group.values.includes(finding.severity)
              : !['critical', 'high', 'medium', 'low', 'info'].includes(finding.severity),
          ).length,
        0,
      ),
    })),
  };
}
