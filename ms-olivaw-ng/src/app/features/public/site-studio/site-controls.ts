import { Component, computed, input, output } from '@angular/core';
import { SiteBlock, SitePreset, siteBlocks, sitePresets } from './site-studio-content';
@Component({
  selector: 'app-site-controls',
  templateUrl: './site-controls.html',
  styleUrl: './site-controls.scss',
})
export class SiteControls {
  readonly presets = sitePresets;
  readonly options = computed(() =>
    siteBlocks.filter((block) => this.activePreset().blocks.includes(block.id)),
  );
  readonly activePreset = input.required<SitePreset>();
  readonly selectedBlocks = input.required<readonly SiteBlock[]>();
  readonly presetChange = output<string>();
  readonly blockToggle = output<SiteBlock>();
  readonly resetRequested = output<void>();
  readonly continueRequested = output<void>();
}
