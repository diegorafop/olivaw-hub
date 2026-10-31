import { Component, computed, HostListener, inject, signal } from '@angular/core';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { LeadsApi } from './leads-api';
import {
  emptyProposal,
  proposalHtml,
  proposalTotals,
  type LeadProposal,
} from './lead-proposal-model';
@Component({
  selector: 'app-lead-proposal',
  imports: [RouterLink, FormsModule, CurrencyPipe, DecimalPipe],
  templateUrl: './lead-proposal.html',
  styleUrl: './lead-proposal.scss',
})
export class LeadProposalPage {
  private readonly api = inject(LeadsApi);
  readonly id = inject(ActivatedRoute).snapshot.paramMap.get('id')!;
  readonly proposal = signal<LeadProposal>(emptyProposal(''));
  readonly ready = signal(false);
  readonly busy = signal(false);
  readonly message = signal('');
  readonly loadError = signal('');
  readonly dirty = signal(false);
  readonly preview = signal(false);
  readonly totals = computed(() => proposalTotals(this.proposal()));
  readonly solutions = [
    ['analysis', 'Análise'],
    ['plugin', 'Plugin'],
    ['integration', 'Integração'],
    ['custom', 'Programação personalizada'],
    ['validation', 'Validação'],
  ];
  readonly statuses = [
    ['draft', 'Rascunho'],
    ['reviewed', 'Revisado'],
    ['sent', 'Enviado'],
    ['negotiation', 'Negociação'],
    ['accepted', 'Aceito'],
    ['declined', 'Recusado'],
  ];
  private revision = 0;
  constructor() {
    void this.load();
  }
  canLeave() {
    return !this.dirty() || window.confirm('Descartar alterações não salvas do orçamento?');
  }
  @HostListener('window:beforeunload', ['$event'])
  beforeUnload(event: BeforeUnloadEvent) {
    if (this.dirty()) event.preventDefault();
  }
  async reload() {
    if (this.canLeave()) await this.load();
  }
  async importFile(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    try {
      if (file.size > 65536) throw new Error();
      const data = JSON.parse(await file.text()) as LeadProposal;
      const sample = emptyProposal('');
      for (const key of Object.keys(sample) as (keyof LeadProposal)[]) {
        if (key !== 'rows' && typeof data[key] !== typeof sample[key]) throw new Error();
      }
      if (
        !Array.isArray(data.rows) ||
        data.rows.length < 1 ||
        data.rows.length > 40 ||
        !data.rows.every(
          (row) =>
            row &&
            typeof row.description === 'string' &&
            typeof row.role === 'string' &&
            this.solutions.some((option) => option[0] === row.solution) &&
            Number.isFinite(row.hours) &&
            row.hours >= 0.25 &&
            Number.isFinite(row.hourlyRate) &&
            row.hourlyRate >= 0.01 &&
            typeof row.optional === 'boolean',
        ) ||
        !Number.isFinite(data.reservePercent) ||
        data.reservePercent < 0 ||
        data.reservePercent > 100 ||
        !Number.isFinite(data.discountPercent) ||
        data.discountPercent < 0 ||
        data.discountPercent > 100 ||
        !Number.isInteger(data.installments) ||
        data.installments < 1 ||
        data.installments > 36
      )
        throw new Error();
      if (!this.canLeave()) return;
      this.proposal.set(data);
      this.dirty.set(true);
      this.message.set('Proposta carregada para revisão. Salve para registrar.');
    } catch {
      this.message.set('JSON de proposta inválido ou maior que 64 KB.');
    }
  }
  async load() {
    this.loadError.set('');
    try {
      const lead = await firstValueFrom(this.api.detail(this.id));
      if (!lead.snapshot || lead.contentUnavailable) {
        this.loadError.set('A pasta não pôde ser lida.');
        return;
      }
      this.revision = lead.snapshot.revision;
      this.proposal.set(
        lead.snapshot.proposal ??
          emptyProposal(lead.snapshot.project?.title ?? 'Proposta de desenvolvimento'),
      );
      this.ready.set(true);
      this.dirty.set(false);
      this.message.set('');
    } catch {
      this.loadError.set('Não foi possível carregar o orçamento. Tente novamente.');
    }
  }
  changed() {
    this.proposal.update((value) => ({ ...value }));
    this.dirty.set(true);
    this.message.set('');
  }
  addRow() {
    this.proposal.update((value) => ({
      ...value,
      rows: [
        ...value.rows,
        {
          description: '',
          role: 'Programador',
          solution: 'integration',
          hours: 1,
          hourlyRate: 100,
          optional: false,
        },
      ],
    }));
    this.dirty.set(true);
  }
  removeRow(index: number) {
    this.proposal.update((value) => ({
      ...value,
      rows: value.rows.filter((_, position) => position !== index),
    }));
    this.dirty.set(true);
  }
  async save() {
    if (this.busy()) return;
    this.busy.set(true);
    this.message.set('');
    try {
      const response = await firstValueFrom(
        this.api.updateProposal(this.id, this.revision, this.proposal()),
      );
      this.revision = response.revision;
      this.dirty.set(false);
      this.message.set('Orçamento salvo.');
    } catch (error) {
      this.message.set(
        error instanceof HttpErrorResponse && error.status === 409
          ? 'A pasta mudou. Guarde suas alterações em HTML antes de recarregar e conferir a versão atual.'
          : 'Não foi possível salvar. Confira os campos e os limites de texto.',
      );
    } finally {
      this.busy.set(false);
    }
  }
  download() {
    const blob = new Blob([proposalHtml(this.proposal())], { type: 'text/html;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = `olivaw-proposta-${this.id}.html`;
    anchor.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }
}
