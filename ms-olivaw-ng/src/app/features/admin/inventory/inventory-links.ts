import {
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import {
  catchError,
  combineLatest,
  debounceTime,
  finalize,
  map,
  of,
  startWith,
  switchMap,
} from 'rxjs';
import { CustomersApi } from '../customers/customers-api';
import {
  InventoryApi,
  relationships,
  environments,
  inventoryError,
  type LinkKind,
  type InventoryLink,
  type Page,
} from './inventory-api';
@Component({
  selector: 'app-inventory-links',
  imports: [DatePipe, RouterLink, ReactiveFormsModule],
  templateUrl: './inventory-links.html',
  styleUrl: './inventory.scss',
})
export class InventoryLinks {
  readonly resource = input('');
  readonly deployment = input('');
  readonly readOnly = input(false);
  readonly preferredCustomer = input('');
  readonly changed = output<void>();
  private readonly api = inject(InventoryApi);
  private readonly customers = inject(CustomersApi);
  private readonly destroy = inject(DestroyRef);
  private prepared = false;
  readonly tab = signal<LinkKind>('customer');
  readonly kind = computed<LinkKind>(() => (this.deployment() ? 'deployment' : this.tab()));
  readonly page = signal(1);
  readonly includeEnded = signal(false);
  readonly refresh = signal(0);
  readonly search = signal('');
  readonly selected = signal<{ externalId: string; name: string } | null>(null);
  readonly pickError = signal('');
  readonly error = signal('');
  readonly success = signal('');
  readonly saving = signal(false);
  readonly relationOptions = Object.entries(relationships);
  readonly relationLabels = relationships;
  readonly environmentLabels = environments;
  readonly form = new FormGroup({
    purpose: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(300)],
    }),
    kind: new FormControl('depends-on', { nonNullable: true }),
    reservedCpu: new FormControl<number | null>(null, [
      Validators.min(0),
      Validators.max(1000000000),
    ]),
    reservedMemoryGiB: new FormControl<number | null>(null, [
      Validators.min(0),
      Validators.max(1000000000),
    ]),
    reservedStorageGiB: new FormControl<number | null>(null, [
      Validators.min(0),
      Validators.max(1000000000),
    ]),
  });
  readonly choices = toSignal(
    combineLatest([
      toObservable(this.search).pipe(debounceTime(200)),
      toObservable(this.kind),
    ]).pipe(
      switchMap(([search, kind]) => {
        this.pickError.set('');
        if (kind === 'customer')
          return this.customers.list(1, search, '').pipe(
            map((page) =>
              page.items.map((item) => ({ externalId: item.externalId, name: item.business })),
            ),
            catchError((error) => {
              this.pickError.set(inventoryError(error));
              return of([]);
            }),
          );
        return this.api.resources(1, { search }).pipe(
          map((page) =>
            page.items
              .filter((item) => item.externalId !== this.resource() && item.status !== 'retired')
              .map((item) => ({ externalId: item.externalId, name: item.name })),
          ),
          catchError((error) => {
            this.pickError.set(inventoryError(error));
            return of([]);
          }),
        );
      }),
    ),
    { initialValue: [] },
  );
  readonly list = toSignal(
    combineLatest([
      toObservable(this.resource),
      toObservable(this.deployment),
      toObservable(this.kind),
      toObservable(this.page),
      toObservable(this.includeEnded),
      toObservable(this.refresh),
    ]).pipe(
      switchMap(([resource, deployment, kind, page, ended]) => {
        const anchor = deployment
          ? { deploymentExternalId: deployment }
          : { resourceExternalId: resource };
        return this.api.links(page, kind, anchor, ended).pipe(
          map((data) => ({ data, loading: false, error: '' })),
          catchError((error) =>
            of({
              data: null as Page<InventoryLink> | null,
              loading: false,
              error: inventoryError(error),
            }),
          ),
          startWith({ data: null as Page<InventoryLink> | null, loading: true, error: '' }),
        );
      }),
    ),
    { initialValue: { data: null as Page<InventoryLink> | null, loading: true, error: '' } },
  );
  constructor() {
    effect(() => {
      const customer = this.preferredCustomer();
      if (!customer || this.prepared) return;
      this.prepared = true;
      this.customers
        .detail(customer)
        .pipe(takeUntilDestroyed(this.destroy))
        .subscribe({
          next: (item) => this.selected.set({ externalId: item.externalId, name: item.business }),
          error: (error) => this.error.set(inventoryError(error)),
        });
    });
  }
  changeTab(kind: LinkKind) {
    this.tab.set(kind);
    this.page.set(1);
    this.selected.set(null);
    this.search.set('');
    this.error.set('');
    this.success.set('');
  }
  toggleHistory(value: boolean) {
    this.includeEnded.set(value);
    this.page.set(1);
  }
  reload() {
    this.refresh.update((value) => value + 1);
  }
  add() {
    if (this.readOnly() || this.saving()) return;
    const selected = this.selected();
    this.form.markAllAsTouched();
    if (!selected || this.form.invalid) {
      this.error.set('Escolha um destino e informe a finalidade e reservas válidas.');
      return;
    }
    const value = this.form.getRawValue();
    const input = {
      resourceExternalId: this.deployment() ? selected.externalId : this.resource(),
      targetExternalId: this.deployment() || selected.externalId,
      purpose: value.purpose,
      kind: value.kind,
      reservedCpu: value.reservedCpu,
      reservedMemoryGiB: value.reservedMemoryGiB,
      reservedStorageGiB: value.reservedStorageGiB,
    };
    this.saving.set(true);
    this.error.set('');
    this.success.set('');
    this.api
      .createLink(this.kind(), input)
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.saving.set(false)),
      )
      .subscribe({
        next: () => {
          this.form.reset();
          this.selected.set(null);
          this.success.set('Vínculo registrado.');
          this.reload();
          this.changed.emit();
        },
        error: (error) => this.error.set(inventoryError(error)),
      });
  }
  close(link: InventoryLink) {
    if (this.saving() || link.endedAt) return;
    if (
      !window.confirm(
        'Encerrar este vínculo? O período e seus autores serão preservados no histórico.',
      )
    )
      return;
    this.saving.set(true);
    this.error.set('');
    this.success.set('');
    this.api
      .closeLink(this.kind(), link.externalId, link.revision)
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.saving.set(false)),
      )
      .subscribe({
        next: () => {
          this.success.set('Vínculo encerrado.');
          this.reload();
          this.changed.emit();
        },
        error: (error) => {
          this.error.set(inventoryError(error));
          this.reload();
        },
      });
  }
}
