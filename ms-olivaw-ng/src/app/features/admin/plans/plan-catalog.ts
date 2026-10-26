import { Component, computed, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
@Component({
  selector: 'app-plan-catalog',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './plan-catalog.html',
  styleUrls: ['../inventory/inventory.scss', './plan-catalog.scss'],
})
export class PlanCatalog {
  readonly fx = signal(5.5);
  readonly infra = signal(15);
  readonly ai = signal(100);
  readonly clients = signal(5);
  readonly hourly = signal(80);
  readonly reserve = signal(15);
  readonly provision = signal(6);
  readonly fixed = computed(() => (this.infra() + this.ai()) * this.fx());
  readonly allocation = computed(() => this.fixed() / Math.max(1, this.clients()));
  readonly plans = [
    {
      name: 'Bronze',
      tag: 'Presença',
      price: 299,
      setup: 'R$ 1.990',
      hours: 1,
      space: '2 GiB',
      domain: 'Endereço Olivaw',
      app: 'Site estático · até 5 páginas',
      api: 'Sem API ou banco do negócio',
      users: 'Sem gestão de conteúdo',
      improvements: '2 pequenas melhorias de até 30 min/mês',
    },
    {
      name: 'Prata / Silver',
      tag: 'Conteúdo',
      price: 699,
      setup: 'R$ 4.990',
      hours: 4,
      space: '3 GiB',
      domain: 'Domínio próprio permitido',
      app: 'Site gerenciável · até 10 páginas',
      api: '1 API e 1 banco PostgreSQL lógico',
      users: 'Até 2 contas editoras',
      improvements: '3 pequenas melhorias de até 1 h/mês',
    },
    {
      name: 'Ouro',
      tag: 'Gestão',
      price: 1299,
      setup: 'A partir de R$ 14.900',
      hours: 8,
      space: '5 GiB',
      domain: 'Domínio próprio permitido',
      app: 'Sistema personalizado com escopo fechado',
      api: 'API, PostgreSQL e regras acordadas',
      users: 'Até 10 contas com perfis de acesso',
      improvements: '6 pequenas melhorias de até 1 h/mês',
    },
  ];
  cost(hours: number) {
    return (this.allocation() + hours * this.hourly()) * (1 + this.reserve() / 100);
  }
  result(price: number, hours: number) {
    return price * (1 - this.provision() / 100) - this.cost(hours);
  }
  set(field: 'fx' | 'infra' | 'ai' | 'clients' | 'hourly' | 'reserve' | 'provision', event: Event) {
    const value = Number((event.target as HTMLInputElement).value);
    if (!Number.isFinite(value)) return;
    const minimum = ['fx', 'clients'].includes(field) ? 1 : 0;
    this[field].set(
      Math.max(
        minimum,
        Math.min(
          field === 'clients' ? 10000 : field === 'reserve' || field === 'provision' ? 100 : 100000,
          value,
        ),
      ),
    );
  }
}
