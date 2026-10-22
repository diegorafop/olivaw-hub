import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { timeout } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR, SKIP_LOADING } from '../../../core/http/http-context';
export const customerStatuses = [
  { value: 'onboarding', label: 'Em implantação' },
  { value: 'active', label: 'Ativo' },
  { value: 'paused', label: 'Pausado' },
  { value: 'closed', label: 'Encerrado' },
];
export const statusLabel = (value: string) =>
  customerStatuses.find((item) => item.value === value)?.label ?? value;
export interface CustomerContact {
  name: string;
  role: string;
  email: string;
  whatsapp: string;
}
export interface CustomerInput {
  business: string;
  legalName: string;
  document: string | null;
  owner: string;
  notes: string;
  contacts: CustomerContact[];
}
export interface CustomerItem {
  externalId: string;
  business: string;
  owner: string;
  status: string;
  createdAt: string;
}
export interface CustomerDetail extends CustomerInput, CustomerItem {
  revision: number;
  sourceLeadExternalId: string | null;
  updatedAt: string | null;
  changes: { at: string; actor: string; action: string; previousStatus: string; status: string }[];
}
export interface CustomerPage {
  page: number;
  pageSize: number;
  total: number;
  items: CustomerItem[];
}
export interface CustomerSummary {
  total: number;
  onboarding: number;
  active: number;
  paused: number;
  closed: number;
  asOf: string;
}
@Injectable({ providedIn: 'root' })
export class CustomersApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(RuntimeConfig);
  private options() {
    return { context: new HttpContext().set(LOCAL_ERROR, true).set(SKIP_LOADING, true) };
  }
  list(page: number, search: string, status: string) {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', 20)
      .set('search', search)
      .set('status', status);
    return this.http
      .get<CustomerPage>(this.config.endpoint('customers'), { ...this.options(), params })
      .pipe(timeout(15000));
  }
  summary() {
    return this.http
      .get<CustomerSummary>(this.config.endpoint('customers/summary'), this.options())
      .pipe(timeout(15000));
  }
  detail(id: string) {
    return this.http
      .get<CustomerDetail>(
        this.config.endpoint(`customers/${encodeURIComponent(id)}`),
        this.options(),
      )
      .pipe(timeout(15000));
  }
  create(input: CustomerInput, sourceLeadExternalId: string | null) {
    return this.http
      .post<{ externalId: string; revision: number }>(
        this.config.endpoint('customers'),
        { ...input, sourceLeadExternalId },
        this.options(),
      )
      .pipe(timeout(15000));
  }
  update(id: string, input: CustomerInput, status: string, revision: number) {
    return this.http
      .put<{ externalId: string; revision: number }>(
        this.config.endpoint(`customers/${encodeURIComponent(id)}`),
        { ...input, status, revision },
        this.options(),
      )
      .pipe(timeout(15000));
  }
}
