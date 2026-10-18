import { Component, DestroyRef, effect, inject, signal, viewChild } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
  FormControl,
  FormGroup,
  FormGroupDirective,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDialog } from '@angular/material/dialog';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { AutenticacaoStore } from '../../core/auth/autenticacao.store';
import { apiErrorMessage } from '../../core/http/api-error';
import { PortfolioService, ManagedEvent, EVENT_CATEGORIES } from '../portfolio/portfolio.service';
import { EventCategory } from '../public/events/event-portfolio.data';
import { PORTFOLIO_SAMPLES } from '../public/events/portfolio-samples.data';
import { DeleteEventDialog } from './delete-event-dialog';

@Component({
  selector: 'app-admin-page',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
  ],
  templateUrl: './admin-page.html',
  styles: `
    :host {
      display: block;
      min-height: 100dvh;
      background: var(--mat-sys-surface);
    }
    button,
    input,
    select {
      min-height: 48px;
    }
    mat-form-field {
      width: 100%;
    }
    .event-card {
      border: 1px solid var(--mat-sys-outline-variant);
      border-radius: 1rem;
      padding: 1.25rem;
    }
    .file-input {
      width: 100%;
      max-width: 100%;
      font: inherit;
      padding-block: 1rem;
    }
    .file-input::file-selector-button {
      min-height: 48px;
      padding: 0.5rem 1rem;
      margin-right: 0.5rem;
    }
  `,
})
export class AdminPage {
  readonly store = inject(AutenticacaoStore);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly api = inject(PortfolioService);
  private readonly dialog = inject(MatDialog);
  private readonly formDirective = viewChild(FormGroupDirective);
  readonly categories = EVENT_CATEGORIES;
  readonly samples = PORTFOLIO_SAMPLES;
  readonly pending = signal(false);
  readonly error = signal('');
  readonly message = signal('');
  readonly events = signal<ManagedEvent[]>([]);
  readonly editingId = signal<string | null>(null);
  readonly page = signal(1);
  readonly selectedPhotos = signal<{ file: File; preview: string }[]>([]);
  readonly form = new FormGroup({
    title: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/\S/), Validators.maxLength(150)],
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/\S/), Validators.maxLength(4000)],
    }),
    category: new FormControl<EventCategory | ''>('', {
      nonNullable: true,
      validators: [
        Validators.required,
        (control) => (EVENT_CATEGORIES.includes(control.value) ? null : { category: true }),
      ],
    }),
  });
  constructor() {
    effect(() => {
      if (!this.store.tokenValido()) void this.router.navigateByUrl('/gestao/entrar');
    });
    inject(DestroyRef).onDestroy(() => this.clearPhotos());
    void this.refresh();
  }
  private async load(): Promise<void> {
    const result = await this.api.list(true, this.page());
    this.events.set(result.items);
  }
  async refresh(page = this.page()): Promise<void> {
    await this.run(async () => {
      this.page.set(page);
      await this.load();
    });
  }
  edit(event: ManagedEvent): void {
    this.reset();
    this.editingId.set(event.id);
    this.form.setValue({
      title: event.title,
      description: event.description,
      category: event.category,
    });
    document.getElementById('event-form')?.scrollIntoView({ behavior: 'smooth' });
  }
  reset(): void {
    this.editingId.set(null);
    this.form.reset();
    this.formDirective()?.resetForm();
    this.clearPhotos();
  }
  private clearPhotos(): void {
    for (const photo of this.selectedPhotos()) URL.revokeObjectURL(photo.preview);
    this.selectedPhotos.set([]);
  }
  selectFiles(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    this.error.set('');
    if (
      files.some(
        (file) =>
          !['image/jpeg', 'image/png', 'image/webp'].includes(file.type) ||
          file.size === 0 ||
          file.size > 10 * 1024 * 1024,
      )
    ) {
      this.error.set('Escolha fotos JPEG, PNG ou WebP de até 10 MB cada.');
      input.value = '';
      return;
    }
    if (this.selectedPhotos().length + files.length > 100) {
      this.error.set('Selecione até 100 fotos.');
      input.value = '';
      return;
    }
    this.selectedPhotos.update((current) => [
      ...current,
      ...files.map((file) => ({ file, preview: URL.createObjectURL(file) })),
    ]);
    input.value = '';
  }
  removePhoto(index: number): void {
    URL.revokeObjectURL(this.selectedPhotos()[index].preview);
    this.selectedPhotos.update((current) => current.filter((_, position) => position !== index));
  }
  async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    await this.run(async () => {
      let id = this.editingId();
      if (id) await this.api.update(id, this.form.getRawValue());
      else {
        id = (await this.api.create(this.form.getRawValue())).id;
        this.editingId.set(id);
      }
      // Rascunho já existe se um upload falhar; repetir não duplica o evento.
      while (this.selectedPhotos().length) {
        const batch = this.selectedPhotos().slice(0, 10);
        await this.api.upload(
          id,
          batch.map((photo) => photo.file),
        );
        for (const photo of batch) URL.revokeObjectURL(photo.preview);
        this.selectedPhotos.update((current) => current.slice(batch.length));
      }
      this.reset();
      await this.load();
      this.message.set('Evento salvo. Use Exibir no site para publicá-lo.');
    });
  }
  async toggle(event: ManagedEvent, visible: boolean): Promise<void> {
    await this.run(async () => {
      const previous = event.isVisible;
      const setVisible = (value: boolean) =>
        this.events.update((items) =>
          items.map((item) => (item.id === event.id ? { ...item, isVisible: value } : item)),
        );
      setVisible(visible);
      try {
        await this.api.visibility(event.id, visible);
        await this.load();
      } catch (error) {
        setVisible(previous);
        throw error;
      }
    });
  }
  async remove(event: ManagedEvent): Promise<void> {
    const confirmed = await firstValueFrom(
      this.dialog
        .open(DeleteEventDialog, {
          data: event.title,
          width: '440px',
          maxWidth: 'calc(100vw - 32px)',
        })
        .afterClosed(),
    );
    if (!confirmed) return;
    await this.run(async () => {
      await this.api.delete(event.id);
      if (this.editingId() === event.id) this.reset();
      await this.load();
    });
  }
  async importInitial(sampleId = PORTFOLIO_SAMPLES[0].id): Promise<void> {
    this.reset();
    const event = PORTFOLIO_SAMPLES.find((item) => item.id === sampleId)!;
    this.form.setValue({
      title: event.title,
      description: event.description,
      category: event.category,
    });
    await this.run(async () => {
      const files: { file: File; preview: string }[] = [];
      try {
        for (const photo of event.photos) {
          const response = await fetch(photo.src);
          if (!response.ok) throw new Error('Não foi possível carregar as fotos iniciais.');
          const file = new File([await response.blob()], photo.src.split('/').pop()!, {
            type: 'image/jpeg',
          });
          files.push({ file, preview: URL.createObjectURL(file) });
        }
        this.selectedPhotos.set(files);
        this.message.set(
          'Acervo carregado no formulário. Revise título, categoria e descrição antes de salvar e publicar.',
        );
      } catch (error) {
        for (const photo of files) URL.revokeObjectURL(photo.preview);
        throw error;
      }
    });
  }
  private async run(action: () => Promise<void>): Promise<void> {
    if (this.pending()) return;
    this.pending.set(true);
    this.error.set('');
    this.message.set('');
    try {
      await action();
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.pending.set(false);
    }
  }
  async logout(): Promise<void> {
    await this.run(async () => {
      try {
        await this.auth.logout();
      } finally {
        await this.router.navigateByUrl('/gestao/entrar');
      }
    });
  }
}
