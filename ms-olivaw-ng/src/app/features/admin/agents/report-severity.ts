import type { Finding, SecurityReport } from './agents-api';
import { reportRisk } from './agents-api';
export const severityGroups = [
  { id: 'danger', label: 'Danger', description: 'Elevados', singular: 'elevado', tone: 'danger' },
  {
    id: 'warning',
    label: 'Warning',
    description: 'Warnings',
    singular: 'warning',
    tone: 'warning',
  },
  { id: 'low', label: 'Baixo', description: 'Baixos', singular: 'baixo', tone: 'low' },
  {
    id: 'info',
    label: 'Info',
    description: 'Informativos',
    singular: 'informativo',
    tone: 'unknown',
  },
  {
    id: 'unknown',
    label: 'Não classificado',
    description: 'Não classificados',
    singular: 'não classificado',
    tone: 'unknown',
  },
] as const;
export type SeverityGroup = (typeof severityGroups)[number]['id'];
export function severityGroup(value: string): SeverityGroup {
  if (value === 'critical' || value === 'high') return 'danger';
  if (value === 'medium') return 'warning';
  if (value === 'low' || value === 'info') return value;
  return 'unknown';
}
export function findingCounts(findings: Finding[]) {
  return severityGroups.map((group) => ({
    ...group,
    count: findings.filter((finding) => severityGroup(finding.severity) === group.id).length,
  }));
}
export function riskTag(report: SecurityReport) {
  const risk = reportRisk(report);
  return {
    ...risk,
    tag: (
      { danger: 'Danger', warning: 'Warning', good: 'Safe', unknown: 'Indisponível' } as Record<
        string,
        string
      >
    )[risk.tone],
  };
}
