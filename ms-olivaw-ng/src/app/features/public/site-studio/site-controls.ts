import { Component, computed, input, output } from '@angular/core';
import { SiteBlock, SitePreset, siteBlocks, sitePresets } from './site-studio-content';
@Component({
  selector: 'app-site-controls',
  templateUrl: './site-controls.html',
  styleUrl: './site-controls.scss',
})
export class SiteControls {
  readonly presets = sitePresets;
  readonly options = siteBlocks;
  readonly activePreset = input.required<SitePreset>();
  readonly selectedBlocks = input.required<readonly SiteBlock[]>();
  readonly reviewing = input(false);
  readonly presetChange = output<string>();
  readonly blockToggle = output<SiteBlock>();
  readonly resetRequested = output<void>();
  readonly reviewChange = output<boolean>();
  readonly selectedLabels = computed(() =>
    siteBlocks
      .filter((block) => this.selectedBlocks().includes(block.id))
      .map((block) => block.label),
  );
}
