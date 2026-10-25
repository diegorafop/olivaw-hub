import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
@Component({
  selector: 'app-agents-page',
  imports: [RouterLink],
  styleUrl: '../inventory/inventory.scss',
  template: `<main id="operation-content" tabindex="-1">
    <header><div><p class="eyebrow">OLIVAW / OPERAÇÃO</p><h1>Agents</h1>
    <p>Relatórios e evidências dos agentes que cuidam do hub.</p></div></header>
    <nav class="resource-grid" aria-label="Agentes disponíveis">
      <a class="resource-card" routerLink="/admin/agents/baley">
        <span class="badge">SEGURANÇA</span><h2>Baley <span aria-hidden="true">↗</span></h2>
        <p>Auditorias do servidor Docker, acessos e recomendações para a operação.</p>
      </a>
    </nav>
  </main>`,
})
export class AgentsPage {}
