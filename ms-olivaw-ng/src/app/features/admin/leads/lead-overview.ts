import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, map, of, startWith, switchMap } from 'rxjs';
import { toObservable } from '@angular/core/rxjs-interop';
import { LeadsApi, type LeadSummary } from './leads-api';
@Component({
  selector: 'app-lead-overview',
  imports: [DatePipe, RouterLink],
  template: ` <section aria-labelledby="lead-overview-title">
    <div>
      <p class="eyebrow">PRIMEIRAS CONVERSAS</p>
      <h2 id="lead-overview-title">Ideias que chegaram à Olivaw.</h2>
      @let result = state();
      @if (result.loading) {
        <p role="status">Consultando leads…</p>
      } @else if (result.data; as data) {
        <p>
          <strong>{{ data.total }}</strong> leads recebidos
          <span>· {{ data.recent }} nos últimos 7 dias</span>
        </p>
        <small>Consulta à API Olivaw · {{ data.asOf | date: 'dd/MM/yyyy HH:mm' }}</small>
      } @else {
        <p role="status">Não foi possível consultar os leads.</p>
        <button type="button" (click)="refresh.update(increment)">Tentar novamente</button>
      }
    </div>
    <a routerLink="/admin/leads">Ver leads <span aria-hidden="true">↗</span></a>
  </section>`,
  styles: `
    :host {
      display: block;
      margin-top: 24px;
    }
    section {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 24px;
      padding: 24px;
      border: 1px solid #dce6dc;
      border-radius: 13px;
      background: white;
    }
    h2 {
      font-size: 16px;
      margin: 0 0 12px;
      font-weight: 500;
      color: #244435;
    }
    .eyebrow {
      font-size: 9px;
      letter-spacing: 0.13em;
      color: #627366;
      margin: 0 0 10px;
    }
    p {
      font-size: 13px;
      color: #536a5b;
      margin: 0 0 9px;
    }
    strong {
      font-size: 25px;
      font-weight: 500;
      margin-right: 4px;
    }
    small {
      font-size: 10px;
      color: #627366;
    }
    a,
    button {
      font: inherit;
      font-size: 12px;
      color: #244435;
    }
    a {
      white-space: nowrap;
    }
    button {
      cursor: pointer;
      background: #edf5ef;
      border: 1px solid #cddccf;
      border-radius: 6px;
      padding: 7px 10px;
    }
    a:focus-visible,
    button:focus-visible {
      outline: 2px solid #256347;
      outline-offset: 4px;
    }
    @media (max-width: 600px) {
      section {
        align-items: flex-start;
        flex-direction: column;
        gap: 18px;
      }
      p span {
        display: block;
        margin-top: 6px;
      }
    }
  `,
})
export class LeadOverview {
  private readonly api = inject(LeadsApi);
  readonly refresh = signal(0);
  readonly increment = (value: number) => value + 1;
  readonly state = toSignal(
    toObservable(this.refresh).pipe(
      switchMap(() =>
        this.api.summary().pipe(
          map((data) => ({ loading: false, data })),
          catchError(() => of({ loading: false, data: null as LeadSummary | null })),
          startWith({ loading: true, data: null as LeadSummary | null }),
        ),
      ),
    ),
    { initialValue: { loading: true, data: null as LeadSummary | null } },
  );
}
