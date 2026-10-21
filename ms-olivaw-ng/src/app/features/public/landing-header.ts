import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
@Component({
  selector: 'app-landing-header',
  imports: [RouterLink],
  template: `<header class="border-b border-outline/20 bg-surface">
    <div class="mx-auto flex max-w-7xl items-center justify-between gap-6 px-6 py-5 lg:px-12">
      <a
        routerLink="/"
        aria-label="Olivaw — início"
        class="flex items-center gap-3 no-underline text-on-surface"
      >
        <img
          class="brand-mark"
          src="assets/brand/olivaw-avatar-web-v2.png"
          width="48"
          height="48"
          alt=""
          aria-hidden="true"
        />
        <span class="text-xl font-bold tracking-tight">Olivaw</span>
      </a>
      <nav aria-label="Navegação principal" class="hidden items-center gap-8 text-sm lg:flex">
        <a href="#solucoes">Soluções</a><a href="#clientes">Clientes</a
        ><a href="#como-funciona">Como funciona</a><a href="#sobre">Sobre nós</a>
        <a href="#contato" class="nav-cta">Contato <span aria-hidden="true">↗</span></a>
      </nav>
      <button
        type="button"
        class="rounded-lg border border-outline/30 bg-transparent px-4 py-2 text-sm lg:hidden"
        aria-controls="menu-mobile"
        [attr.aria-expanded]="open()"
        (click)="open.set(!open())"
      >
        {{ open() ? 'Fechar' : 'Menu' }}
      </button>
    </div>
    @if (open()) {
      <nav
        id="menu-mobile"
        aria-label="Navegação mobile"
        class="flex flex-col gap-5 border-t border-outline/20 px-6 py-6 lg:hidden"
      >
        <a href="#solucoes" (click)="open.set(false)">Soluções</a
        ><a href="#clientes" (click)="open.set(false)">Clientes</a
        ><a href="#como-funciona" (click)="open.set(false)">Como funciona</a>
        <a href="#sobre" (click)="open.set(false)">Sobre nós</a
        ><a href="#contato" (click)="open.set(false)">Contato</a>
      </nav>
    }
  </header>`,
  styles: `
    a {
      color: inherit;
      text-decoration: none;
    }
    a:hover {
      color: var(--mat-sys-primary);
    }
    .brand-mark {
      display: block;
      width: 3rem;
      height: 3rem;
      border-radius: 0;
      object-fit: contain;
      flex-shrink: 0;
    }
    .nav-cta {
      background: var(--action-background);
      color: var(--action-foreground);
      border: 1px solid var(--action-border);
      min-height: 44px;
      box-sizing: border-box;
      display: inline-flex;
      align-items: center;
      gap: 0.75rem;
      transition:
        background 0.2s,
        border-color 0.2s;
      padding: 0.65rem 1.1rem;
      border-radius: 2rem;
    }
    .nav-cta:hover {
      color: var(--action-foreground);
      background: var(--action-background-hover);
      border-color: var(--action-border);
    }
  `,
})
export class LandingHeader {
  readonly open = signal(false);
}
