import { Component, DestroyRef, computed, effect, inject, signal, untracked } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { finalize, type Subscription } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR, SKIP_LOADING } from '../../../core/http/http-context';
import { CustomersApi } from '../customers/customers-api';
import { inventoryError } from '../inventory/inventory-api';
interface Expense {
  externalId: string;
  description: string;
  category: string;
  kind: string;
  amount: number;
  currency: string;
  amountBrl: number;
  exchangeRateToBrl: number;
  occurredAt: string;
  source: string;
  customerName: string;
}
interface Report {
  grossBrl: number;
  creditsBrl: number;
  netBrl: number;
  unallocatedBrl: number;
  total: number;
  items: Expense[];
  series: { date: string; costBrl: number; creditsBrl: number; netBrl: number }[];
}
@Component({
  selector: 'app-expenses-page',
  imports: [RouterLink, ReactiveFormsModule, CurrencyPipe, DatePipe],
  templateUrl: './expenses-page.html',
  styleUrls: ['../inventory/inventory.scss', './expenses-page.scss'],
})
export class ExpensesPage {
  private readonly http = inject(HttpClient);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly query = toSignal(this.route.queryParamMap, {
    initialValue: this.route.snapshot.queryParamMap,
  });
  private readonly runtime = inject(RuntimeConfig);
  private readonly customersApi = inject(CustomersApi);
  private readonly destroy = inject(DestroyRef);
  readonly report = signal<Report | null>(null);
  readonly customers = signal<{ externalId: string; business: string }[]>([]);
  readonly customerPage = signal(1);
  readonly moreCustomers = signal(false);
  readonly error = signal('');
  readonly success = signal('');
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly showForm = signal(false);
  readonly page = signal(1);
  readonly categories: Record<string, string> = {
    infrastructure: 'Infraestrutura',
    tools: 'Ferramentas',
    labor: 'Trabalho',
    other: 'Outros',
  };
  readonly categoryOptions = Object.entries(this.categories);
  private entryKey = crypto.randomUUID();
  private submitted: unknown = null;
  private currentQuery?: Subscription;
  private readonly today = new Date().toISOString().slice(0, 10);
  readonly filters = new FormGroup({
    from: new FormControl(this.today.slice(0, 7) + '-01', {
      nonNullable: true,
      validators: Validators.required,
    }),
    to: new FormControl(this.today, { nonNullable: true, validators: Validators.required }),
    groupBy: new FormControl('day', { nonNullable: true }),
    customer: new FormControl('', { nonNullable: true }),
  });
  readonly form = new FormGroup({
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(160)],
    }),
    category: new FormControl('infrastructure', { nonNullable: true }),
    kind: new FormControl('cost', { nonNullable: true }),
    amount: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(0.01),
      Validators.max(100000000),
    ]),
    currency: new FormControl('BRL', { nonNullable: true }),
    exchangeRateToBrl: new FormControl(1, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0.000001), Validators.max(1000)],
    }),
    occurredAt: new FormControl(this.today, { nonNullable: true, validators: Validators.required }),
    customerExternalId: new FormControl('', { nonNullable: true }),
    source: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(300)],
    }),
  });
  readonly currency = toSignal(this.form.controls.currency.valueChanges, {
    initialValue: this.form.controls.currency.value,
  });
  readonly chart = computed(() => {
    const points = this.report()?.series ?? [];
    const min = Math.min(0, ...points.map((p) => p.netBrl));
    const max = Math.max(1, ...points.map((p) => p.netBrl));
    return points
      .map(
        (p, i) =>
          `${i ? 'L' : 'M'}${30 + (i * 740) / Math.max(1, points.length - 1)},${170 - ((p.netBrl - min) / (max - min)) * 140}`,
      )
      .join(' ');
  });
  readonly chartPoints = computed(() => {
    const points = this.report()?.series ?? [];
    const min = Math.min(0, ...points.map((p) => p.netBrl));
    const max = Math.max(1, ...points.map((p) => p.netBrl));
    return points.map((p, i) => ({
      date: p.date,
      value: p.netBrl,
      x: 30 + (i * 740) / Math.max(1, points.length - 1),
      y: 170 - ((p.netBrl - min) / (max - min)) * 140,
    }));
  });
  private options() {
    return { context: new HttpContext().set(LOCAL_ERROR, true).set(SKIP_LOADING, true) };
  }
  constructor() {
    this.loadCustomers();
    effect(() => {
      const params = this.query();
      this.filters.reset({
        from: params.get('from') || this.today.slice(0, 7) + '-01',
        to: params.get('to') || this.today,
        groupBy: params.get('groupBy') || 'day',
        customer: params.get('customer') || '',
      });
      this.page.set(Math.max(1, Number(params.get('page')) || 1));
      untracked(() => this.load());
    });
  }
  loadCustomers() {
    this.customersApi
      .list(this.customerPage(), '', '')
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (r) => {
          this.customers.update((items) => [...items, ...r.items]);
          this.moreCustomers.set(this.customerPage() * 20 < r.total);
          this.customerPage.update((v) => v + 1);
        },
        error: () =>
          this.error.set(
            'Não foi possível listar clientes. Tente novamente antes de lançar uma despesa individual.',
          ),
      });
  }
  filter() {
    void this.router
      .navigate([], {
        relativeTo: this.route,
        queryParams: { ...this.filters.getRawValue(), page: 1 },
      })
      .then((changed) => {
        if (!changed) this.load();
      });
  }
  load() {
    if (this.filters.invalid) return;
    const f = this.filters.getRawValue();
    const end = new Date(f.to + 'T00:00:00Z');
    end.setUTCDate(end.getUTCDate() + 1);
    if (!Number.isFinite(end.getTime())) return;
    let params = new HttpParams()
      .set('from', f.from + 'T00:00:00Z')
      .set('to', end.toISOString())
      .set('groupBy', f.groupBy)
      .set('page', this.page());
    if (f.customer === 'shared') params = params.set('unallocatedOnly', true);
    else if (f.customer) params = params.set('customerExternalId', f.customer);
    this.currentQuery?.unsubscribe();
    this.loading.set(true);
    this.error.set('');
    this.report.set(null);
    this.currentQuery = this.http
      .get<Report>(this.runtime.endpoint('expenses'), { ...this.options(), params })
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (r) => this.report.set(r),
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  go(offset: number) {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { ...this.filters.getRawValue(), page: this.page() + offset },
    });
  }
  save() {
    if (this.saving()) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    const f = this.form.getRawValue();
    const body = {
      ...f,
      entryKey: this.entryKey,
      customerExternalId: f.customerExternalId || null,
      occurredAt: f.occurredAt + 'T00:00:00Z',
      exchangeRateToBrl: f.currency === 'BRL' ? 1 : f.exchangeRateToBrl,
    };
    if (this.submitted && JSON.stringify(body) !== JSON.stringify(this.submitted)) {
      this.error.set('Confira se o envio anterior foi salvo antes de alterar e lançar novamente.');
      return;
    }
    this.submitted = body;
    this.saving.set(true);
    this.form.disable();
    this.error.set('');
    this.http
      .post(this.runtime.endpoint('expenses'), body, this.options())
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => {
          this.saving.set(false);
          this.form.enable();
        }),
      )
      .subscribe({
        next: () => {
          this.success.set(
            'Despesa registrada. Confira o período selecionado para encontrá-la na lista.',
          );
          this.showForm.set(false);
          this.entryKey = crypto.randomUUID();
          this.submitted = null;
          this.form.reset({
            category: 'infrastructure',
            kind: 'cost',
            currency: 'BRL',
            exchangeRateToBrl: 1,
            occurredAt: this.today,
            description: '',
            amount: null,
            customerExternalId: '',
            source: '',
          });
          this.load();
        },
        error: (e) => {
          if (e.status === 400 || e.status === 422) this.submitted = null;
          this.error.set(inventoryError(e));
        },
      });
  }
}
