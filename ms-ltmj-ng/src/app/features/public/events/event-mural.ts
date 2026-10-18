import { Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { EventDetails } from './event-details';
import {
  PUBLISHED_EVENTS,
  PREVIEW_EVENTS,
  EventCategory,
  PortfolioEvent,
} from './event-portfolio.data';
import { LTMJ_CONTACT } from '../ltmj-content';
@Component({
  selector: 'app-event-mural',
  imports: [MatButtonModule],
  templateUrl: './event-mural.html',
  styleUrl: './event-mural.scss',
})
export class EventMural {
  private readonly dialog = inject(MatDialog);
  readonly contact = LTMJ_CONTACT;
  readonly isPreview = PUBLISHED_EVENTS.length === 0;
  readonly categories: ('Todos' | EventCategory)[] = [
    'Todos',
    'Casamentos',
    'Aniversários',
    'Confraternizações',
    'Corporativos',
  ];
  readonly selected = signal<'Todos' | EventCategory>('Todos');
  readonly events = this.isPreview ? PREVIEW_EVENTS : PUBLISHED_EVENTS;
  readonly visibleEvents = computed(() =>
    this.events.filter(
      (event) => this.selected() === 'Todos' || event.category === this.selected(),
    ),
  );
  open(event: PortfolioEvent): void {
    this.dialog.open(EventDetails, {
      data: event,
      width: '760px',
      maxWidth: 'calc(100vw - 32px)',
      autoFocus: 'first-heading',
    });
  }
}
