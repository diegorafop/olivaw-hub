import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { CustomersApi } from '../customers/customers-api';
import {
  InventoryApi,
  resourceTypes,
  resourceStatuses,
  environments,
  inventoryError,
  type Page,
  type ResourceItem,
  type ResourceFilters,
} from './inventory-api';
@Component({
  selector: 'app-resources-page',
  imports: [RouterLink, ReactiveFormsModule],
  templateUrl: './resources-page.html',
  styleUrl: './inventory.scss',
})
export class ResourcesPage {
  private readonly api = inject(InventoryApi);
  private readonly customers = inject(CustomersApi);
  private readonly router = inject(Router);
  readonly route = inject(ActivatedRoute);
  readonly refresh = signal(0);
  readonly types = resourceTypes;
  readonly statuses = resourceStatuses;
  readonly typeOptions = Object.entries(resourceTypes);
  readonly statusOptions = Object.entries(resourceStatuses);
  readonly environmentOptions = Object.entries(environments);
  readonly linkCustomer = this.route.snapshot.queryParamMap.get('linkCustomer');
  readonly customerSearch = signal('');
  readonly customerError = signal(false);
  readonly customerChoices = toSignal(
    toObservable(this.customerSearch).pipe(
      switchMap((search) => {
        this.customerError.set(false);
        return this.customers.list(1, search, '').pipe(
          map((page) => page.items),
          catchError(() => {
            this.customerError.set(true);
            return of([]);
          }),
        );
      }),
    ),
    { initialValue: [] },
  );
  readonly form = new FormGroup({
    search: new FormControl('', { nonNullable: true }),
    type: new FormControl('', { nonNullable: true }),
    provider: new FormControl('', { nonNullable: true }),
    status: new FormControl('', { nonNullable: true }),
    environment: new FormControl('', { nonNullable: true }),
    shared: new FormControl('', { nonNullable: true }),
    customerExternalId: new FormControl('', { nonNullable: true }),
  });
  readonly list = toSignal(
    combineLatest([this.route.queryParamMap, toObservable(this.refresh)]).pipe(
      switchMap(([params]) => {
        const values: ResourceFilters = Object.fromEntries(
          Object.keys(this.form.controls).map((key) => [key, params.get(key) ?? '']),
        );
        this.form.reset(values);
        return this.api.resources(Number(params.get('page') ?? 1), values).pipe(
          map((data) => ({ data, loading: false, error: '' })),
          catchError((error) =>
            of({
              data: null as Page<ResourceItem> | null,
              loading: false,
              error: inventoryError(error),
            }),
          ),
          startWith({ data: null as Page<ResourceItem> | null, loading: true, error: '' }),
        );
      }),
    ),
    { initialValue: { data: null as Page<ResourceItem> | null, loading: true, error: '' } },
  );
  reload() {
    this.refresh.update((value) => value + 1);
  }
  apply() {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { ...this.form.getRawValue(), page: 1 },
      queryParamsHandling: 'merge',
    });
  }
  clear() {
    this.form.reset();
    this.apply();
  }
  go(page: number) {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page },
      queryParamsHandling: 'merge',
    });
  }
}
