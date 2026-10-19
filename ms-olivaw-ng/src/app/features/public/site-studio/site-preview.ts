import {
  afterNextRender,
  Component,
  computed,
  ElementRef,
  inject,
  Injector,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { StudioTable } from './studio-table';
import { SiteArt } from './site-art';
import { SiteBlock, SitePreset } from './site-studio-content';

@Component({
  selector: 'app-site-preview',
  imports: [SiteArt, StudioTable],
  templateUrl: './site-preview.html',
  styleUrl: './site-preview.scss',
})
export class SitePreview {
  readonly preset = input.required<SitePreset>();
  readonly blocks = input.required<readonly SiteBlock[]>();
  readonly mobile = input(false);
  readonly primaryBlock = computed(() => {
    const preferred = this.preset().blocks[0];
    if (preferred && this.has(preferred)) return preferred;
    const order: readonly SiteBlock[] = ['services', 'gallery', 'products', 'questions', 'contact'];
    return order.find((block) => this.has(block));
  });
  readonly primaryLabel = computed(() => {
    const labels: Record<SiteBlock, string> = {
      services: 'Conheça nossos serviços',
      gallery: 'Explore os trabalhos',
      products: 'Conheça a seleção',
      questions: 'Tire suas dúvidas',
      contact: 'Vamos conversar',
      spreadsheet: 'Explore a tabela',
    };
    const block = this.primaryBlock();
    return block ? labels[block] : 'Explore seu site';
  });
  readonly openProduct = signal<number | null>(null);
  readonly contactOpen = signal(false);
  private readonly viewport = viewChild.required<ElementRef<HTMLDivElement>>('viewport');
  private readonly injector = inject(Injector);

  has(block: SiteBlock): boolean {
    return this.blocks().includes(block);
  }
  reveal(block: SiteBlock | 'top'): void {
    afterNextRender(
      () => {
        const viewport = this.viewport().nativeElement;
        const target = viewport.querySelector<HTMLElement>(`[data-block="${block}"]`);
        const reduced = viewport.ownerDocument.defaultView?.matchMedia(
          '(prefers-reduced-motion: reduce)',
        ).matches;
        const top =
          block === 'top'
            ? 0
            : target
              ? viewport.scrollTop +
                target.getBoundingClientRect().top -
                viewport.getBoundingClientRect().top -
                16
              : viewport.scrollTop;
        viewport.scrollTo({ top, behavior: reduced ? 'instant' : 'smooth' });
      },
      { injector: this.injector },
    );
  }
  flyPiece(block: SiteBlock): void {
    const viewport = this.viewport().nativeElement;
    const doc = viewport.ownerDocument;
    if (doc.defaultView?.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    const source = doc.activeElement?.getBoundingClientRect();
    if (!source) return;
    const destination = viewport.getBoundingClientRect();
    const piece = doc.createElement('span');
    piece.textContent = block === 'spreadsheet' ? '▦' : '✳';
    Object.assign(piece.style, {
      position: 'fixed', left: source.left + 'px', top: source.top + 'px',
      width: '56px', height: '56px', display: 'grid', placeItems: 'center',
      background: '#162c39', color: '#fff', borderRadius: '16px',
      fontSize: '32px', zIndex: '1000', pointerEvents: 'none',
      boxShadow: '0 12px 32px #162c3933',
    });
    doc.body.append(piece);
    const animation = piece.animate([
      { transform: 'translate(0,0) scale(1)', opacity: 1 },
      { transform: `translate(${destination.left + destination.width / 2 - source.left}px,${destination.top + 90 - source.top}px) scale(.45) rotate(12deg)`, opacity: 0 },
    ], { duration: 650, easing: 'cubic-bezier(.22,.8,.25,1)' });
    animation.onfinish = () => piece.remove();
    animation.oncancel = () => piece.remove();
  }
  explore(): void {
    const first = this.primaryBlock();
    if (first) this.reveal(first);
  }
}
