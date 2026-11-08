import { Component, DestroyRef, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Meta } from '@angular/platform-browser';
import { PORTFOLIO_SAMPLES } from './events/portfolio-samples.data';
import { LandingHeader } from './landing-header';
import { LtmjBrand } from './ltmj-brand';
import { EventMural } from './events/event-mural';
import { QuoteBuilder } from './quote-builder';
import { LTMJ_CONTACT } from './ltmj-content';
@Component({
  selector: 'app-public-page',
  imports: [QuoteBuilder, LandingHeader, LtmjBrand, EventMural],
  templateUrl: './public-page.html',
  styleUrl: './public-page.scss',
})
export class PublicPage {
  readonly portfolioSamples = inject(ActivatedRoute).snapshot.data['portfolioPreview']
    ? PORTFOLIO_SAMPLES
    : null;
  constructor() {
    if (this.portfolioSamples) {
      const meta = inject(Meta);
      const previous = meta.getTag('name="robots"')?.content;
      meta.updateTag({ name: 'robots', content: 'noindex, nofollow' });
      inject(DestroyRef).onDestroy(() => {
        if (previous) meta.updateTag({ name: 'robots', content: previous });
        else meta.removeTag('name="robots"');
      });
    }
  }
  readonly contact = LTMJ_CONTACT;
  readonly year = new Date().getFullYear();
}
