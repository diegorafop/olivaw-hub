import { Component, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { PortfolioEvent } from './event-portfolio.data';
@Component({
  selector: 'app-event-details',
  imports: [MatDialogModule, MatButtonModule],
  template: `<h2 mat-dialog-title>{{ event.title }}</h2>
    <mat-dialog-content>
      <p class="mb-5 text-sm text-primary">
        {{ event.category }}
        @if (event.preview) {
          · Modelo de apresentação
        }
      </p>
      @if (event.photos[activePhoto()]; as photo) {
        <figure>
          <img [src]="photo.src" [alt]="photo.alt" width="1000" height="700" class="event-photo" />
          @if (photo.caption) {
            <figcaption class="mt-2 text-sm text-on-surface/70">{{ photo.caption }}</figcaption>
          }
        </figure>
        @if (event.photos.length > 1) {
          <div
            class="mt-3 flex flex-wrap items-center justify-between gap-2"
            aria-label="Navegação das fotos"
          >
            <button mat-button type="button" (click)="previous()">Foto anterior</button>
            <span role="status" class="text-sm"
              >{{ activePhoto() + 1 }} de {{ event.photos.length }}</span
            >
            <button mat-button type="button" (click)="next()">Próxima foto</button>
          </div>
        }
      } @else {
        <div class="photo-placeholder">
          <span aria-hidden="true">✦</span><strong>As fotos dão vida à história.</strong>
          <p>Este espaço receberá o acervo aprovado do evento.</p>
        </div>
      }
      <p class="my-6 leading-relaxed">{{ event.description }}</p>
      @if (event.location || event.dateLabel) {
        <p class="mb-5 text-sm">
          {{ event.location }}
          @if (event.location && event.dateLabel) {
            ·
          }
          {{ event.dateLabel }}
        </p>
      }
      <h3 class="mb-3 text-sm font-bold">
        {{ event.preview ? 'Exemplo de serviços que poderão aparecer' : 'A participação da LTMJ' }}
      </h3>
      <ul class="mb-5 flex list-none flex-wrap gap-2 p-0">
        @for (service of event.services; track service) {
          <li class="rounded-full border border-outline/40 px-3 py-2 text-xs">{{ service }}</li>
        }
      </ul>
      @if (event.preview) {
        <p class="text-sm text-on-surface/65">
          Prévia de layout. Este cartão não representa um evento realizado, cliente atendido ou
          contratação confirmada.
        </p>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end"
      ><button mat-button mat-dialog-close type="button">Fechar</button
      ><a mat-flat-button href="#orcamento" (click)="dialog.close()"
        >Planejar meu evento</a
      ></mat-dialog-actions
    >`,
  styles: `
    .event-photo {
      display: block;
      width: 100%;
      max-height: 26rem;
      object-fit: contain;
      border-radius: 0.75rem;
      background: var(--brand-ink);
    }
    .photo-placeholder {
      min-height: 13rem;
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
      gap: 0.8rem;
      text-align: center;
      padding: 1.5rem;
      border: 1px solid #d4b77940;
      border-radius: 0.75rem;
      background: var(--brand-ink);
    }
    .photo-placeholder > span {
      color: var(--brand-gold);
      font-size: 2rem;
    }
    .photo-placeholder p {
      font-size: 0.85rem;
      color: var(--mat-sys-on-surface-variant);
    }
  `,
})
export class EventDetails {
  readonly event = inject<PortfolioEvent>(MAT_DIALOG_DATA);
  readonly dialog = inject(MatDialogRef<EventDetails>);
  readonly activePhoto = signal(0);
  previous(): void {
    this.activePhoto.update(
      (index) => (index - 1 + this.event.photos.length) % this.event.photos.length,
    );
  }
  next(): void {
    this.activePhoto.update((index) => (index + 1) % this.event.photos.length);
  }
}
