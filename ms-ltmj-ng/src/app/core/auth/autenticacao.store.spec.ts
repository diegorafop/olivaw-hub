import { TestBed } from '@angular/core/testing';
import { AutenticacaoStore } from './autenticacao.store';
describe('Estado autenticacao', () => {
  beforeEach(() => sessionStorage.clear());
  afterEach(() => sessionStorage.clear());
  it('valida permissões e rejeita sessão expirada', () => {
    const store = TestBed.inject(AutenticacaoStore);
    const sessao = {
      accessToken: 'token',
      expiresAt: new Date(Date.now() + 60000).toISOString(),
      usuario: {
        userProfileId: '1',
        name: 'Teste',
        isRoot: false,
        permissions: ['eventos.editar'],
      },
    };
    store.iniciar(sessao);
    expect(store.permite('eventos.editar')).toBe(true);
    expect(store.permite('usuarios.editar')).toBe(false);
    store.iniciar({ ...sessao, expiresAt: new Date(Date.now() - 1000).toISOString() });
    expect(store.tokenValido()).toBeNull();
    expect(store.permite('eventos.editar')).toBe(false);
  });
});

describe('Restauração na mesma aba', () => {
  const session = {
    accessToken: 'token-reload',
    expiresAt: new Date(Date.now() + 60000).toISOString(),
    usuario: {
      userProfileId: '1',
      name: 'Equipe',
      isRoot: false,
      permissions: ['portfolio.manage'],
    },
  };
  afterEach(() => sessionStorage.clear());
  it('restaura token válido e limpa ao sair', () => {
    sessionStorage.setItem('ltmj.session', JSON.stringify(session));
    const store = TestBed.inject(AutenticacaoStore);
    expect(store.tokenValido()).toBe('token-reload');
    expect(store.permite('portfolio.manage')).toBe(true);
    store.limpar();
    expect(sessionStorage.getItem('ltmj.session')).toBeNull();
  });
  it('descarta sessão vencida', () => {
    sessionStorage.setItem(
      'ltmj.session',
      JSON.stringify({ ...session, expiresAt: new Date(Date.now() - 1000).toISOString() }),
    );
    expect(TestBed.inject(AutenticacaoStore).tokenValido()).toBeNull();
    expect(sessionStorage.getItem('ltmj.session')).toBeNull();
  });
  it('descarta armazenamento inválido', () => {
    sessionStorage.setItem('ltmj.session', '{broken');
    expect(TestBed.inject(AutenticacaoStore).autenticacao()).toBeNull();
  });
});
