import { describe, expect, it } from 'vitest';
import { reportContent, reportRisk, type SecurityReport } from './agents-api';
function report(status = 'completed', findings: unknown[] = []): SecurityReport {
  return { externalId: 'test', runId: 'run', agentId: 'baley', hostId: 'host', completedAt: '', createdAt: '', executionStatus: status, rawData: JSON.stringify({ result: { findings } }) };
}
describe('Security report risk', () => {
  it('never marks failed, partial or incompatible reports green', () => {
    expect(reportRisk(report('failed')).tone).toBe('unknown');
    expect(reportRisk(report('partial')).tone).toBe('warning');
    expect(reportRisk({ ...report(), rawData: '{}' }).tone).toBe('unknown');
    expect(reportRisk({ ...report(), rawData: '{bad' }).tone).toBe('unknown');
  });
  it('prioritizes critical findings and scopes green to checked controls', () => {
    expect(reportRisk(report('partial', [{ title: 'Privileged', severity: 'critical' }])).tone).toBe('danger');
    expect(reportRisk(report('completed', [{ title: 'Logs', severity: 'medium' }])).tone).toBe('warning');
    expect(reportRisk(report()).label).toBe('Sem achados nos controles avaliados');
  });
  it('preserves optional and future fields without unsafe html rendering', () => {
    expect(reportContent({ ...report(), rawData: JSON.stringify({ result: { findings: [null, { title: '<script>', severity: 'low' }], future: true } }) }).findings).toHaveLength(1);
  });
});
