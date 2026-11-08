import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { PortalApi, type PortalPlan, type PortalHomeSummary } from './portal-api';
import { inventoryError } from '../admin/inventory/inventory-api';
@Component({
  selector: 'app-portal-home',
  imports: [DatePipe, DecimalPipe, RouterLink],
  templateUrl: './portal-home.html',
  styleUrl: './portal-home.scss',
})
export class PortalHome {
  readonly customer = input.required<string>();
  readonly preview = input(false);
  readonly plan = input<PortalPlan | null>(null);
  readonly requests = output<void>();
  readonly seePlan = output<void>();
  private readonly api = inject(PortalApi);
  readonly data = signal<PortalHomeSummary | null>(null);
  readonly error = signal('');
  readonly loading = signal(false);
  readonly period = signal('month');
  readonly periods = [
    ['day', 'Dia'],
    ['week', 'Semana'],
    ['month', 'Mês'],
    ['year', 'Ano'],
  ];
  readonly ring = computed(() => Math.min(100, Math.max(0, this.data()?.storagePercent ?? 0)));
  constructor() {
    effect((cleanup) => {
      const customer = this.customer(),
        preview = this.preview(),
        period = this.period();
      this.data.set(null);
      this.error.set('');
      if (!customer) return;
      this.loading.set(true);
      const subscription = this.api.home(customer, preview, period).subscribe({
        next: (r) => {
          this.data.set(r);
          this.loading.set(false);
        },
        error: (e) => {
          this.error.set(inventoryError(e));
          this.loading.set(false);
        },
      });
      cleanup(() => subscription.unsubscribe());
    });
  }
  points(kind: 'storageUsedMiB' | 'pageViews') {
    return (
      this.data()?.series.map((point, index) => ({
        date: point.date,
        value: point[kind],
        index,
      })) ?? []
    );
  }
  max(kind: 'storageUsedMiB' | 'pageViews') {
    return Math.max(
      1,
      ...this.points(kind).map((p) => p.value ?? 0),
      kind === 'storageUsedMiB' ? (this.data()?.storageLimitMiB ?? 0) : 0,
    );
  }
  x(index: number) {
    return 15 + (index * 570) / Math.max(1, (this.data()?.series.length ?? 1) - 1);
  }
  y(value: number, kind: 'storageUsedMiB' | 'pageViews') {
    return 172 - (value / this.max(kind)) * 155;
  }
  path(kind: 'storageUsedMiB' | 'pageViews') {
    let previous = false;
    return this.points(kind)
      .map((p) => {
        if (p.value === null) {
          previous = false;
          return '';
        }
        const segment = `${previous ? 'L' : 'M'}${this.x(p.index)},${this.y(p.value, kind)}`;
        previous = true;
        return segment;
      })
      .join(' ');
  }
  has(kind: 'storageUsedMiB' | 'pageViews') {
    return this.points(kind).some((p) => p.value !== null);
  }
}
