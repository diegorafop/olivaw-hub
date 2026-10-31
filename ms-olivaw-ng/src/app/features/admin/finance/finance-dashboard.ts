import { Component, DestroyRef, computed, effect, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FinanceApi, type FinanceReport } from './finance-api';
import { financeRange, type FinancePeriod } from './finance-period';
import { CustomersApi } from '../customers/customers-api';
import { inventoryError } from '../inventory/inventory-api';
@Component({
  selector: 'app-finance-dashboard',
  imports: [RouterLink, ReactiveFormsModule, CurrencyPipe, DatePipe],
  templateUrl: './finance-dashboard.html',
  styleUrls: ['./finance-dashboard.scss', './finance-charts.scss'],
})
export class FinanceDashboard {
  private readonly api = inject(FinanceApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroy = inject(DestroyRef);
  private readonly customerApi = inject(CustomersApi);
  private readonly params = toSignal(this.route.queryParamMap, {
    initialValue: this.route.snapshot.queryParamMap,
  });
  readonly today = new Date().toISOString().slice(0, 10);
  readonly periods = [
    ['day', 'Dia'],
    ['week', 'Semana'],
    ['month', 'Mês'],
    ['year', 'Ano'],
  ];
  readonly period = computed<FinancePeriod>(() => {
    const value = this.params().get('period');
    return value === 'day' || value === 'week' || value === 'year' ? value : 'month';
  });
  readonly anchor = computed(() => this.params().get('date') || this.today);
  readonly customer = computed(() => this.params().get('customer') || '');
  readonly range = computed(() => {
    try {
      return financeRange(this.period(), this.anchor());
    } catch {
      return null;
    }
  });
  readonly data = signal<FinanceReport | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly retry = signal(0);
  readonly customers = signal<{ externalId: string; business: string }[]>([]);
  readonly customerPage = signal(1);
  readonly moreCustomers = signal(false);
  readonly customerError = signal(false);
  readonly customersLoading = signal(false);
  readonly form = new FormGroup({
    date: new FormControl(this.anchor(), { nonNullable: true }),
    customer: new FormControl(this.customer(), { nonNullable: true }),
  });
  readonly categories: Record<string, string> = {
    infrastructure: 'Infraestrutura',
    tools: 'Ferramentas',
    labor: 'Trabalho',
    other: 'Outros',
  };
  readonly expenseParams = computed(() => ({
    from: this.range()?.first,
    to: this.range()?.last,
    groupBy: this.range()?.groupBy,
    customer: this.customer() || null,
  }));
  readonly chartMax = computed(() =>
    Math.max(1, ...(this.data()?.series ?? []).flatMap((x) => [x.costBrl, x.creditsBrl])),
  );
  readonly chartPoints = computed(() =>
    (this.data()?.series ?? []).map((p, i, a) => ({
      ...p,
      x: 20 + (i * 720) / Math.max(1, a.length - 1),
      costY: 174 - (p.costBrl / this.chartMax()) * 150,
      creditY: 174 - (p.creditsBrl / this.chartMax()) * 150,
    })),
  );
  readonly categoryMax = computed(() =>
    Math.max(1, ...(this.data()?.byCategory ?? []).map((x) => x.grossBrl)),
  );
  readonly customerMax = computed(() =>
    Math.max(1, ...(this.data()?.byCustomer ?? []).map((x) => x.grossBrl)),
  );
  constructor() {
    this.loadCustomers();
    effect((cleanup) => {
      const range = this.range(),
        customer = this.customer();
      this.retry();
      this.form.reset({ date: this.anchor(), customer });
      this.data.set(null);
      this.error.set('');
      if (!range) {
        this.loading.set(false);
        this.error.set('Informe uma data válida para consultar o período.');
        return;
      }
      this.loading.set(true);
      const subscription = this.api.read(range.from, range.to, range.groupBy, customer).subscribe({
        next: (r) => {
          this.data.set(r);
          this.loading.set(false);
        },
        error: (e) => {
          this.error.set(inventoryError(e));
          this.loading.set(false);
        },
      });
      cleanup(() => subscription.unsubscribe());
    });
  }
  path(kind: 'cost' | 'credit') {
    return this.chartPoints()
      .map((p, i) => `${i ? 'L' : 'M'}${p.x},${kind === 'cost' ? p.costY : p.creditY}`)
      .join(' ');
  }
  apply() {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        period: this.period(),
        date: this.form.controls.date.value,
        customer: this.form.controls.customer.value || null,
      },
    });
  }
  reload() {
    this.retry.update((v) => v + 1);
  }
  shift(offset: number) {
    const range = this.range();
    if (!range) return;
    const d = new Date(range.from);
    if (this.period() === 'year') d.setUTCFullYear(d.getUTCFullYear() + offset);
    else if (this.period() === 'month') d.setUTCMonth(d.getUTCMonth() + offset);
    else d.setUTCDate(d.getUTCDate() + offset * (this.period() === 'week' ? 7 : 1));
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        period: this.period(),
        date: d.toISOString().slice(0, 10),
        customer: this.customer() || null,
      },
    });
  }
  loadCustomers() {
    if (this.customersLoading()) return;
    this.customersLoading.set(true);
    this.customerError.set(false);
    this.customerApi
      .list(this.customerPage(), '', '')
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (r) => {
          this.customers.update((items) => [...items, ...r.items]);
          this.moreCustomers.set(this.customerPage() * 20 < r.total);
          this.customerPage.update((v) => v + 1);
          this.customersLoading.set(false);
        },
        error: () => {
          this.customerError.set(true);
          this.customersLoading.set(false);
        },
      });
  }
}
