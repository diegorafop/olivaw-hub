import { Component } from '@angular/core';
import { LandingHeader } from './landing-header';
import { LandingContact } from './landing-contact';
import { LandingClients } from './landing-clients';
@Component({
  selector: 'app-public-page',
  imports: [LandingHeader, LandingContact, LandingClients],
  templateUrl: './public-page.html',
  styleUrl: './public-page.scss',
})
export class PublicPage {
  readonly year = new Date().getFullYear();
}
