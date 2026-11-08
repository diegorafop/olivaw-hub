import { computed, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { Sessao } from './auth.models';

const SESSION_KEY = 'ltmj.session';

function storage(): Storage | null {
  if (!isPlatformBrowser(inject(PLATFORM_ID))) return null;
  try {
    return window.sessionStorage;
  } catch {
    return null;
  }
}

function restore(): Sessao | null {
  const target = storage();
  try {
    const value: unknown = JSON.parse(target?.getItem(SESSION_KEY) ?? 'null');
    if (value && typeof value === 'object') {
      const session = value as Partial<Sessao>;
      const user = session.usuario;
      if (
        typeof session.accessToken === 'string' &&
        session.accessToken &&
        typeof session.expiresAt === 'string' &&
        Date.parse(session.expiresAt) > Date.now() &&
        user &&
        typeof user.userProfileId === 'string' &&
        typeof user.name === 'string' &&
        typeof user.isRoot === 'boolean' &&
        Array.isArray(user.permissions) &&
        user.permissions.every((permission) => typeof permission === 'string')
      )
        return session as Sessao;
    }
    target?.removeItem(SESSION_KEY);
  } catch {
    try {
      target?.removeItem(SESSION_KEY);
    } catch {
      /* Armazenamento indisponível. */
    }
  }
  return null;
}

export const AutenticacaoStore = signalStore(
  { providedIn: 'root' },
  withState<{ autenticacao: Sessao | null }>(() => ({ autenticacao: restore() })),
  withComputed(({ autenticacao }) => ({
    usuario: computed(() => autenticacao()?.usuario ?? null),
  })),
  withMethods((store) => {
    const target = storage();
    return {
      iniciar(sessao: Sessao): void {
        patchState(store, { autenticacao: sessao });
        try {
          target?.setItem(SESSION_KEY, JSON.stringify(sessao));
        } catch {
          /* Sessão continua em memória. */
        }
      },
      limpar(): void {
        patchState(store, { autenticacao: null });
        try {
          target?.removeItem(SESSION_KEY);
        } catch {
          /* Armazenamento indisponível. */
        }
      },
      tokenValido(): string | null {
        const sessao = store.autenticacao();
        return sessao && Date.parse(sessao.expiresAt) > Date.now() ? sessao.accessToken : null;
      },
      permite(permissao: string): boolean {
        const sessao = store.autenticacao();
        return (
          !!sessao &&
          Date.parse(sessao.expiresAt) > Date.now() &&
          (sessao.usuario.isRoot || sessao.usuario.permissions.includes(permissao))
        );
      },
    };
  }),
);
