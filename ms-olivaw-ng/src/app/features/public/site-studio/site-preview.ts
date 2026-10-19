import {
  afterNextRender,
  Component,
  computed,
  DestroyRef,
  ElementRef,
  inject,
  Injector,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { animateSitePiece } from './site-piece-motion';
import { StudioEmpty } from './studio-empty';
import { StudioTable } from './studio-table';
import { SiteBlock, SitePreset } from './site-studio-content';

@Component({
  selector: 'app-site-preview',
  imports: [StudioTable, StudioEmpty],
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
  readonly activeTab = signal<'home' | 'spreadsheet'>('home');
  readonly assembling = signal(false);
  readonly pendingPiece = signal<SiteBlock | null>(null);
  readonly pieceLabels: Record<SiteBlock, string> = {
    services: 'Serviços',
    gallery: 'Trabalhos',
    products: 'Produtos',
    questions: 'Dúvidas',
    contact: 'Vamos conversar',
    spreadsheet: 'Produtos e estoque',
  };
  readonly menuBlocks: readonly SiteBlock[] = ['services', 'gallery', 'products', 'questions'];
  readonly assemblyMessage = computed(() => {
    const piece = this.pendingPiece();
    return piece
      ? `Encaixando ${this.pieceLabels[piece]} no site…`
      : 'Novo menu criado. Abrindo Produtos e estoque…';
  });
  openSection(block: SiteBlock): void {
    this.activeTab.set('home');
    this.reveal(block);
  }
  readonly spreadsheetReady = signal(true);
  private sequence = 0;
  private stopMotion?: () => void;
  private openTimer?: ReturnType<typeof setTimeout>;
  private readonly destroyRef = inject(DestroyRef);
  constructor() {
    this.destroyRef.onDestroy(() => this.resetNavigation());
  }
  resetNavigation(): void {
    this.sequence++;
    this.stopMotion?.();
    clearTimeout(this.openTimer);
    this.assembling.set(false);
    this.pendingPiece.set(null);
    this.spreadsheetReady.set(true);
    this.activeTab.set('home');
  }
  navigate(tab: 'home' | 'spreadsheet'): void {
    this.activeTab.set(tab);
    this.reveal('top');
  }
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
    this.resetNavigation();
    const current = this.sequence;
    this.assembling.set(true);
    this.pendingPiece.set(block);
    if (block === 'spreadsheet') this.spreadsheetReady.set(false);
    this.reveal('top');
    this.stopMotion = animateSitePiece(this.viewport().nativeElement, block, () => {
      if (current !== this.sequence) return;
      if (!this.has(block)) {
        this.assembling.set(false);
        this.spreadsheetReady.set(true);
        return;
      }
      this.pendingPiece.set(null);
      if (block !== 'spreadsheet') {
        this.assembling.set(false);
        this.openSection(block);
        return;
      }
      this.spreadsheetReady.set(true);
      const reduced = this.viewport().nativeElement.ownerDocument.defaultView?.matchMedia(
        '(prefers-reduced-motion: reduce)',
      ).matches;
      this.openTimer = setTimeout(
        () => {
          if (current !== this.sequence || !this.has('spreadsheet')) return;
          this.activeTab.set('spreadsheet');
          this.assembling.set(false);
          this.reveal('top');
        },
        reduced ? 0 : 750,
      );
    });
  }
  explore(): void {
    const first = this.primaryBlock();
    if (first) this.reveal(first);
  }
}
