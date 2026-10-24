import { DOCUMENT } from '@angular/common';
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
import { RouterLink } from '@angular/router';
import { ContactFields } from '../contact-fields';
import { ContactWelcome } from '../contact-welcome';
import { ProjectPlans } from './project-plans';
import { ProjectJourney } from './project-journey';
import { ProjectDraft } from './project-draft';

@Component({
  selector: 'app-project-folder',
  imports: [RouterLink, ProjectJourney, ProjectPlans, ContactFields, ContactWelcome],
  templateUrl: './project-folder.html',
  styleUrl: './project-folder.scss',
})
export class ProjectFolder {
  readonly draft = inject(ProjectDraft);
  private readonly document = inject(DOCUMENT);
  readonly step = this.draft.journeyStep;
  readonly ready = computed(() =>
    Boolean(this.draft.business().trim() && this.draft.requirements().trim()),
  );
  readonly furthest = signal(this.ready() ? this.step() : 0);
  private readonly content = viewChild.required<ElementRef<HTMLElement>>('journeyContent');
  private readonly injector = inject(Injector);
  constructor() {
    this.draft.enter(this.draft.seed()?.source ?? 'discovery');
    if (!this.ready()) this.step.set(0);
  }
  goTo(index: number): void {
    if (index < 0 || index > 4 || index > this.furthest() + 1 || (index > 0 && !this.ready()))
      return;
    if (index === this.step()) return;
    this.step.set(index);
    this.furthest.update((value) => Math.max(value, index));
    this.draft.record('journey-step', String(index + 1));
    afterNextRender(
      () => {
        const panel = this.content().nativeElement.querySelector<HTMLElement>(
          `[data-step="${index}"]`,
        );
        const title = panel?.querySelector<HTMLElement>('h2');
        title?.focus({ preventScroll: true });
        const reduced = this.document.defaultView?.matchMedia(
          '(prefers-reduced-motion: reduce)',
        ).matches;
        panel?.scrollIntoView({ block: 'start', behavior: reduced ? 'instant' : 'smooth' });
      },
      { injector: this.injector },
    );
  }
  readonly copied = signal('');
  readonly reviewing = signal(false);
  readonly choices = computed(() => this.draft.seed()?.choices ?? []);
  readonly included = computed(() =>
    this.choices().filter((choice) => !this.draft.excluded().includes(choice)),
  );
  readonly services = [
    {
      icon: '◎',
      title: 'Entendemos sua rotina',
      text: 'Sua ideia vira um plano de ação, com prioridades que fazem sentido para você.',
      detail:
        'Mapeamos processos, requisitos e integrações. A equipe avalia o que aproveitar, o que construir e por onde começar.',
    },
    {
      icon: '✳',
      title: 'Construímos seu sistema',
      text: 'Telas e funcionalidades pensadas para o jeito que seu negócio trabalha.',
      detail:
        'Escolhemos tecnologias adequadas ao projeto e cuidamos do desenvolvimento, dos testes e da experiência de uso.',
    },
    {
      icon: '↗',
      title: 'Colocamos para funcionar',
      text: 'Da ideia ao acesso no navegador. Cuidamos da estrutura por trás da experiência.',
      detail:
        'Planejamos domínio em nome do cliente, hospedagem em nuvem, servidor de aplicação e banco de dados. Serviços e responsabilidades são definidos na proposta.',
    },
    {
      icon: '∞',
      title: 'Crescemos com você',
      text: 'Seu projeto continua vivo, com cuidado e espaço para melhorar.',
      detail:
        'Backups, monitoramento, atendimento e manutenção entram conforme o plano contratado. Melhorias e novas funcionalidades têm escopo e condições combinados antes da execução.',
    },
  ];
  readonly summary = computed(() =>
    [
      'MINHA PASTA DE PROJETO · OLIVAW HUB',
      `Negócio: ${this.draft.business().trim() || 'Ainda não informado'}`,
      `Intenção: ${this.draft.seed()?.title ?? 'Quero explorar uma ideia'}`,
      `Possibilidades: ${this.included().join(', ') || 'A definir juntos'}`,
      this.draft.seed()?.context ? `Contexto: ${this.draft.seed()?.context}` : '',
      `Minha ideia: ${this.draft.requirements().trim() || 'Quero ajuda para organizar os requisitos.'}`,
      this.draft.plan()
        ? `Plano que despertou interesse: ${this.draft.plan()} (a avaliar)`
        : 'Plano: a avaliar com a equipe',
      '',
      'Próximo passo: análise pela equipe Olivaw, definição do MVP e planejamento das etapas E01 e E02.',
      'Escopo, investimento inicial e plano de continuidade serão definidos em proposta. Este resumo não é um orçamento.',
    ]
      .filter(Boolean)
      .join('\n\n'),
  );

  toggle(choice: string): void {
    this.draft.excluded.update((items) =>
      items.includes(choice) ? items.filter((item) => item !== choice) : [...items, choice],
    );
  }
  async copy(): Promise<void> {
    try {
      const clipboard = this.document.defaultView?.navigator.clipboard;
      if (!clipboard) throw new Error('Unavailable');
      await clipboard.writeText(this.summary());
      this.copied.set('Resumo copiado. Você pode guardá-lo para nossa conversa.');
    } catch {
      this.copied.set(
        'Selecione e copie o resumo acima. A cópia automática não está disponível neste navegador.',
      );
    }
  }
}
