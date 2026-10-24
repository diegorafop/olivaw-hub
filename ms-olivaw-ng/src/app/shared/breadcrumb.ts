import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
export interface BreadcrumbItem {
  label: string;
  url: string;
}
@Component({
  selector: 'app-breadcrumb',
  imports: [RouterLink],
  template: ` <nav aria-label="Caminho de navegação">
    <ol>
      @for (item of items(); track $index; let last = $last; let first = $first) {
        <li>
          @if (!first) {
            <span aria-hidden="true">/</span>
          }
          @if (last) {
            <strong aria-current="page">{{ item.label }}</strong>
          } @else {
            <a [routerLink]="item.url">{{ item.label }}</a>
          }
        </li>
      }
    </ol>
  </nav>`,
  styleUrl: './breadcrumb.scss',
})
export class Breadcrumb {
  readonly items = input.required<readonly BreadcrumbItem[]>();
}
