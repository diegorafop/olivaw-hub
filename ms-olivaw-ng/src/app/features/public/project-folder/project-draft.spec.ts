import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ProjectDraft } from './project-draft';

function setup() {
  TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
  return { draft: TestBed.inject(ProjectDraft), http: TestBed.inject(HttpTestingController) };
}
beforeEach(() => sessionStorage.clear());
afterEach(() => {
  TestBed.inject(HttpTestingController).verify();
  sessionStorage.clear();
});

it('reúne a jornada sem chamadas e limita o histórico', () => {
  const { draft, http } = setup();
  draft.enter('studio');
  for (let i = 0; i < 55; i++) draft.record('option-added', String(i));
  draft.requirements.set('Meu controle de entregas');
  expect(draft.events()).toHaveLength(40);
  expect(draft.snapshot().entry).toBe('studio');
  http.expectNone(() => true);
});

it('reenvia a mesma revisão após falha sem perder o rascunho', async () => {
  const { draft, http } = setup();
  draft.start({ source: 'studio', title: 'Minha loja', choices: ['Categorias'], context: '' });
  draft.email.set('lead@example.test');
  draft.requirements.set('Entregas organizadas');
  const first = draft.submit();
  const failed = http.expectOne((request) => request.method === 'PUT');
  const revision = failed.request.body.revision;
  const key = failed.request.headers.get('X-Lead-Key');
  const url = failed.request.url;
  failed.flush({}, { status: 503, statusText: 'Unavailable' });
  await first;
  expect(draft.sent()).toBe(false);
  expect(draft.requirements()).toBe('Entregas organizadas');
  const retry = draft.submit();
  const saved = http.expectOne(url);
  expect(saved.request.body.revision).toBe(revision);
  expect(saved.request.headers.get('X-Lead-Key')).toBe(key);
  saved.flush({ revision });
  await retry;
  expect(draft.sent()).toBe(true);
});

it('não envia um e-mail inválido', async () => {
  const { draft, http } = setup();
  draft.email.set('invalido');
  await draft.submit();
  http.expectNone(() => true);
  expect(draft.deliveryMessage()).toContain('e-mail ou WhatsApp válido');
});

it('restaura o JSON da aba após recriar o estado', async () => {
  const { draft, http } = setup();
  draft.start({ source: 'studio', title: 'Minha loja', choices: [], context: '' });
  draft.requirements.set('Preservar minha ideia');
  draft.journeyStep.set(3);
  draft.email.set('lead@example.test');
  const send = draft.submit();
  http.expectOne((request) => request.method === 'PUT').flush({});
  await send;
  TestBed.resetTestingModule();
  const restored = setup().draft;
  expect(restored.seed()?.title).toBe('Minha loja');
  expect(restored.requirements()).toBe('Preservar minha ideia');
  expect(restored.email()).toBe('lead@example.test');
  expect(restored.journeyStep()).toBe(3);
});

it('não confirma como enviada uma alteração feita durante o envio', async () => {
  const { draft, http } = setup();
  draft.start({ source: 'studio', title: 'Minha loja', choices: [], context: '' });
  draft.email.set('lead@example.test');
  draft.requirements.set('Primeira ideia');
  const sending = draft.submit();
  const request = http.expectOne((item) => item.method === 'PUT');
  draft.requirements.set('Ideia atualizada durante o envio');
  request.flush({});
  await sending;
  expect(draft.sent()).toBe(false);
  expect(draft.deliveryMessage()).toContain('novas alterações');
  expect(draft.requirements()).toBe('Ideia atualizada durante o envio');
});

it('envia WhatsApp sem e-mail e restaura o contato após recarregar', async () => {
  const { draft, http } = setup();
  draft.start({ source: 'studio', title: 'Pastelaria', choices: ['Categorias'], context: '' });
  draft.business.set('Pastelaria');
  draft.requirements.set('Quero receber pedidos');
  draft.editWhatsapp('(11) 98765-4321');
  const send = draft.submit();
  const request = http.expectOne((item) => item.method === 'PUT');
  expect(request.request.body.whatsapp).toBe('11987654321');
  expect(request.request.body.email).toBe('');
  expect(request.request.body.business).toBe('Pastelaria');
  request.flush({});
  await send;
  expect(draft.sent()).toBe(true);
  draft.editWhatsapp('21987654321');
  expect(draft.sent()).toBe(false);
  // Envio persiste a última versão do rascunho antes de chamar a API.
  const update = draft.submit();
  http.expectOne((item) => item.method === 'PUT').flush({});
  await update;
  TestBed.resetTestingModule();
  expect(setup().draft.whatsapp()).toBe('21987654321');
});
it('bloqueia ausência de contato e ambos os campos quando algum está inválido', async () => {
  const { draft, http } = setup();
  await draft.submit();
  expect(draft.contactAttempted()).toBe(true);
  draft.editWhatsapp('11987654321');
  draft.editEmail('pastelaria');
  await draft.submit();
  http.expectNone(() => true);
  draft.editEmail('lead@example.test');
  draft.editWhatsapp('11');
  await draft.submit();
  http.expectNone(() => true);
});
