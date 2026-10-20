import { Component, computed, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SiteControls } from './site-controls';
import { SitePreview } from './site-preview';
import { SiteBlock, siteBlocks, sitePresets } from './site-studio-content';

@Component({
  selector: 'app-site-studio',
  imports: [RouterLink, SitePreview, SiteControls],
  templateUrl: './site-studio.html',
  styleUrl: './site-studio.scss',
})
export class SiteStudio {
  readonly presets = sitePresets;
  readonly options = siteBlocks;
  readonly activePreset = signal(sitePresets[0]);
  readonly selectedBlocks = signal<readonly SiteBlock[]>([]);
  readonly mobile = signal(false);
  readonly reviewing = signal(false);
  readonly notice = signal(
    'Uma primeira ideia. Escolha uma possibilidade para vê-la ganhar forma.',
  );
  readonly selectedLabels = computed(() =>
    siteBlocks
      .filter((block) => this.selectedBlocks().includes(block.id))
      .map((block) => block.label),
  );
  private readonly preview = viewChild(SitePreview);

  choosePreset(id: string): void {
    const preset = sitePresets.find((item) => item.id === id);
    if (!preset || preset.id === this.activePreset().id) return;
    this.preview()?.resetNavigation();
    this.activePreset.set(preset);
    this.selectedBlocks.set([]);
    this.reviewing.set(false);
    this.notice.set(
      `Vamos explorar: ${preset.label.toLowerCase()}. Escolha o que faz sentido para você.`,
    );
    this.preview()?.reveal('top');
  }
  toggleBlock(id: SiteBlock): void {
    const selected = this.selectedBlocks().includes(id);
    this.selectedBlocks.update((blocks) =>
      selected ? blocks.filter((block) => block !== id) : [...blocks, id],
    );
    const label = siteBlocks.find((block) => block.id === id)?.label ?? '';
    this.notice.set(`${label}: ${selected ? 'seção removida' : 'seção adicionada'} à sua prévia.`);
    if (!selected) {
      this.preview()?.flyPiece(id);
    } else {
      this.preview()?.resetNavigation();
    }
  }
  reset(): void {
    this.preview()?.resetNavigation();
    this.activePreset.set(sitePresets[0]);
    this.selectedBlocks.set([]);
    this.reviewing.set(false);
    this.mobile.set(false);
    this.notice.set('Voltamos ao começo. Sua base inicial está pronta para explorar.');
    this.preview()?.reveal('top');
  }
}
