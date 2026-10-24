import { Component, DestroyRef, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { AutenticacaoStore } from '../../../core/auth/autenticacao.store';
import { InventoryLinks } from './inventory-links';
import {
  InventoryApi,
  resourceTypes,
  resourceStatuses,
  inventoryError,
  type ResourceDetail,
} from './inventory-api';
@Component({
  selector: 'app-resource-editor',
  imports: [DatePipe, RouterLink, ReactiveFormsModule, InventoryLinks],
  templateUrl: './resource-editor.html',
  styleUrl: './inventory.scss',
})
export class ResourceEditor {
  private readonly api = inject(InventoryApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroy = inject(DestroyRef);
  private readonly auth = inject(AutenticacaoStore);
  readonly id = this.route.snapshot.paramMap.get('id');
  readonly customer = this.route.snapshot.queryParamMap.get('customer') ?? '';
  readonly data = signal<ResourceDetail | null>(null);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly conflict = signal(false);
  readonly error = signal('');
  readonly success = signal('');
  readonly typeOptions = Object.entries(resourceTypes);
  readonly statusOptions = Object.entries(resourceStatuses);
  readonly statuses = resourceStatuses;
  readonly form = new FormGroup({
    technicalId: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.pattern(/\S/),
        Validators.maxLength(80),
        Validators.pattern(/^[a-z0-9]+(?:-[a-z0-9]+)*$/),
      ],
    }),
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/\S/), Validators.maxLength(120)],
    }),
    type: new FormControl('compute', { nonNullable: true, validators: [Validators.maxLength(30)] }),
    purpose: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(1000)] }),
    provider: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(80)] }),
    accountReference: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(120)],
    }),
    region: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(80)] }),
    providerResourceId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(180)],
    }),
    owner: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(120)] }),
    payer: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(120)] }),
    operator: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(120)] }),
    scope: new FormControl('platform', {
      nonNullable: true,
      validators: [Validators.maxLength(20)],
    }),
    status: new FormControl('reported', {
      nonNullable: true,
      validators: [Validators.maxLength(20)],
    }),
    source: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(500)] }),
    monitoring: new FormControl('none', {
      nonNullable: true,
      validators: [Validators.maxLength(20)],
    }),
    referenceUrl: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(300)],
    }),
    cpuCapacity: new FormControl<number | null>(null, [
      Validators.min(0),
      Validators.max(1000000000),
    ]),
    memoryCapacityGiB: new FormControl<number | null>(null, [
      Validators.min(0),
      Validators.max(1000000000),
    ]),
    technology: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(80)] }),
    technologyVersion: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(40)],
    }),
    logicalName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(120)],
    }),
    renewalAt: new FormControl('', { nonNullable: true }),
    certificateExpiresAt: new FormControl('', { nonNullable: true }),
    backupIntervalHours: new FormControl<number | null>(null, [
      Validators.min(1),
      Validators.max(87600),
      Validators.pattern(/^\d+$/),
    ]),
    backupRetentionDays: new FormControl<number | null>(null, [
      Validators.min(1),
      Validators.max(36500),
      Validators.pattern(/^\d+$/),
    ]),
    storageCapacityGiB: new FormControl<number | null>(null, [
      Validators.min(0),
      Validators.max(1000000000),
    ]),
  });
  readonly resourceType = toSignal(this.form.controls.type.valueChanges, {
    initialValue: 'compute',
  });
  supportsTechnology() {
    return [
      'compute',
      'database-instance',
      'database',
      'message-broker',
      'cache-instance',
      'proxy',
      'observability',
      'registry',
      'external-service',
    ].includes(this.resourceType());
  }
  supportsLogicalName() {
    return ['database', 'message-namespace', 'cache-namespace', 'subdomain', 'dns-record'].includes(
      this.resourceType(),
    );
  }
  changeTypeDetails() {
    if (!this.supportsTechnology()) this.form.patchValue({ technology: '', technologyVersion: '' });
    if (!this.supportsLogicalName()) this.form.controls.logicalName.setValue('');
    if (this.resourceType() !== 'domain') this.form.controls.renewalAt.setValue('');
    if (this.resourceType() !== 'certificate') this.form.controls.certificateExpiresAt.setValue('');
    if (this.resourceType() !== 'backup')
      this.form.patchValue({ backupIntervalHours: null, backupRetentionDays: null });
  }
  constructor() {
    this.load();
  }
  load() {
    if (!this.id) return;
    this.loading.set(true);
    this.error.set('');
    this.conflict.set(false);
    this.api
      .resource(this.id)
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (data) => {
          this.data.set(data);
          this.form.reset({
            ...data,
            technology: data.technology ?? '',
            technologyVersion: data.technologyVersion ?? '',
            logicalName: data.logicalName ?? '',
            renewalAt: data.renewalAt?.slice(0, 10) ?? '',
            certificateExpiresAt: data.certificateExpiresAt?.slice(0, 10) ?? '',
            backupIntervalHours: data.backupIntervalHours ?? null,
            backupRetentionDays: data.backupRetentionDays ?? null,
          });
          this.lockIdentity();
        },
        error: (error) => this.error.set(inventoryError(error)),
      });
  }
  private lockIdentity() {
    if (this.id) this.form.controls.technicalId.disable();
    if (this.data()?.providerIdentityLocked) {
      this.form.controls.provider.disable();
      this.form.controls.accountReference.disable();
      this.form.controls.region.disable();
      this.form.controls.providerResourceId.disable();
      this.form.controls.type.disable();
    }
  }
  canLeave() {
    if (!this.auth.tokenValido()) return true;
    if (this.saving()) return false;
    if (!this.form.dirty) return true;
    return window.confirm('Há alterações não salvas. Deseja sair da ficha?');
  }
  save() {
    if (this.saving() || this.loading() || this.conflict()) return;
    if (this.id && !this.data()) return;
    this.form.markAllAsTouched();
    this.error.set('');
    this.success.set('');
    if (this.form.invalid) {
      this.error.set('Confira nome, identificador e os limites dos campos.');
      return;
    }
    const { technicalId, ...values } = this.form.getRawValue();
    const input = {
      ...values,
      renewalAt: values.renewalAt ? values.renewalAt + 'T00:00:00Z' : null,
      certificateExpiresAt: values.certificateExpiresAt
        ? values.certificateExpiresAt + 'T00:00:00Z'
        : null,
    };
    if (input.status === 'verified' && !input.source.trim()) {
      this.error.set('Informe a fonte da conferência.');
      return;
    }
    if (
      input.status === 'retired' &&
      this.data()?.status !== 'retired' &&
      !window.confirm(
        'Desativar este recurso? É necessário encerrar os vínculos vigentes antes. O histórico será preservado.',
      )
    )
      return;
    this.saving.set(true);
    this.form.disable();
    const request = this.id
      ? this.api.updateResource(this.id, this.data()!.revision, input)
      : this.api.createResource(technicalId, input);
    request
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => {
          this.saving.set(false);
          this.form.enable();
          this.lockIdentity();
        }),
      )
      .subscribe({
        next: (result) => {
          this.form.markAsPristine();
          this.saving.set(false);
          if (this.id) {
            this.success.set('Recurso atualizado.');
            this.load();
          } else
            void this.router.navigate(['/admin/resources', result.externalId], {
              replaceUrl: true,
            });
        },
        error: (error: { status?: number }) => {
          if (error.status === 409) this.conflict.set(true);
          this.error.set(inventoryError(error));
        },
      });
  }
  refreshContext() {
    if (!this.form.dirty) this.load();
  }
}
