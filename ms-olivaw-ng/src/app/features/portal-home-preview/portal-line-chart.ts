import { Component, computed, input, signal } from '@angular/core';
import type { Point } from './preview-data';
@Component({
  selector: 'app-portal-line-chart',
  templateUrl: './portal-line-chart.html',
  styleUrl: './portal-line-chart.scss',
})
export class PortalLineChart {
  readonly points = input.required<Point[]>();
  readonly kind = input<'storage' | 'accesses'>('storage');
  readonly empty = input(false);
  readonly limit = input<number | null>(null);
  readonly selected = signal<number | null>(null);
  readonly max = computed(
    () =>
      this.limit() ??
      Math.ceil(Math.max(...this.points().map((point) => point.value), 1) / 50) * 50,
  );
  readonly active = computed(() =>
    Math.min(Math.max(this.selected() ?? this.points().length - 1, 0), this.points().length - 1),
  );
  readonly point = computed(() => this.points()[this.active()]);
  readonly x = computed(() => (this.active() * 600) / Math.max(this.points().length - 1, 1));
  readonly y = computed(() => 180 - ((this.point()?.value ?? 0) / this.max()) * 164);
  readonly line = computed(() =>
    this.points()
      .map(
        (point, index) =>
          `${index ? 'L' : 'M'}${(index * 600) / Math.max(this.points().length - 1, 1)},${180 - (point.value / this.max()) * 164}`,
      )
      .join(' '),
  );
  readonly area = computed(() => `${this.line()} L600,180 L0,180 Z`);
  readonly ticks = computed(() => [
    this.max(),
    this.max() * 0.75,
    this.max() * 0.5,
    this.max() * 0.25,
    0,
  ]);
  readonly labels = computed(() =>
    [
      0,
      Math.floor((this.points().length - 1) / 3),
      Math.floor(((this.points().length - 1) * 2) / 3),
      this.points().length - 1,
    ].map((index) => this.points()[index]?.label ?? ''),
  );
  format(value: number): string {
    if (this.kind() === 'storage') return value === 1000 ? '1 GB' : `${Math.round(value)} MB`;
    return new Intl.NumberFormat('pt-BR').format(Math.round(value));
  }
  preview(event: PointerEvent) {
    const bounds = (event.currentTarget as SVGSVGElement).getBoundingClientRect();
    this.selected.set(
      Math.round(
        Math.min(1, Math.max(0, (event.clientX - bounds.left) / bounds.width)) *
          (this.points().length - 1),
      ),
    );
  }
  navigate(event: KeyboardEvent) {
    if (!['ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(event.key)) return;
    event.preventDefault();
    const offset = event.key === 'ArrowLeft' ? -1 : 1;
    this.selected.set(
      event.key === 'Home'
        ? 0
        : event.key === 'End'
          ? this.points().length - 1
          : Math.max(0, Math.min(this.points().length - 1, this.active() + offset)),
    );
  }
}
