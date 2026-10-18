import { computed, Injectable, signal } from '@angular/core';
@Injectable({ providedIn: 'root' })
export class HttpState {
  private readonly count = signal(0);
  readonly loading = computed(() => this.count() > 0);
  readonly error = signal<string | null>(null);
  start(): void {
    this.count.update((count) => count + 1);
  }
  finish(): void {
    this.count.update((count) => Math.max(0, count - 1));
  }
}
