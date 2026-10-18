import { computed } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { Sessao } from './auth.models';

export const AutenticacaoStore = signalStore(
  { providedIn: 'root' },
  withState<{ autenticacao: Sessao | null }>({ autenticacao: null }),
  withComputed(({ autenticacao }) => ({
    usuario: computed(() => autenticacao()?.usuario ?? null),
  })),
  withMethods((store) => ({
    iniciar(sessao: Sessao): void {
      patchState(store, { autenticacao: sessao });
    },
    limpar(): void {
      patchState(store, { autenticacao: null });
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
        (sessao.usuario.root || sessao.usuario.autorizacoes.includes(permissao))
      );
    },
  })),
);
