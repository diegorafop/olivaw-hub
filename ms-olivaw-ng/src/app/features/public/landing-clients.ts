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
      <div class="continuity" aria-hidden="true"><span></span><i></i></div>
    </div>
  `,
  styleUrl: './landing-clients.scss',
})
export class LandingClients {}
