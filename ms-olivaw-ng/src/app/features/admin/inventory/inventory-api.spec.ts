import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { InventoryApi, inventoryError } from './inventory-api';
import { ServicesApi } from '../services/services-api';
import { RuntimeConfig } from '../../../core/config/runtime-config';

describe('Contratos HTTP do inventário', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    TestBed.inject(RuntimeConfig).apiUrl = 'https://example.test/base';
  });
  afterEach(() => TestBed.inject(HttpTestingController).verify());
  it('omite filtros vazios e preserva false, cliente e ambiente', () => {
    TestBed.inject(InventoryApi)
      .resources(2, {
        search: '',
        type: '',
        shared: 'false',
        customerExternalId: 'customer-id',
        environment: 'production',
      })
      .subscribe();
    const request = TestBed.inject(HttpTestingController).expectOne((call) =>
      call.url.endsWith('/resources'),
    );
    expect(request.request.params.has('search')).toBe(false);
    expect(request.request.params.has('type')).toBe(false);
    expect(request.request.params.get('shared')).toBe('false');
    expect(request.request.params.get('customerExternalId')).toBe('customer-id');
    expect(request.request.params.get('page')).toBe('2');
    request.flush({ page: 2, pageSize: 20, items: [], total: 0 });
  });
  it('envia período encerrado, revisão e IDs escapados sem autor no body', () => {
    TestBed.inject(InventoryApi).closeLink('dependency', 'id/with spaces', 7).subscribe();
    const request = TestBed.inject(HttpTestingController).expectOne(
      'https://example.test/base/resource-dependencies/id%2Fwith%20spaces',
    );
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ revision: 7 });
    request.flush({ externalId: 'id', revision: 8 });
  });
  it('consulta vínculos com paginação e false explícito', () => {
    TestBed.inject(InventoryApi)
      .links(3, 'deployment', { deploymentExternalId: 'deployment-id' }, false)
      .subscribe();
    const request = TestBed.inject(HttpTestingController).expectOne((call) =>
      call.url.endsWith('/inventory-links'),
    );
    expect(request.request.params.get('includeEnded')).toBe('false');
    expect(request.request.params.get('deploymentExternalId')).toBe('deployment-id');
    expect(request.request.params.has('resourceExternalId')).toBe(false);
    request.flush({ page: 3, pageSize: 20, total: 0, items: [] });
  });
  it('serviços também omitem filtros vazios para evitar HTTP 400', () => {
    TestBed.inject(ServicesApi).list(1, '', '', '').subscribe();
    const request = TestBed.inject(HttpTestingController).expectOne((call) =>
      call.url.endsWith('/services'),
    );
    expect(request.request.params.keys()).toEqual(['page', 'pageSize']);
    request.flush({ page: 1, pageSize: 20, total: 0, items: [] });
  });
  it('exibe validações sem esconder a finalidade ou a reserva inválida', () => {
    expect(
      inventoryError({
        error: {
          errors: { Purpose: ['Informe a finalidade.'], ReservedCpu: ['Reserva inválida.'] },
        },
      }),
    ).toBe('Informe a finalidade. Reserva inválida.');
    expect(inventoryError({ error: { detail: 'O inventário mudou.' } })).toBe(
      'O inventário mudou.',
    );
  });
});
