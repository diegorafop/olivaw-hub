import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  provideRouter,
  Router,
  RouterStateSnapshot,
  Route,
} from '@angular/router';
import { authGuard, permissionGuard } from './auth.guard';
import { AutenticacaoStore } from './autenticacao.store';

describe('Proteção da gestão', () => {
  beforeEach(() => TestBed.configureTestingModule({ providers: [provideRouter([])] }));
  function session(expired = false, permissions: string[] = []) {
    TestBed.inject(AutenticacaoStore).iniciar({
      accessToken: 'token',
      expiresAt: new Date(Date.now() + (expired ? -1000 : 60000)).toISOString(),
      usuario: { usuarioId: '1', nome: 'Equipe', root: false, autorizacoes: permissions },
    });
  }
  function match() {
    return TestBed.runInInjectionContext(() =>
      authGuard({} as Route, [], {} as Parameters<typeof authGuard>[2]),
    );
  }
  it('redireciona sem sessão', () => {
    expect(match()).toEqual(TestBed.inject(Router).createUrlTree(['/gestao/entrar']));
  });
  it('rejeita sessão expirada', () => {
    session(true);
    expect(match()).toEqual(TestBed.inject(Router).createUrlTree(['/gestao/entrar']));
  });
  it('aceita sessão válida', () => {
    session();
    expect(match()).toBe(true);
  });
  it('exige permissão de portfólio', () => {
    session();
    const route = { data: { permissao: 'portfolio.manage' } } as unknown as ActivatedRouteSnapshot;
    const result = TestBed.runInInjectionContext(() =>
      permissionGuard(route, {} as RouterStateSnapshot),
    );
    expect(result).toEqual(TestBed.inject(Router).createUrlTree(['/acesso-negado']));
    session(false, ['portfolio.manage']);
    expect(
      TestBed.runInInjectionContext(() => permissionGuard(route, {} as RouterStateSnapshot)),
    ).toBe(true);
  });
});
