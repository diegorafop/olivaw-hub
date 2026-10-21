import { Component, computed, inject, signal, viewChild } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ProjectDraft } from '../project-folder/project-draft';
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
  private readonly draft = inject(ProjectDraft);
  private readonly router = inject(Router);
  readonly presets = sitePresets;
  readonly options = siteBlocks;
  readonly activePreset = signal(
    sitePresets.find((preset) => preset.id === this.draft.seed()?.presetId) ?? sitePresets[0],
  );
  readonly selectedBlocks = signal<readonly SiteBlock[]>(
    siteBlocks
      .filter((block) => this.draft.seed()?.blockIds?.includes(block.id))
      .map((block) => block.id),
  );
  readonly mobile = signal(false);
  readonly notice = signal(
    'Uma primeira ideia. Escolha uma possibilidade para vê-la ganhar forma.',
  );
  readonly selectedLabels = computed(() =>
    siteBlocks
      .filter((block) => this.selectedBlocks().includes(block.id))
      .map((block) => block.label),
  );
  private readonly preview = viewChild(SitePreview);

  constructor() {
    this.draft.enter('studio');
    this.rememberStudio();
  }
  private rememberStudio(): void {
    this.draft.seed.set({
      source: 'studio',
      title: this.activePreset().label,
      choices: this.selectedLabels(),
      context:
        'Uma primeira ideia explorada no estúdio. Vamos descobrir juntos o que vale construir.',
      presetId: this.activePreset().id,
      blockIds: this.selectedBlocks(),
    });
  }
  openProject(): void {
    this.draft.start({
      source: 'studio',
      title: this.activePreset().label,
      choices: this.selectedLabels(),
      context:
        'Uma primeira ideia explorada no estúdio. Vamos descobrir juntos o que vale construir.',
      presetId: this.activePreset().id,
      blockIds: this.selectedBlocks(),
    });
    void this.router.navigate(['/projeto']);
  }
  choosePreset(id: string): void {
    const preset = sitePresets.find((item) => item.id === id);
    if (!preset || preset.id === this.activePreset().id) return;
    this.preview()?.resetNavigation();
    this.activePreset.set(preset);
    this.selectedBlocks.set([]);
    this.draft.record('intention-selected', preset.id);
    this.rememberStudio();
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
    this.draft.record(selected ? 'option-removed' : 'option-added', id);
    this.rememberStudio();
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
    this.mobile.set(false);
    this.rememberStudio();
    this.draft.record('studio-reset', 'business');
    this.notice.set('Voltamos ao começo. Sua base inicial está pronta para explorar.');
    this.preview()?.reveal('top');
  }
}
