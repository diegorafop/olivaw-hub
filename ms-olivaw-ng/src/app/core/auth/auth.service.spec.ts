import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { SessionStorage } from './session-storage';
import { AutenticacaoStore } from './autenticacao.store';
import { RuntimeConfig } from '../config/runtime-config';

describe('Restauração de sessão', () => {
  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
  });
  afterEach(() => {
    TestBed.inject(AuthService).clear();
    TestBed.inject(HttpTestingController).verify();
  });
  it('revalida o token e carrega permissões atuais antes de restaurar', async () => {
    const saved = { accessToken: 'token', expiresAt: new Date(Date.now() + 60000).toISOString() };
    TestBed.inject(SessionStorage).save(saved);
    const promise = TestBed.inject(AuthService).restore();
    const request = TestBed.inject(HttpTestingController).expectOne(
      TestBed.inject(RuntimeConfig).endpoint('identity/authentication/access'),
    );
    expect(request.request.headers.get('Authorization')).toBe('Bearer token');
    request.flush({
      name: 'Operador',
      userProfileId: '1',
      isRoot: false,
      permissions: ['leads.manage'],
    });
    await promise;
    const store = TestBed.inject(AutenticacaoStore);
    expect(store.tokenValido()).toBe('token');
    expect(store.permite('leads.manage')).toBe(true);
    expect(store.usuario()?.isRoot).toBe(false);
  });
  it('remove a sessão quando a API rejeita o token', async () => {
    TestBed.inject(SessionStorage).save({
      accessToken: 'revoked',
      expiresAt: new Date(Date.now() + 60000).toISOString(),
    });
    const promise = TestBed.inject(AuthService).restore();
    TestBed.inject(HttpTestingController)
      .expectOne(TestBed.inject(RuntimeConfig).endpoint('identity/authentication/access'))
      .flush({}, { status: 401, statusText: 'Unauthorized' });
    await promise;
    expect(TestBed.inject(AutenticacaoStore).tokenValido()).toBeNull();
    expect(TestBed.inject(SessionStorage).read()).toBeNull();
  });
  it('não consulta a API quando não existe token válido', async () => {
    await TestBed.inject(AuthService).restore();
    expect(TestBed.inject(AutenticacaoStore).tokenValido()).toBeNull();
  });
});
