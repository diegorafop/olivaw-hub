import { Component, computed, Injectable, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Breadcrumb } from '../../shared/breadcrumb';
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
@Injectable({ providedIn: 'root' })
export class PortalPreviewState {
  readonly requests = signal([...initialRequests]);
  readonly scenario = signal<Scenario>('normal');
  readonly period = signal<Period>('month');
}
@Component({
  selector: 'app-portal-home-preview',
  imports: [RouterLink, PortalIcon, PortalLineChart, Breadcrumb, RouterLinkActive],
  templateUrl: './portal-home-preview.html',
  styleUrls: [
    './portal-home-preview.scss',
    './portal-home-cards.scss',
    './portal-home-dialog.scss',
    './portal-utilization.scss',
    './portal-home-responsive.scss',
  ],
})
export class PortalHomePreview {
  private readonly state = inject(PortalPreviewState);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly section: string = this.route.snapshot.data['portalSection'] ?? 'home';
  readonly titles: Record<string, string> = {
    home: 'Início',
    requests: 'Seus chamados',
    improvements: 'Suas melhorias',
    plan: 'Seu plano',
    indicator: 'Como calculamos o uso',
    technical: 'Área técnica',
    new: 'Nova solicitação',
    request: 'Detalhes do chamado',
    improvement: 'Detalhes da melhoria',
  };
  readonly title = this.titles[this.section];
  readonly detailId = Number(this.route.snapshot.paramMap.get('itemId'));
  readonly request = computed(() => this.requests().find((item) => item.id === this.detailId));
  readonly improvement = initialImprovements.find((item) => item.id === this.detailId);
  readonly breadcrumbs = [
    { label: 'Início', url: '/portal' },
    ...(['request', 'improvement', 'new'].includes(this.section)
      ? [
          {
            label: this.section === 'improvement' ? 'Melhorias' : 'Chamados',
            url: this.section === 'improvement' ? '/portal/melhorias' : '/portal/chamados',
          },
        ]
      : []),
    ...(this.section === 'home' ? [] : [{ label: this.title, url: '' }]),
  ];
  readonly periods = periods;
  readonly period = this.state.period;
  readonly scenario = this.state.scenario;
  readonly used = computed(() => (this.scenario() === 'attention' ? 1840 : 1280));
  readonly storagePercent = computed(() => Math.round(this.used() / 20));
  readonly planPercent = computed(() => Math.round((this.storagePercent() + 50) / 2));
  readonly missing = computed(() => this.scenario() === 'missing');
  readonly storage = computed(() => series(this.period(), 'storage', this.used()));
  readonly accesses = computed(() => series(this.period(), 'accesses', this.used()));
  readonly total = computed(() =>
    this.accesses()
      .reduce((total, point) => total + point.value, 0)
      .toLocaleString('pt-BR'),
  );
  readonly caption = computed(() => periods.find((item) => item.id === this.period())!.caption);
  readonly requests = this.state.requests;
  readonly improvements = initialImprovements;
  readonly opened = computed(() => this.requests().filter((item) => item.status !== 'done').length);
  readonly labels: Record<string, string> = {
    received: 'Recebido',
    working: 'Em andamento',
    waiting: 'Sua resposta',
    done: 'Concluído',
    queued: 'Em espera',
  };
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
    void this.router.navigate(['/portal/chamados', this.requests()[0].id], {
      queryParams: { simulado: 'true' },
    });
  }
}
