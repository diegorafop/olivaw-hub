import {
  planOfferings,
  creationOfferTerms,
  improvementPointRules,
  improvementAllowanceLabel,
} from '../../public/plans/plan-offerings';
import { PlanCostSheet, type CostSheetResult } from './plan-cost-sheet';
import { Component, computed, inject, signal, effect, untracked, viewChild } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin, timeout } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR } from '../../../core/http/http-context';
import { toSignal } from '@angular/core/rxjs-interop';
interface Version {
  externalId: string;
  planKey: string;
  version: number;
  monthlyPrice: number;
  estimatedMonthlyCost: number | null;
  suggestedMonthlyPrice: number | null;
  effectiveAt: string;
  conditions: string;
  storageLimitGiB: number | null;
  memoryLimitMiB: number | null;
  cpuLimit: number | null;
  planStudyExternalId: string | null;
}
interface Versions {
  items: Version[];
  current: Version | null;
  nextBeforeId: number | null;
}
interface Study {
  costSheetData?: string;
  externalId: string;
  name: string;
  missingCosts: number;
  supportHours: number;
  hourlyCost: number;
  reservePercent: number;
  marginPercent: number;
  costs: { name: string; monthlyCost: number | null; allocationPercent: number; source: string }[];
}
interface CustomerCondition {
  externalId: string;
  name: string;
  monthlyPrice: number;
  version: number | null;
  startsAt: string;
  approvalReference: string;
}
interface CustomerConditions {
  items: CustomerCondition[];
  current: CustomerCondition | null;
  nextBeforeId: number | null;
}
@Component({
  selector: 'app-plan-catalog',
  imports: [CurrencyPipe, DatePipe, RouterLink, ReactiveFormsModule, PlanCostSheet],
  templateUrl: './plan-catalog.html',
  styleUrls: ['../inventory/inventory.scss', './plan-catalog.scss'],
})
export class PlanCatalog {
  private readonly params = toSignal(inject(ActivatedRoute).paramMap);
  readonly plans = [
    {
      id: 'bronze',
      name: 'Bronze',
      tag: 'Seu site no ar',
      proposed: 49,
      space: '2 GB',
      domain: 'Endereço Olivaw',
      app: 'Site estático, sem API ou banco do negócio',
      users: 'Conteúdo atualizado pela Olivaw sob orçamento',
      improvements: improvementAllowanceLabel('bronze') + ' para ajustes simples de conteúdo',
      resources: 'Hipótese de teste: até 0,10 vCPU e 64 MiB para o servidor estático',
      setup:
        'Criação e implantação incluídas na oferta Bronze Essencial, com mais de 12 meses de assinatura',
    },
    {
      id: 'prata',
      name: 'Prata',
      tag: 'Conteúdo nas suas mãos',
      proposed: null,
      space: '5 GB',
      domain: 'Domínio próprio permitido',
      app: 'Site com API e PostgreSQL para conteúdo',
      users: '2 contas administrativas para as funcionalidades contratadas',
      improvements: improvementAllowanceLabel('prata'),
      resources: 'CPU e memória da API serão dimensionadas por medição; não há reserva aprovada',
      setup:
        'Criação e implantação incluídas na oferta Prata Essencial ou Intermediário, com mais de 12 meses de assinatura',
    },
    {
      id: 'ouro',
      name: 'Ouro',
      tag: 'Gestão do seu negócio',
      proposed: null,
      space: '5 GiB',
      domain: 'Domínio próprio permitido',
      app: 'Sistema de gestão com API e PostgreSQL',
      users: 'Até 10 contas com perfis, em estudo',
      improvements: improvementAllowanceLabel('ouro'),
      resources: 'Capacidade depende dos módulos, usuários e carga medida',
      setup: 'Desenvolvimento inicial contratado separadamente',
    },
    {
      id: 'diamante',
      name: 'Diamante',
      tag: 'Contrato sob medida',
      proposed: null,
      space: 'Sob contrato',
      domain: 'Domínio próprio permitido',
      app: 'Integrações, capacidade e operação personalizadas',
      users: 'Usuários e permissões sob contrato',
      improvements: improvementAllowanceLabel('diamante'),
      resources: 'Infraestrutura compartilhada ou individual conforme contrato',
      setup: 'Implantação e mensalidade sob proposta',
    },
  ];
  readonly creationOfferTerms = creationOfferTerms;
  readonly publicOffer = computed(() => planOfferings.find((p) => p.id === this.selected()?.id));
  readonly selected = computed(() =>
    this.plans.find((plan) => plan.id === this.params()?.get('planId')),
  );
  readonly isDetail = computed(() => !!this.params()?.get('planId'));

  private readonly http = inject(HttpClient);
  private readonly config = inject(RuntimeConfig);
  private readonly fb = inject(FormBuilder);
  readonly versions = signal<Record<string, Versions>>({});
  readonly error = signal('');
  readonly success = signal('');
  readonly pending = signal(false);
  readonly loading = signal(false);
  readonly linkedStudy = signal<Study | null>(null);
  readonly selectedVersion = computed(
    () => this.versions()[this.selected()?.id || '']?.current ?? null,
  );
  readonly selectedHistory = computed(
    () => this.versions()[this.selected()?.id || '']?.items ?? [],
  );
  readonly costForm = this.fb.group({
    monthlyPrice: [null as number | null, Validators.required],
    storageLimitGiB: [null as number | null],
    memoryLimitMiB: [null as number | null],
    cpuLimit: [null as number | null],
    conditions: ['', Validators.required],
    approvalReference: ['', Validators.required],
    effectiveAt: [new Date().toISOString().slice(0, 7), Validators.required],
    planStudyExternalId: [''],
  });
  readonly assignmentForm = this.fb.group({
    customerExternalId: ['', Validators.required],
    planVersionExternalId: ['', Validators.required],
    month: [new Date().toISOString().slice(0, 7), Validators.required],
    agreedMonthlyPrice: [null as number | null],
    approvalReference: ['', Validators.required],
  });
  readonly customers = signal<{ externalId: string; business: string }[]>([]);
  readonly studies = signal<{ externalId: string; name: string }[]>([]);
  readonly customerPage = signal<number | null>(1);
  readonly studyCursor = signal<number | null>(0);
  readonly choicesLoading = signal(false);
  moreChoices() {
    if (this.choicesLoading()) return;
    this.choicesLoading.set(true);
    const page = this.customerPage();
    const cursor = this.studyCursor();
    const tasks = [];
    if (page !== null)
      tasks.push(
        this.http
          .get<{
            items: { externalId: string; business: string }[];
            total: number;
            pageSize: number;
          }>(this.config.endpoint('customers'), {
            ...this.options(),
            params: new HttpParams().set('page', page).set('pageSize', 20),
          })
          .pipe(timeout(15000))
          .subscribe({
            next: (r) => {
              this.customers.update((rows) => [...rows, ...r.items]);
              this.customerPage.set(page * 20 < r.total ? page + 1 : null);
            },
            error: () => this.error.set('Não foi possível consultar clientes.'),
          }),
      );
    if (cursor !== null)
      tasks.push(
        this.http
          .get<{ items: { externalId: string; name: string }[]; nextBeforeId: number | null }>(
            this.config.endpoint('plans/studies'),
            { ...this.options(), params: new HttpParams().set('beforeId', cursor) },
          )
          .pipe(timeout(15000))
          .subscribe({
            next: (r) => {
              this.studies.update((rows) => [...rows, ...r.items]);
              this.studyCursor.set(r.nextBeforeId ?? null);
            },
            error: () => this.error.set('Não foi possível consultar os estudos de custo.'),
          }),
      );
    // Duas consultas curtas e independentes; o botão pode tentar novamente em caso de falha.
    let remaining = tasks.length;
    if (!remaining) this.choicesLoading.set(false);
    for (const task of tasks)
      task.add(() => {
        remaining--;
        if (!remaining) this.choicesLoading.set(false);
      });
  }
  olderVersions() {
    const plan = this.selected();
    const cursor = plan ? this.versions()[plan.id]?.nextBeforeId : null;
    if (!plan || !cursor || this.loading()) return;
    this.loading.set(true);
    this.http
      .get<Versions>(this.config.endpoint('plans/versions'), {
        ...this.options(),
        params: new HttpParams().set('planKey', plan.id).set('beforeId', cursor),
      })
      .pipe(timeout(15000))
      .subscribe({
        next: (r) => {
          this.versions.update((rows) => ({
            ...rows,
            [plan.id]: { ...r, items: [...rows[plan.id].items, ...r.items] },
          }));
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Não foi possível consultar versões anteriores.');
          this.loading.set(false);
        },
      });
  }
  reuseCostSheet() {
    const data = this.linkedStudy()?.costSheetData;
    if (data) this.costSheet()?.loadFrom(data);
  }
  onCostSheetSaved(result: CostSheetResult) {
    this.costForm.controls.planStudyExternalId.setValue(result.externalId);
    this.studies.update((rows) => [
      {
        externalId: result.externalId,
        name: (this.selected()?.name || 'Plano') + ' — composição de custos',
      },
      ...rows,
    ]);
    this.success.set(
      'Estudo vinculado ao formulário da próxima versão. Confira preço e condições antes de publicar.',
    );
  }
  useCurrent() {
    const current = this.selectedVersion();
    if (!current) return;
    this.costForm.patchValue({
      monthlyPrice: current.monthlyPrice,
      storageLimitGiB: current.storageLimitGiB,
      memoryLimitMiB: current.memoryLimitMiB,
      cpuLimit: current.cpuLimit,
      conditions: current.conditions,
      planStudyExternalId: current.planStudyExternalId || '',
    });
  }
  private readonly costSheet = viewChild(PlanCostSheet);
  canLeave() {
    if (this.pending() || this.costSheet()?.pending()) return false;
    return (
      !(this.costForm.dirty || this.assignmentForm.dirty || this.costSheet()?.form.dirty) ||
      window.confirm('Sair sem salvar as alterações do plano?')
    );
  }
  readonly customerHistory = signal<CustomerConditions | null>(null);
  readonly historyLoading = signal(false);
  private readonly assignmentCustomer = toSignal(
    this.assignmentForm.controls.customerExternalId.valueChanges,
    { initialValue: this.assignmentForm.controls.customerExternalId.value },
  );
  private readonly assignmentMonth = toSignal(this.assignmentForm.controls.month.valueChanges, {
    initialValue: this.assignmentForm.controls.month.value,
  });
  loadCustomerHistory(cursor = 0) {
    const id = this.assignmentCustomer();
    const month = this.assignmentMonth();
    if (!id || !month || !/^\d{4}-(0[1-9]|1[0-2])$/.test(month)) {
      this.customerHistory.set(null);
      return null;
    }
    if (!cursor) this.customerHistory.set(null);
    this.historyLoading.set(true);
    return this.http
      .get<CustomerConditions>(this.config.endpoint('plans/conditions'), {
        ...this.options(),
        params: new HttpParams()
          .set('customerExternalId', id)
          .set('at', month + '-01T00:00:00Z')
          .set('beforeId', cursor),
      })
      .pipe(timeout(15000))
      .subscribe({
        next: (r) => {
          this.customerHistory.update((previous) =>
            cursor ? { ...r, items: [...(previous?.items || []), ...r.items] } : r,
          );
          this.historyLoading.set(false);
        },
        error: () => {
          this.error.set('Não foi possível consultar o histórico deste cliente.');
          this.historyLoading.set(false);
        },
      });
  }
  private publicationKey = crypto.randomUUID();
  constructor() {
    this.load();
    this.moreChoices();
    effect((cleanup) => {
      const request = this.loadCustomerHistory();
      if (request) cleanup(() => request.unsubscribe());
    });
    effect((cleanup) => {
      const id = this.selectedVersion()?.planStudyExternalId;
      this.linkedStudy.set(null);
      if (!id) return;
      const subscription = this.http
        .get<{ items: Study[] }>(this.config.endpoint('plans/studies'), {
          ...this.options(),
          params: new HttpParams().set('externalId', id),
        })
        .pipe(timeout(15000))
        .subscribe({
          next: (r) => this.linkedStudy.set(r.items[0] ?? null),
          error: () =>
            this.error.set('Não foi possível carregar a composição do estudo vinculado.'),
        });
      cleanup(() => subscription.unsubscribe());
    });
    effect(() => {
      const plan = this.selected();
      untracked(() => {
        this.costForm.reset({
          effectiveAt: new Date().toISOString().slice(0, 7),
          conditions: plan
            ? [
                plan.app,
                plan.domain,
                plan.users,
                plan.improvements,
                improvementPointRules,
                plan.setup,
                ...(plan.id === 'prata'
                  ? ['Armazenamento total: 5 GB (5.000.000.000 bytes).']
                  : []),
                ...(['bronze', 'prata'].includes(plan.id) ? [creationOfferTerms] : []),
              ].join('\n')
            : '',
        });
        this.success.set('');
      });
    });
  }
  private options() {
    return { context: new HttpContext().set(LOCAL_ERROR, true) };
  }
  load() {
    this.loading.set(true);
    this.error.set('');
    forkJoin(
      this.plans.map((plan) =>
        this.http.get<Versions>(this.config.endpoint('plans/versions'), {
          ...this.options(),
          params: new HttpParams().set('planKey', plan.id),
        }),
      ),
    )
      .pipe(timeout(15000))
      .subscribe({
        next: (rows) => {
          this.versions.set(Object.fromEntries(rows.map((row, i) => [this.plans[i].id, row])));
          this.loading.set(false);
        },
        error: () => {
          this.error.set(
            'Não foi possível consultar os preços publicados. As propostas permanecem visíveis; confira a publicação da API.',
          );
          this.loading.set(false);
        },
      });
  }
  publish() {
    const plan = this.selected();
    if (!plan || this.pending()) return;
    if (this.costForm.invalid) {
      this.costForm.markAllAsTouched();
      this.error.set('Informe mensalidade, condições, competência e referência da aprovação.');
      return;
    }
    const v = this.costForm.getRawValue();
    if (!/^\d{4}-(0[1-9]|1[0-2])$/.test(v.effectiveAt || '')) {
      this.error.set('Informe uma competência válida.');
      return;
    }
    this.pending.set(true);
    this.error.set('');
    this.http
      .post(
        this.config.endpoint('plans/versions'),
        {
          ...v,
          planKey: plan.id,
          publicationKey: this.publicationKey,
          effectiveAt: v.effectiveAt + '-01T00:00:00Z',
          planStudyExternalId: v.planStudyExternalId || null,
        },
        this.options(),
      )
      .pipe(timeout(15000))
      .subscribe({
        next: () => {
          this.pending.set(false);
          this.publicationKey = crypto.randomUUID();
          this.costForm.markAsPristine();
          this.success.set(
            'Nova versão publicada no catálogo. Contratos existentes foram preservados.',
          );
          this.load();
        },
        error: () => {
          this.pending.set(false);
          this.error.set(
            'Publicação não confirmada. Confira vigência, capacidades, aprovação e histórico antes de repetir.',
          );
        },
      });
  }
  assign() {
    if (this.pending()) return;
    if (this.assignmentForm.invalid) {
      this.assignmentForm.markAllAsTouched();
      this.error.set('Informe cliente, versão, competência e aprovação.');
      return;
    }
    const v = this.assignmentForm.getRawValue();
    if (!/^\d{4}-(0[1-9]|1[0-2])$/.test(v.month || '')) {
      this.error.set('Informe competência válida.');
      return;
    }
    const [year, month] = (v.month || '').split('-').map(Number);
    this.pending.set(true);
    this.error.set('');
    this.http
      .post(
        this.config.endpoint('plans/assignments'),
        {
          customerExternalId: v.customerExternalId,
          planVersionExternalId: v.planVersionExternalId,
          year,
          month,
          agreedMonthlyPrice: v.agreedMonthlyPrice,
          approvalReference: v.approvalReference,
        },
        this.options(),
      )
      .pipe(timeout(15000))
      .subscribe({
        next: () => {
          this.pending.set(false);
          this.assignmentForm.markAsPristine();
          this.success.set(
            'Condição do cliente registrada para a competência. Histórico preservado.',
          );
          this.loadCustomerHistory();
        },
        error: () => {
          this.pending.set(false);
          this.error.set(
            'Vínculo não confirmado. Confira competência, versão, cliente e aprovação antes de repetir.',
          );
        },
      });
  }
}
