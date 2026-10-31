import { Component, computed, effect, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { timeout } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR } from '../../../core/http/http-context';
interface Account {
  externalId: string;
  name: string;
  openingBalance: number;
  openedAt: string;
}
interface Entry {
  externalId: string;
  description: string;
  direction: string;
  category: string;
  amount: number;
  settledAmount: number;
  dueAt: string;
  source: string;
  status: string;
  customerName: string;
  cancellationReason: string;
  revision: number;
}
interface Detail {
  entry: Entry;
  settlements: {
    externalId: string;
    accountName: string;
    amount: number;
    isRefund: boolean;
    paidAt: string;
    source: string;
  }[];
}
interface Summary {
  receivableBrl: number;
  payableBrl: number;
  receivedBrl: number;
  paidBrl: number;
  openingBalanceBrl: number;
  closingBalanceBrl: number;
  hasAccounts: boolean;
  series: { date: string; receivedBrl: number; paidBrl: number }[];
}
@Component({
  selector: 'app-financial-control',
  imports: [CurrencyPipe, DatePipe, RouterLink, ReactiveFormsModule],
  templateUrl: './financial-control.html',
  styleUrls: ['../inventory/inventory.scss', '../plans/plan-catalog.scss'],
})
export class FinancialControl {
  private readonly http = inject(HttpClient);
  private readonly config = inject(RuntimeConfig);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly params = toSignal(this.route.paramMap);
  private readonly query = toSignal(this.route.queryParamMap);
  readonly view = this.route.snapshot.data['financeView'] as string;
  readonly id = computed(() => this.params()?.get('entryId') || '');
  readonly month = computed(() => {
    const v = this.query()?.get('month');
    return v && /^\d{4}-(0[1-9]|1[0-2])$/.test(v) ? v : new Date().toISOString().slice(0, 7);
  });
  readonly accounts = signal<Account[]>([]);
  readonly entries = signal<Entry[]>([]);
  readonly detail = signal<Detail | null>(null);
  readonly summary = signal<Summary | null>(null);
  readonly error = signal('');
  readonly success = signal('');
  readonly pending = signal(false);
  readonly loading = signal(false);
  readonly nextAccount = signal<number | null>(null);
  readonly nextEntry = signal<number | null>(null);
  private accountKey = crypto.randomUUID();
  private entryKey = crypto.randomUUID();
  private settlementKey = crypto.randomUUID();
  private transferKey = crypto.randomUUID();
  readonly accountForm = this.fb.group({
    name: ['', Validators.required],
    openingBalance: [null as number | null, Validators.required],
    openedAt: [new Date().toISOString().slice(0, 10), Validators.required],
  });
  readonly entryForm = this.fb.group({
    description: ['', Validators.required],
    direction: ['payable'],
    category: ['other'],
    amount: [null as number | null, Validators.required],
    dueAt: [new Date().toISOString().slice(0, 10), Validators.required],
    source: ['', Validators.required],
  });
  readonly settlementForm = this.fb.group({
    accountExternalId: ['', Validators.required],
    amount: [null as number | null, Validators.required],
    isRefund: [false],
    paidAt: [new Date().toISOString().slice(0, 10), Validators.required],
    source: ['', Validators.required],
  });
  readonly transferForm = this.fb.group({
    fromAccountExternalId: ['', Validators.required],
    toAccountExternalId: ['', Validators.required],
    amount: [null as number | null, Validators.required],
    transferredAt: [new Date().toISOString().slice(0, 10), Validators.required],
    source: ['', Validators.required],
  });
  readonly cancelForm = this.fb.group({ cancellationReason: ['', Validators.required] });
  constructor() {
    this.loadAccounts();
    effect((cleanup) => {
      const id = this.id();
      const month = this.month();
      if (this.view === 'accounts') return;
      this.loading.set(true);
      this.error.set('');
      const options = this.options();
      const [year, m] = month.split('-').map(Number);
      const from = month + '-01T00:00:00Z';
      const to = new Date(Date.UTC(year, m, 1)).toISOString();
      const request = id
        ? this.http.get<Detail | Summary | { items: Entry[]; nextBeforeId: number | null }>(
            this.config.endpoint('finance/entries/' + encodeURIComponent(id)),
            options,
          )
        : this.http.get<Detail | Summary | { items: Entry[]; nextBeforeId: number | null }>(
            this.config.endpoint(this.view === 'control' ? 'finance/summary' : 'finance/entries'),
            { ...options, params: new HttpParams().set('from', from).set('to', to) },
          );
      const subscription = request.pipe(timeout(15000)).subscribe({
        next: (r) => {
          if ('entry' in r) this.detail.set(r);
          else if ('series' in r) this.summary.set(r);
          else {
            this.entries.set(r.items);
            this.nextEntry.set(r.nextBeforeId);
          }
          this.loading.set(false);
        },
        error: () => {
          this.error.set(
            'Não foi possível consultar o controle financeiro. Confira a publicação da API.',
          );
          this.loading.set(false);
        },
      });
      cleanup(() => subscription.unsubscribe());
    });
  }
  private options() {
    return { context: new HttpContext().set(LOCAL_ERROR, true) };
  }
  loadAccounts(cursor = 0) {
    this.http
      .get<{ items: Account[]; nextBeforeId: number | null }>(
        this.config.endpoint('finance/accounts'),
        { ...this.options(), params: new HttpParams().set('beforeId', cursor) },
      )
      .pipe(timeout(15000))
      .subscribe({
        next: (r) => {
          this.accounts.update((rows) => (cursor ? [...rows, ...r.items] : r.items));
          this.nextAccount.set(r.nextBeforeId);
        },
        error: () => this.error.set('Não foi possível consultar as contas financeiras.'),
      });
  }
  olderEntries() {
    const cursor = this.nextEntry();
    if (!cursor) return;
    const [year, m] = this.month().split('-').map(Number);
    this.http
      .get<{ items: Entry[]; nextBeforeId: number | null }>(
        this.config.endpoint('finance/entries'),
        {
          ...this.options(),
          params: new HttpParams()
            .set('from', this.month() + '-01T00:00:00Z')
            .set('to', new Date(Date.UTC(year, m, 1)).toISOString())
            .set('beforeId', cursor),
        },
      )
      .pipe(timeout(15000))
      .subscribe({
        next: (r) => {
          this.entries.update((rows) => [...rows, ...r.items]);
          this.nextEntry.set(r.nextBeforeId);
        },
        error: () => this.error.set('Não foi possível consultar obrigações anteriores.'),
      });
  }
  changeMonth(event: Event) {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { month: (event.target as HTMLInputElement).value },
    });
  }
  private send(path: string, payload: unknown, done: (response: { externalId: string }) => void) {
    if (this.pending()) return;
    this.pending.set(true);
    this.error.set('');
    this.http
      .post<{ externalId: string }>(this.config.endpoint(path), payload, this.options())
      .pipe(timeout(15000))
      .subscribe({
        next: (r) => {
          this.pending.set(false);
          done(r);
        },
        error: () => {
          this.pending.set(false);
          this.error.set(
            'Operação não confirmada. Campos preservados: confira o histórico e valores antes de repetir.',
          );
        },
      });
  }
  createAccount() {
    if (this.accountForm.invalid) {
      this.error.set('Informe nome, saldo inicial explícito e data.');
      return;
    }
    const v = this.accountForm.getRawValue();
    this.send(
      'finance/accounts',
      { ...v, accountKey: this.accountKey, openedAt: v.openedAt + 'T00:00:00Z' },
      () => {
        this.accountKey = crypto.randomUUID();
        this.accountForm.markAsPristine();
        this.success.set('Conta registrada, sem movimentar dinheiro.');
        this.loadAccounts();
      },
    );
  }
  createEntry() {
    if (this.entryForm.invalid) {
      this.error.set('Informe descrição, valor, vencimento e fonte.');
      return;
    }
    const v = this.entryForm.getRawValue();
    this.send(
      'finance/entries',
      { ...v, entryKey: this.entryKey, dueAt: v.dueAt + 'T00:00:00Z' },
      (r) => {
        this.entryKey = crypto.randomUUID();
        this.entryForm.markAsPristine();
        void this.router.navigate(['/admin/finance/entries', r.externalId]);
      },
    );
  }
  settle() {
    const entry = this.detail()?.entry;
    if (!entry || this.settlementForm.invalid) {
      this.error.set('Informe conta, valor, data e comprovante.');
      return;
    }
    const v = this.settlementForm.getRawValue();
    this.send(
      'finance/settlements',
      {
        ...v,
        settlementKey: this.settlementKey,
        entryExternalId: entry.externalId,
        revision: entry.revision,
        paidAt: v.paidAt + 'T00:00:00Z',
      },
      () => {
        this.settlementKey = crypto.randomUUID();
        this.settlementForm.markAsPristine();
        this.reloadDetail();
        this.success.set('Baixa registrada no histórico; nenhum pagamento bancário foi executado.');
      },
    );
  }
  transfer() {
    if (this.transferForm.invalid) {
      this.error.set('Informe origem, destino, valor, data e fonte.');
      return;
    }
    const v = this.transferForm.getRawValue();
    this.send(
      'finance/transfers',
      { ...v, transferKey: this.transferKey, transferredAt: v.transferredAt + 'T00:00:00Z' },
      () => {
        this.transferKey = crypto.randomUUID();
        this.transferForm.markAsPristine();
        this.success.set(
          'Transferência registrada, sem movimentação bancária e sem gerar receita ou despesa.',
        );
      },
    );
  }
  cancel() {
    const entry = this.detail()?.entry;
    if (!entry || this.cancelForm.invalid || this.pending()) return;
    this.pending.set(true);
    this.http
      .patch(
        this.config.endpoint('finance/entries/' + entry.externalId),
        {
          revision: entry.revision,
          cancellationReason: this.cancelForm.controls.cancellationReason.value,
        },
        this.options(),
      )
      .pipe(timeout(15000))
      .subscribe({
        next: () => {
          this.pending.set(false);
          this.cancelForm.markAsPristine();
          this.reloadDetail();
        },
        error: () => {
          this.pending.set(false);
          this.error.set(
            'Cancelamento não confirmado. Estorne baixas e confira se o registro mudou.',
          );
        },
      });
  }
  private reloadDetail() {
    this.http
      .get<Detail>(this.config.endpoint('finance/entries/' + this.id()), this.options())
      .pipe(timeout(15000))
      .subscribe({
        next: (r) => this.detail.set(r),
        error: () =>
          this.error.set('Atualização não confirmada. Reabra o detalhe antes de outra baixa.'),
      });
  }
  label(value: string) {
    return (
      (
        {
          payable: 'A pagar',
          receivable: 'A receber',
          open: 'Em aberto',
          partial: 'Parcial',
          settled: 'Quitado',
          cancelled: 'Cancelado',
        } as Record<string, string>
      )[value] || value
    );
  }
}
