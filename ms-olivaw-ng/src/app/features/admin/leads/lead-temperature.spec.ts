import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';
import { LeadTemperature } from './lead-temperature';
describe('termômetro de interesse', () => {
  it('apresenta desconhecido sem sugerir interesse por padrão', () => {
    const fixture = TestBed.createComponent(LeadTemperature);
    fixture.componentRef.setInput('detail', true);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Interesse não confirmado');
    expect(fixture.nativeElement.querySelectorAll('.filled')).toHaveLength(0);
  });
  it('mostra motivo e intensidade textual, independente das cores', () => {
    const fixture = TestBed.createComponent(LeadTemperature);
    fixture.componentRef.setInput('engagement', {
      level: 'hot',
      reason: 'Pediu uma proposta.',
      updatedAt: '2026-11-08T10:00:00Z',
    });
    fixture.componentRef.setInput('detail', true);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Quente');
    expect(fixture.nativeElement.textContent).toContain('Pediu uma proposta.');
    expect(fixture.nativeElement.querySelectorAll('.filled')).toHaveLength(3);
    fixture.componentRef.setInput('engagement', {
      level: 'closed',
      reason: 'Pediu para não receber contatos.',
    });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Não contatar');
    expect(fixture.nativeElement.querySelectorAll('.filled')).toHaveLength(0);
  });
});
