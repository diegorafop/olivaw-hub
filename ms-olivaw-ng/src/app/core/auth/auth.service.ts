import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpContext } from '@angular/common/http';
import { SessionStorage } from './session-storage';
import { firstValueFrom, timeout } from 'rxjs';
import { RuntimeConfig } from '../config/runtime-config';
import { LOCAL_ERROR, SKIP_AUTH } from '../http/http-context';
import { AcessoAtual, LoginRequest, LoginResponse } from './auth.models';
import { AutenticacaoStore } from './autenticacao.store';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(RuntimeConfig);
  private readonly store = inject(AutenticacaoStore);
  private readonly storage = inject(SessionStorage);
  private expiryTimer?: ReturnType<typeof setTimeout>;

  async restore(): Promise<void> {
    const session = this.storage.read();
    if (!session) return;
    try {
      const usuario = await firstValueFrom(
        this.http
          .get<AcessoAtual>(this.config.endpoint('identity/authentication/access'), {
            headers: { Authorization: `Bearer ${session.accessToken}` },
            context: new HttpContext().set(SKIP_AUTH, true).set(LOCAL_ERROR, true),
          })
          .pipe(timeout(15000)),
      );
      if (Date.parse(session.expiresAt) <= Date.now()) {
        this.clear();
        return;
      }
      this.start({ ...session, usuario });
    } catch {
      this.clear();
    }
  }

  private start(session: import('./auth.models').Sessao): void {
    clearTimeout(this.expiryTimer);
    this.store.iniciar(session);
    this.expiryTimer = setTimeout(
      () => this.clear(),
      Math.min(Date.parse(session.expiresAt) - Date.now(), 2_147_483_647),
    );
  }

  async login(request: LoginRequest): Promise<void> {
    this.clear();
    const result = await firstValueFrom(
      this.http.post<LoginResponse>(
        this.config.endpoint('identity/authentication/login'),
        request,
        {
          context: new HttpContext().set(SKIP_AUTH, true).set(LOCAL_ERROR, true),
        },
      ),
    );
    const remaining = Date.parse(result.expiresAt) - Date.now();
    if (!result.accessToken || !Number.isFinite(remaining) || remaining <= 0)
      throw new Error('Sessão inválida.');
    const usuario = await firstValueFrom(
      this.http.get<AcessoAtual>(this.config.endpoint('identity/authentication/access'), {
        headers: { Authorization: `Bearer ${result.accessToken}` },
        context: new HttpContext().set(SKIP_AUTH, true).set(LOCAL_ERROR, true),
      }),
    );
    if (Date.parse(result.expiresAt) <= Date.now()) throw new Error('Sessão expirada.');
    this.start({ ...result, usuario });
  }

  async logout(): Promise<void> {
    try {
      if (this.store.tokenValido())
        await firstValueFrom(
          this.http.post(
            this.config.endpoint('identity/authentication/logout'),
            {},
            { context: new HttpContext().set(LOCAL_ERROR, true) },
          ),
        );
    } finally {
      this.clear();
    }
  }

  clear(): void {
    clearTimeout(this.expiryTimer);
    this.store.limpar();
  }
}
