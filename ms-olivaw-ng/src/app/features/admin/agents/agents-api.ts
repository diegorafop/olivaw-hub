import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { timeout } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR } from '../../../core/http/http-context';
export interface SecurityReport {
  externalId: string;
  runId: string;
  agentId: string;
  hostId: string;
  executionStatus: string;
  completedAt: string;
  createdAt: string;
  rawData: string;
}
export interface Finding {
  ruleId: string;
  title: string;
  severity: string;
  evidence: string;
  recommendation: string;
}
export function reportContent(report: SecurityReport): {
  findings: Finding[];
  limitations: string[];
  coverage: Record<string, unknown>;
  valid: boolean;
  analysis: { summary: string; recommendations: string[] } | null;
} {
  try {
    const raw = JSON.parse(report.rawData);
    if (!raw || typeof raw !== 'object' || !raw.result || typeof raw.result !== 'object')
      throw new Error('Invalid report');
    return {
      findings: Array.isArray(raw.result.findings)
        ? raw.result.findings.filter(
            (item: Finding) =>
              item && typeof item.title === 'string' && typeof item.severity === 'string',
          )
        : [],
      limitations: Array.isArray(raw.result.limitations)
        ? raw.result.limitations.filter((item: unknown) => typeof item === 'string')
        : [],
      coverage:
        raw.result.coverage && typeof raw.result.coverage === 'object' ? raw.result.coverage : {},
      valid: Array.isArray(raw.result.findings),
      analysis:
        raw.ai?.status === 'completed' && typeof raw.ai.summary === 'string'
          ? {
              summary: raw.ai.summary,
              recommendations: Array.isArray(raw.ai.recommendations)
                ? raw.ai.recommendations.filter((item: unknown) => typeof item === 'string')
                : [],
            }
          : null,
    };
  } catch {
    return {
      findings: [],
      limitations: ['Conteúdo incompatível ou indisponível.'],
      coverage: {},
      valid: false,
      analysis: null,
    };
  }
}
export function reportRisk(report: SecurityReport): { tone: string; label: string } {
  const data = reportContent(report);
  if (data.findings.some((finding) => ['critical', 'high'].includes(finding.severity)))
    return { tone: 'danger', label: 'Risco elevado' };
  if (report.executionStatus === 'failed') return { tone: 'unknown', label: 'Coleta falhou' };
  if (!data.valid || !['completed', 'partial'].includes(report.executionStatus))
    return { tone: 'unknown', label: 'Resultado indisponível' };
  if (report.executionStatus === 'partial') return { tone: 'warning', label: 'Coleta parcial' };
  if (data.findings.length) return { tone: 'warning', label: 'Requer atenção' };
  return { tone: 'good', label: 'Sem achados nos controles avaliados' };
}
@Injectable({ providedIn: 'root' })
export class AgentsApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(RuntimeConfig);
  list(beforeId = 0) {
    return this.http
      .get<{ items: SecurityReport[]; nextBeforeId: number | null }>(
        this.config.endpoint('security-reports'),
        {
          params: new HttpParams().set('beforeId', beforeId),
          context: new HttpContext().set(LOCAL_ERROR, true),
        },
      )
      .pipe(timeout(15000));
  }
  detail(id: string) {
    return this.http
      .get<{ item: SecurityReport }>(
        this.config.endpoint('security-reports/' + encodeURIComponent(id)),
        { context: new HttpContext().set(LOCAL_ERROR, true) },
      )
      .pipe(timeout(15000));
  }
}
