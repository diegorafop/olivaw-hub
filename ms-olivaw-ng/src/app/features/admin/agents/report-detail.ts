import { Component, computed, inject, signal, DestroyRef } from '@angular/core';
import { DatePipe, JsonPipe, KeyValuePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal, takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  catchError,
  map,
  of,
  startWith,
  switchMap,
  timer,
  merge,
  Subject,
  finalize,
  exhaustMap,
} from 'rxjs';
import {
  AgentsApi,
  reportContent,
  remediationLabels,
  type SecurityRemediation,
} from './agents-api';
import { findingCounts, riskTag, severityGroup, type SeverityGroup } from './report-severity';
@Component({
  selector: 'app-report-detail',
  imports: [DatePipe, JsonPipe, KeyValuePipe, RouterLink],
  styleUrls: ['./agents.scss', './report-tags.scss', './remediation.scss'],
  template: `<main id="operation-content" tabindex="-1">
    <header>
      <div>
        <p class="eyebrow">BALEY / RELATÓRIO</p>
        <h1>Detalhes da auditoria</h1>
      </div>
      <div class="header-actions">
        <a routerLink="/admin/agents/baley">Voltar aos relatórios</a>
        @if (reviewCount()) {
          <button type="button" [disabled]="pending() !== null" (click)="confirmAll.set(true)">
            Autorizar todas
          </button>
        }
      </div>
    </header>
    @if (state().loading) {
      <p role="status">Carregando relatório…</p>
    }
    @if (state().error) {
      <p role="alert">Relatório indisponível, removido ou sem permissão de acesso.</p>
    }
    @if (state().report; as report) {
      @if (confirmAll()) {
        <div class="approval-box">
          <p>Autorizar as {{ reviewCount() }} verificações pendentes deste relatório?</p>
          <p class="reference">
            Inclui todos os níveis, mesmo com filtro ativo. Itens sem procedimento seguro serão
            encaminhados para intervenção. Nenhuma ação ocorre com infraestrutura sem saúde
            confirmada.
          </p>
          <button type="button" [disabled]="pending() !== null" (click)="approveAll()">
            Confirmar autorizações
          </button>
          <button type="button" [disabled]="pending() !== null" (click)="confirmAll.set(false)">
            Cancelar
          </button>
        </div>
      }
      @if (batchMessage()) {
        <p role="status">{{ batchMessage() }}</p>
      }
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
      @if (tasks().error) {
        <p class="reference" role="status">
          Aprovações indisponíveis neste momento; consulta do relatório preservada.
        </p>
      }
      @if (actionError()) {
        <p role="alert">Não foi possível registrar a autorização. Atualize e tente novamente.</p>
      }
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
            <div class="finding-body">
              <div class="finding-action">
                <p>{{ finding.recommendation }}</p>
              </div>
              @if (finding.evidence) {
                <aside class="finding-evidence">
                  <span>Evidência</span>
                  <p>{{ finding.evidence }}</p>
                </aside>
              }
            </div>
            @if (task(finding.findingIndex); as action) {
              <footer class="remediation-footer">
                <div>
                  <span [class]="'workflow-state ' + action.status">{{
                    statusLabel(action.status)
                  }}</span>
                  @if (action.message) {
                    <p class="reference">{{ action.message }}</p>
                  }
                </div>
                @if (action.status === 'review') {
                  <button
                    type="button"
                    [disabled]="pending() !== null"
                    (click)="confirming.set(finding.findingIndex ?? null)"
                  >
                    {{ action.supported ? 'Autorizar correção' : 'Encaminhar para intervenção' }}
                  </button>
                }
              </footer>
              @if (confirming() === finding.findingIndex) {
                <div class="approval-box">
                  <p>{{ action.planTitle }}</p>
                  <p class="reference">
                    A autorização vale só para este procedimento. Saúde instável bloqueia a
                    execução.
                  </p>
                  <button
                    type="button"
                    [disabled]="pending() !== null"
                    (click)="approve(finding.findingIndex!)"
                  >
                    {{
                      pending() === finding.findingIndex ? 'Registrando…' : 'Confirmar autorização'
                    }}
                  </button>
                  <button
                    type="button"
                    [disabled]="pending() !== null"
                    (click)="confirming.set(null)"
                  >
                    Cancelar
                  </button>
                </div>
              }
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
      <section class="report-summary">
        <h2>
          Cobertura e limitações
          <span class="reference"
            >{{ content(report).limitations.length }}
            {{ content(report).limitations.length === 1 ? 'observação' : 'observações' }}</span
          >
        </h2>
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
      </section>
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
  private readonly destroy = inject(DestroyRef);
  private readonly refreshTasks = new Subject<void>();
  readonly confirmAll = signal(false);
  readonly batchMessage = signal('');
  readonly confirming = signal<number | null>(null);
  readonly pending = signal<number | null>(null);
  readonly actionError = signal(false);
  readonly tasks = toSignal(
    this.route.paramMap.pipe(
      switchMap((params) =>
        merge(timer(0, 5000), this.refreshTasks).pipe(
          exhaustMap(() =>
            this.api.remediations(params.get('reportId') ?? '').pipe(
              map((response) => ({
                items: Array.isArray(response.items) ? response.items : [],
                error: !Array.isArray(response.items),
              })),
              catchError(() => of({ items: [] as SecurityRemediation[], error: true })),
            ),
          ),
        ),
      ),
    ),
    { initialValue: { items: [] as SecurityRemediation[], error: false } },
  );
  readonly reviewCount = computed(
    () => this.tasks().items.filter((item) => item.status === 'review').length,
  );
  approveAll() {
    if (this.pending() !== null) return;
    const id = this.route.snapshot.paramMap.get('reportId');
    if (!id) return;
    this.pending.set(-1);
    this.actionError.set(false);
    this.api
      .authorizeAll(id)
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.pending.set(null)),
      )
      .subscribe({
        next: (result) => {
          this.confirmAll.set(false);
          this.refreshTasks.next();
          this.batchMessage.set(
            `${result.authorizedCount} autorizadas · ${result.interventionCount} para intervenção · ${result.alreadyHandledCount} já tratadas`,
          );
        },
        error: () => this.actionError.set(true),
      });
  }
  task(index: number | undefined) {
    return this.tasks().items.find((item) => item.findingIndex === index);
  }
  statusLabel(status: string) {
    return remediationLabels[status] ?? 'Requer revisão';
  }
  approve(index: number) {
    if (this.pending() !== null) return;
    const id = this.route.snapshot.paramMap.get('reportId');
    if (!id) return;
    this.pending.set(index);
    this.actionError.set(false);
    this.api
      .authorize(id, index)
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.pending.set(null)),
      )
      .subscribe({
        next: () => {
          this.confirming.set(null);
          this.refreshTasks.next();
        },
        error: () => this.actionError.set(true),
      });
  }
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
