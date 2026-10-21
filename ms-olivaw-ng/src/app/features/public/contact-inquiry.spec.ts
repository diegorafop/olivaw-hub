import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ContactInquiry } from './contact-inquiry';
import { ProjectDraft } from './project-folder/project-draft';
import { LOCAL_ERROR, SKIP_AUTH } from '../../core/http/http-context';

function setup() {
  TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
  return { inquiry: TestBed.inject(ContactInquiry), http: TestBed.inject(HttpTestingController) };
}
beforeEach(() => sessionStorage.clear());
afterEach(() => {
  TestBed.inject(HttpTestingController).verify();
  sessionStorage.clear();
});

it('não chama a API ao escrever nem envia um contato incompleto', async () => {
  const { inquiry, http } = setup();
  inquiry.editMessage('Tenho uma pergunta');
  await inquiry.send();
  http.expectNone(() => true);
  expect(inquiry.feedback()).toContain('Informe seu e-mail');
});

it('repete a mesma revisão após falha sem alterar a pasta de projeto', async () => {
  const { inquiry, http } = setup();
  const draft = TestBed.inject(ProjectDraft);
  draft.start({ source: 'studio', title: 'Minha loja', choices: ['Categorias'], context: '' });
  draft.requirements.set('Minha ideia no estúdio');
  inquiry.editEmail('lead@example.test');
  inquiry.editMessage('Tenho uma dúvida sobre os planos');
  const first = inquiry.send();
  const failed = http.expectOne((request) => request.method === 'PUT');
  const url = failed.request.url;
  const body = failed.request.body;
  const key = failed.request.headers.get('X-Lead-Key');
  expect(failed.request.context.get(SKIP_AUTH)).toBe(true);
  expect(failed.request.context.get(LOCAL_ERROR)).toBe(true);
  expect(body.project.title).toBe('Quero conversar com a Olivaw');
  expect(body.requirements).toBe('Tenho uma dúvida sobre os planos');
  failed.flush({}, { status: 503, statusText: 'Unavailable' });
  await first;
  expect(inquiry.sent()).toBe(false);
  const retry = inquiry.send();
  const saved = http.expectOne(url);
  expect(saved.request.body).toEqual(body);
  expect(saved.request.headers.get('X-Lead-Key')).toBe(key);
  saved.flush({});
  await retry;
  expect(inquiry.sent()).toBe(true);
  expect(draft.seed()?.title).toBe('Minha loja');
  expect(draft.requirements()).toBe('Minha ideia no estúdio');
});

it('impede envio simultâneo e identifica alterações feitas enquanto envia', async () => {
  const { inquiry, http } = setup();
  inquiry.editEmail('lead@example.test');
  inquiry.editMessage('Primeira pergunta');
  const sending = inquiry.send();
  const request = http.expectOne((item) => item.method === 'PUT');
  await inquiry.send();
  http.expectNone(() => true);
  inquiry.editMessage('Uma nova pergunta');
  request.flush({});
  await sending;
  expect(inquiry.sent()).toBe(false);
  expect(inquiry.feedback()).toContain('novas alterações');
  expect(inquiry.message()).toBe('Uma nova pergunta');
});
