import { describe, expect, it } from 'vitest';
import { reportContent, reportRisk, type SecurityReport } from './agents-api';
function report(status = 'completed', findings: unknown[] = []): SecurityReport {
  return {
    externalId: 'test',
    runId: 'run',
    agentId: 'baley',
    hostId: 'host',
    completedAt: '',
    createdAt: '',
    executionStatus: status,
    rawData: JSON.stringify({ result: { findings } }),
  };
}
describe('Security report risk', () => {
  it('never marks failed, partial or incompatible reports green', () => {
    expect(reportRisk(report('failed')).tone).toBe('unknown');
    expect(reportRisk(report('partial')).tone).toBe('warning');
    expect(reportRisk({ ...report(), rawData: '{}' }).tone).toBe('unknown');
    expect(reportRisk({ ...report(), rawData: '{bad' }).tone).toBe('unknown');
  });
  it('prioritizes critical findings and scopes green to checked controls', () => {
    expect(
      reportRisk(report('partial', [{ title: 'Privileged', severity: 'critical' }])).tone,
    ).toBe('danger');
    expect(reportRisk(report('completed', [{ title: 'Logs', severity: 'medium' }])).tone).toBe(
      'warning',
    );
    expect(reportRisk(report()).label).toBe('Sem achados nos controles avaliados');
  });
  it('preserves optional and future fields without unsafe html rendering', () => {
    expect(
      reportContent({
        ...report(),
        rawData: JSON.stringify({
          result: { findings: [null, { title: '<script>', severity: 'low' }], future: true },
        }),
      }).findings,
    ).toHaveLength(1);
  });
});

describe('Severity groups', () => {
  it('counts critical and high as Danger and preserves unknown severities', async () => {
    const { findingCounts } = await import('./report-severity');
    const findings = ['critical', 'high', 'medium', 'low', 'future'].map((severity) => ({
      ruleId: severity,
      title: severity,
      severity,
      evidence: '',
      recommendation: '',
    }));
    expect(findingCounts(findings).map((group) => group.count)).toEqual([2, 1, 1, 0, 1]);
  });
});

describe('Approval target identity', () => {
  it('preserves the original finding index when malformed entries are hidden', () => {
    const data = reportContent({
      ...report(),
      rawData: JSON.stringify({
        result: {
          findings: [
            null,
            { title: 'Primeiro válido', severity: 'high' },
            { title: 'Sem gravidade' },
            { title: 'Segundo válido', severity: 'low' },
          ],
        },
      }),
    });
    expect(data.findings.map((item) => item.findingIndex)).toEqual([1, 3]);
  });
});
