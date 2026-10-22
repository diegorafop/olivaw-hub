import { formatWhatsapp } from '../../public/contact-details';
import { Component, computed, input } from '@angular/core';
import { DatePipe } from '@angular/common';
import type { LeadDetailData } from './leads-api';
@Component({
  selector: 'app-lead-detail',
  imports: [DatePipe],
  templateUrl: './lead-detail.html',
  styleUrl: './lead-detail.scss',
})
export class LeadDetail {
  readonly formatPhone = formatWhatsapp;
  readonly lead = input.required<LeadDetailData>();
  readonly snapshot = computed(() => this.lead().snapshot);
  readonly included = computed(
    () =>
      this.snapshot()?.project?.choices.filter(
        (choice) => !this.snapshot()?.excluded.includes(choice),
      ) ?? [],
  );
  readonly whatsappUrl = computed(() => {
    const number = this.snapshot()?.whatsapp ?? '';
    return /^[1-9][0-9]9[0-9]{8}$/.test(number) ? `https://wa.me/55${number}` : null;
  });
  readonly emailUrl = computed(() => `mailto:${encodeURIComponent(this.snapshot()?.email ?? '')}`);
  readonly eventNames: Record<string, string> = {
    entered: 'Entrada na jornada',
    'project-opened': 'Pasta iniciada',
    'journey-step': 'Etapa visitada',
    'option-added': 'Opção adicionada',
  };
}
