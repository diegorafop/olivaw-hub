import { Component, computed, ElementRef, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PortalIcon } from './portal-icon';
import { PortalLineChart } from './portal-line-chart';
import {
  initialImprovements,
  initialRequests,
  periods,
  series,
  type Period,
  type Scenario,
} from './preview-data';
@Component({
  selector: 'app-portal-home-preview',
  imports: [RouterLink, PortalIcon, PortalLineChart],
  templateUrl: './portal-home-preview.html',
  styleUrls: [
    './portal-home-preview.scss',
    './portal-home-cards.scss',
    './portal-home-dialog.scss',
    './portal-home-responsive.scss',
  ],
})
export class PortalHomePreview {
  readonly periods = periods;
  readonly period = signal<Period>('month');
  readonly scenario = signal<Scenario>('normal');
  readonly used = computed(() => (this.scenario() === 'attention' ? 920 : 720));
  readonly missing = computed(() => this.scenario() === 'missing');
  readonly storage = computed(() => series(this.period(), 'storage', this.used()));
  readonly accesses = computed(() => series(this.period(), 'accesses', this.used()));
  readonly total = computed(() =>
    this.accesses()
      .reduce((total, point) => total + point.value, 0)
      .toLocaleString('pt-BR'),
  );
  readonly caption = computed(() => periods.find((item) => item.id === this.period())!.caption);
  readonly requests = signal([...initialRequests]);
  readonly improvements = initialImprovements;
  readonly opened = computed(() => this.requests().filter((item) => item.status !== 'done').length);
  readonly modal = signal('');
  readonly sent = signal(false);
  readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');
  readonly labels: Record<string, string> = {
    received: 'Recebido',
    working: 'Em andamento',
    waiting: 'Sua resposta',
    done: 'Concluído',
    queued: 'Em espera',
  };
  open(name: string) {
    this.modal.set(name);
    this.sent.set(false);
    this.dialog().nativeElement.showModal();
  }
  close() {
    this.dialog().nativeElement.close();
  }
  submit(event: Event) {
    event.preventDefault();
    const form = event.target as HTMLFormElement;
    const title = String(new FormData(form).get('title') ?? '').trim();
    if (!title) return;
    this.requests.update((items) => [
      {
        id: Math.max(...items.map((item) => item.id)) + 1,
        title,
        status: 'received',
        date: 'Agora',
      },
      ...items,
    ]);
    this.sent.set(true);
    form.reset();
  }
}
