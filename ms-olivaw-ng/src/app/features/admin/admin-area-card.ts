import { Component, input } from '@angular/core';
@Component({
  selector: 'app-admin-area-card',
  templateUrl: './admin-area-card.html',
  styleUrl: './admin-area-card.scss',
})
export class AdminAreaCard {
  readonly area = input.required<{
    number: string;
    title: string;
    status: string;
    text: string;
    icon: string;
  }>();
}
