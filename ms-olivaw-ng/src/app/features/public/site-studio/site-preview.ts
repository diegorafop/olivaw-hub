import {
  afterNextRender,
  Component,
  ElementRef,
  inject,
  Injector,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { StudioTable } from './studio-table';
import { SiteBlock, SitePreset, siteBlocks } from './site-studio-content';
@Component({
  selector: 'app-site-preview',
  imports: [StudioTable],
  templateUrl: './site-preview.html',
  styleUrl: './site-preview.scss',
})
export class SitePreview {
  readonly preset = input.required<SitePreset>();
  readonly blocks = input.required<readonly SiteBlock[]>();
  readonly mobile = input(false);
  readonly options = siteBlocks;
  readonly category = signal('Todos');
  readonly cartCount = signal(0);
  readonly paid = signal(false);
  private readonly viewport = viewChild.required<ElementRef<HTMLDivElement>>('viewport');
  private readonly injector = inject(Injector);
  has(block: SiteBlock): boolean {
    return this.blocks().includes(block);
  }
  resetNavigation(): void {
    this.category.set('Todos');
    this.cartCount.set(0);
    this.paid.set(false);
  }
  flyPiece(block: SiteBlock): void {
    this.reveal(block);
  }
  reveal(block: SiteBlock | 'top'): void {
    afterNextRender(
      () => {
        const viewport = this.viewport().nativeElement;
        const target = viewport.querySelector<HTMLElement>(`[data-block="${block}"]`);
        const reduced = viewport.ownerDocument.defaultView?.matchMedia(
          '(prefers-reduced-motion: reduce)',
        ).matches;
        viewport.scrollTo({
          top:
            block === 'top'
              ? 0
              : target
                ? viewport.scrollTop +
                  target.getBoundingClientRect().top -
                  viewport.getBoundingClientRect().top -
                  16
                : viewport.scrollTop,
          behavior: reduced ? 'instant' : 'smooth',
        });
      },
      { injector: this.injector },
    );
  }
}
