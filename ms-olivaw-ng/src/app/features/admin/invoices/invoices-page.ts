import { Component, computed, effect, inject, signal } from '@angular/core';
import { CurrencyPipe, JsonPipe } from '@angular/common';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { timeout } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR } from '../../../core/http/http-context';
interface Invoice {
  externalId: string;
  issuer: string;
  accountReference: string;
  invoiceNumber: string | null;
  billingYear: number;
  billingMonth: number;
  currency: string;
  documentStatus: string;
  completeness: string;
  grossAmount: number | null;
  creditAmount: number | null;
  taxAmount: number | null;
  netAmount: number | null;
  exchangeRateToBrl: number | null;
  exchangeRateNature: string;
  source: string;
}
interface Detail {
  invoice: Invoice;
  rawData: string;
  expenses: { externalId: string; description: string; amountBrl: number; kind: string }[];
}
@Component({
  selector: 'app-invoices-page',
  imports: [CurrencyPipe, JsonPipe, ReactiveFormsModule, RouterLink],
  templateUrl: './invoices-page.html',
  styleUrls: ['../inventory/inventory.scss', '../plans/plan-catalog.scss'],
})
export class InvoicesPage {
  private readonly http = inject(HttpClient);
  private readonly config = inject(RuntimeConfig);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly params = toSignal(this.route.paramMap);
  readonly id = computed(() => this.params()?.get('invoiceId') || '');
  readonly creating = computed(() => this.route.snapshot.data['create'] === true);
  readonly items = signal<Invoice[]>([]);
  readonly detail = signal<Detail | null>(null);
  readonly loading = signal(false);
  readonly pending = signal(false);
  readonly error = signal('');
  readonly cursor = signal<number | null>(null);
  readonly raw = computed(() => {
    try {
      return JSON.parse(this.detail()?.rawData || '{}') as unknown;
    } catch {
      return this.detail()?.rawData;
    }
  });
  private readonly key = crypto.randomUUID();
  readonly form = inject(FormBuilder).group({
    issuer: ['', Validators.required],
    accountReference: ['', Validators.required],
    invoiceNumber: [''],
    billingYear: [new Date().getUTCFullYear(), Validators.required],
    billingMonth: [new Date().getUTCMonth() + 1, Validators.required],
    currency: ['USD'],
    documentStatus: ['provisional'],
    completeness: ['partial'],
    grossAmount: [null as number | null],
    creditAmount: [null as number | null],
    taxAmount: [null as number | null],
    netAmount: [null as number | null],
    exchangeRateToBrl: [null as number | null],
    exchangeRateNature: ['unknown'],
    source: ['', Validators.required],
    rawData: ['', Validators.required],
    expenseIds: [''],
  });
  constructor() {
    effect((cleanup) => {
      const id = this.id();
      if (this.creating()) return;
      this.loading.set(true);
      this.error.set('');
      this.detail.set(null);
      const subscription = this.http
        .get<Detail | { items: Invoice[]; nextBeforeId: number | null }>(
          this.config.endpoint(id ? 'aws-invoices/' + encodeURIComponent(id) : 'aws-invoices'),
          this.options(),
        )
        .pipe(timeout(15000))
        .subscribe({
          next: (result) => {
            if ('invoice' in result) this.detail.set(result);
            else {
              this.items.set(result.items);
              this.cursor.set(result.nextBeforeId);
            }
            this.loading.set(false);
          },
          error: () => {
            this.error.set('Não foi possível consultar as faturas. Confira a publicação da API.');
            this.loading.set(false);
          },
        });
      cleanup(() => subscription.unsubscribe());
    });
  }
  private options() {
    return { context: new HttpContext().set(LOCAL_ERROR, true) };
  }
  older() {
    const cursor = this.cursor();
    if (!cursor || this.loading()) return;
    this.loading.set(true);
    this.error.set('');
    this.http
      .get<{ items: Invoice[]; nextBeforeId: number | null }>(
        this.config.endpoint('aws-invoices'),
        { ...this.options(), params: new HttpParams().set('beforeId', cursor) },
      )
      .pipe(timeout(15000))
      .subscribe({
        next: (r) => {
          this.items.update((items) => [...items, ...r.items]);
          this.cursor.set(r.nextBeforeId);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Não foi possível carregar as anteriores.');
          this.loading.set(false);
        },
      });
  }
  save() {
    if (this.form.invalid || this.pending()) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    try {
      const raw: unknown = JSON.parse(v.rawData || '');
      if (!raw || typeof raw !== 'object' || Array.isArray(raw)) throw new Error();
    } catch {
      this.error.set('Informe um objeto JSON válido nos dados originais.');
      return;
    }
    const { expenseIds, ...data } = v;
    this.pending.set(true);
    this.error.set('');
    this.http
      .post<{ externalId: string }>(
        this.config.endpoint('aws-invoices'),
        {
          ...data,
          submissionKey: this.key,
          invoiceNumber: v.invoiceNumber || null,
          expenseExternalIds: (expenseIds || '').split(/[\s,;]+/).filter(Boolean),
        },
        this.options(),
      )
      .pipe(timeout(15000))
      .subscribe({
        next: (r) => {
          this.pending.set(false);
          void this.router.navigate(['/admin/invoices', r.externalId]);
        },
        error: () => {
          this.pending.set(false);
          this.error.set(
            'Salvamento não confirmado. Confira os dados, totais, vínculos e a lista antes de repetir; o formulário foi preservado.',
          );
        },
      });
  }
  label(value: string) {
    return (
      (
        {
          provisional: 'Provisório',
          issued: 'Emitida',
          paid: 'Paga',
          complete: 'Documento completo',
          'issuer-section': 'Seção de emissor',
          partial: 'Captura parcial',
          managerial: 'Gerencial',
          payment: 'Efetivo do pagamento',
          unknown: 'Não informado',
        } as Record<string, string>
      )[value] || value
    );
  }
}
