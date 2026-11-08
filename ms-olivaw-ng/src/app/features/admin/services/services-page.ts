import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { ServicesApi, serviceStatuses, serviceTypes, type ServicePage } from './services-api';
@Component({
  selector: 'app-services-page',
  imports: [RouterLink, ReactiveFormsModule, DatePipe],
  templateUrl: './services-page.html',
  styleUrls: ['./services.scss', './service-cards.scss'],
})
export class ServicesPage {
  private readonly api = inject(ServicesApi);
  private readonly router = inject(Router);
  readonly route = inject(ActivatedRoute);
  readonly refresh = signal(0);
  readonly statuses = serviceStatuses;
  readonly types = serviceTypes;
  readonly statusOptions = Object.entries(serviceStatuses);
  readonly typeOptions = Object.entries(serviceTypes);
  readonly form = new FormGroup({
    search: new FormControl('', { nonNullable: true }),
    status: new FormControl('', { nonNullable: true }),
    type: new FormControl('', { nonNullable: true }),
  });
  readonly contextCustomer = this.route.snapshot.queryParamMap.get('linkCustomer') ?? '';
  readonly customerFilter = this.route.snapshot.queryParamMap.get('customer') ?? '';
  readonly list = toSignal(
    combineLatest([this.route.queryParamMap, toObservable(this.refresh)]).pipe(
      switchMap(([params]) => {
        const search = params.get('search') ?? '',
          status = params.get('status') ?? '',
          type = params.get('type') ?? '';
        this.form.reset({ search, status, type });
        return this.api
          .list(Number(params.get('page') ?? 1), search, status, type, params.get('customer') ?? '')
          .pipe(
            map((data) => ({ data, loading: false, error: false })),
            catchError(() => of({ data: null as ServicePage | null, loading: false, error: true })),
            startWith({ data: null as ServicePage | null, loading: true, error: false }),
          );
      }),
    ),
    { initialValue: { data: null as ServicePage | null, loading: true, error: false } },
  );
  apply() {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { ...this.form.getRawValue(), page: 1 },
      queryParamsHandling: 'merge',
    });
  }
  go(page: number) {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page },
      queryParamsHandling: 'merge',
    });
  }
  reload() {
    this.refresh.update((value) => value + 1);
  }
}
