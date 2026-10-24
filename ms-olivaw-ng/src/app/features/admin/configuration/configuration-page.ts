import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
export const configurationCards = [
  {
    path: 'deliveries',
    title: 'Entregas',
    description: 'Rascunhos e notas de versão publicados por empresa.',
  },
  {
    path: 'portal-access',
    title: 'Portal do cliente',
    description: 'Convites, vínculos e prévia por empresa.',
  },
  { path: 'customers', title: 'Clientes', description: 'Empresas, contatos e responsáveis.' },
  { path: 'services', title: 'Serviços', description: 'Aplicações e seus vínculos com clientes.' },
  {
    path: 'resources',
    title: 'Recursos',
    description: 'Infraestrutura, dependências, ambientes e reservas.',
  },
  { path: 'health', title: 'Saúde', description: 'Verificações pontuais, fontes e atualização.' },
  {
    path: 'incidents',
    title: 'Incidentes',
    description: 'Impacto, responsáveis e acompanhamento da recuperação.',
  },
  { path: 'usage', title: 'Consumo', description: 'Registros de uso e qualidade da informação.' },
  {
    path: 'plans',
    title: 'Planos',
    description: 'Base de custos e estudos de limites e funcionalidades.',
  },
  {
    path: 'care',
    title: 'Cuidados',
    description: 'Evidências de publicações, backups e restauração.',
  },
  {
    path: 'support',
    title: 'Atendimento',
    description: 'Pedidos internos, próximas ações e histórico.',
  },
];
@Component({
  selector: 'app-configuration-page',
  imports: [RouterLink],
  template: ` <main id="operation-content" tabindex="-1">
    <header>
      <div>
        <p class="eyebrow">OLIVAW / OPERAÇÃO</p>
        <h1>Configuração</h1>
        <p>Cadastros e ferramentas para organizar a operação.</p>
      </div>
    </header>
    <nav class="resource-grid" aria-label="Cadastros e ferramentas">
      @for (card of cards; track card.path) {
        <a class="resource-card" [routerLink]="['/admin', card.path]" [attr.aria-label]="card.title"
          ><span class="badge" aria-hidden="true">↗</span>
          <h2>{{ card.title }}</h2>
          <p>{{ card.description }}</p></a
        >
      }
    </nav>
  </main>`,
  styleUrl: '../inventory/inventory.scss',
})
export class ConfigurationPage {
  readonly cards = configurationCards;
}
