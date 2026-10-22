import { Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { toSignal, toObservable } from '@angular/core/rxjs-interop';
import { catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { CustomersApi, customerStatuses, statusLabel, type CustomerPage } from './customers-api';
@Component({
  selector: 'app-customers-page',
  imports: [DatePipe, RouterLink, ReactiveFormsModule],
  templateUrl: './customers-page.html',
  styleUrls: ['./customers.scss', './customer-layout.scss', './customer-details.scss'],
})
export class CustomersPage {
  private readonly api = inject(CustomersApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly statuses = customerStatuses;
  readonly label = statusLabel;
  readonly refresh = signal(0);
  readonly form = new FormGroup({
    search: new FormControl('', { nonNullable: true }),
    status: new FormControl('', { nonNullable: true }),
  });
  readonly state = toSignal(
    combineLatest([this.route.queryParamMap, toObservable(this.refresh)]).pipe(
      switchMap(([params]) => {
        const search = params.get('search') ?? '',
          status = params.get('status') ?? '';
        this.form.reset({ search, status });
        return this.api.list(Number(params.get('page') ?? 1), search, status).pipe(
          map((data) => ({ loading: false, data, error: '' })),
          catchError(() =>
            of({
              loading: false,
              data: null as CustomerPage | null,
              error:
                'Não foi possível carregar os clientes. Confira a disponibilidade da API e tente novamente.',
            }),
          ),
          startWith({ loading: true, data: null as CustomerPage | null, error: '' }),
        );
      }),
    ),
    { initialValue: { loading: true, data: null as CustomerPage | null, error: '' } },
  );
  readonly pageCount = computed(() => Math.ceil((this.state().data?.total ?? 0) / 20));
  apply() {
    void this.router.navigate(['/admin/customers'], {
      queryParams: { ...this.form.getRawValue(), page: 1 },
    });
  }
  go(page: number) {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page },
      queryParamsHandling: 'merge',
    });
  }
  clear() {
    void this.router.navigate(['/admin/customers']);
  }
  reload() {
    this.refresh.update((value) => value + 1);
  }
}
