import { TestBed } from '@angular/core/testing';
import { HttpClient, HttpContext, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AutenticacaoStore } from '../auth/autenticacao.store';
import { HttpState } from './http-state';
import { authInterceptor, errorInterceptor, loadingInterceptor } from './interceptors';
import { SKIP_LOADING } from './http-context';

describe('Interceptors', () => {
  let http: HttpClient;
  let requests: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(
          withInterceptors([authInterceptor, errorInterceptor, loadingInterceptor]),
        ),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    requests = TestBed.inject(HttpTestingController);
    TestBed.inject(AutenticacaoStore).iniciar({
      accessToken: 'token',
      expiresAt: new Date(Date.now() + 60000).toISOString(),
      usuario: { usuarioId: '1', nome: 'Teste', root: false, autorizacoes: [] },
    });
  });
  afterEach(() => requests.verify());
  it('envia token apenas à origem e ao prefixo da API', () => {
    for (const url of [
      'http://localhost:5137/identity/dados',
      'http://localhost:51370/identity/dados',
      'https://externo.example/identity/dados',
    ]) {
      http.get(url).subscribe();
      const request = requests.expectOne(url);
      expect(request.request.headers.get('Authorization')).toBe(
        url === 'http://localhost:5137/identity/dados' ? 'Bearer token' : null,
      );
      request.flush({});
    }
  });
  it('mantém loading durante concorrência e libera após erro e cancelamento', () => {
    const state = TestBed.inject(HttpState);
    http.get('http://localhost:5137/a').subscribe({ error: () => undefined });
    const subscription = http.get('http://localhost:5137/b').subscribe();
    const a = requests.expectOne('http://localhost:5137/a');
    requests.expectOne('http://localhost:5137/b');
    expect(state.loading()).toBe(true);
    a.flush({}, { status: 500, statusText: 'Erro' });
    expect(state.loading()).toBe(true);
    subscription.unsubscribe();
    expect(state.loading()).toBe(false);
  });
  it('permite requisições sem loading', () => {
    http
      .get('http://localhost:5137/a', { context: new HttpContext().set(SKIP_LOADING, true) })
      .subscribe();
    expect(TestBed.inject(HttpState).loading()).toBe(false);
    requests.expectOne('http://localhost:5137/a').flush({});
  });
  it('invalida a sessão após 401 autenticado', () => {
    http.get('http://localhost:5137/a').subscribe({ error: () => undefined });
    requests
      .expectOne('http://localhost:5137/a')
      .flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(TestBed.inject(AutenticacaoStore).autenticacao()).toBeNull();
  });
});
