import { LEAD_STATUSES } from './lead-status';
import { formatWhatsapp } from '../../public/contact-details';
import { Component, computed, effect, ElementRef, inject, signal, viewChild } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { toSignal, toObservable } from '@angular/core/rxjs-interop';
import { catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { LeadsApi, type LeadPage, type LeadDetailData, type LeadFilters } from './leads-api';
import { LeadDetail } from './lead-detail';
@Component({
  selector: 'app-leads-page',
  imports: [DatePipe, RouterLink, ReactiveFormsModule, LeadDetail],
  templateUrl: './leads-page.html',
  styleUrl: './leads-page.scss',
})
export class LeadsPage {
  readonly statuses = LEAD_STATUSES;
  readonly statusOptions = Object.entries(LEAD_STATUSES);
  readonly formatPhone = formatWhatsapp;
  private readonly detailPanel = viewChild<ElementRef<HTMLElement>>('detailPanel');
  constructor() {
    effect(() => {
      if (!this.detail().loading) this.detailPanel()?.nativeElement.focus({ preventScroll: true });
    });
  }
  readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(LeadsApi);
  readonly refresh = signal(0);
  readonly formError = signal('');
  readonly form = new FormGroup({
    search: new FormControl('', { nonNullable: true }),
    entry: new FormControl('', { nonNullable: true }),
    followUpStatus: new FormControl('', { nonNullable: true }),
    from: new FormControl('', { nonNullable: true }),
    until: new FormControl('', { nonNullable: true }),
  });
  private readonly params = toSignal(this.route.queryParamMap, {
    initialValue: this.route.snapshot.queryParamMap,
  });
  readonly selected = toSignal(this.route.paramMap.pipe(map((params) => params.get('id'))), {
    initialValue: this.route.snapshot.paramMap.get('id'),
  });
  readonly list = toSignal(
    combineLatest([this.route.queryParamMap, toObservable(this.refresh)]).pipe(
      switchMap(([params]) => {
        const filters: LeadFilters = {
          page: Number(params.get('page') ?? 1),
          search: params.get('search') ?? '',
          entry: params.get('entry') ?? '',
          followUpStatus: params.get('followUpStatus') ?? '',
          from: params.get('from') ?? '',
          until: params.get('until') ?? '',
        };
        this.form.reset({
          search: filters.search,
          entry: filters.entry,
          followUpStatus: filters.followUpStatus,
          from: this.datePart(filters.from),
          until: this.datePart(filters.until, true),
        });
        return this.api.list(filters).pipe(
          map((data) => ({ loading: false, data, error: '' })),
          catchError(() =>
            of({
              loading: false,
              data: null as LeadPage | null,
              error:
                'Não foi possível carregar os leads. Confira se a API está atualizada e tente novamente.',
            }),
          ),
          startWith({ loading: true, data: null as LeadPage | null, error: '' }),
        );
      }),
    ),
    { initialValue: { loading: true, data: null as LeadPage | null, error: '' } },
  );
  readonly summary = toSignal(
    toObservable(this.refresh).pipe(
      switchMap(() => this.api.summary().pipe(catchError(() => of(null)))),
    ),
  );
  readonly detail = toSignal(
    combineLatest([this.route.paramMap, toObservable(this.refresh)]).pipe(
      switchMap(([params]) => {
        const id = params.get('id');
        if (!id) return of({ loading: false, data: null as LeadDetailData | null, error: '' });
        return this.api.detail(id).pipe(
          map((data) => ({ loading: false, data, error: '' })),
          catchError(() =>
            of({
              loading: false,
              data: null as LeadDetailData | null,
              error: 'Não foi possível abrir este lead. Ele pode não estar mais disponível.',
            }),
          ),
          startWith({ loading: true, data: null as LeadDetailData | null, error: '' }),
        );
      }),
    ),
    { initialValue: { loading: false, data: null as LeadDetailData | null, error: '' } },
  );
  readonly queryParams = computed(() =>
    Object.fromEntries(this.params().keys.map((key) => [key, this.params().get(key)])),
  );
  readonly pageCount = computed(() => Math.ceil((this.list().data?.total ?? 0) / 20));
  reload(): void {
    this.refresh.update((value) => value + 1);
  }
  private datePart(value: string, exclusive = false): string {
    if (!value) return '';
    const date = new Date(value);
    if (!Number.isFinite(date.getTime())) return '';
    if (exclusive) date.setDate(date.getDate() - 1);
    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
  }
  apply(): void {
    const values = this.form.getRawValue();
    this.formError.set('');
    if (values.from && values.until && values.from > values.until) {
      this.formError.set('A data final deve ser igual ou posterior à inicial.');
      return;
    }
    const boundary = (value: string, nextDay = false) => {
      if (!value) return '';
      const [year, month, day] = value.split('-').map(Number);
      return new Date(year, month - 1, day + Number(nextDay)).toISOString();
    };
    void this.router.navigate(['/admin/leads'], {
      queryParams: {
        page: 1,
        search: values.search.trim() || null,
        entry: values.entry || null,
        followUpStatus: values.followUpStatus || null,
        from: boundary(values.from) || null,
        until: boundary(values.until, true) || null,
      },
    });
  }
  clear(): void {
    this.form.reset();
    this.formError.set('');
    void this.router.navigate(['/admin/leads']);
  }
  go(page: number): void {
    void this.router.navigate(['/admin/leads'], { queryParams: { ...this.queryParams(), page } });
  }
}
