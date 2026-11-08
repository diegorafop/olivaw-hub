import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-project-journey',
  template: `
    <nav class="journey-nav" aria-label="Etapas do seu projeto">
      <ol>
        @for (label of steps; track label; let index = $index) {
          <li [class.visited]="index < step()" [class.current]="index === step()">
            <button
              type="button"
              [disabled]="index > furthest() || (index > 0 && !ready())"
              [attr.aria-current]="index === step() ? 'step' : null"
              (click)="navigate.emit(index)"
            >
              <span class="number" aria-hidden="true">{{
                index < step() ? '✓' : '0' + (index + 1)
              }}</span>
              <span>{{ label }}</span>
            </button>
          </li>
        }
      </ol>
    </nav>
    <div class="journey-content"><ng-content /></div>
    <div class="journey-actions">
      <div class="journey-hint">
        <span class="step-count" role="status">Etapa {{ step() + 1 }} de 5</span>
        <p id="advance-hint">
          {{
            step() === 0 && !ready()
              ? 'Preencha o nome do negócio e conte sua ideia para continuar.'
              : hints[step()]
          }}
        </p>
      </div>
      <div class="buttons">
        @if (step() > 0) {
          <button type="button" class="back" (click)="navigate.emit(step() - 1)">← Anterior</button>
        }
        @if (step() < 4) {
          <button
            type="button"
            class="next"
            [disabled]="!ready()"
            aria-describedby="advance-hint"
            (click)="navigate.emit(step() + 1)"
          >
            Próximo: {{ steps[step() + 1] }} <span aria-hidden="true">→</span>
          </button>
        }
      </div>
    </div>
  `,
  styleUrl: './project-journey.scss',
})
export class ProjectJourney {
  readonly step = input.required<number>();
  readonly furthest = input.required<number>();
  readonly ready = input.required<boolean>();
  readonly navigate = output<number>();
  readonly steps = [
    'Sua ideia',
    'O que somamos',
    'Primeira entrega',
    'Nossa parceria',
    'Sua pasta',
  ];
  readonly hints = [
    'Sua ideia não precisa nascer pronta. Vamos dar o próximo passo juntos.',
    'As partes se conectam ao seu projeto. Explore os detalhes ou siga no seu ritmo.',
    'Começamos pelo que resolve o principal. A evolução vem com o uso.',
    'O plano pode ser definido com a equipe. Você pode continuar sem escolher um.',
    'Revise sua ideia. Só enviamos para a Olivaw quando você confirmar.',
  ];
}
