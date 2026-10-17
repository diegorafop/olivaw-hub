import { Component } from '@angular/core';
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
  readonly contact = LTMJ_CONTACT;
  readonly year = new Date().getFullYear();
}
