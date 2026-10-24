import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpContext, HttpHeaders, HttpParams } from '@angular/common/http';
import { RuntimeConfig } from '../../core/config/runtime-config';
import { LOCAL_ERROR, SKIP_AUTH, SKIP_LOADING } from '../../core/http/http-context';
import { timeout } from 'rxjs';
export interface PortalCompany {
  externalId: string;
  business: string;
  role: string;
}
export interface PortalService {
  externalId: string;
  serviceExternalId: string;
  name: string;
  publicUrl: string;
  state: string;
  observedAt: string | null;
}
export interface PortalPlan {
  name: string;
  monthlyPrice: number;
  currency: string;
  storageLimitGiB: number | null;
  memoryLimitGiB: number | null;
  conditions: string;
  startsAt: string;
}
export interface PortalDashboard {
  customerExternalId: string;
  business: string;
  preview: boolean;
  canRequest: boolean;
  canReadBilling: boolean;
  services: PortalService[];
  currentPlan: PortalPlan | null;
  nextBeforeId: number | null;
}
export interface PortalRequest {
  externalId: string;
  protocol: string;
  title: string;
  status: string;
  requestType: string;
  publicSummary: string;
  serviceLabel: string;
  desiredStorageGB: number | null;
  desiredMemoryMiB: number | null;
  desiredPlan: string;
  createdAt: string;
  updatedAt: string | null;
  revision: number;
}
export interface PortalMessage {
  externalId: string;
  body: string;
  author: string;
  createdAt: string;
}
export interface PortalDelivery {
  externalId: string;
  title: string;
  summary: string;
  version: string;
  category: string;
  serviceLabel: string;
  deliveredAt: string;
  publishedAt: string;
  updatedAt: string | null;
  revision: number;
}
@Injectable({ providedIn: 'root' })
export class PortalApi {
  private readonly http = inject(HttpClient);
  private readonly runtime = inject(RuntimeConfig);
  readonly session = signal<{ accessToken: string; expiresAt: string } | null>(null);
  readonly pendingInvitation = signal('');
  token() {
    const session = this.session();
    if (!session || Date.parse(session.expiresAt) <= Date.now()) return '';
    return session.accessToken;
  }
  private options(preview = false, params?: HttpParams) {
    let headers = new HttpHeaders();
    if (!preview && this.token()) headers = headers.set('Authorization', 'Bearer ' + this.token());
    return {
      headers,
      params,
      context: new HttpContext()
        .set(SKIP_AUTH, !preview)
        .set(LOCAL_ERROR, true)
        .set(SKIP_LOADING, true),
    };
  }
  login(email: string, password: string) {
    return this.http
      .post<{ accessToken: string; expiresAt: string }>(
        this.runtime.endpoint('portal/sessions'),
        { email, password },
        this.options(),
      )
      .pipe(timeout(15000));
  }
  invitation(token: string) {
    return this.http
      .post<{ email: string; business: string; existingAccount: boolean; expiresAt: string }>(
        this.runtime.endpoint('portal/invitations/read'),
        { token },
        this.options(),
      )
      .pipe(timeout(15000));
  }
  accept(token: string, password: string) {
    return this.http
      .post(this.runtime.endpoint('portal/invitations/accept'), { token, password }, this.options())
      .pipe(timeout(15000));
  }
  logout() {
    return this.http
      .delete(this.runtime.endpoint('portal/sessions'), this.options())
      .pipe(timeout(15000));
  }
  companies(beforeId = 0) {
    return this.http
      .get<{ items: PortalCompany[]; nextBeforeId: number | null }>(
        this.runtime.endpoint('portal/companies'),
        this.options(false, new HttpParams().set('beforeId', beforeId)),
      )
      .pipe(timeout(15000));
  }
  dashboard(customerExternalId: string, preview: boolean, beforeId = 0) {
    return this.http
      .get<PortalDashboard>(
        this.runtime.endpoint(preview ? 'portal-preview/dashboard' : 'portal/dashboard'),
        this.options(
          preview,
          new HttpParams().set('customerExternalId', customerExternalId).set('beforeId', beforeId),
        ),
      )
      .pipe(timeout(15000));
  }
  requests(customerExternalId: string, preview: boolean, beforeId = 0) {
    return this.http
      .get<{ items: PortalRequest[]; nextBeforeId: number | null }>(
        this.runtime.endpoint(preview ? 'portal-preview/requests' : 'portal/requests'),
        this.options(
          preview,
          new HttpParams().set('customerExternalId', customerExternalId).set('beforeId', beforeId),
        ),
      )
      .pipe(timeout(15000));
  }
  messages(customerExternalId: string, requestExternalId: string, preview: boolean, beforeId = 0) {
    return this.http
      .get<{ items: PortalMessage[]; nextBeforeId: number | null }>(
        this.runtime.endpoint(
          preview ? 'portal-preview/requests/messages' : 'portal/requests/messages',
        ),
        this.options(
          preview,
          new HttpParams()
            .set('customerExternalId', customerExternalId)
            .set('requestExternalId', requestExternalId)
            .set('beforeId', beforeId),
        ),
      )
      .pipe(timeout(15000));
  }
  createRequest(body: Record<string, unknown>) {
    return this.http
      .post<{ externalId: string; protocol: string }>(
        this.runtime.endpoint('portal/requests'),
        body,
        this.options(),
      )
      .pipe(timeout(15000));
  }
  createMessage(body: Record<string, unknown>) {
    return this.http
      .post(this.runtime.endpoint('portal/requests/messages'), body, this.options())
      .pipe(timeout(15000));
  }
  deliveries(
    customer: string,
    preview: boolean,
    filters: {
      serviceExternalId?: string;
      category?: string;
      fromAt?: string;
      toAt?: string;
      beforeAt?: string;
      beforeId?: number;
    } = {},
  ) {
    let params = new HttpParams().set('customerExternalId', customer);
    for (const [key, value] of Object.entries(filters))
      if (value !== undefined && value !== '') params = params.set(key, value);
    return this.http
      .get<{ items: PortalDelivery[]; nextBeforeId: number | null; nextBeforeAt: string | null }>(
        this.runtime.endpoint(preview ? 'portal-preview/deliveries' : 'portal/deliveries'),
        this.options(preview, params),
      )
      .pipe(timeout(15000));
  }
}
