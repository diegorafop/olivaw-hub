import { Component, DestroyRef, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, debounceTime, finalize, map, of, switchMap } from 'rxjs';
import { AutenticacaoStore } from '../../../core/auth/autenticacao.store';
import { CustomersApi } from '../customers/customers-api';
import { ServicesApi, serviceStatuses, serviceTypes, type ServiceDetail } from './services-api';
@Component({
  selector: 'app-service-editor',
  imports: [DatePipe, RouterLink, ReactiveFormsModule],
  templateUrl: './service-editor.html',
  styleUrl: './services.scss',
})
export class ServiceEditor {
  private readonly api = inject(ServicesApi);
  private readonly customers = inject(CustomersApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroy = inject(DestroyRef);
  readonly auth = inject(AutenticacaoStore);
  private preparedCustomer = false;
  readonly preparing = signal(false);
  readonly id = this.route.snapshot.paramMap.get('id');
  readonly customer = this.route.snapshot.queryParamMap.get('customer');
  readonly data = signal<ServiceDetail | null>(null);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly conflict = signal(false);
  readonly error = signal('');
  readonly success = signal('');
  readonly chosen = signal<{ externalId: string; business: string }[]>([]);
  readonly search = signal('');
  readonly searchError = signal(false);
  readonly choices = toSignal(
    toObservable(this.search).pipe(
      debounceTime(200),
      switchMap((search) => {
        this.searchError.set(false);
        return this.customers.list(1, search, '').pipe(
          map((result) => result.items),
          catchError(() => {
            this.searchError.set(true);
            return of([]);
          }),
        );
      }),
    ),
    { initialValue: [] },
  );
  readonly statuses = serviceStatuses;
  readonly types = serviceTypes;
  readonly statusOptions = Object.entries(serviceStatuses);
  readonly typeOptions = Object.entries(serviceTypes);
  readonly form = new FormGroup({
    technicalId: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(80),
        Validators.pattern(/^[a-z0-9]+(?:-[a-z0-9]+)*$/),
      ],
    }),
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/\S/), Validators.maxLength(120)],
    }),
    type: new FormControl('api', { nonNullable: true }),
    scope: new FormControl('customer', { nonNullable: true }),
    status: new FormControl('planned', { nonNullable: true }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(1000)],
    }),
    owner: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(120)] }),
  });
  constructor() {
    this.load();
  }
  load() {
    this.error.set('');
    this.conflict.set(false);
    if (!this.id) {
      this.prepareCustomer();
      return;
    }
    this.loading.set(true);
    this.api
      .detail(this.id)
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (data) => {
          this.data.set(data);
          this.form.reset(data);
          this.form.controls.technicalId.disable();
          this.chosen.set(
            data.customers.map((item) => ({
              externalId: item.customerExternalId,
              business: item.business,
            })),
          );
          this.form.markAsPristine();
          this.prepareCustomer();
        },
        error: () =>
          this.error.set('Não foi possível abrir o serviço. Tente novamente antes de editar.'),
      });
  }
  private prepareCustomer() {
    if (!this.customer) return;
    if (this.preparedCustomer) return;
    this.preparedCustomer = true;
    this.preparing.set(true);
    this.customers
      .detail(this.customer)
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.preparing.set(false)),
      )
      .subscribe({
        next: (data) => {
          if (this.data()?.status === 'closed') {
            this.error.set('Reative o serviço antes de preparar um novo vínculo.');
            return;
          }
          this.form.controls.scope.setValue('customer');
          this.add(data);
        },
        error: () =>
          this.error.set(
            'Não foi possível consultar o cliente do vínculo. Volte à ficha do cliente e tente novamente.',
          ),
      });
  }
  add(customer: { externalId: string; business: string }) {
    if (this.chosen().some((item) => item.externalId === customer.externalId)) return;
    if (this.chosen().length >= 100) return;
    this.chosen.update((items) => [...items, customer]);
    this.form.markAsDirty();
  }
  remove(id: string) {
    this.chosen.update((items) => items.filter((item) => item.externalId !== id));
    this.form.markAsDirty();
  }
  canLeave() {
    if (!this.auth.tokenValido()) return true;
    if (this.saving()) return false;
    if (!this.form.dirty) return true;
    return window.confirm('Há alterações não salvas. Deseja sair da ficha?');
  }
  save() {
    if (this.saving() || this.loading() || this.preparing() || this.conflict()) return;
    if (this.id && !this.data()) return;
    this.error.set('');
    this.success.set('');
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.error.set('Confira nome e identificador técnico.');
      return;
    }
    const value = this.form.getRawValue();
    let selected = this.chosen().map((item) => item.externalId);
    if (value.scope === 'internal') selected = [];
    if (value.status === 'closed') selected = [];
    if (value.scope === 'customer' && value.status !== 'closed' && selected.length === 0) {
      this.error.set('Selecione ao menos um cliente.');
      return;
    }
    const current = this.data()?.customers ?? [];
    const removing = current.some((item) => !selected.includes(item.customerExternalId));
    if (
      removing &&
      !window.confirm(
        'Encerrar os vínculos retirados? O histórico será preservado; infraestrutura e cobrança não serão alteradas.',
      )
    )
      return;
    const payload = {
      name: value.name.trim(),
      type: value.type,
      status: value.status,
      scope: value.scope,
      description: value.description.trim(),
      owner: value.owner.trim(),
      customerExternalIds: selected,
    };
    this.saving.set(true);
    this.form.disable();
    const request = this.id
      ? this.api.update(this.id, this.data()!.revision, payload)
      : this.api.create(value.technicalId, payload);
    request
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => {
          this.saving.set(false);
          this.form.enable();
          if (this.id) this.form.controls.technicalId.disable();
        }),
      )
      .subscribe({
        next: (result) => {
          this.form.markAsPristine();
          this.saving.set(false);
          if (this.id) {
            this.success.set('Serviço atualizado.');
            this.load();
          } else
            void this.router.navigate(['/admin/services', result.externalId], { replaceUrl: true });
        },
        error: (error: { status?: number; error?: { detail?: string } }) => {
          if (error.status === 409) this.conflict.set(true);
          this.error.set(
            error.error?.detail ??
              'Não foi possível confirmar o salvamento. Os campos foram mantidos; confira a lista antes de tentar novamente.',
          );
        },
      });
  }
}
