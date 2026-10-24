import { AutenticacaoStore } from '../../../core/auth/autenticacao.store';
import { RouterLink } from '@angular/router';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR, SKIP_LOADING } from '../../../core/http/http-context';
import { inventoryError } from '../inventory/inventory-api';
interface Cost {
  externalId: string;
  name: string;
  monthlyCost: number | null;
  source: string;
  effectiveAt: string;
}
interface Estimate {
  knownMonthlyCost: number;
  unallocatedMonthlyCost: number;
  suggestedMonthlyPrice: number | null;
  missingCosts: number;
}
interface Study extends Estimate {
  supportHours: number;
  hourlyCost: number;
  reservePercent: number;
  marginPercent: number;
  externalId: string;
  name: string;
  includedFeatures: string;
  storageLimitGB: number | null;
  memoryLimitMiB: number | null;
  createdAt: string;
  costs: (Omit<Cost, 'externalId'> & {
    costReferenceExternalId: string;
    allocationPercent: number;
  })[];
}
@Component({
  selector: 'app-plans-page',
  imports: [RouterLink, ReactiveFormsModule, CurrencyPipe, DatePipe],
  templateUrl: './plans-page.html',
  styleUrl: '../inventory/inventory.scss',
})
export class PlansPage {
  private readonly auth = inject(AutenticacaoStore);
  private readonly http = inject(HttpClient);
  private readonly runtime = inject(RuntimeConfig);
  private readonly destroy = inject(DestroyRef);
  private readonly options = {
    context: new HttpContext().set(LOCAL_ERROR, true).set(SKIP_LOADING, true),
  };
  readonly costs = signal<Cost[]>([]);
  readonly studies = signal<Study[]>([]);
  readonly nextCost = signal<number | null>(null);
  readonly nextStudy = signal<number | null>(null);
  readonly selected = signal<Record<string, number>>({});
  readonly selectedCosts = signal<Cost[]>([]);
  readonly result = signal<Estimate | null>(null);
  readonly pending = signal(false);
  readonly error = signal('');
  readonly success = signal('');
  readonly costForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(120)],
    }),
    monthlyCost: new FormControl<number | null>(null, [
      Validators.min(0),
      Validators.max(1000000000),
    ]),
    source: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(300)],
    }),
  });
  readonly studyForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(120)],
    }),
    includedFeatures: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(3000)],
    }),
    storageLimitGB: new FormControl<number | null>(null, [
      Validators.min(0),
      Validators.max(1000000000),
    ]),
    memoryLimitMiB: new FormControl<number | null>(null, [
      Validators.min(0),
      Validators.max(1000000000),
    ]),
    supportHours: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.min(0), Validators.max(1000)],
    }),
    hourlyCost: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.min(0), Validators.max(1000000)],
    }),
    reservePercent: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.min(0), Validators.max(100)],
    }),
    marginPercent: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.min(0), Validators.max(99.99)],
    }),
  });
  constructor() {
    this.loadCosts();
    this.loadStudies();
    this.studyForm.valueChanges
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe(() => this.result.set(null));
  }
  canLeave() {
    if (!this.auth.tokenValido()) return true;
    if (this.pending()) return false;
    if (!this.costForm.dirty && !this.studyForm.dirty) return true;
    return window.confirm('Há alterações não salvas. Deseja sair?');
  }
  loadCosts(beforeId = 0) {
    this.http
      .get<{ items: Cost[]; nextBeforeId: number | null }>(this.runtime.endpoint('plans/costs'), {
        ...this.options,
        params: new HttpParams().set('beforeId', beforeId),
      })
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (data) => {
          this.costs.set(data.items);
          this.nextCost.set(data.nextBeforeId);
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  loadStudies(beforeId = 0) {
    this.http
      .get<{ items: Study[]; nextBeforeId: number | null }>(
        this.runtime.endpoint('plans/studies'),
        { ...this.options, params: new HttpParams().set('beforeId', beforeId) },
      )
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: (data) => {
          this.studies.set(data.items);
          this.nextStudy.set(data.nextBeforeId);
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  private setBusy(value: boolean) {
    this.pending.set(value);
    if (value) {
      this.costForm.disable({ emitEvent: false });
      this.studyForm.disable({ emitEvent: false });
      return;
    }
    this.costForm.enable({ emitEvent: false });
    this.studyForm.enable({ emitEvent: false });
  }
  selectCost(cost: Cost, checked: boolean) {
    if (this.pending()) return;
    const selected = { ...this.selected() };
    if (checked) {
      selected[cost.externalId] = 0;
      this.selectedCosts.update((items) => [
        ...items.filter((item) => item.externalId !== cost.externalId),
        cost,
      ]);
    } else {
      delete selected[cost.externalId];
      this.selectedCosts.update((items) =>
        items.filter((item) => item.externalId !== cost.externalId),
      );
    }
    this.selected.set(selected);
    this.result.set(null);
    this.studyForm.markAsDirty();
  }
  useStudy(study: Study) {
    if (this.studyForm.dirty && !window.confirm('Substituir o estudo ainda não salvo?')) return;
    this.studyForm.reset({
      name: study.name + ' — novo estudo',
      includedFeatures: study.includedFeatures,
      storageLimitGB: study.storageLimitGB,
      memoryLimitMiB: study.memoryLimitMiB,
      supportHours: study.supportHours,
      hourlyCost: study.hourlyCost,
      reservePercent: study.reservePercent,
      marginPercent: study.marginPercent,
    });
    this.selected.set(
      Object.fromEntries(
        study.costs.map((cost) => [cost.costReferenceExternalId, cost.allocationPercent]),
      ),
    );
    this.selectedCosts.set(
      study.costs.map((cost) => ({ ...cost, externalId: cost.costReferenceExternalId })),
    );
    this.studyForm.markAsDirty();
    this.result.set(null);
    this.success.set(
      'Cenário carregado como base. Ajuste o novo estudo; a versão anterior será preservada.',
    );
  }
  allocation(id: string, raw: string) {
    if (this.pending()) return;
    this.selected.update((selected) => ({ ...selected, [id]: Number(raw) }));
    this.result.set(null);
    this.studyForm.markAsDirty();
  }
  saveCost() {
    if (this.pending()) return;
    this.costForm.markAllAsTouched();
    if (this.costForm.invalid) {
      this.error.set('Confira o nome, o custo e a fonte.');
      return;
    }
    this.setBusy(true);
    this.error.set('');
    this.http
      .post(
        this.runtime.endpoint('plans/costs'),
        { ...this.costForm.getRawValue(), effectiveAt: new Date().toISOString() },
        this.options,
      )
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.setBusy(false)),
      )
      .subscribe({
        next: () => {
          this.costForm.reset();
          this.costForm.markAsPristine();
          this.success.set('Versão de custo registrada.');
          this.loadCosts();
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
  calculate(save = false) {
    if (this.pending()) return;
    this.error.set('');
    this.success.set('');
    this.studyForm.markAllAsTouched();
    if (this.studyForm.invalid || Object.keys(this.selected()).length === 0) {
      this.error.set('Identifique o estudo, as funcionalidades e selecione as origens de custo.');
      return;
    }
    const lines = Object.entries(this.selected()).map(
      ([costReferenceExternalId, allocationPercent]) => ({
        costReferenceExternalId,
        allocationPercent,
      }),
    );
    const body = { ...this.studyForm.getRawValue(), lines };
    this.setBusy(true);
    const endpoint = save ? 'plans/studies' : 'plans/estimate';
    this.http
      .post<Estimate>(this.runtime.endpoint(endpoint), body, this.options)
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.setBusy(false)),
      )
      .subscribe({
        next: (data) => {
          if (save) {
            this.studyForm.markAsPristine();
            this.success.set('Cenário salvo como estudo; nenhum contrato foi alterado.');
            this.loadStudies();
          } else this.result.set(data);
        },
        error: (e) => {
          this.result.set(null);
          this.error.set(inventoryError(e));
        },
      });
  }
}
