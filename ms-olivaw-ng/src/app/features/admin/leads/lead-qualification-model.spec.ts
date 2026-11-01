import { describe, expect, it } from 'vitest';
import { approachSuggestion, emptyQualification, webUrl } from './lead-qualification-model';
describe('qualificação e abordagem', () => {
  it('prepara uma conversa pertinente sem inventar observações ou enviar contato', () => {
    const qualification = {
      ...emptyQualification(),
      segment: 'eventos',
      region: 'Curitiba',
      presence: 'website',
      positives: 'Portfólio organizado por tipo de evento.',
    };
    const result = approachSuggestion('Empresa de exemplo', qualification);
    expect(result).toContain('pessoal da Empresa de exemplo');
    expect(result).toContain('eventos em Curitiba');
    expect(result).toContain('Portfólio organizado');
    expect(result).toContain('O site atual atende bem');
    expect(result).not.toContain('site ruim');
    expect(result).not.toContain('sem backup');
    expect(qualification.approachStatus).toBe('draft');
  });
  it('não inventa região, ramo ou site existente quando faltam dados', () => {
    const result = approachSuggestion('', emptyQualification());
    expect(result).toContain('Olá! Somos da Olivaw');
    expect(result).not.toContain('Curitiba');
    expect(result).not.toContain('site atual');
    expect(result).not.toContain('destacamos:');
  });
  it('só permite links web sem credenciais', () => {
    expect(webUrl('javascript:alert(1)')).toBeNull();
    expect(webUrl('https://user:pass@example.test')).toBeNull();
    expect(webUrl('file:///etc/passwd')).toBeNull();
    expect(webUrl('')).toBeNull();
    expect(webUrl('https://example.test/contato')).toBe('https://example.test/contato');
  });
});
