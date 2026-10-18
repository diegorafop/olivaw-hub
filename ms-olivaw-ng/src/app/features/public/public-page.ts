import { Component } from '@angular/core';
import { LandingHeader } from './landing-header';
import { ProjectContact } from './project-contact';
@Component({
  selector: 'app-public-page',
  imports: [LandingHeader, ProjectContact],
  templateUrl: './public-page.html',
  styleUrl: './public-page.scss',
})
export class PublicPage {
  readonly year = new Date().getFullYear();
}
