import { HttpClient } from '@angular/common/http';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Subject, of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { TrevizeDashboard } from './trevize-dashboard';
import { LeadsApi, type LeadPage } from '../leads/leads-api';
const item = {
  externalId: 'lead-one',
  createdAt: new Date().toISOString(),
  updatedAt: null,
  business: 'Eventos Aurora',
  title: 'Site',
  entry: 'prospecting',
  email: '',
  whatsapp: '',
  followUpStatus: 'conversation',
};
const page = { page: 1, pageSize: 20, total: 2, asOf: new Date().toISOString(), items: [item] };
function setup(list = vi.fn().mockReturnValue(of(page))) {
  TestBed.configureTestingModule({
    imports: [TrevizeDashboard],
    providers: [
      provideRouter([]),
      { provide: LeadsApi, useValue: { list } },
      {
        provide: HttpClient,
        useValue: {
          get: vi
            .fn()
            .mockReturnValue(
              of({ inspected: 1, truncated: false, asOf: new Date().toISOString(), counts: {} }),
            ),
        },
      },
      { provide: RuntimeConfig, useValue: { endpoint: (path: string) => path } },
    ],
  });
  return { fixture: TestBed.createComponent(TrevizeDashboard), list };
}
describe('Dashboard de captação', () => {
  it('exibe visão gerencial e permite navegar para a ficha sem modal', async () => {
    const { fixture, list } = setup();
    await fixture.whenStable();
    const element: HTMLElement = fixture.nativeElement;
    expect(list).toHaveBeenCalledWith(expect.objectContaining({ entry: 'prospecting', page: 1 }));
    expect(element.textContent).toContain('Com avanço comercial');
    expect(element.textContent).toContain('Inclui captação manual');
    expect(element.querySelector('a[href="/admin/leads/lead-one/details"]')).not.toBeNull();
    expect(element.querySelectorAll('app-agent-chart')).toHaveLength(2);
  });
  it('não duplica registros ao carregar outra página', () => {
    const { fixture, list } = setup();
    const component = fixture.componentInstance;
    component.load(false);
    expect(list).toHaveBeenLastCalledWith(expect.objectContaining({ page: 2 }));
    expect(component.items()).toHaveLength(1);
  });
  it('bloqueia consultas concorrentes e permite tentar novamente após erro', async () => {
    const pending = new Subject<LeadPage>();
    const list = vi.fn().mockReturnValue(pending);
    const { fixture } = setup(list);
    fixture.componentInstance.load(true);
    expect(list).toHaveBeenCalledTimes(1);
    pending.error(new Error('offline'));
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).not.toBeNull();
    list.mockReturnValue(of(page));
    fixture.componentInstance.load(true);
    expect(fixture.componentInstance.error()).toBe(false);
    expect(fixture.componentInstance.items()).toHaveLength(1);
  });
  it('não perde os contatos carregados quando a próxima página falha', () => {
    const { fixture, list } = setup();
    list.mockReturnValue(throwError(() => new Error('offline')));
    fixture.componentInstance.load(false);
    expect(fixture.componentInstance.items()).toHaveLength(1);
    expect(fixture.componentInstance.error()).toBe(true);
  });
});
