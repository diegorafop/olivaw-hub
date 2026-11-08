import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpContext } from '@angular/common/http';
import { firstValueFrom, timeout } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR, SKIP_AUTH, SKIP_LOADING } from '../../../core/http/http-context';
import type { LeadSnapshot } from './project-draft';

@Injectable({ providedIn: 'root' })
export class LeadGateway {
  private readonly http = inject(HttpClient);
  private readonly config = inject(RuntimeConfig);
  async save(id: string, key: string, snapshot: LeadSnapshot): Promise<void> {
    await firstValueFrom(
      this.http
        .put(this.config.endpoint(`leads/${id}`), snapshot, {
          headers: { 'X-Lead-Key': key },
          context: new HttpContext()
            .set(SKIP_AUTH, true)
            .set(SKIP_LOADING, true)
            .set(LOCAL_ERROR, true),
        })
        .pipe(timeout(15000)),
    );
  }
}
