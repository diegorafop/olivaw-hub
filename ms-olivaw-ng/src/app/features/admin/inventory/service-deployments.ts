import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import {
  InventoryApi,
  deploymentStatuses,
  environments,
  inventoryError,
  type Page,
  type DeploymentItem,
} from './inventory-api';
@Component({
  selector: 'app-service-deployments',
  imports: [RouterLink],
  styleUrl: './inventory.scss',
  template: ` <section class="surface" aria-label="Ambientes do serviço">
    <header>
      <div>
        <h2>Ambientes e implantações</h2>
        <p class="help">
          Uma implantação vigente por serviço e ambiente. Encerramentos preservam o histórico.
        </p>
      </div>
      @if (!closed()) {
        <a
          class="primary"
          routerLink="/admin/deployments/new"
          [queryParams]="{ service: service() }"
          >Cadastrar ambiente</a
        >
      }
    </header>
    @if (list().loading) {
      <p role="status">Carregando ambientes…</p>
    } @else if (list().error) {
      <p role="alert">{{ list().error }}</p>
      <button type="button" (click)="reload()">Tentar novamente</button>
    } @else if (list().data; as data) {
      @for (item of data.items; track item.externalId) {
        <article class="record">
          <a [routerLink]="['/admin/deployments', item.externalId]">{{ item.name }}</a>
          <p>{{ environments[item.environment] }} · {{ statuses[item.status] }}</p>
        </article>
      } @empty {
        <p>Nenhum ambiente cadastrado.</p>
      }
      <footer>
        <button type="button" (click)="page.set(page() - 1)" [disabled]="page() <= 1">
          Anterior</button
        ><small>{{ data.total }} implantação(ões) · página {{ page() }}</small
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
export class ServiceDeployments {
  readonly service = input.required<string>();
  readonly closed = input(false);
  readonly page = signal(1);
  readonly refresh = signal(0);
  private readonly api = inject(InventoryApi);
  readonly environments = environments;
  readonly statuses = deploymentStatuses;
  readonly list = toSignal(
    combineLatest([
      toObservable(this.service),
      toObservable(this.page),
      toObservable(this.refresh),
    ]).pipe(
      switchMap(([service, page]) =>
        this.api.deployments(page, service).pipe(
          map((data) => ({ data, loading: false, error: '' })),
          catchError((error) =>
            of({
              data: null as Page<DeploymentItem> | null,
              loading: false,
              error: inventoryError(error),
            }),
          ),
          startWith({ data: null as Page<DeploymentItem> | null, loading: true, error: '' }),
        ),
      ),
    ),
    { initialValue: { data: null as Page<DeploymentItem> | null, loading: true, error: '' } },
  );
  reload() {
    this.refresh.update((value) => value + 1);
  }
}
