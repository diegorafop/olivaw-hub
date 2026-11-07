import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { AgentsApi } from './agents-api';
import { BaleyReports } from './baley-reports';
describe('Visão gerencial do Baley', () => {
  it('mostra falha como coleta incompleta e gráficos antes da lista', async () => {
    TestBed.configureTestingModule({
      imports: [BaleyReports],
      providers: [
        provideRouter([]),
        {
          provide: AgentsApi,
          useValue: {
            list: () =>
              of({
                items: [
                  {
                    externalId: 'report',
                    runId: 'run',
                    agentId: 'baley',
                    hostId: 'host',
                    executionStatus: 'failed',
                    completedAt: new Date().toISOString(),
                    createdAt: new Date().toISOString(),
                    rawData: '{}',
                  },
                ],
                nextBeforeId: null,
              }),
          },
        },
      ],
    });
    const fixture = TestBed.createComponent(BaleyReports);
    await fixture.whenStable();
    expect(fixture.componentInstance.metrics().incomplete).toBe(1);
    expect(fixture.componentInstance.metrics().elevated).toBe(0);
    const element: HTMLElement = fixture.nativeElement;
    expect(element.querySelectorAll('app-agent-chart')).toHaveLength(2);
    expect(element.querySelector('.metrics')).not.toBeNull();
    expect(element.querySelector('a[href="/admin/agents/baley/report"]')).not.toBeNull();
    expect(element.textContent).not.toContain('Safe');
  });
});
