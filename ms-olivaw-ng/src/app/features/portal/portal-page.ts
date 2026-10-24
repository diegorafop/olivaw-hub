import { PortalDeliveries } from './portal-deliveries';
import { PortalRequests } from './portal-requests';
import { Component, DestroyRef, inject, signal, viewChild } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, type Subscription } from 'rxjs';
import { RuntimeConfig } from '../../core/config/runtime-config';
import { LOCAL_ERROR, SKIP_LOADING } from '../../core/http/http-context';
import { PortalApi, type PortalCompany, type PortalDashboard } from './portal-api';
import { inventoryError } from '../admin/inventory/inventory-api';
@Component({
  selector: 'app-portal-page',
  imports: [
    PortalDeliveries,
    PortalRequests,
    ReactiveFormsModule,
    RouterLink,
    DatePipe,
    CurrencyPipe,
  ],
  templateUrl: './portal-page.html',
  styleUrl: '../admin/inventory/inventory.scss',
})
export class PortalPage {
  readonly api = inject(PortalApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroy = inject(DestroyRef);
  private readonly http = inject(HttpClient);
  private readonly runtime = inject(RuntimeConfig);
  readonly preview = this.route.snapshot.data['portalPreview'] === true;
  readonly customer = new FormControl(this.route.snapshot.queryParamMap.get('customer') ?? '', {
    nonNullable: true,
  });
  readonly companies = signal<PortalCompany[]>([]);
  readonly nextCompany = signal<number | null>(null);
  readonly companyPage = signal(1);
  readonly data = signal<PortalDashboard | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly tab = signal('overview');
  constructor() {
    if (!this.preview && !this.api.token()) {
      void this.router.navigate(['/meu-negocio/entrar']);
      return;
    }
    this.loadCompanies();
    if (this.customer.value) this.load();
  }
  loadCompanies(beforeId = 0) {
    if (this.preview) {
      this.http
        .get<{ items: { externalId: string; business: string }[]; total: number }>(
          this.runtime.endpoint('customers'),
          {
            params: new HttpParams().set('page', this.companyPage()).set('pageSize', 20),
            context: new HttpContext().set(LOCAL_ERROR, true).set(SKIP_LOADING, true),
          },
        )
        .pipe(takeUntilDestroyed(this.destroy))
        .subscribe({
          next: (response) => {
            this.companies.set(response.items.map((item) => ({ ...item, role: 'preview' })));
            this.nextCompany.set(
              this.companyPage() * 20 < response.total ? this.companyPage() + 1 : null,
            );
          },
          error: (e) => this.error.set(inventoryError(e)),
        });
      return;
    }
    this.api
      .companies(beforeId)
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (response) => {
          this.companies.set(response.items);
          this.nextCompany.set(response.nextBeforeId);
          if (!this.customer.value && response.items.length === 1) {
            this.customer.setValue(response.items[0].externalId);
            this.load();
          }
        },
        error: (e) => this.handleError(e),
      });
  }
  moreCompanies() {
    const next = this.nextCompany();
    if (next === null) return;
    if (this.preview) this.companyPage.set(next);
    this.loadCompanies(next);
  }
  private currentRequest?: Subscription;
  load(beforeId = 0) {
    if (!this.customer.value) {
      this.data.set(null);
      return;
    }
    if (
      (this.preview || this.api.token()) &&
      this.requestsView() &&
      !this.requestsView()!.canLeave()
    ) {
      if (this.data()) this.customer.setValue(this.data()!.customerExternalId);
      return;
    }
    this.currentRequest?.unsubscribe();
    const selected = this.customer.value;
    this.loading.set(true);
    this.error.set('');
    this.data.set(null);
    this.currentRequest = this.api
      .dashboard(this.customer.value, this.preview, beforeId)
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (response) => {
          if (this.customer.value === selected) this.data.set(response);
        },
        error: (e) => this.handleError(e),
      });
  }
  stateLabel(state: string) {
    const labels: Record<string, string> = {
      unknown: 'Estamos atualizando a verificação',
      stale: 'A verificação precisa ser atualizada',
      available: 'Disponível na última verificação',
      attention: 'A equipe precisa conferir a situação',
    };
    return labels[state] ?? labels['unknown'];
  }
  private handleError(error: unknown) {
    this.error.set(inventoryError(error));
    const status = (error as { status?: number }).status;
    if (!this.preview && status === 401) {
      this.api.session.set(null);
      void this.router.navigate(['/meu-negocio/entrar']);
    }
  }
  setTab(value: string) {
    if (value === this.tab()) return;
    if (this.requestsView() && !this.requestsView()!.canLeave()) return;
    this.tab.set(value);
  }
  readonly requestsView = viewChild(PortalRequests);
  canLeave() {
    if (!this.preview && !this.api.token()) return true;
    return this.requestsView()?.canLeave() ?? true;
  }
  signOut() {
    this.api
      .logout()
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => {
          this.api.session.set(null);
          void this.router.navigate(['/meu-negocio/entrar']);
        }),
      )
      .subscribe({ error: () => undefined });
  }
}
