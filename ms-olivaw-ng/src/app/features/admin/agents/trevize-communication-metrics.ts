import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpClient, HttpContext } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { firstValueFrom, timeout } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR, SKIP_LOADING } from '../../../core/http/http-context';

interface CommunicationSummary {
  inspected: number;
  truncated: boolean;
  asOf: string;
  counts: Record<string, number>;
}

@Component({
  selector: 'app-trevize-communication-metrics',
  imports: [RouterLink, DatePipe],
  styleUrl: '../inventory/inventory.scss',
  template: `<section aria-label="Contato e atendimento">
    <header>
      <div>
        <p class="eyebrow">OLIVAW / AGENTS</p>
        <h2>Contato e atendimento</h2>
        <p>Contatos descobertos e acompanhamento das oportunidades.</p>
      </div>
      <button type="button" (click)="load()" [disabled]="loading()">Atualizar indicadores</button>
    </header>
    <p>Este painel consulta o CRM. Abrir a página não inicia campanha, coleta ou envio.</p>
    <a routerLink="/admin/leads" [queryParams]="{ entry: 'prospecting' }"
      >Revisar leads de prospecção</a
    >
    @if (error()) {
      <p role="alert">Não foi possível consultar os indicadores. Tente atualizar.</p>
    }
    @if (loading()) {
      <p role="status">Consultando indicadores...</p>
    }
    @if (summary(); as data) {
      <p>
        Últimas {{ data.inspected }} fichas consultadas · {{ data.asOf | date: 'dd/MM/yyyy HH:mm' }}
      </p>
      @if (data.truncated) {
        <p role="status">
          Amostra das 500 fichas mais recentes; os números não representam o cadastro inteiro.
        </p>
      }
      <section class="resource-grid" aria-label="Indicadores do Trevize">
        @for (metric of metrics; track metric.key) {
          <article class="resource-card">
            <h2>{{ metric.label }}</h2>
            <p>{{ data.counts[metric.key] || 0 }}</p>
          </article>
        }
      </section>
      <p>
        Contatos, recusas e pausas contam fichas. Envios, respostas e rascunhos contam registros
        históricos. Sem e-mail não significa contato realizado. Enviados não comprova entrega ou
        leitura.
      </p>
      <p>
        Ativação, OAuth e bloqueios locais ficam sob controle do agente; este painel não confirma
        que ele esteja em execução.
      </p>
    }
  </section>`,
})
export class TrevizeCommunicationMetrics {
  private readonly http = inject(HttpClient);
  private readonly config = inject(RuntimeConfig);
  readonly loading = signal(false);
  readonly error = signal(false);
  readonly summary = signal<CommunicationSummary | null>(null);
  readonly metrics = [
    { key: 'collected', label: 'Leads de prospecção' },
    { key: 'enriched', label: 'Com e-mail localizado' },
    { key: 'noEmail', label: 'Pesquisa sem e-mail' },
    { key: 'inconclusive', label: 'Pesquisa inconclusiva ou divergente' },
    { key: 'ready', label: 'Disponíveis para revisão de contato' },
    { key: 'prepared', label: 'Mensagens preparadas' },
    { key: 'sent', label: 'Envios confirmados' },
    { key: 'received', label: 'Respostas registradas' },
    { key: 'pending', label: 'Envios a reconciliar' },
    { key: 'drafts', label: 'Rascunhos de atendimento gerados' },
    { key: 'suppressed', label: 'Recusas registradas' },
    { key: 'handoff', label: 'Encaminhados ao operador' },
    { key: 'human', label: 'Conversas assumidas manualmente' },
    { key: 'cancelled', label: 'Mensagens canceladas' },
    { key: 'unreadable', label: 'Fichas que precisam de revisão' },
  ];
  constructor() {
    void this.load();
  }
  async load() {
    if (this.loading()) return;
    this.loading.set(true);
    this.error.set(false);
    try {
      const data = await firstValueFrom(
        this.http
          .get<CommunicationSummary>(this.config.endpoint('leads/communication-summary'), {
            context: new HttpContext().set(LOCAL_ERROR, true).set(SKIP_LOADING, true),
          })
          .pipe(timeout(15000)),
      );
      if (!data || !data.counts || typeof data.inspected !== 'number')
        throw new Error('Invalid summary');
      this.summary.set(data);
    } catch {
      this.error.set(true);
    } finally {
      this.loading.set(false);
    }
  }
}
