import { Component, inject, signal } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
@Component({
  selector: 'app-project-contact',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `<form
    [formGroup]="form"
    (ngSubmit)="copy()"
    class="flex flex-col gap-3"
    aria-label="Resumo do projeto"
  >
    <mat-form-field appearance="outline"
      ><mat-label>Seu negócio</mat-label>
      <input
        matInput
        formControlName="business"
        maxlength="120"
        placeholder="Ex.: uma empresa de eventos"
        required
      />
      @if (form.controls.business.invalid) {
        <mat-error>Conte qual é o seu negócio.</mat-error>
      }
    </mat-form-field>
    <mat-form-field appearance="outline"
      ><mat-label>O que você gostaria de simplificar?</mat-label>
      <textarea
        matInput
        formControlName="need"
        rows="4"
        maxlength="2000"
        placeholder="Quero divulgar meus serviços e organizar minha agenda…"
        required
      ></textarea>
      @if (form.controls.need.invalid) {
        <mat-error>Descreva sua ideia em poucas palavras.</mat-error>
      }
    </mat-form-field>
    <button mat-flat-button type="submit">
      Copiar resumo do projeto <span aria-hidden="true">↗</span>
    </button>
    <p class="text-sm leading-relaxed opacity-75">
      Os canais de contato serão divulgados em breve. Este formulário apenas copia seu resumo: nada
      é enviado ou armazenado pelo site.
    </p>
    <p role="status" aria-live="polite" class="text-sm">{{ message() }}</p>
  </form>`,
})
export class ProjectContact {
  private readonly document = inject(DOCUMENT);
  readonly message = signal('');
  readonly form = new FormGroup({
    business: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(120)],
    }),
    need: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(2000)],
    }),
  });
  async copy(): Promise<void> {
    this.message.set('');
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { business, need } = this.form.getRawValue();
    try {
      const clipboard = this.document.defaultView?.navigator.clipboard;
      if (!clipboard) throw new Error('Área de transferência indisponível.');
      await clipboard.writeText(`Meu negócio: ${business}\nO que preciso: ${need}`);
      this.message.set('Resumo copiado. Você pode guardá-lo para nossa conversa.');
    } catch {
      this.message.set(
        'Não foi possível copiar automaticamente. Selecione e copie o texto dos campos.',
      );
    }
  }
}
