import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import {
  InventoryApi,
  resourceTypes,
  resourceStatuses,
  environments,
  relationships,
  inventoryError,
  type Page,
  type ResourceItem,
} from './inventory-api';
@Component({
  selector: 'app-customer-resources',
  imports: [RouterLink],
  styleUrl: './inventory.scss',
  template: ` <section class="surface" aria-label="Recursos do cliente">
    <header>
      <div>
        <h2>Recursos utilizados</h2>
        <p class="help">
          Vínculos diretos e dependências dos serviços, reunidos sem duplicar ativos.
        </p>
      </div>
      <a routerLink="/admin/resources" [queryParams]="{ linkCustomer: customer() }"
        >Vincular recurso existente</a
      >
    </header>
    @if (list().loading) {
      <p role="status">Carregando recursos…</p>
    } @else if (list().error) {
      <p role="alert">{{ list().error }}</p>
      <button type="button" (click)="reload()">Tentar novamente</button>
    } @else if (list().data; as data) {
      @for (item of data.items; track item.externalId) {
        <article class="record">
          <a [routerLink]="['/admin/resources', item.externalId]">{{ item.name }}</a>
          <p>
            {{ types[item.type] }} · {{ statuses[item.status] }} ·
            {{
              item.customerCount > 1
                ? 'Compartilhado por ' + item.customerCount + ' clientes'
                : '1 cliente identificado'
            }}
          </p>
          @for (origin of item.origins; track $index) {
            <p class="origin">
              @if (origin.kind === 'direct') {
                Vínculo direto
              } @else if (origin.kind === 'deployment') {
                Via
                <a [routerLink]="['/admin/deployments', origin.deploymentExternalId]"
                  >{{ origin.serviceName }} / {{ environments[origin.environment] }}</a
                >
              } @else {
                Via
                <a [routerLink]="['/admin/resources', origin.fromResourceExternalId]">{{
                  origin.fromResourceName
                }}</a>
                · {{ relationships[origin.relationshipKind] }}
              }
              · {{ origin.purpose }}
            </p>
          }
          <p class="help">
            {{ item.originCount }} origem(ns) · até 100 na prévia. Abra os recursos de origem para
            acompanhar a cadeia.
          </p>
        </article>
      } @empty {
        <p>Nenhum recurso identificado nos vínculos atuais.</p>
      }
      <footer>
        <button type="button" (click)="page.set(page() - 1)" [disabled]="page() <= 1">
          Anterior</button
        ><small>{{ data.total }} recurso(s) · página {{ page() }}</small
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
export class CustomerResources {
  readonly customer = input.required<string>();
  readonly page = signal(1);
  readonly refresh = signal(0);
  private readonly api = inject(InventoryApi);
  readonly types = resourceTypes;
  readonly statuses = resourceStatuses;
  readonly environments = environments;
  readonly relationships = relationships;
  readonly list = toSignal(
    combineLatest([
      toObservable(this.customer),
      toObservable(this.page),
      toObservable(this.refresh),
    ]).pipe(
      switchMap(([customer, page]) =>
        this.api.resources(page, { customerExternalId: customer }).pipe(
          map((data) => ({ data, loading: false, error: '' })),
          catchError((error) =>
            of({
              data: null as Page<ResourceItem> | null,
              loading: false,
              error: inventoryError(error),
            }),
          ),
          startWith({ data: null as Page<ResourceItem> | null, loading: true, error: '' }),
        ),
      ),
    ),
    { initialValue: { data: null as Page<ResourceItem> | null, loading: true, error: '' } },
  );
  reload() {
    this.refresh.update((value) => value + 1);
  }
}
