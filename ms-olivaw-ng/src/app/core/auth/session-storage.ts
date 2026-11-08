import { DOCUMENT, isPlatformBrowser } from '@angular/common';
import { inject, Injectable, PLATFORM_ID } from '@angular/core';
import { RuntimeConfig } from '../config/runtime-config';
import type { LoginResponse } from './auth.models';

@Injectable({ providedIn: 'root' })
export class SessionStorage {
  private readonly document = inject(DOCUMENT);
  private readonly platform = inject(PLATFORM_ID);
  private readonly config = inject(RuntimeConfig);
  private get key(): string {
    return `olivaw.session:${this.config.apiUrl}`;
  }
  private get storage(): Storage | undefined {
    if (!isPlatformBrowser(this.platform)) return undefined;
    try {
      return this.document.defaultView?.sessionStorage;
    } catch {
      return undefined;
    }
  }
  read(): LoginResponse | null {
    try {
      const text = this.storage?.getItem(this.key);
      if (!text) return null;
      const value: unknown = JSON.parse(text);
      if (
        value &&
        typeof value === 'object' &&
        'accessToken' in value &&
        typeof value.accessToken === 'string' &&
        value.accessToken.length > 0 &&
        'expiresAt' in value &&
        typeof value.expiresAt === 'string' &&
        Date.parse(value.expiresAt) > Date.now()
      ) {
        return { accessToken: value.accessToken, expiresAt: value.expiresAt };
      }
    } catch {
      /* Storage indisponível ou conteúdo inválido: continuar sem sessão. */
    }
    this.clear();
    return null;
  }
  save(session: LoginResponse): void {
    try {
      this.storage?.setItem(
        this.key,
        JSON.stringify({ accessToken: session.accessToken, expiresAt: session.expiresAt }),
      );
    } catch {
      /* Login em memória continua disponível quando o navegador bloqueia storage. */
    }
  }
  clear(): void {
    try {
      this.storage?.removeItem(this.key);
    } catch {
      /* Storage pode estar bloqueado. */
    }
  }
}
