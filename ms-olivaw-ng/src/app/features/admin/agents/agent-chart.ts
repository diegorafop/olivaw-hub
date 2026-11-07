import { Component, computed, input } from '@angular/core';
import type { ChartPoint } from './agent-metrics';
@Component({
  selector: 'app-agent-chart',
  styleUrl: './agent-dashboard.scss',
  template: `<section class="chart-panel">
    <h2>{{ title() }}</h2>
    <p class="muted">{{ caption() }}</p>
    @if (total() === 0) {
      <p class="chart-empty">Nenhum registro neste recorte.</p>
    }
    <div class="bars" [class.timeline]="timeline()" role="list" [attr.aria-label]="title()">
      @for (point of points(); track $index) {
        <div class="bar-row" role="listitem" [attr.aria-label]="point.label + ': ' + point.value">
          <span class="bar-label">{{ point.label }}</span>
          <div class="bar-track">
            <span class="bar-fill" [style.width.%]="(point.value / maximum()) * 100"></span>
          </div>
          <strong>{{ point.value }}</strong>
        </div>
      }
    </div>
  </section>`,
})
export class AgentChart {
  readonly title = input.required<string>();
  readonly caption = input('');
  readonly points = input.required<ChartPoint[]>();
  readonly timeline = input(false);
  readonly maximum = computed(() => Math.max(1, ...this.points().map((point) => point.value)));
  readonly total = computed(() => this.points().reduce((sum, point) => sum + point.value, 0));
}
