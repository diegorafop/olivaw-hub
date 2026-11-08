import { TestBed } from '@angular/core/testing';
import { PLATFORM_ID } from '@angular/core';
import { SessionStorage } from './session-storage';
import { RuntimeConfig } from '../config/runtime-config';

describe('Persistência da sessão na aba', () => {
  beforeEach(() => sessionStorage.clear());
  afterEach(() => sessionStorage.clear());
  it('guarda somente token e expiração e remove no logout', () => {
    const storage = TestBed.inject(SessionStorage);
    const session = { accessToken: 'token', expiresAt: new Date(Date.now() + 60000).toISOString() };
    storage.save(session);
    expect(storage.read()).toEqual(session);
    const saved = JSON.parse(
      sessionStorage.getItem(`olivaw.session:${TestBed.inject(RuntimeConfig).apiUrl}`)!,
    );
    expect(Object.keys(saved).sort()).toEqual(['accessToken', 'expiresAt']);
    storage.clear();
    expect(storage.read()).toBeNull();
  });
  it('descarta token expirado e conteúdo inválido', () => {
    const storage = TestBed.inject(SessionStorage);
    const key = `olivaw.session:${TestBed.inject(RuntimeConfig).apiUrl}`;
    for (const content of [
      '{',
      '{}',
      JSON.stringify({ accessToken: 'token', expiresAt: 'invalid' }),
      JSON.stringify({
        accessToken: 'token',
        expiresAt: new Date(Date.now() - 1000).toISOString(),
      }),
    ]) {
      sessionStorage.setItem(key, content);
      expect(storage.read()).toBeNull();
      expect(sessionStorage.getItem(key)).toBeNull();
    }
  });
  it('isola o token por endereço de API', () => {
    const config = TestBed.inject(RuntimeConfig);
    const storage = TestBed.inject(SessionStorage);
    storage.save({ accessToken: 'token', expiresAt: new Date(Date.now() + 60000).toISOString() });
    config.apiUrl = 'https://other.example.test';
    expect(storage.read()).toBeNull();
  });
  it('não acessa o storage durante prerender', () => {
    TestBed.configureTestingModule({ providers: [{ provide: PLATFORM_ID, useValue: 'server' }] });
    const storage = TestBed.inject(SessionStorage);
    storage.save({ accessToken: 'token', expiresAt: new Date(Date.now() + 60000).toISOString() });
    expect(storage.read()).toBeNull();
    expect(sessionStorage.length).toBe(0);
  });
});
