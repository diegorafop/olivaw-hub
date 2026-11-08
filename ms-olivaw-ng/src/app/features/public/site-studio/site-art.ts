import { Component, input } from '@angular/core';
@Component({
  selector: 'app-site-art',
  templateUrl: './site-art.html',
  styleUrl: './site-art.scss',
})
export class SiteArt {
  readonly variant = input('business');
}
