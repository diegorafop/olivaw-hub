import { Component, computed, inject, input, output, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { HttpClient, HttpContext } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { timeout } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR } from '../../../core/http/http-context';
export interface CostSheetResult {
  externalId: string;
  monthlyCostWithReserve: number;
  suggestedMonthlyPrice: number | null;
  unallocatedMonthlyCost: number;
}
@Component({
  selector: 'app-plan-cost-sheet',
  imports: [CurrencyPipe, ReactiveFormsModule],
  templateUrl: './plan-cost-sheet.html',
  styleUrls: ['../inventory/inventory.scss', './plan-catalog.scss'],
})
export class PlanCostSheet {
  readonly planKey = input.required<string>();
  readonly saved = output<CostSheetResult>();
  private readonly fb = inject(FormBuilder);
  private readonly http = inject(HttpClient);
  private readonly config = inject(RuntimeConfig);
  readonly error = signal('');
  readonly pending = signal(false);
  readonly result = signal<CostSheetResult | null>(null);
  private key = crypto.randomUUID();
  readonly definitions = [
    { key: 'vm', label: 'VM / processamento' },
    { key: 'storage', label: 'Armazenamento' },
    { key: 'network', label: 'Rede e IP público' },
    { key: 'backup', label: 'Backups' },
    { key: 'domain', label: 'Domínio' },
    { key: 'tools', label: 'IA e ferramentas' },
    { key: 'licenses', label: 'Licenças e serviços' },
    { key: 'other', label: 'Outros custos' },
  ];
  readonly lines = this.fb.array(
    this.definitions.map((row) =>
      this.fb.group({
        key: [row.key],
        amount: [
          row.key === 'domain' ? 40 : (null as number | null),
          [Validators.required, Validators.min(0), Validators.max(100000000)],
        ],
        period: [row.key === 'domain' ? 'annual' : 'monthly'],
        sharedClients: [
          null as number | null,
          [
            Validators.required,
            Validators.min(1),
            Validators.max(1000),
            Validators.pattern(/^\d+$/),
          ],
        ],
      }),
    ),
  );
  readonly form = this.fb.group({
    lines: this.lines,
    supportHours: [
      null as number | null,
      [Validators.required, Validators.min(0), Validators.max(1000)],
    ],
    hourlyCost: [
      null as number | null,
      [Validators.required, Validators.min(0), Validators.max(1000000)],
    ],
    reservePercent: [15, [Validators.required, Validators.min(0), Validators.max(100)]],
    marginPercent: [20, [Validators.required, Validators.min(0), Validators.max(99.99)]],
    commercialPercent: [6, [Validators.required, Validators.min(0), Validators.max(99.99)]],
    source: ['', [Validators.required, Validators.maxLength(300)]],
  });
  private readonly changes = toSignal(this.form.valueChanges, { initialValue: this.form.value });
  readonly preview = computed(() => {
    this.changes();
    const v = this.form.getRawValue();
    if (
      this.lines.invalid ||
      ['supportHours', 'hourlyCost', 'reservePercent', 'marginPercent', 'commercialPercent'].some(
        (key) => this.form.get(key)?.invalid,
      ) ||
      Number(v.marginPercent) + Number(v.commercialPercent) >= 100
    )
      return null;
    const money = (n: number) => Math.round((n + Number.EPSILON) * 100) / 100;
    const cost = v.lines.reduce(
      (sum, row) =>
        sum +
        (money(Number(row.amount) / (row.period === 'annual' ? 12 : 1)) *
          money(100 / Number(row.sharedClients))) /
          100,
      0,
    );
    const monthly = money(
      (money(cost) + Number(v.supportHours) * Number(v.hourlyCost)) *
        (1 + Number(v.reservePercent) / 100),
    );
    return {
      monthly,
      suggested: money(
        monthly / (1 - (Number(v.marginPercent) + Number(v.commercialPercent)) / 100),
      ),
    };
  });
  lineMonthly(index: number) {
    this.changes();
    const row = this.lines.at(index).getRawValue();
    if (row.amount === null || !row.sharedClients) return null;
    return (
      ((Math.round(
        (Number(row.amount) / (row.period === 'annual' ? 12 : 1) + Number.EPSILON) * 100,
      ) /
        100) *
        (Math.round((100 / row.sharedClients) * 100) / 100)) /
      100
    );
  }
  loadFrom(raw: string) {
    try {
      const value = JSON.parse(raw);
      if (
        !value ||
        value.planKey !== this.planKey() ||
        !Array.isArray(value.lines) ||
        value.lines.length !== 8
      )
        throw new Error();
      for (let index = 0; index < this.definitions.length; index++) {
        const line = value.lines.find(
          (row: { key: string }) => row.key === this.definitions[index].key,
        );
        if (
          !line ||
          typeof line.amount !== 'number' ||
          typeof line.sharedClients !== 'number' ||
          !['monthly', 'annual'].includes(line.period)
        )
          throw new Error();
        this.lines
          .at(index)
          .patchValue({
            amount: line.amount,
            sharedClients: line.sharedClients,
            period: line.period,
          });
      }
      for (const key of [
        'supportHours',
        'hourlyCost',
        'reservePercent',
        'marginPercent',
        'commercialPercent',
      ]) {
        if (typeof value[key] !== 'number') throw new Error();
        this.form.get(key)?.setValue(value[key]);
      }
      this.form.controls.source.setValue(typeof value.source === 'string' ? value.source : '');
      this.form.markAsDirty();
      this.error.set('');
    } catch {
      this.error.set(
        'Não foi possível recuperar as bases deste estudo. O estudo original foi preservado.',
      );
    }
  }
  save() {
    if (this.pending()) return;
    if (!this.preview() || this.form.invalid) {
      this.form.markAllAsTouched();
      this.error.set(
        'Preencha todos os custos e o rateio. Use zero para custo confirmado inexistente; margem e despesas comerciais devem somar menos de 100%.',
      );
      return;
    }
    this.pending.set(true);
    this.error.set('');
    const v = this.form.getRawValue();
    this.http
      .post<CostSheetResult>(
        this.config.endpoint('plans/cost-sheets'),
        {
          ...v,
          scenarioKey: this.key,
          planKey: this.planKey(),
          includedFeatures:
            'Estudo de custo do plano ' +
            this.planKey() +
            '. Condições comerciais e capacidades aprovadas são definidas na versão do catálogo.',
        },
        { context: new HttpContext().set(LOCAL_ERROR, true) },
      )
      .pipe(timeout(15000))
      .subscribe({
        next: (r) => {
          this.pending.set(false);
          this.result.set(r);
          this.key = crypto.randomUUID();
          this.form.markAsPristine();
          this.saved.emit(r);
        },
        error: () => {
          this.pending.set(false);
          this.error.set(
            'Estudo não confirmado. Campos preservados; confira a publicação da API e o histórico antes de repetir.',
          );
        },
      });
  }
}
