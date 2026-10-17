import { TestBed } from '@angular/core/testing';
import { AutenticacaoStore } from './autenticacao.store';
describe('Estado autenticacao', () => {
  it('valida permissões e rejeita sessão expirada', () => {
    const store = TestBed.inject(AutenticacaoStore);
    const sessao = {
      accessToken: 'token',
      expiresAt: new Date(Date.now() + 60000).toISOString(),
      usuario: { usuarioId: '1', nome: 'Teste', root: false, autorizacoes: ['eventos.editar'] },
    };
    store.iniciar(sessao);
    expect(store.permite('eventos.editar')).toBe(true);
    expect(store.permite('usuarios.editar')).toBe(false);
    store.iniciar({ ...sessao, expiresAt: new Date(Date.now() - 1000).toISOString() });
    expect(store.tokenValido()).toBeNull();
    expect(store.permite('eventos.editar')).toBe(false);
  });
});
