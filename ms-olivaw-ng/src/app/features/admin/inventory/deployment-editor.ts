import { Component, DestroyRef, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { AutenticacaoStore } from '../../../core/auth/autenticacao.store';
import { ServicesApi } from '../services/services-api';
import { InventoryLinks } from './inventory-links';
import {
  InventoryApi,
  environments,
  deploymentStatuses,
  inventoryError,
  type DeploymentDetail,
} from './inventory-api';
@Component({
  selector: 'app-deployment-editor',
  imports: [DatePipe, RouterLink, ReactiveFormsModule, InventoryLinks],
  templateUrl: './deployment-editor.html',
  styleUrl: './inventory.scss',
})
export class DeploymentEditor {
  private readonly api = inject(InventoryApi);
  private readonly services = inject(ServicesApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroy = inject(DestroyRef);
  private readonly auth = inject(AutenticacaoStore);
  readonly id = this.route.snapshot.paramMap.get('id');
  readonly service = signal(this.route.snapshot.queryParamMap.get('service') ?? '');
  readonly serviceName = signal('');
  readonly serviceReady = signal(false);
  readonly data = signal<DeploymentDetail | null>(null);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly conflict = signal(false);
  readonly error = signal('');
  readonly success = signal('');
  readonly environmentOptions = Object.entries(environments);
  readonly statusOptions = Object.entries(deploymentStatuses);
  readonly environments = environments;
  readonly statuses = deploymentStatuses;
  readonly form = new FormGroup({
    environment: new FormControl('production', { nonNullable: true }),
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/\S/), Validators.maxLength(120)],
    }),
    location: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(200)] }),
    endpoint: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(300)] }),
    status: new FormControl('planned', { nonNullable: true }),
  });
  constructor() {
    this.load();
  }
  load() {
    this.error.set('');
    this.conflict.set(false);
    if (!this.id) {
      if (!this.service()) {
        this.error.set('Abra um serviço e selecione Cadastrar ambiente.');
        return;
      }
      this.loading.set(true);
      this.services
        .detail(this.service())
        .pipe(
          takeUntilDestroyed(this.destroy),
          finalize(() => this.loading.set(false)),
        )
        .subscribe({
          next: (item) => {
            this.serviceName.set(item.name);
            this.serviceReady.set(item.status !== 'closed');
            if (item.status === 'closed') this.error.set('Serviço encerrado.');
          },
          error: (error) => this.error.set(inventoryError(error)),
        });
      return;
    }
    this.loading.set(true);
    this.api
      .deployment(this.id)
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (data) => {
          this.data.set(data);
          this.service.set(data.serviceExternalId);
          this.serviceName.set(data.serviceName);
          this.serviceReady.set(true);
          this.form.reset(data);
          this.form.controls.environment.disable();
          if (data.endedAt) this.form.disable();
        },
        error: (error) => this.error.set(inventoryError(error)),
      });
  }
  canLeave() {
    if (!this.auth.tokenValido()) return true;
    if (this.saving()) return false;
    if (!this.form.dirty) return true;
    return window.confirm('Há alterações não salvas. Deseja sair da ficha?');
  }
  save() {
    if (
      this.saving() ||
      this.loading() ||
      this.conflict() ||
      !this.serviceReady() ||
      this.data()?.endedAt
    )
      return;
    if (this.id && !this.data()) return;
    this.form.markAllAsTouched();
    this.error.set('');
    this.success.set('');
    if (this.form.invalid) {
      this.error.set('Confira nome e limites dos campos.');
      return;
    }
    const { environment, ...input } = this.form.getRawValue();
    if (
      input.status === 'closed' &&
      !window.confirm(
        'Encerrar esta implantação e todos os seus usos de recursos? O histórico será preservado. Para retomar depois, cadastre um novo período.',
      )
    )
      return;
    this.saving.set(true);
    this.form.disable();
    const request = this.id
      ? this.api.updateDeployment(this.id, this.data()!.revision, input)
      : this.api.createDeployment(this.service(), environment, input);
    request
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => {
          this.saving.set(false);
          if (!this.data()?.endedAt) this.form.enable();
          if (this.id) this.form.controls.environment.disable();
        }),
      )
      .subscribe({
        next: (result) => {
          this.form.markAsPristine();
          this.saving.set(false);
          if (this.id) {
            this.success.set('Implantação atualizada.');
            this.load();
          } else
            void this.router.navigate(['/admin/deployments', result.externalId], {
              replaceUrl: true,
            });
        },
        error: (error: { status?: number }) => {
          if (error.status === 409) this.conflict.set(true);
          this.error.set(inventoryError(error));
        },
      });
  }
}
