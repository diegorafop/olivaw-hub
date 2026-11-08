import { Component, DestroyRef, effect, inject, input, signal, untracked } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { PortalApi, type PortalDelivery, type PortalService } from './portal-api';
import { inventoryError } from '../admin/inventory/inventory-api';
@Component({
  selector: 'app-portal-deliveries',
  imports: [ReactiveFormsModule, DatePipe],
  template: ` <section aria-label="Histórico de entregas">
    <h3>O que foi entregue</h3>
    <p>
      Funcionalidades, melhorias e correções publicadas pela equipe. Pedidos em análise não
      representam entregas concluídas.
    </p>
    <form [formGroup]="filters" (ngSubmit)="load()" class="fields" aria-label="Filtrar entregas">
      <label
        >Sistema<select formControlName="serviceExternalId">
          <option value="">Todos os sistemas</option>
          @for (service of services(); track service.serviceExternalId) {
            <option [value]="service.serviceExternalId">{{ service.name }}</option>
          }
        </select></label
      ><label
        >Tipo<select formControlName="category">
          <option value="">Todos</option>
          <option value="feature">Funcionalidades</option>
          <option value="improvement">Melhorias</option>
          <option value="fix">Correções</option>
        </select></label
      ><label>Desde<input type="date" formControlName="from" /></label
      ><label>Até<input type="date" formControlName="to" /></label
      ><button type="submit" [disabled]="loading()">Filtrar</button>
    </form>
    @if (error()) {
      <p role="alert">{{ error() }}</p>
    }
    @if (loading()) {
      <p role="status">Carregando entregas…</p>
    }
    @for (note of notes(); track note.externalId) {
      <article class="surface">
        <p>
          {{ note.deliveredAt | date: 'dd/MM/yyyy' }} · {{ labels[note.category] || note.category }}
        </p>
        <h4>{{ note.title }}</h4>
        @if (note.serviceLabel) {
          <p>{{ note.serviceLabel }}</p>
        }
        <p style="white-space: pre-line">{{ note.summary }}</p>
        @if (note.version) {
          <p>Versão {{ note.version }}</p>
        }
        @if (note.updatedAt) {
          <p>Nota atualizada em {{ note.updatedAt | date: 'dd/MM/yyyy HH:mm' }}</p>
        }
      </article>
    } @empty {
      <p>Nenhuma entrega publicada neste período.</p>
    }
    @if (nextId() && nextAt()) {
      <button type="button" [disabled]="loading()" (click)="load(true)">Entregas anteriores</button>
    }
  </section>`,
  styleUrl: '../admin/inventory/inventory.scss',
})
export class PortalDeliveries {
  readonly customer = input.required<string>();
  readonly preview = input(false);
  readonly services = input<PortalService[]>([]);
  private readonly api = inject(PortalApi);
  private readonly destroy = inject(DestroyRef);
  readonly labels: Record<string, string> = {
    feature: 'Funcionalidade',
    improvement: 'Melhoria',
    fix: 'Correção',
  };
  readonly notes = signal<PortalDelivery[]>([]);
  readonly nextId = signal<number | null>(null);
  readonly nextAt = signal<string | null>(null);
  readonly error = signal('');
  readonly loading = signal(false);
  readonly filters = new FormGroup({
    serviceExternalId: new FormControl('', { nonNullable: true }),
    category: new FormControl('', { nonNullable: true }),
    from: new FormControl('', { nonNullable: true }),
    to: new FormControl('', { nonNullable: true }),
  });
  constructor() {
    effect(() => {
      const customer = this.customer();
      untracked(() => this.load(false, customer));
    });
  }
  load(older = false, customer = this.customer()) {
    const values = this.filters.getRawValue();
    this.error.set('');
    if (values.from && values.to && values.from > values.to) {
      this.error.set('A data final deve ser posterior à inicial.');
      return;
    }
    const fromAt = values.from ? new Date(values.from + 'T00:00:00').toISOString() : undefined;
    const toAt = values.to ? new Date(values.to + 'T23:59:59.999').toISOString() : undefined;
    this.loading.set(true);
    this.api
      .deliveries(customer, this.preview(), {
        serviceExternalId: values.serviceExternalId,
        category: values.category,
        fromAt,
        toAt,
        beforeId: older ? (this.nextId() ?? undefined) : undefined,
        beforeAt: older ? (this.nextAt() ?? undefined) : undefined,
      })
      .pipe(
        takeUntilDestroyed(this.destroy),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (response) => {
          if (customer !== this.customer()) return;
          this.notes.set(response.items);
          this.nextId.set(response.nextBeforeId);
          this.nextAt.set(response.nextBeforeAt);
        },
        error: (e) => this.error.set(inventoryError(e)),
      });
  }
}
