import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { timeout } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR, SKIP_LOADING } from '../../../core/http/http-context';
import type { LeadSnapshot } from '../../public/project-folder/project-draft';
export interface LeadItem {
  externalId: string;
  createdAt: string;
  updatedAt: string | null;
  business: string;
  title: string;
  entry: string;
  email: string;
  whatsapp: string;
}
export interface LeadPage {
  page: number;
  pageSize: number;
  total: number;
  asOf: string;
  items: LeadItem[];
}
export interface LeadDetailData {
  externalId: string;
  createdAt: string;
  updatedAt: string | null;
  snapshot: LeadSnapshot | null;
  contentUnavailable: boolean;
}
export interface LeadSummary {
  total: number;
  recent: number;
  since: string;
  asOf: string;
}
export interface LeadFilters {
  page: number;
  search: string;
  entry: string;
  from: string;
  until: string;
}
@Injectable({ providedIn: 'root' })
export class LeadsApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(RuntimeConfig);
  private context() {
    return new HttpContext().set(LOCAL_ERROR, true).set(SKIP_LOADING, true);
  }
  list(filters: LeadFilters) {
    let params = new HttpParams().set('page', filters.page).set('pageSize', 20);
    for (const [key, value] of Object.entries(filters))
      if (key !== 'page' && value) params = params.set(key, value);
    return this.http
      .get<LeadPage>(this.config.endpoint('leads'), { params, context: this.context() })
      .pipe(timeout(15000));
  }
  detail(id: string) {
    return this.http
      .get<LeadDetailData>(this.config.endpoint(`leads/${encodeURIComponent(id)}`), {
        context: this.context(),
      })
      .pipe(timeout(15000));
  }
  summary() {
    return this.http
      .get<LeadSummary>(this.config.endpoint('leads/summary'), { context: this.context() })
      .pipe(timeout(15000));
  }
}
