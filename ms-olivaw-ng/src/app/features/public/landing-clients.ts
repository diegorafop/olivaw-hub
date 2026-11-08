import { Component } from '@angular/core';

@Component({
  selector: 'app-landing-clients',
  template: `
    <div class="band-heading">
      <p class="eyebrow">Quem confia na Olivaw</p>
      <h2>Quem já constrói<br />com a gente.</h2>
    </div>
    <div class="client-band">
      <a
        class="client-logo"
        href="https://ltmj.olivaw.com.br/"
        target="_blank"
        rel="noopener noreferrer"
        aria-label="Visitar o site da LTMJ Eventos (abre em nova aba)"
      >
        <h3 aria-label="LTMJ Eventos">LTMJ</h3>
        <span class="logo-caption">GASTRONOMIA & EVENTOS</span>
        <span class="visit">Visitar o site <span aria-hidden="true">↗</span></span>
      </a>
      <a
        class="client-logo client-logo--cavalcante"
        href="https://cavalcante.olivaw.com.br/"
        target="_blank"
        rel="noopener noreferrer"
        aria-label="Visitar o site da Cavalcante Terraplanagem (abre em nova aba)"
      >
        <span class="cavalcante-wordmark"
          ><img src="/assets/clients/cavalcante-tractor.svg" width="40" height="40" alt="" />
          <h3 aria-label="Cavalcante Terraplanagem">CAVALCANTE</h3></span
        >
        <span class="logo-caption">TERRAPLENAGEM & LOCAÇÃO</span>
        <span class="visit">Visitar o site <span aria-hidden="true">↗</span></span>
      </a>
    </div>
  `,
  styleUrl: './landing-clients.scss',
})
export class LandingClients {}
