import { describe, expect, it } from 'vitest';
import {
  approachSuggestion,
  emptyQualification,
  webUrl,
  assessmentStrategy,
  type WebsiteAssessment,
} from './lead-qualification-model';
describe('qualificação e abordagem', () => {
  it('usa observação específica e origem somente para o endereço encontrado', () => {
    const qualification = {
      ...emptyQualification(),
      presence: 'website',
      commercialObservation: 'Montagem de estandes corporativos.',
      publicEmails: [{ email: 'contato@example.test', sourceUrl: 'https://example.test/contato' }],
    };
    const result = approachSuggestion('Empresa de exemplo', qualification, 'contato@example.test');
    expect(result).toContain('Montagem de estandes corporativos.');
    expect(result).toContain('https://example.test/contato');
    expect(
      approachSuggestion('Empresa de exemplo', qualification, 'outro@example.test'),
    ).not.toContain('Encontramos este e-mail');
    expect(result).not.toContain('sem backup');
    expect(qualification.approachStatus).toBe('draft');
  });
  it('mantém desconhecido separado de zero e escolhe oportunidade sem expor notas', () => {
    const metrics = Object.fromEntries(
      ['modernity', 'content', 'ui', 'ux', 'management'].map((key) => [
        key,
        { score: null, evidence: 'Não avaliado.', confidence: 'unknown', sourceUrl: '' },
      ]),
    );
    const assessment = {
      schemaVersion: 1,
      assessedAt: '2026-11-08T10:00:00Z',
      basis: 'simulation',
      strategy: 'discovery',
      metrics,
    } as WebsiteAssessment;
    expect(assessmentStrategy(assessment)).toBe('discovery');
    assessment.metrics.ui = {
      score: 5,
      evidence: 'Simulação: melhorar consistência visual.',
      confidence: 'provisional',
      sourceUrl: '',
    };
    assessment.metrics.ux = { ...assessment.metrics.ui, score: 5 };
    expect(assessmentStrategy(assessment)).toBe('ui');
    const result = approachSuggestion('Empresa Exemplo', {
      ...emptyQualification(),
      websiteAssessment: assessment,
    });
    expect(result).toContain('identidade');
    expect(result).not.toContain('5/10');
    expect(result).not.toContain('ruim');
    assessment.metrics.content.score = 0;
    expect(assessmentStrategy(assessment)).toBe('content');
  });
  it('não inventa visita nem elogio quando faltam dados', () => {
    const result = approachSuggestion('', emptyQualification());
    expect(result).toContain('Somos da Olivaw');
    expect(result).not.toContain('Curitiba');
    expect(result).not.toContain('Ao conhecer');
    expect(result).not.toContain('Encontramos');
  });
  it('só permite links web sem credenciais', () => {
    expect(webUrl('javascript:alert(1)')).toBeNull();
    expect(webUrl('https://user:pass@example.test')).toBeNull();
    expect(webUrl('file:///etc/passwd')).toBeNull();
    expect(webUrl('')).toBeNull();
    expect(webUrl('https://example.test/contato')).toBe('https://example.test/contato');
  });
});
