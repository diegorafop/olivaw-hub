import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { timeout } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR, SKIP_LOADING } from '../../../core/http/http-context';
export const serviceStatuses: Record<string, string> = {
  planned: 'Em análise',
  onboarding: 'Em implantação',
  active: 'Em operação',
  closed: 'Encerrado',
};
export const serviceTypes: Record<string, string> = {
  unknown: 'Unknown',
  dotnet: 'Dotnet',
  angular: 'Angular',
  postgres: 'PostgreSQL',
  rabbitmq: 'RabbitMQ',
  redis: 'Redis',
};
export interface ServiceItem {
  externalId: string;
  technicalId: string;
  name: string;
  type: string;
  status: string;
  scope: string;
  customerCount: number;
  createdAt?: string;
}
export interface ServiceInput {
  name: string;
  type: string;
  description: string;
  owner: string;
  scope: string;
  status: string;
  customerExternalIds: string[];
}
export interface ServiceCustomer {
  customerExternalId: string;
  business: string;
  startedByName: string;
  endedByName: string | null;
  startedAt: string;
  endedAt: string | null;
}
export interface ServiceDetail extends Omit<ServiceItem, 'customerCount'> {
  revision: number;
  description: string;
  owner: string;
  customers: ServiceCustomer[];
  previousCustomers: ServiceCustomer[];
  changes: {
    revision: number;
    actorName: string;
    createdAt: string;
    status: string;
    previousStatus: string;
    name: string;
    type: string;
    scope: string;
    description: string;
    owner: string;
  }[];
}
export interface ServicePage {
  page: number;
  pageSize: number;
  total: number;
  items: ServiceItem[];
}
@Injectable({ providedIn: 'root' })
export class ServicesApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(RuntimeConfig);
  private options() {
    return { context: new HttpContext().set(LOCAL_ERROR, true).set(SKIP_LOADING, true) };
  }
  list(page: number, search: string, status: string, type: string, customer = '') {
    let params = new HttpParams().set('page', page).set('pageSize', 20);
    if (customer) params = params.set('customerExternalId', customer);
    if (search) params = params.set('search', search);
    if (status) params = params.set('status', status);
    if (type) params = params.set('type', type);
    return this.http
      .get<ServicePage>(this.config.endpoint('services'), { ...this.options(), params })
      .pipe(timeout(15000));
  }
  detail(id: string) {
    return this.http
      .get<ServiceDetail>(
        this.config.endpoint(`services/${encodeURIComponent(id)}`),
        this.options(),
      )
      .pipe(timeout(15000));
  }
  create(technicalId: string, payload: ServiceInput) {
    return this.http
      .post<{ externalId: string; revision: number }>(
        this.config.endpoint('services'),
        { ...payload, technicalId },
        this.options(),
      )
      .pipe(timeout(15000));
  }
  update(id: string, revision: number, payload: ServiceInput) {
    return this.http
      .put<{ externalId: string; revision: number }>(
        this.config.endpoint(`services/${encodeURIComponent(id)}`),
        { ...payload, revision },
        this.options(),
      )
      .pipe(timeout(15000));
  }
}
