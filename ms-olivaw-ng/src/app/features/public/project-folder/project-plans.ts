import { Component, inject, input } from '@angular/core';
import { ProjectDraft } from './project-draft';
@Component({
  selector: 'app-project-plans',
  templateUrl: './project-plans.html',
  styleUrl: './project-plans.scss',
})
export class ProjectPlans {
  readonly active = input(false);
  readonly draft = inject(ProjectDraft);
  readonly plans = [
    {
      name: 'Bronze',
      note: 'Para começar',
      text: 'Uma base para colocar a primeira versão em operação.',
      features: [
        'Capacidade inicial de armazenamento',
        'Rotina de backup definida',
        'Atendimento e manutenção essenciais',
      ],
    },
    {
      name: 'Prata',
      note: 'Para ganhar ritmo',
      text: 'Mais espaço para a rotina e os primeiros ajustes.',
      features: [
        'Mais capacidade para seus dados',
        'Atendimento com condições ampliadas',
        'Franquia de ajustes mensais',
      ],
    },
    {
      name: 'Ouro',
      note: 'Para expandir',
      text: 'Mais capacidade e acompanhamento para uma operação maior.',
      features: [
        'Capacidade ampliada',
        'Mais espaço para melhorias mensais',
        'Acompanhamento da evolução',
      ],
    },
    {
      name: 'Diamante',
      note: 'Na sua escala',
      text: 'Um plano dimensionado junto com a sua empresa.',
      features: [
        'Capacidade e atendimento sob medida',
        'Evolução planejada com a equipe',
        'Condições definidas em proposta',
      ],
    },
  ];
}
