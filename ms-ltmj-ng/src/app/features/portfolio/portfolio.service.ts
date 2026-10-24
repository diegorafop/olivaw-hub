import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpContext } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { LOCAL_ERROR } from '../../core/http/http-context';
import { RuntimeConfig } from '../../core/config/runtime-config';
import { EventCategory } from '../public/events/event-portfolio.data';

export interface ManagedEvent {
  id: string;
  title: string;
  description: string;
  category: EventCategory;
  isVisible: boolean;
  photos: { id: string; url: string }[];
}
export interface EventInput {
  title: string;
  description: string;
  category: EventCategory | '';
}
export const EVENT_CATEGORIES: EventCategory[] = [
  'Casamentos',
  'Aniversários',
  'Confraternizações',
  'Corporativos',
];

@Injectable({ providedIn: 'root' })
export class PortfolioService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(RuntimeConfig);
  private readonly options = { context: new HttpContext().set(LOCAL_ERROR, true) };
  private url(path = ''): string {
    return this.config.endpoint(`portfolio/events${path}`);
  }
  list(management = false, page = 1): Promise<{ items: ManagedEvent[] }> {
    return firstValueFrom(
      this.http.get<{ items: ManagedEvent[] }>(this.url(management ? '/management' : ''), {
        ...this.options,
        params: { page, pageSize: 12 },
      }),
    );
  }
  create(input: EventInput): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(this.url(), input, this.options));
  }
  update(id: string, input: EventInput): Promise<unknown> {
    return firstValueFrom(this.http.put(this.url(`/${id}`), input, this.options));
  }
  visibility(id: string, isVisible: boolean): Promise<unknown> {
    return firstValueFrom(
      this.http.patch(this.url(`/${id}/visibility`), { isVisible }, this.options),
    );
  }
  delete(id: string): Promise<unknown> {
    return firstValueFrom(this.http.delete(this.url(`/${id}`), this.options));
  }
  upload(id: string, files: File[]): Promise<unknown> {
    const body = new FormData();
    for (const file of files) body.append('files', file, file.name);
    // O navegador define o boundary; os interceptors existentes cuidam de JWT/loading.
    return firstValueFrom(this.http.post(this.url(`/${id}/photos`), body, this.options));
  }
  photoUrl(path: string): string {
    return this.config.endpoint(path);
  }
}
