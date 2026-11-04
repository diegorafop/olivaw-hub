import { Component, inject, signal, DestroyRef, computed } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AgentsApi, reportContent, type SecurityReport } from './agents-api';
import { AgentChart } from './agent-chart';
import { dailySeries, securityMetrics, withinPeriod } from './agent-metrics';
import { findingCounts, riskTag } from './report-severity';
@Component({
  selector: 'app-baley-reports',
  imports: [RouterLink, DatePipe, AgentChart],
  styleUrls: ['./agents.scss', './report-tags.scss', './agent-dashboard.scss'],
  template: `<main id="operation-content" tabindex="-1">
    <header>
      <div>
        <p class="eyebrow">AGENTS / SEGURANÇA</p>
        <h1>Baley</h1>
        <p>Verificações do servidor e dos serviços.</p>
      </div>
      <button type="button" [disabled]="loading()" (click)="load(true)">Atualizar</button>
    </header>
    @if (error()) {
      <p role="alert">Não foi possível consultar os relatórios. Tente novamente.</p>
    }
    @if (loading()) {
      <p role="status">Consultando relatórios…</p>
    }
    @if (!loading() && !error() && !reports().length) {
      <section class="empty">
        <h2>Aguardando o primeiro relatório</h2>
        <p>Os resultados aparecem aqui após a confirmação de recebimento pela API.</p>
      </section>
    }
    <div class="dashboard-toolbar">
      <label for="security-period">Período da auditoria</label>
      <select id="security-period" [value]="days()" (change)="days.set(+$any($event.target).value)">
        <option value="7">Últimos 7 dias</option>
        <option value="30">Últimos 30 dias</option>
        <option value="90">Últimos 90 dias</option>
      </select>
    </div>
    <p class="muted">
      {{ reports().length }} relatórios carregados; {{ visible().length }} no período. Carregue os
      anteriores para ampliar a análise. A API registra resultados, não o estado do processo local.
    </p>
    <section class="metrics" aria-label="Resumo de segurança">
      <article class="metric">
        <span>Auditorias recebidas</span><strong>{{ visible().length }}</strong
        ><small>Resultados no período carregado</small>
      </article>
      <article class="metric">
        <span>Com risco elevado</span><strong>{{ metrics().elevated }}</strong
        ><small>Auditorias com achados altos ou críticos</small>
      </article>
      <article class="metric">
        <span>Coletas incompletas</span><strong>{{ metrics().incomplete }}</strong
        ><small>Parciais, falhas ou conteúdo indisponível</small>
      </article>
      <article class="metric">
        <span>Servidores avaliados</span><strong>{{ metrics().hosts }}</strong
        ><small>Hosts distintos neste recorte</small>
      </article>
    </section>
    <div class="charts">
      <app-agent-chart
        title="Ritmo de auditoria"
        caption="Relatórios recebidos por data de conclusão."
        [points]="series()"
        [timeline]="true"
      />
      <app-agent-chart
        title="Distribuição dos achados"
        caption="Ocorrências nas auditorias; um mesmo problema pode reaparecer em vários relatórios."
        [points]="metrics().severity"
      />
    </div>
    <h2 class="list-title">Auditorias e evidências</h2>
    <div class="report-grid" aria-label="Relatórios do Baley">
      @for (report of visible(); track report.externalId) {
        <a class="report-card" [routerLink]="['/admin/agents/baley', report.externalId]">
          <span [class]="'tag ' + risk(report).tone" [attr.title]="risk(report).label"
            ><span class="dot" aria-hidden="true"></span>{{ risk(report).tag }}</span
          >
          <div class="report-heading">
            <strong>{{ report.hostId }}</strong
            ><span>{{ report.completedAt | date: 'dd/MM/yyyy · HH:mm' }}</span>
          </div>
          <div class="report-tags" aria-label="Achados por gravidade">
            @for (group of counts(content(report).findings); track group.id) {
              @if (group.count) {
                <span [class]="'tag ' + group.tone"
                  >{{ group.count }}
                  {{ group.count === 1 ? group.singular : group.description.toLowerCase() }}</span
                >
              }
            }
            @if (report.executionStatus === 'partial') {
              <span class="tag unknown">Coleta parcial</span>
            }
          </div>
          <span class="report-arrow" aria-hidden="true">↗</span>
        </a>
      }
    </div>
    @if (nextBeforeId()) {
      <button type="button" [disabled]="loading()" (click)="load(false)">
        Carregar anteriores
      </button>
    }
  </main>`,
})
export class BaleyReports {
  private readonly api = inject(AgentsApi);
  private readonly destroy = inject(DestroyRef);
  readonly reports = signal<SecurityReport[]>([]);
  readonly days = signal(30);
  readonly visible = computed(() =>
    this.reports().filter((report) => withinPeriod(report.completedAt, this.days())),
  );
  readonly metrics = computed(() => securityMetrics(this.visible()));
  readonly series = computed(() =>
    dailySeries(
      this.visible().map((report) => report.completedAt),
      this.days(),
    ),
  );
  readonly nextBeforeId = signal<number | null>(null);
  readonly loading = signal(false);
  readonly error = signal(false);
  readonly risk = riskTag;
  readonly counts = findingCounts;
  readonly content = reportContent;
  constructor() {
    this.load(true);
  }
  load(reset: boolean) {
    if (this.loading()) return;
    this.loading.set(true);
    this.error.set(false);
    this.api
      .list(reset ? 0 : (this.nextBeforeId() ?? 0))
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (page) => {
          this.reports.update((current) =>
            reset
              ? page.items
              : [
                  ...current,
                  ...page.items.filter(
                    (item) => !current.some((row) => row.externalId === item.externalId),
                  ),
                ],
          );
          this.nextBeforeId.set(page.nextBeforeId);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.error.set(true);
        },
      });
  }
}
