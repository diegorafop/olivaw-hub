import { Component } from '@angular/core';
import { TrevizeCommunicationMetrics } from './trevize-communication-metrics';
@Component({
  selector: 'app-trevize-dashboard',
  imports: [TrevizeCommunicationMetrics],
  template: `<main id="operation-content" tabindex="-1">
    <h1>Trevize</h1>
    <app-trevize-communication-metrics />
  </main>`,
})
export class TrevizeDashboard {}
