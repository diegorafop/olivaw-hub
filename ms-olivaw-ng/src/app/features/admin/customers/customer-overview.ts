import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { toSignal, toObservable } from '@angular/core/rxjs-interop';
import { catchError, map, of, startWith, switchMap } from 'rxjs';
import { CustomersApi, type CustomerSummary } from './customers-api';
@Component({
  selector: 'app-customer-overview',
  imports: [RouterLink],
  template: `<section aria-labelledby="customer-overview-title">
    <div class="heading">
      <div>
        <p class="eyebrow">RELAÇÕES QUE CONSTRUÍMOS</p>
        <h2 id="customer-overview-title">Nossos clientes</h2>
      </div>
      <a routerLink="/admin/customers">Ver clientes ↗</a>
    </div>
    @if (state().loading) {
      <p role="status">Consultando clientes…</p>
    } @else if (state().data; as data) {
      <div class="metrics">
        <a routerLink="/admin/customers"
          ><strong>{{ data.total }}</strong
          ><span>Clientes cadastrados</span></a
        >
        <a routerLink="/admin/customers" [queryParams]="{ status: 'onboarding' }"
          ><strong>{{ data.onboarding }}</strong
          ><span>Em implantação</span></a
        >
        <a routerLink="/admin/customers" [queryParams]="{ status: 'active' }"
          ><strong>{{ data.active }}</strong
          ><span>Ativos</span></a
        >
        <a routerLink="/admin/customers" [queryParams]="{ status: 'paused' }"
          ><strong>{{ data.paused }}</strong
          ><span>Pausados</span></a
        >
      </div>
      <p class="caption">Situação cadastral · não representa saúde dos sistemas</p>
    } @else {
      <p role="status">Não foi possível consultar os clientes.</p>
      <button type="button" (click)="refresh.update(increment)">Tentar novamente</button>
    }
  </section>`,
  styles: `
    :host {
      display: block;
      margin-top: 24px;
    }
    section {
      background: #fff;
      border: 1px solid #dce6dc;
      border-radius: 13px;
      padding: 24px;
      color: #244435;
    }
    .heading {
      display: flex;
      justify-content: space-between;
      gap: 16px;
      align-items: center;
    }
    .eyebrow {
      font-size: 9px;
      letter-spacing: 0.13em;
      color: #627366;
      margin: 0 0 10px;
    }
    h2 {
      font-size: 18px;
      font-weight: 500;
      margin: 0;
    }
    a,
    button,
    p {
      font: inherit;
      font-size: 12px;
      color: inherit;
    }
    button {
      cursor: pointer;
    }
    .metrics {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 16px;
      margin-top: 24px;
    }
    .metrics a {
      text-decoration: none;
      border-left: 2px solid #dfebdf;
      padding-left: 16px;
    }
    .metrics strong {
      display: block;
      font-size: 30px;
      font-weight: 500;
      margin-bottom: 7px;
    }
    .metrics span {
      color: #627366;
    }
    .caption {
      font-size: 10px;
      color: #627366;
      margin: 22px 0 0;
    }
    a:focus-visible,
    button:focus-visible {
      outline: 2px solid #256347;
      outline-offset: 4px;
    }
    @media (max-width: 600px) {
      .metrics {
        grid-template-columns: 1fr 1fr;
        row-gap: 24px;
      }
      .heading {
        align-items: flex-start;
      }
    }
  `,
})
export class CustomerOverview {
  private readonly api = inject(CustomersApi);
  readonly refresh = signal(0);
  readonly increment = (value: number) => value + 1;
  readonly state = toSignal(
    toObservable(this.refresh).pipe(
      switchMap(() =>
        this.api.summary().pipe(
          map((data) => ({ loading: false, data })),
          catchError(() => of({ loading: false, data: null as CustomerSummary | null })),
          startWith({ loading: true, data: null as CustomerSummary | null }),
        ),
      ),
    ),
    { initialValue: { loading: true, data: null as CustomerSummary | null } },
  );
}
