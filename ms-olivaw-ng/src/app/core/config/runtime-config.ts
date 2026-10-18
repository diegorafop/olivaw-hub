import { DOCUMENT, isPlatformBrowser } from '@angular/common';
import { inject, Injectable, PLATFORM_ID } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class RuntimeConfig {
  private readonly document = inject(DOCUMENT);
  private readonly platform = inject(PLATFORM_ID);
  apiUrl = 'http://localhost:5137';

  async load(): Promise<void> {
    // Prerender não depende da API nem da configuração de um ambiente específico.
    if (!isPlatformBrowser(this.platform)) return;
    const response = await fetch(new URL('config.json', this.document.baseURI), {
      cache: 'no-store',
    });
    if (!response.ok) throw new Error('Não foi possível carregar a configuração pública.');
    const config: unknown = await response.json();
    if (
      !config ||
      typeof config !== 'object' ||
      !('apiUrl' in config) ||
      typeof config.apiUrl !== 'string'
    ) {
      throw new Error('apiUrl inválida na configuração pública.');
    }
    const url = new URL(config.apiUrl);
    if (
      !['http:', 'https:'].includes(url.protocol) ||
      url.username ||
      url.password ||
      url.search ||
      url.hash
    ) {
      throw new Error('A URL da API deve usar HTTP(S), sem credenciais, query ou fragmento.');
    }
    this.apiUrl = url.href.replace(/\/$/, '');
  }

  endpoint(path: string): string {
    return `${this.apiUrl}/${path.replace(/^\//, '')}`;
  }

  isApiUrl(value: string): boolean {
    const target = new URL(value, this.document.baseURI);
    const base = new URL(this.apiUrl, this.document.baseURI);
    const prefix = base.pathname.replace(/\/$/, '');
    return (
      target.origin === base.origin &&
      (target.pathname === prefix || target.pathname.startsWith(`${prefix}/`))
    );
  }
}
