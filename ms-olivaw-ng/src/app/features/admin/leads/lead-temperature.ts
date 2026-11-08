import { Component, computed, input } from '@angular/core';
import { DatePipe } from '@angular/common';
export interface LeadEngagement {
  level: string;
  reason?: string;
  updatedAt?: string;
  messageId?: string;
}
const LEVELS: Record<string, { label: string; bars: number }> = {
  unassessed: { label: 'Interesse não confirmado', bars: 0 },
  cold: { label: 'Frio', bars: 1 },
  warm: { label: 'Morno', bars: 2 },
  hot: { label: 'Quente', bars: 3 },
  closed: { label: 'Não contatar', bars: 0 },
};
@Component({
  selector: 'app-lead-temperature',
  imports: [DatePipe],
  template: `
    <div
      class="temperature"
      [attr.data-level]="state().level"
      [attr.title]="state().reason || label().label"
    >
      <span class="bars" aria-hidden="true">
        @for (bar of [1, 2, 3]; track bar) {
          <i [class.filled]="bar <= label().bars"></i>
        }
      </span>
      <strong>{{ label().label }}</strong>
    </div>
    @if (detail()) {
      <p>
        {{
          state().reason ||
            'O diagnóstico do site indica oportunidades; ainda não há sinal de interesse confirmado na conversa.'
        }}
      </p>
      @if (state().updatedAt) {
        <small>Sinal registrado em {{ state().updatedAt | date: 'dd/MM/yyyy HH:mm' }}</small>
      }
    }
  `,
  styles: `
    :host {
      display: block;
    }
    .temperature {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.8rem;
    }
    .bars {
      display: flex;
      align-items: end;
      gap: 3px;
      height: 18px;
    }
    i {
      width: 5px;
      height: 6px;
      background: #cbd5e1;
      border-radius: 2px;
    }
    i:nth-child(2) {
      height: 12px;
    }
    i:nth-child(3) {
      height: 18px;
    }
    .filled {
      background: #475569;
    }
    [data-level='warm'] .filled {
      background: #996300;
    }
    [data-level='hot'] .filled {
      background: #b42318;
    }
    [data-level='closed'] strong {
      color: #b42318;
    }
    p {
      font-size: 0.85rem;
      margin: 0.5rem 0;
    }
    small {
      color: #64748b;
    }
  `,
})
export class LeadTemperature {
  readonly engagement = input<LeadEngagement | null>();
  readonly level = input<string>();
  readonly reason = input<string>();
  readonly updatedAt = input<string>();
  readonly detail = input(false);
  readonly state = computed(
    () =>
      this.engagement() || {
        level: this.level() || 'unassessed',
        reason: this.reason(),
        updatedAt: this.updatedAt(),
      },
  );
  readonly label = computed(() => LEVELS[this.state().level] || LEVELS['unassessed']);
}
