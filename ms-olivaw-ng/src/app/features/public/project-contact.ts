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
import { ProjectDraft } from './project-folder/project-draft';
import { discoveryPaths, DiscoveryOption, DiscoveryPath } from './project-discovery';

@Component({
  selector: 'app-project-contact',
  imports: [],
  templateUrl: './project-contact.html',
  styleUrl: './project-contact.scss',
})
export class ProjectContact {
  private readonly router = inject(Router);
  private readonly draft = inject(ProjectDraft);
  private readonly injector = inject(Injector);
  private readonly stepTitle = viewChild.required<ElementRef<HTMLHeadingElement>>('stepTitle');
  readonly paths = discoveryPaths;
  readonly step = signal(0);
  readonly path = signal<DiscoveryPath | null>(null);
  readonly purpose = signal<DiscoveryOption | null>(null);
  readonly context = signal<DiscoveryOption | null>(null);
  readonly title = computed(() => {
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
  choosePath(path: DiscoveryPath): void {
    this.draft.enter('discovery');
    this.draft.record('path-selected', path.id);
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
    this.draft.record(this.step() === 1 ? 'purpose-selected' : 'context-selected', option.id);
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
    this.step.update((step) => Math.max(0, step - 1));
    this.focusTitle();
  }

  restart(): void {
    this.path.set(null);
    this.purpose.set(null);
    this.context.set(null);
    this.advance(0);
  }

  openProject(): void {
    this.draft.start({
      source: 'discovery',
      title: this.path()?.label ?? 'Explorar minha ideia',
      choices: [this.purpose()?.label, this.context()?.label].filter((label): label is string =>
        Boolean(label),
      ),
      context: this.contextNote() || this.path()?.recommendationDescription || '',
    });
    void this.router.navigate(['/projeto']);
  }

  private advance(step: number): void {
    this.step.set(step);
    this.focusTitle();
  }

  private focusTitle(): void {
    afterNextRender(() => this.stepTitle().nativeElement.focus({ preventScroll: true }), {
      injector: this.injector,
    });
  }
}
