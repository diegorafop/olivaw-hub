import { cnpjDigits, formatCnpj } from './customer-document';
import { CustomerResources } from '../inventory/customer-resources';
import { CustomerServices } from '../services/customer-services';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormArray, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { CustomersApi, customerStatuses, statusLabel, type CustomerDetail } from './customers-api';
import { LeadsApi } from '../leads/leads-api';
import { contactValid, formatWhatsapp, whatsappDigits } from '../../public/contact-details';
function contactForm() {
  return new FormGroup(
    {
      name: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.pattern(/\S/), Validators.maxLength(120)],
      }),
      role: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(120)] }),
      email: new FormControl('', { nonNullable: true }),
      whatsapp: new FormControl('', { nonNullable: true }),
    },
    {
      validators: (group) =>
        contactValid(group.value.email ?? '', group.value.whatsapp ?? '')
          ? null
          : { contact: true },
    },
  );
}
@Component({
  selector: 'app-customer-editor',
  imports: [DatePipe, RouterLink, ReactiveFormsModule, CustomerServices, CustomerResources],
  templateUrl: './customer-editor.html',
  styleUrls: ['./customers.scss', './customer-layout.scss', './customer-details.scss'],
})
export class CustomerEditor {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(CustomersApi);
  private readonly leads = inject(LeadsApi);
  private readonly destroy = inject(DestroyRef);
  readonly id = this.route.snapshot.paramMap.get('id');
  readonly source = this.id ? null : this.route.snapshot.queryParamMap.get('lead');
  readonly data = signal<CustomerDetail | null>(null);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly attempted = signal(false);
  readonly error = signal('');
  readonly success = signal('');
  readonly existingId = signal<string | null>(null);
  readonly label = statusLabel;
  readonly statuses = customerStatuses;
  readonly form = new FormGroup({
    business: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/\S/), Validators.maxLength(120)],
    }),
    legalName: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(180)] }),
    document: new FormControl('', {
      nonNullable: true,
      validators: [Validators.pattern(/^(?:\d{14}|\d{2}\.\d{3}\.\d{3}\/\d{4}-\d{2})$/)],
    }),
    owner: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(120)] }),
    notes: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(3000)] }),
    status: new FormControl('onboarding', { nonNullable: true }),
    contacts: new FormArray([contactForm()]),
  });
  get contacts() {
    return this.form.controls.contacts;
  }
  constructor() {
    this.load();
  }
  load() {
    this.error.set('');
    if (this.id) {
      this.loading.set(true);
      this.api
        .detail(this.id)
        .pipe(
          takeUntilDestroyed(this.destroy),
          finalize(() => this.loading.set(false)),
        )
        .subscribe({
          next: (data) => this.populate(data),
          error: () =>
            this.error.set('Não foi possível abrir a ficha. Tente novamente antes de editar.'),
        });
    } else if (this.source) {
      this.loading.set(true);
      this.leads
        .detail(this.source)
        .pipe(
          takeUntilDestroyed(this.destroy),
          finalize(() => this.loading.set(false)),
        )
        .subscribe({
          next: (lead) => {
            if (!lead.snapshot) {
              this.error.set(
                'O conteúdo do lead não está disponível. Volte aos leads para conferir.',
              );
              return;
            }
            const draft = lead.snapshot;
            this.form.patchValue({
              business: draft.business,
              notes: draft.requirements,
              contacts: [{ email: draft.email, whatsapp: formatWhatsapp(draft.whatsapp ?? '') }],
            });
            this.form.markAsPristine();
          },
          error: () =>
            this.error.set('Não foi possível consultar a origem. Volte ao lead e tente novamente.'),
        });
    }
  }
  private populate(data: CustomerDetail) {
    this.data.set(data);
    this.contacts.clear();
    for (const contact of data.contacts) {
      const group = contactForm();
      group.patchValue({ ...contact, whatsapp: formatWhatsapp(contact.whatsapp) });
      this.contacts.push(group);
    }
    this.form.patchValue({
      business: data.business,
      legalName: data.legalName,
      document: formatCnpj(data.document ?? ''),
      owner: data.owner,
      notes: data.notes,
      status: data.status,
    });
    this.form.markAsPristine();
    this.attempted.set(false);
  }
  addContact() {
    if (this.contacts.length >= 8) return;
    this.contacts.push(contactForm());
    this.form.markAsDirty();
  }
  removeContact(index: number) {
    if (this.contacts.length <= 1) return;
    this.contacts.removeAt(index);
    this.form.markAsDirty();
  }
  maskDocument(event: Event) {
    const input = event.target as HTMLInputElement;
    const count = input.value
      .slice(0, input.selectionStart ?? input.value.length)
      .replace(/\D/g, '').length;
    input.value = formatCnpj(input.value);
    this.form.controls.document.setValue(input.value);
    let digits = 0,
      caret = 0;
    while (caret < input.value.length && digits < count) {
      if (/\d/.test(input.value[caret])) digits++;
      caret++;
    }
    input.setSelectionRange(caret, caret);
  }
  maskPhone(index: number, event: Event) {
    const input = event.target as HTMLInputElement;
    input.value = formatWhatsapp(input.value);
    this.contacts.at(index).controls.whatsapp.setValue(input.value);
  }
  allowedStatus(value: string) {
    const current = this.data()?.status ?? 'onboarding';
    if (current === value) return true;
    if (current === 'closed') return value === 'onboarding';
    if (current === 'active') return value === 'paused' || value === 'closed';
    return true;
  }
  canLeave(): boolean {
    return (
      !this.saving() &&
      (!this.form.dirty || window.confirm('Você tem alterações não salvas. Deseja sair da ficha?'))
    );
  }
  save() {
    this.attempted.set(true);
    this.error.set('');
    this.success.set('');
    this.existingId.set(null);
    this.form.markAllAsTouched();
    if (this.form.invalid || this.saving() || this.loading()) return;
    const value = this.form.getRawValue();
    if (
      this.data() &&
      value.status !== this.data()?.status &&
      !window.confirm(
        `Alterar a situação para ${this.label(value.status)}? Isso não suspende sistemas nem altera cobranças.`,
      )
    )
      return;
    const input = {
      business: value.business.trim(),
      legalName: value.legalName.trim(),
      document: cnpjDigits(value.document) || null,
      owner: value.owner.trim(),
      notes: value.notes.trim(),
      contacts: value.contacts.map((contact) => ({
        ...contact,
        name: contact.name.trim(),
        role: contact.role.trim(),
        email: contact.email.trim(),
        whatsapp: whatsappDigits(contact.whatsapp),
      })),
    };
    this.saving.set(true);
    this.form.disable({ emitEvent: false });
    const request = this.id
      ? this.api.update(this.id, input, value.status, this.data()?.revision ?? 0)
      : this.api.create(input, this.source);
    request
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => {
          this.saving.set(false);
          this.form.enable({ emitEvent: false });
        }),
      )
      .subscribe({
        next: (result) => {
          this.form.markAsPristine();
          this.saving.set(false);
          if (this.id) {
            this.success.set('Cadastro atualizado.');
            this.load();
          } else
            void this.router.navigate(['/admin/customers', result.externalId], {
              replaceUrl: true,
            });
        },
        error: (error: {
          status?: number;
          error?: { detail?: string; existingExternalId?: string };
        }) => {
          this.error.set(
            error.status === 409
              ? (error.error?.detail ??
                  'Este cadastro mudou. Reabra a ficha e confira antes de salvar.')
              : 'Não foi possível salvar. Confira os campos e tente novamente. Se houve uma interrupção, confira a lista antes de cadastrar de novo.',
          );
          this.existingId.set(error.error?.existingExternalId ?? null);
        },
      });
  }
}
