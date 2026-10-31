import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { timeout } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR, SKIP_LOADING } from '../../../core/http/http-context';
export interface FinanceBreakdown {
  key: string;
  label: string;
  grossBrl: number;
  creditsBrl: number;
  netBrl: number;
  entries: number;
}
export interface FinanceReport {
  grossBrl: number;
  creditsBrl: number;
  netBrl: number;
  unallocatedBrl: number;
  total: number;
  series: { date: string; costBrl: number; creditsBrl: number; netBrl: number }[];
  byCategory?: FinanceBreakdown[];
  byCustomer?: FinanceBreakdown[];
  items: {
    externalId: string;
    description: string;
    kind: string;
    amountBrl: number;
    occurredAt: string;
    customerName: string;
    source: string;
  }[];
}
@Injectable({ providedIn: 'root' })
export class FinanceApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(RuntimeConfig);
  read(from: string, to: string, groupBy: string, customer: string) {
    let params = new HttpParams()
      .set('from', from)
      .set('to', to)
      .set('groupBy', groupBy)
      .set('pageSize', 6)
      .set('includeCredits', false);
    if (customer === 'shared') params = params.set('unallocatedOnly', true);
    else if (customer) params = params.set('customerExternalId', customer);
    return this.http
      .get<FinanceReport>(this.config.endpoint('expenses'), {
        params,
        context: new HttpContext().set(LOCAL_ERROR, true).set(SKIP_LOADING, true),
      })
      .pipe(timeout(15000));
  }
}
