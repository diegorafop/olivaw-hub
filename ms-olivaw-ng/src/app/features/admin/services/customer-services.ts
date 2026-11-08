import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { ServicesApi, serviceStatuses, type ServicePage } from './services-api';
@Component({
  selector: 'app-customer-services',
  imports: [RouterLink],
  styleUrl: './services.scss',
  template: ` <section class="surface" aria-label="Serviços do cliente">
    <h2>Serviços</h2>
    <p>
      <a routerLink="/admin/services" [queryParams]="{ linkCustomer: customer() }"
        >Vincular serviço existente</a
      >
      ·
      <a routerLink="/admin/services/new" [queryParams]="{ customer: customer() }"
        >Cadastrar serviço para este cliente</a
      >
    </p>
    @if (list().loading) {
      <p role="status">Carregando serviços…</p>
    } @else if (list().error) {
      <p role="status">Não foi possível consultar os serviços.</p>
      <button type="button" (click)="retry()">Tentar novamente</button>
    } @else if (list().data; as data) {
      <ul>
        @for (item of data.items; track item.externalId) {
          <li>
            <a [routerLink]="['/admin/services', item.externalId]">{{ item.name }}</a> ·
            {{ statuses[item.status] }} · {{ item.technicalId }}
          </li>
        } @empty {
          <li>Nenhum serviço vinculado.</li>
        }
      </ul>
      <footer>
        <button type="button" (click)="page.set(page() - 1)" [disabled]="page() <= 1">
          Anterior</button
        ><span>Página {{ page() }}</span
        ><button
          type="button"
          (click)="page.set(page() + 1)"
          [disabled]="page() * 20 >= data.total"
        >
          Próxima
        </button>
      </footer>
    }
  </section>`,
})
export class CustomerServices {
  readonly customer = input.required<string>();
  readonly page = signal(1);
  private readonly refresh = signal(0);
  private readonly api = inject(ServicesApi);
  readonly statuses = serviceStatuses;
  readonly list = toSignal(
    combineLatest([
      toObservable(this.customer),
      toObservable(this.page),
      toObservable(this.refresh),
    ]).pipe(
      switchMap(([customer, page]) =>
        this.api.list(page, '', '', '', customer).pipe(
          map((data) => ({ data, loading: false, error: false })),
          catchError(() => of({ data: null as ServicePage | null, loading: false, error: true })),
          startWith({ data: null as ServicePage | null, loading: true, error: false }),
        ),
      ),
    ),
    { initialValue: { data: null as ServicePage | null, loading: true, error: false } },
  );
  retry() {
    this.refresh.update((value) => value + 1);
  }
}
