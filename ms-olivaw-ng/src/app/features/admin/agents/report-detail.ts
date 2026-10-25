import { Component, computed, inject } from '@angular/core';
import { DatePipe, JsonPipe, KeyValuePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, map, of, startWith, switchMap } from 'rxjs';
import { AgentsApi, reportContent, reportRisk } from './agents-api';
@Component({
  selector: 'app-report-detail',
  imports: [DatePipe, JsonPipe, KeyValuePipe, RouterLink],
  styleUrl: './agents.scss',
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
      <section class="report-summary">
        <span [class]="'risk ' + risk(report).tone"
          ><span class="dot" aria-hidden="true"></span>{{ risk(report).label }}</span
        >
        <h2>{{ report.hostId }}</h2>
        <p>{{ report.completedAt | date: 'dd/MM/yyyy · HH:mm:ss' }}</p>
        <p class="reference">Execução {{ report.runId }}</p>
      </section>
      @if (content(report).analysis; as analysis) {
        <section class="report-summary">
          <h2>Leitura do Baley</h2>
          <p>{{ analysis.summary }}</p>
          <ul>
            @for (recommendation of analysis.recommendations; track $index) {
              <li>{{ recommendation }}</li>
            }
          </ul>
        </section>
      }
      <h2>Achados e recomendações</h2>
      @for (finding of content(report).findings; track $index) {
        <article class="finding">
          <span class="severity">{{ severity(finding.severity) }}</span>
          <h3>{{ finding.title }}</h3>
          <p>{{ finding.evidence }}</p>
          <p>{{ finding.recommendation }}</p>
        </article>
      } @empty {
        <p>
          Nenhum achado registrado. Confira a cobertura e as limitações antes de concluir sobre o
          ambiente.
        </p>
      }
      <h2>Cobertura da coleta</h2>
      <dl class="coverage">
        @for (entry of content(report).coverage | keyvalue; track entry.key) {
          <div>
            <dt>{{ entry.key }}</dt>
            <dd>{{ entry.value | json }}</dd>
          </div>
        }
      </dl>
      <h2>Limitações</h2>
      <ul>
        @for (limitation of content(report).limitations; track $index) {
          <li>{{ limitation }}</li>
        }
      </ul>
      <details>
        <summary>JSON completo do relatório</summary>
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
  readonly risk = reportRisk;
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
