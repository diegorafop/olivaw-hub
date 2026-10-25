import { Component, computed, inject, signal } from '@angular/core';
import { DatePipe, JsonPipe, KeyValuePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, map, of, startWith, switchMap } from 'rxjs';
import { AgentsApi, reportContent } from './agents-api';
import { findingCounts, riskTag, severityGroup, type SeverityGroup } from './report-severity';
@Component({
  selector: 'app-report-detail',
  imports: [DatePipe, JsonPipe, KeyValuePipe, RouterLink],
  styleUrls: ['./agents.scss', './report-tags.scss'],
  template: `<main id="operation-content" tabindex="-1">
    <header>
      <div>
        <p class="eyebrow">BALEY / RELATÓRIO</p>
        <h1>Detalhes da auditoria</h1>
      </div>
      <a routerLink="/admin/agents/baley">Voltar aos relatórios</a>
    </header>
    @if (state().loading) {
      <p role="status">Carregando relatório…</p>
    }
    @if (state().error) {
      <p role="alert">Relatório indisponível, removido ou sem permissão de acesso.</p>
    }
    @if (state().report; as report) {
      <section class="report-summary compact-summary">
        <div>
          <h2>{{ report.hostId }}</h2>
          <span class="reference">{{ report.completedAt | date: 'dd/MM/yyyy · HH:mm' }}</span>
        </div>
        <span [class]="'tag ' + risk(report).tone" [attr.title]="risk(report).label">{{
          risk(report).tag
        }}</span>
        @if (report.executionStatus === 'partial') {
          <span class="tag unknown">Coleta parcial</span>
        }
      </section>
      <section aria-label="Verificações de segurança">
        <div class="findings-title">
          <h2>Verificações</h2>
          <span class="reference" aria-live="polite"
            >{{ filteredFindings().length }} de {{ findings().length }}</span
          >
        </div>
        <div class="severity-filters" role="group" aria-label="Filtrar por gravidade">
          <button
            type="button"
            [attr.aria-pressed]="selected() === 'all'"
            (click)="selected.set('all')"
          >
            Todos <span>{{ findings().length }}</span>
          </button>
          @for (group of counts(); track group.id) {
            @if (group.id !== 'unknown' || group.count) {
              <button
                type="button"
                [class]="group.tone"
                [attr.aria-pressed]="selected() === group.id"
                [disabled]="!group.count"
                (click)="selected.set(group.id)"
              >
                {{ group.label }} <span>{{ group.count }}</span>
              </button>
            }
          }
        </div>
        @for (finding of filteredFindings(); track $index) {
          <article class="finding">
            <div class="finding-heading">
              <h3>{{ finding.title }}</h3>
              <span [class]="'tag ' + group(finding.severity)">{{
                severity(finding.severity)
              }}</span>
            </div>
            <p>{{ finding.recommendation }}</p>
            @if (finding.evidence) {
              <details class="evidence">
                <summary>Evidência</summary>
                <p>{{ finding.evidence }}</p>
              </details>
            }
          </article>
        } @empty {
          <p class="empty">
            {{
              findings().length
                ? 'Nenhuma verificação neste nível.'
                : 'Nenhum achado registrado nos controles avaliados.'
            }}
          </p>
        }
      </section>
      @if (content(report).analysis; as analysis) {
        <details class="report-summary">
          <summary>Leitura do Baley</summary>
          <p>{{ analysis.summary }}</p>
          <ul>
            @for (recommendation of analysis.recommendations; track $index) {
              <li>{{ recommendation }}</li>
            }
          </ul>
        </details>
      }
      <details class="report-summary">
        <summary>
          Cobertura e limitações
          <span class="reference"
            >{{ content(report).limitations.length }}
            {{ content(report).limitations.length === 1 ? 'observação' : 'observações' }}</span
          >
        </summary>
        <dl class="coverage">
          @for (entry of content(report).coverage | keyvalue; track entry.key) {
            <div>
              <dt>{{ entry.key }}</dt>
              <dd>{{ entry.value | json }}</dd>
            </div>
          }
        </dl>
        <ul>
          @for (limitation of content(report).limitations; track $index) {
            <li>{{ limitation }}</li>
          }
        </ul>
      </details>
      <details>
        <summary>Dados técnicos</summary>
        <p class="reference">Execução {{ report.runId }}</p>
        <pre>{{ raw() }}</pre>
      </details>
    }
  </main>`,
})
export class ReportDetail {
  private readonly api = inject(AgentsApi);
  private readonly route = inject(ActivatedRoute);
  readonly state = toSignal(
    this.route.paramMap.pipe(
      switchMap((params) =>
        this.api.detail(params.get('reportId') ?? '').pipe(
          map((response) => ({ report: response.item, loading: false, error: false })),
          startWith({ report: null, loading: true, error: false }),
          catchError(() => of({ report: null, loading: false, error: true })),
        ),
      ),
    ),
    { initialValue: { report: null, loading: true, error: false } },
  );
  readonly raw = computed(() => {
    try {
      return JSON.stringify(JSON.parse(this.state().report?.rawData ?? '{}'), null, 2);
    } catch {
      return 'JSON incompatível.';
    }
  });
  readonly content = reportContent;
  readonly risk = riskTag;
  readonly group = severityGroup;
  readonly selected = signal<SeverityGroup | 'all'>('all');
  readonly findings = computed(() => {
    const report = this.state().report;
    return report ? reportContent(report).findings : [];
  });
  readonly counts = computed(() => findingCounts(this.findings()));
  readonly filteredFindings = computed(() =>
    this.findings().filter(
      (finding) => this.selected() === 'all' || severityGroup(finding.severity) === this.selected(),
    ),
  );
  severity(value: string): string {
    return (
      (
        {
          critical: 'Crítico',
          high: 'Alto',
          medium: 'Médio',
          low: 'Baixo',
          info: 'Informativo',
        } as Record<string, string>
      )[value] ?? 'Não classificado'
    );
  }
}
