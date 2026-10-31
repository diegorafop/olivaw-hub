import { Breadcrumb } from '../../shared/breadcrumb';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map } from 'rxjs';
import { DOCUMENT } from '@angular/common';
import { Component, inject, computed, input, output, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
@Component({
  selector: 'app-admin-layout',
  imports: [Breadcrumb, RouterLink, RouterLinkActive],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss',
})
export class AdminLayout {
  private readonly document = inject(DOCUMENT);
  focusContent(event: Event): void {
    event.preventDefault();
    this.document.getElementById('operation-content')?.focus();
  }
  private readonly router = inject(Router);
  private readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter((event) => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects),
    ),
    { initialValue: this.router.url },
  );
  private readonly pathParts = computed(
    () =>
      this.router
        .parseUrl(this.currentUrl())
        .root.children['primary']?.segments.map((segment) => segment.path) ?? [],
  );
  private readonly sectionTitles: Record<string, string> = {
    configuration: 'Configuração',
    agents: 'Agents',
    deliveries: 'Entregas',
    'portal-access': 'Portal do cliente',
    'portal-preview': 'Prévia do cliente',
    resources: 'Recursos',
    deployments: 'Ambientes',
    services: 'Serviços',
    customers: 'Clientes',
    leads: 'Leads',
    health: 'Saúde',
    incidents: 'Incidentes',
    usage: 'Consumo',
    plans: 'Planos',
    expenses: 'Despesas',
    finance: 'Financeiro',
    invoices: 'Faturas AWS',
    care: 'Cuidados',
    support: 'Atendimento',
  };
  readonly isConfiguration = computed(() =>
    [
      'configuration',
      'deliveries',
      'portal-access',
      'portal-preview',
      'customers',
      'services',
      'resources',
      'deployments',
      'health',
      'incidents',
      'usage',
      'plans',
      'care',
      'support',
    ].includes(this.pathParts()[1]),
  );
  readonly isFinance = computed(() =>
    ['finance', 'expenses', 'invoices'].includes(this.pathParts()[1]),
  );
  readonly section = computed(() => this.sectionTitles[this.pathParts()[1]] ?? 'Visão geral');
  readonly breadcrumbs = computed(() => {
    const [, section, detail, reportId] = this.pathParts();
    const items: { label: string; url: string }[] = [];
    if (this.isRoot()) items.push({ label: 'Visão geral', url: '/admin' });
    if (this.isConfiguration()) items.push({ label: 'Configuração', url: '/admin/configuration' });
    if (this.isFinance()) {
      items.push({ label: 'Financeiro', url: '/admin/finance' });
      if (section === 'finance' && detail) {
        const labels: Record<string, string> = {
          control: 'Controle financeiro',
          accounts: 'Contas financeiras',
          entries: 'A pagar e receber',
        };
        items.push({
          label: labels[detail] || 'Controle financeiro',
          url: '/admin/finance/' + detail,
        });
        if (reportId)
          items.push({
            label: reportId === 'new' ? 'Nova obrigação' : 'Detalhes da obrigação',
            url: this.currentUrl(),
          });
      }
      if (section === 'expenses') items.push({ label: 'Despesas', url: '/admin/expenses' });
      if (section === 'invoices') {
        items.push({ label: 'Faturas AWS', url: '/admin/invoices' });
        if (detail)
          items.push({
            label: detail === 'new' ? 'Registrar fatura' : 'Detalhes da fatura',
            url: this.currentUrl(),
          });
      }
      return items;
    }
    if (!section || section === 'configuration') return items;
    if (section === 'deployments') {
      items.push({ label: 'Serviços', url: '/admin/services' });
      items.push({
        label: detail === 'new' ? 'Nova implantação' : 'Implantação',
        url: this.currentUrl(),
      });
      return items;
    }
    const label = this.sectionTitles[section];
    if (!label) return items;
    items.push({ label, url: '/admin/' + section });
    if (section === 'agents' && detail === 'baley') {
      items.push({ label: 'Baley', url: '/admin/agents/baley' });
      if (reportId) items.push({ label: 'Relatório', url: this.currentUrl() });
      return items;
    }
    if (section === 'leads' && detail) {
      items.push({
        label: detail === 'new' ? 'Novo lead' : 'Ficha do lead',
        url: '/admin/leads/' + detail,
      });
      if (reportId === 'proposal') items.push({ label: 'Orçamento', url: this.currentUrl() });
      return items;
    }
    if (section === 'plans' && detail) {
      const names: Record<string, string> = {
        bronze: 'Bronze',
        prata: 'Prata',
        ouro: 'Ouro',
        diamante: 'Diamante',
        simulator: 'Simulador de custos',
      };
      items.push({ label: names[detail] || 'Plano', url: this.currentUrl() });
      return items;
    }
    if (detail) {
      const singular: Record<string, string> = {
        resources: 'recurso',
        services: 'serviço',
        customers: 'cliente',
        leads: 'lead',
      };
      const entity = singular[section];
      if (entity)
        items.push({
          label: detail === 'new' ? 'Novo ' + entity : 'Ficha do ' + entity,
          url: this.currentUrl(),
        });
    }
    return items;
  });
  readonly isRoot = input(true);
  readonly collapsed = signal(false);
  readonly operator = input.required<string>();
  readonly initials = input('');
  readonly pending = input(false);
  readonly signOut = output<void>();
}
