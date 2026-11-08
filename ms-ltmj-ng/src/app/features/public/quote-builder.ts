import { DOCUMENT } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { LTMJ_CONTACT, LTMJ_SERVICES } from './ltmj-content';
@Component({
  selector: 'app-quote-builder',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: './quote-builder.html',
})
export class QuoteBuilder {
  private readonly document = inject(DOCUMENT);
  readonly services = LTMJ_SERVICES;
  readonly contact = LTMJ_CONTACT;
  readonly summary = signal('');
  readonly message = signal('');
  readonly form = new FormGroup({
    occasion: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    guests: new FormControl<number | null>(null, {
      validators: [Validators.min(1), Validators.max(100000), Validators.pattern(/^\d+$/)],
    }),
    date: new FormControl('', { nonNullable: true }),
    city: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/\S/), Validators.maxLength(100)],
    }),
    services: new FormControl<string[]>([], { nonNullable: true }),
    notes: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(1500)] }),
  });
  readonly whatsappHref = computed(() => {
    if (!/^\d{10,15}$/.test(this.contact.whatsappNumber) || !this.summary()) return null;
    return `https://wa.me/${this.contact.whatsappNumber}?text=${encodeURIComponent(this.summary())}`;
  });
  constructor() {
    this.form.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => {
      this.summary.set('');
      this.message.set('');
    });
  }
  toggle(service: string): void {
    const current = this.form.controls.services.value;
    this.form.controls.services.setValue(
      current.includes(service)
        ? current.filter((item) => item !== service)
        : [...current, service],
    );
  }
  prepare(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const draft = this.form.getRawValue();
    const date = draft.date ? draft.date.split('-').reverse().join('/') : 'Ainda estou definindo';
    this.summary.set(
      `Olá, LTMJ! Gostaria de solicitar um orçamento.\n\nOcasião: ${draft.occasion}\nCidade: ${draft.city.trim()}\nData: ${date}\nConvidados: ${draft.guests ?? 'A definir'}\nServiços: ${draft.services.join(', ') || 'Gostaria de orientação'}\nObservações: ${draft.notes.trim() || 'Podemos conversar sobre as opções.'}`,
    );
    this.message.set('Resumo pronto para revisão. Nenhum pedido foi enviado.');
  }
  async copy(): Promise<void> {
    try {
      const clipboard = this.document.defaultView?.navigator.clipboard;
      if (!clipboard) throw new Error('Área de transferência indisponível.');
      await clipboard.writeText(this.summary());
      this.message.set('Resumo copiado! Envie-o à LTMJ pelo canal de atendimento.');
    } catch {
      this.message.set(
        'Não foi possível copiar automaticamente. Selecione o resumo e copie manualmente.',
      );
    }
  }
}
