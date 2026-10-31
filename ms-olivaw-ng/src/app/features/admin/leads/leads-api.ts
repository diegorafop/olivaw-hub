import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { timeout } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR, SKIP_LOADING } from '../../../core/http/http-context';
import type { LeadSnapshot } from '../../public/project-folder/project-draft';
import type { LeadCreateData, LeadProposal } from './lead-proposal-model';
export interface LeadItem {
  externalId: string;
  createdAt: string;
  updatedAt: string | null;
  business: string;
  title: string;
  entry: string;
  email: string;
  whatsapp: string;
  followUpStatus: string;
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
  followUpStatus: string;
  followUpRevision: number;
  followUpUpdatedAt: string | null;
}
export interface LeadSummary {
  total: number;
  recent: number;
  byStatus: Record<string, number>;
  since: string;
  asOf: string;
}
export interface LeadFilters {
  page: number;
  search: string;
  entry: string;
  followUpStatus: string;
  from: string;
  until: string;
}
export interface LeadHistoryItem {
  externalId: string;
  revision: number;
  previousStatus: string;
  status: string;
  note: string;
  actorName: string;
  createdAt: string;
}
export interface LeadHistory {
  items: LeadHistoryItem[];
  nextBeforeRevision: number | null;
}
@Injectable({ providedIn: 'root' })
export class LeadsApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(RuntimeConfig);
  private context() {
    return new HttpContext().set(LOCAL_ERROR, true).set(SKIP_LOADING, true);
  }
  create(payload: LeadCreateData) {
    return this.http
      .post<{ externalId: string }>(this.config.endpoint('leads'), payload, {
        context: this.context(),
      })
      .pipe(timeout(15000));
  }
  updateProposal(id: string, revision: number, proposal: LeadProposal) {
    return this.http
      .put<{ revision: number }>(
        this.config.endpoint(`leads/${encodeURIComponent(id)}/proposal`),
        { revision, proposal },
        { context: this.context() },
      )
      .pipe(timeout(15000));
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
  updateFollowUp(id: string, payload: { revision: number; status: string; note: string }) {
    return this.http
      .put<{ revision: number }>(
        this.config.endpoint(`leads/${encodeURIComponent(id)}/follow-up`),
        payload,
        { context: this.context() },
      )
      .pipe(timeout(15000));
  }
  history(id: string, beforeRevision: number | null) {
    let params = new HttpParams();
    if (beforeRevision !== null) params = params.set('beforeRevision', beforeRevision);
    return this.http
      .get<LeadHistory>(this.config.endpoint(`leads/${encodeURIComponent(id)}/history`), {
        params,
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
