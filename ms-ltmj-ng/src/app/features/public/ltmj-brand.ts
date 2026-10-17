import { Component, input } from '@angular/core';
@Component({
  selector: 'app-ltmj-brand',
  template: `@if (portrait()) {
      <figure>
        <img
          src="assets/ltmj/perfil-facebook.jpg"
          alt="Logotipo oficial LTMJ Eventos, dourado sobre fundo preto, com símbolo de churrasqueira"
          width="720"
          height="720"
          fetchpriority="high"
        />
        <figcaption>Experiências que ficam. Momentos que marcam.</figcaption>
      </figure>
    } @else {
      <span class="name">LTMJ</span><span class="caption">GASTRONOMIA & EVENTOS</span>
    }`,
  styles: `
    :host {
      display: flex;
      flex-direction: column;
    }
    .name {
      font:
        2.1rem Georgia,
        serif;
      letter-spacing: 0.22em;
      color: var(--brand-gold);
    }
    .caption {
      font-size: 0.5rem;
      letter-spacing: 0.22em;
      margin-top: 0.3rem;
    }
    figure {
      margin: 0;
      text-align: center;
      border: 1px solid #d4b77940;
      padding: 1.2rem;
      border-radius: 13rem 13rem 1.5rem 1.5rem;
      background: #000;
      overflow: hidden;
    }
    img {
      display: block;
      width: 100%;
      height: auto;
      border-radius: 12rem 12rem 0 0;
    }
    figcaption {
      font-family: Georgia, serif;
      font-size: 0.9rem;
      color: var(--brand-gold);
      padding: 0.5rem 0 1rem;
    }
  `,
})
export class LtmjBrand {
  readonly portrait = input(false);
}
