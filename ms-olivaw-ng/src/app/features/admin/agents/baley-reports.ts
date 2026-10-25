import { Component, inject, signal, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AgentsApi, reportContent, type SecurityReport } from './agents-api';
import { findingCounts, riskTag } from './report-severity';
@Component({
  selector: 'app-baley-reports',
  imports: [RouterLink, DatePipe],
  styleUrls: ['./agents.scss', './report-tags.scss'],
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
    <div class="report-grid" aria-label="Relatórios do Baley">
      @for (report of reports(); track report.externalId) {
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
