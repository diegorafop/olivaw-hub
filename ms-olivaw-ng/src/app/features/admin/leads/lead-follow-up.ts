import {
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { LeadsApi, type LeadDetailData, type LeadHistory } from './leads-api';
import { LEAD_STATUSES } from './lead-status';

@Component({
  selector: 'app-lead-follow-up',
  imports: [DatePipe, ReactiveFormsModule],
  templateUrl: './lead-follow-up.html',
  styleUrl: './lead-follow-up.scss',
})
export class LeadFollowUp {
  readonly lead = input.required<LeadDetailData>();
  readonly saved = output<void>();
  readonly reload = output<void>();
  readonly statuses = LEAD_STATUSES;
  readonly options = Object.entries(LEAD_STATUSES);
  private readonly api = inject(LeadsApi);
  private readonly destroyRef = inject(DestroyRef);
  readonly pending = signal(false);
  readonly error = signal('');
  readonly conflict = signal(false);
  readonly before = signal<number | null>(null);
  readonly historyRefresh = signal(0);
  readonly form = new FormGroup({
    status: new FormControl('new', { nonNullable: true }),
    note: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(3000)] }),
  });
  readonly canSave = computed(() => !this.pending() && !this.conflict());
  readonly history = toSignal(
    combineLatest([
      toObservable(this.lead),
      toObservable(this.before),
      toObservable(this.historyRefresh),
    ]).pipe(
      switchMap(([lead, before]) =>
        this.api.history(lead.externalId, before).pipe(
          map((data) => ({ data, loading: false, error: false })),
          catchError(() => of({ data: null as LeadHistory | null, loading: false, error: true })),
          startWith({ data: null as LeadHistory | null, loading: true, error: false }),
        ),
      ),
    ),
    { initialValue: { data: null as LeadHistory | null, loading: true, error: false } },
  );
  constructor() {
    effect(() => {
      const lead = this.lead();
      this.form.reset({ status: lead.followUpStatus ?? 'new', note: '' });
      this.before.set(null);
      this.error.set('');
      this.conflict.set(false);
    });
  }
  retryHistory(): void {
    this.historyRefresh.update((value) => value + 1);
  }
  save(): void {
    if (!this.canSave()) return;
    const value = this.form.getRawValue();
    const note = value.note.trim();
    this.error.set('');
    if (this.form.invalid) {
      this.error.set('A nota deve ter até 3.000 caracteres.');
      return;
    }
    if (value.status === (this.lead().followUpStatus ?? 'new') && !note) {
      this.error.set('Informe uma nota ou altere a etapa.');
      return;
    }
    this.pending.set(true);
    this.api
      .updateFollowUp(this.lead().externalId, {
        revision: this.lead().followUpRevision ?? 0,
        status: value.status,
        note,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.pending.set(false);
          this.saved.emit();
        },
        error: (error: unknown) => {
          this.pending.set(false);
          if (error instanceof HttpErrorResponse && error.status === 409) {
            this.conflict.set(true);
            this.error.set(
              'Outra pessoa atualizou este lead. Copie sua nota e recarregue a ficha para conferir antes de salvar novamente.',
            );
            return;
          }
          this.error.set(
            'Não foi possível confirmar o salvamento. Seu texto foi mantido. Confira o histórico antes de tentar novamente.',
          );
        },
      });
  }
}
