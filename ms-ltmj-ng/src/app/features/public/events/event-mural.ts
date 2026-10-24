import { afterNextRender, Component, computed, inject, input, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { EventDetails } from './event-details';
import { EventCategory, PortfolioEvent } from './event-portfolio.data';
import { PortfolioService } from '../../portfolio/portfolio.service';
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
  private readonly api = inject(PortfolioService);
  readonly samples = input<PortfolioEvent[] | null>(null);
  get isPreview(): boolean {
    return this.samples() !== null;
  }
  readonly loading = signal(false);
  readonly error = signal(false);
  readonly hasMore = signal(false);
  private page = 0;
  constructor() {
    afterNextRender(() => {
      if (!this.samples()) void this.loadMore();
    });
  }
  async loadMore(): Promise<void> {
    if (this.loading()) return;
    this.loading.set(true);
    this.error.set(false);
    try {
      const result = await this.api.list(false, this.page + 1);
      this.events.update((current) => [
        ...current,
        ...result.items.map((event) => ({
          id: event.id,
          title: event.title,
          description: event.description,
          category: event.category,
          services: [],
          photos: event.photos.map((photo, index) => ({
            src: this.api.photoUrl(photo.url),
            alt: `${event.title} — foto ${index + 1}`,
          })),
        })),
      ]);
      this.page++;
      this.hasMore.set(result.items.length === 12);
    } catch {
      this.error.set(true);
    } finally {
      this.loading.set(false);
    }
  }
  readonly categories: ('Todos' | EventCategory)[] = [
    'Todos',
    'Casamentos',
    'Aniversários',
    'Confraternizações',
    'Corporativos',
  ];
  readonly selected = signal<'Todos' | EventCategory>('Todos');
  readonly events = signal<PortfolioEvent[]>([]);
  readonly visibleEvents = computed(() =>
    (this.samples() ?? this.events()).filter(
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
