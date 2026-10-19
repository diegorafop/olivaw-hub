import {
  afterNextRender,
  Component,
  computed,
  ElementRef,
  inject,
  Injector,
  signal,
  viewChild,
} from '@angular/core';
import { Router } from '@angular/router';
import { DOCUMENT } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { discoveryPaths, DiscoveryOption, DiscoveryPath } from './project-discovery';

@Component({
  selector: 'app-project-contact',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule],
  templateUrl: './project-contact.html',
  styleUrl: './project-contact.scss',
})
export class ProjectContact {
  private readonly document = inject(DOCUMENT);
  private readonly router = inject(Router);
  private readonly injector = inject(Injector);
  private readonly stepTitle = viewChild.required<ElementRef<HTMLHeadingElement>>('stepTitle');
  readonly paths = discoveryPaths;
  readonly step = signal(0);
  readonly path = signal<DiscoveryPath | null>(null);
  readonly purpose = signal<DiscoveryOption | null>(null);
  readonly context = signal<DiscoveryOption | null>(null);
  readonly reviewOpen = signal(false);
  readonly message = signal('');
  readonly title = computed(() => {
    if (this.reviewOpen()) return 'Vamos guardar essa ideia?';
    if (this.step() === 0) return 'O que você gostaria de melhorar hoje?';
    if (this.step() === 1) return this.path()?.question ?? '';
    if (this.step() === 2) return this.path()?.contextQuestion ?? '';
    return 'Seu próximo passo pode começar aqui.';
  });
  readonly choices = computed(() =>
    this.step() === 1 ? (this.path()?.choices ?? []) : (this.path()?.contexts ?? []),
  );
  readonly contextNote = computed(() => {
    if (this.context()?.id === 'existing-site') {
      return 'Como você já tem um site, podemos começar revisando o que existe antes de propor uma nova versão.';
    }
    if (this.context()?.id === 'system' || this.context()?.id === 'existing') {
      return 'Você já usa uma solução. Antes de trocar, vale entender o que pode ser aproveitado e o que precisa mudar.';
    }
    if (this.context()?.id === 'spreadsheet') {
      return 'Sua planilha pode ajudar a entender o processo que você já usa e o que merece ser simplificado.';
    }
    if (this.purpose()?.id === 'unsure') {
      return 'Tudo bem ainda não ter uma prioridade. Podemos explorar exemplos juntos antes de escolher.';
    }
    return '';
  });
  readonly form = new FormGroup({
    business: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(120)] }),
    summary: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(3000)],
    }),
  });

  choosePath(path: DiscoveryPath): void {
    if (path.id === 'website') {
      void this.router.navigate(['/estudio']);
      return;
    }
    if (this.path()?.id !== path.id) {
      this.purpose.set(null);
      this.context.set(null);
    }
    this.path.set(path);
    this.advance(1);
  }

  chooseAnswer(option: DiscoveryOption): void {
    if (this.step() === 1) {
      this.purpose.set(option);
      this.advance(2);
      return;
    }
    this.context.set(option);
    this.advance(3);
  }

  isSelected(option: DiscoveryOption): boolean {
    if (this.step() === 1) return this.purpose()?.id === option.id;
    return this.context()?.id === option.id;
  }

  back(): void {
    this.message.set('');
    if (this.reviewOpen()) {
      this.reviewOpen.set(false);
      this.focusTitle();
      return;
    }
    this.step.update((step) => Math.max(0, step - 1));
    this.focusTitle();
  }

  restart(): void {
    this.path.set(null);
    this.purpose.set(null);
    this.context.set(null);
    this.reviewOpen.set(false);
    this.form.reset();
    this.advance(0);
  }

  prepareConversation(): void {
    if (!this.form.controls.summary.value) {
      this.form.controls.summary.setValue(
        [
          `Meu objetivo: ${this.path()?.label ?? ''}`,
          `Minha prioridade: ${this.purpose()?.label ?? ''}`,
          `Meu momento: ${this.context()?.label ?? ''}`,
          '',
          `Um possível começo: ${this.path()?.recommendation ?? ''}`,
          this.contextNote(),
          'Quero conversar sobre esse caminho e entender os próximos passos.',
        ].join('\n'),
      );
    }
    this.reviewOpen.set(true);
    this.message.set('');
    this.focusTitle();
  }

  async copy(): Promise<void> {
    this.message.set('');
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { business, summary } = this.form.getRawValue();
    const text = business.trim() ? `Meu negócio: ${business.trim()}\n\n${summary}` : summary;
    try {
      const clipboard = this.document.defaultView?.navigator.clipboard;
      if (!clipboard) throw new Error('Área de transferência indisponível.');
      await clipboard.writeText(text);
      this.message.set('Resumo copiado. Sua ideia está pronta para uma próxima conversa.');
    } catch {
      this.message.set(
        'Não foi possível copiar automaticamente. Seu resumo está no campo acima: você pode selecionar e copiar o texto.',
      );
    }
  }

  private advance(step: number): void {
    this.step.set(step);
    this.reviewOpen.set(false);
    this.form.controls.summary.reset('');
    this.message.set('');
    this.focusTitle();
  }

  private focusTitle(): void {
    afterNextRender(() => this.stepTitle().nativeElement.focus({ preventScroll: true }), {
      injector: this.injector,
    });
  }
}
