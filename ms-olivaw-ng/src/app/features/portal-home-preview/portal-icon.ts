import { Component, input } from '@angular/core';
const paths: Record<string, string> = {
  home: 'M3 10 12 3l9 7M5 9v12h5v-7h4v7h5V9',
  message: 'M21 11a8 8 0 0 1-8 8H6l-4 3V7a5 5 0 0 1 5-5h6a8 8 0 0 1 8 9ZM7 8h9M7 12h6',
  spark: 'm12 3 2.5 6.5L21 12l-6.5 2.5L12 21l-2.5-6.5L3 12l6.5-2.5L12 3Z',
  plan: 'M4 5h16v16H4zM8 2v6M16 2v6M4 11h16M8 15h3M8 18h7',
  code: 'm8 6-6 6 6 6M16 6l6 6-6 6M14 3l-4 18',
  arrow: 'M5 12h14m-6-6 6 6-6 6',
  external: 'M14 3h7v7m0-7L10 14M10 3H3v18h18v-7',
  disk: 'M5 4h14l3 12v4H2v-4L5 4ZM2 15h20M6 18h.01M10 18h.01',
  activity: 'M3 12h4l3-8 4 16 3-8h4',
  globe: 'M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0ZM3 12h18M12 3a17 17 0 0 1 0 18 17 17 0 0 1 0-18Z',
  check: 'm6 12 4 4 8-9',
  clock: 'M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0ZM12 7v5l3 2',
  info: 'M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0ZM12 11v6M12 7h.01',
  chevron: 'm9 5 7 7-7 7',
  close: 'm6 6 12 12M6 18 18 6',
  menu: 'M4 6h16M4 12h16M4 18h16',
};
@Component({
  selector: 'app-portal-icon',
  template: `<svg
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    stroke-width="1.6"
    stroke-linecap="round"
    stroke-linejoin="round"
    aria-hidden="true"
  >
    <path [attr.d]="path()" />
  </svg>`,
  styles:
    ':host { display:inline-flex; width:20px; height:20px; flex-shrink:0; } svg { width:100%; height:100%; }',
})
export class PortalIcon {
  readonly name = input('home');
  path() {
    return paths[this.name()] ?? paths['info'];
  }
}
