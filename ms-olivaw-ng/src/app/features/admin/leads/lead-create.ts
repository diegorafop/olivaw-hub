import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { LeadsApi } from './leads-api';
@Component({
  selector: 'app-lead-create',
  imports: [RouterLink, ReactiveFormsModule],
  templateUrl: './lead-create.html',
  styleUrl: './lead-proposal.scss',
})
export class LeadCreate {
  private readonly api = inject(LeadsApi);
  private readonly router = inject(Router);
  private id: string = crypto.randomUUID();
  readonly busy = signal(false);
  readonly error = signal('');
  readonly form = new FormGroup({
    initiative: new FormControl<'outbound' | 'inbound'>('outbound', { nonNullable: true }),
    origin: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(120)],
    }),
    externalReference: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(120)],
    }),
    sourceUrl: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(1000), Validators.pattern(/^https?:\/\/[^\s]+$/)],
    }),
    business: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(120)] }),
    title: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(160)],
    }),
    requirements: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(10000)],
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.email, Validators.maxLength(254)],
    }),
    whatsapp: new FormControl('', {
      nonNullable: true,
      validators: [Validators.pattern(/^[1-9][0-9]9[0-9]{8}$/)],
    }),
  });
  async importFile(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    try {
      if (file.size > 65536) throw new Error();
      const data = JSON.parse(await file.text());
      if (
        !/^[a-f0-9-]{36}$/i.test(data.externalId) ||
        !['inbound', 'outbound'].includes(data.initiative)
      )
        throw new Error();
      const values = this.form.getRawValue();
      for (const key of Object.keys(values) as (keyof typeof values)[]) {
        if (typeof data[key] !== 'string') throw new Error();
      }
      this.id = data.externalId;
      this.form.setValue(
        Object.fromEntries(Object.keys(values).map((key) => [key, data[key]])) as typeof values,
      );
      this.error.set('Arquivo carregado para revisão. Nada foi cadastrado ainda.');
    } catch {
      this.error.set('Arquivo inválido. Use um JSON de cadastro de lead de até 64 KB.');
    }
  }
  async save() {
    if (this.busy()) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.error.set('Confira os campos obrigatórios, o endereço e os contatos preenchidos.');
      return;
    }
    this.busy.set(true);
    this.error.set('');
    try {
      const result = await firstValueFrom(
        this.api.create({ externalId: this.id, ...this.form.getRawValue() }),
      );
      await this.router.navigate(['/admin/leads', result.externalId, 'proposal']);
    } catch (error) {
      this.error.set(
        error instanceof HttpErrorResponse && error.status === 409
          ? 'Este cadastro já existe. Confira a lista antes de cadastrar novamente.'
          : 'Não foi possível cadastrar. Confira se a API foi atualizada e tente novamente.',
      );
    } finally {
      this.busy.set(false);
    }
  }
}
