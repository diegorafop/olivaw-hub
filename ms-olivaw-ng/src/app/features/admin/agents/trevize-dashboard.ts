import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LeadsApi, type LeadItem } from '../leads/leads-api';
import { LEAD_STATUSES } from '../leads/lead-status';
import { AgentChart } from './agent-chart';
import { dailySeries, leadMetrics, withinPeriod } from './agent-metrics';
@Component({
  selector: 'app-trevize-dashboard',
  imports: [RouterLink, DatePipe, AgentChart],
  styleUrls: ['./agents.scss', './agent-dashboard.scss'],
  template: `<main id="operation-content" tabindex="-1">
    <header>
      <div>
        <p class="eyebrow">AGENTS / CAPTAÇÃO</p>
        <h1>Trevize</h1>
        <p>Da descoberta à conversa: acompanhe a evolução das oportunidades.</p>
      </div>
      <button type="button" [disabled]="loading()" (click)="load(true)">Atualizar</button>
    </header>
    <div class="dashboard-toolbar">
      <label for="capture-period">Período de entrada</label>
      <select id="capture-period" [value]="days()" (change)="days.set(+$any($event.target).value)">
        <option value="7">Últimos 7 dias</option>
        <option value="30">Últimos 30 dias</option>
        <option value="90">Últimos 90 dias</option>
      </select>
      <a class="dashboard-link" routerLink="/admin/leads" [queryParams]="{ entry: 'prospecting' }"
        >Trabalhar oportunidades ↗</a
      >
    </div>
    <p class="muted">
      Prospecção ativa · {{ items().length }} de {{ total() }} registros carregados. Indicadores
      calculados sobre {{ visible().length }} entradas do período entre os registros carregados.
      Inclui captação manual: a origem ainda não identifica exclusivamente o Trevize.
    </p>
    @if (loading()) {
      <p role="status">Consultando captação…</p>
    }
    @if (error()) {
      <p role="alert">Não foi possível consultar a captação. Tente atualizar.</p>
    }
    <section class="metrics" aria-label="Resumo de captação">
      <article class="metric">
        <span>Entradas</span><strong>{{ visible().length }}</strong
        ><small>Candidatos registrados na API</small>
      </article>
      <article class="metric">
        <span>Com avanço comercial</span><strong>{{ metrics().active }}</strong
        ><small>Já saíram de novo contato</small>
      </article>
      <article class="metric">
        <span>Aguardando triagem</span><strong>{{ metrics().pending }}</strong
        ><small>Revisão humana pendente</small>
      </article>
      <article class="metric">
        <span>Projetos entregues</span><strong>{{ metrics().delivered }}</strong
        ><small>Etapa atual: entregue</small>
      </article>
    </section>
    <div class="charts">
      <app-agent-chart
        title="Ritmo de captação"
        caption="Entradas por dia; não representa execuções do agente."
        [points]="series()"
        [timeline]="true"
      />
      <app-agent-chart
        title="Destino das oportunidades"
        caption="Etapa atual dos contatos captados no período; não é um funil de conversão histórica."
        [points]="metrics().stages"
      />
    </div>
    <p class="muted">
      Ciclos sem resultados, descartes e falhas locais ainda não são enviados à API. Esta visão
      mostra os contatos persistidos, sem estimar atividade ou disponibilidade do agente.
    </p>
    <h2 class="list-title">Contatos captados</h2>
    <div class="report-grid">
      @for (item of visible(); track item.externalId) {
        <a class="report-card" [routerLink]="['/admin/leads', item.externalId, 'details']"
          ><div class="report-heading">
            <strong>{{ item.business || item.title }}</strong
            ><span
              >{{ item.segment || 'Segmento pendente' }} ·
              {{ item.createdAt | date: 'dd/MM/yyyy · HH:mm' }}</span
            >
          </div>
          <span>{{ statuses[item.followUpStatus] || 'Não classificado' }}</span
          ><span aria-hidden="true">↗</span></a
        >
      }
      @if (!loading() && !error() && !visible().length) {
        <p>Nenhum contato neste recorte. Amplie o período ou carregue mais registros.</p>
      }
    </div>
    @if (items().length < total()) {
      <button type="button" [disabled]="loading()" (click)="load(false)">
        Carregar mais contatos
      </button>
    }
  </main>`,
})
export class TrevizeDashboard {
  private readonly api = inject(LeadsApi);
  private readonly destroy = inject(DestroyRef);
  readonly items = signal<LeadItem[]>([]);
  readonly total = signal(0);
  readonly loading = signal(false);
  readonly error = signal(false);
  readonly days = signal(30);
  readonly statuses = LEAD_STATUSES;
  private page = 0;
  readonly visible = computed(() =>
    this.items().filter((item) => withinPeriod(item.createdAt, this.days())),
  );
  readonly metrics = computed(() => leadMetrics(this.visible()));
  readonly series = computed(() =>
    dailySeries(
      this.visible().map((item) => item.createdAt),
      this.days(),
    ),
  );
  constructor() {
    this.load(true);
  }
  load(reset: boolean) {
    if (this.loading()) return;
    this.loading.set(true);
    this.error.set(false);
    const page = reset ? 1 : this.page + 1;
    this.api
      .list({ page, search: '', entry: 'prospecting', followUpStatus: '', from: '', until: '' })
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (result) => {
          this.items.update((current) =>
            reset
              ? result.items
              : [
                  ...current,
                  ...result.items.filter(
                    (item) => !current.some((row) => row.externalId === item.externalId),
                  ),
                ],
          );
          this.total.set(result.total);
          this.page = page;
          this.loading.set(false);
        },
        error: () => {
          this.error.set(true);
          this.loading.set(false);
        },
      });
  }
}
