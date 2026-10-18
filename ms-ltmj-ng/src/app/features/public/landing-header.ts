import { Component, signal } from '@angular/core';
import { LtmjBrand } from './ltmj-brand';
@Component({
  selector: 'app-landing-header',
  imports: [LtmjBrand],
  templateUrl: './landing-header.html',
  styles: `
    a {
      color: inherit;
      text-decoration: none;
    }
    a:hover {
      color: var(--brand-gold);
    }
    .mobile-toggle {
      border: 1px solid var(--mat-sys-outline);
      background: transparent;
      color: inherit;
      padding: 0.6rem 1rem;
      border-radius: 2rem;
    }
    .header-cta {
      display: inline-flex;
      background: var(--brand-gold);
      color: var(--brand-ink);
      border-radius: 2rem;
      padding: 0.8rem 1.2rem;
      font-size: 0.75rem;
      font-weight: 700;
    }
    a.header-cta:hover {
      background: var(--brand-cream);
      color: var(--brand-ink);
    }
  `,
})
export class LandingHeader {
  readonly menuOpen = signal(false);
}
