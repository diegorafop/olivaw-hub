import { expect, test, type Page } from '@playwright/test';
import type { ServiceDetail } from '../src/app/features/admin/services/services-api';
const id = '30000000-0000-0000-0000-000000000018';
const customers = [
  {
    externalId: '10000000-0000-0000-0000-000000000001',
    business: 'Empresa Aurora',
    owner: '',
    status: 'active',
    createdAt: '2026-10-22T12:00:00Z',
  },
  {
    externalId: '10000000-0000-0000-0000-000000000002',
    business: 'Empresa Horizonte',
    owner: '',
    status: 'active',
    createdAt: '2026-10-22T12:00:00Z',
  },
];
async function setup(page: Page, conflict = false) {
  const writes: Record<string, unknown>[] = [];
  let service: (ServiceDetail & { customerExternalIds: string[] }) | null = null;
  await page.route('https://api.olivaw.com.br/**', (route) => {
    const url = new URL(route.request().url());
    const path = url.pathname;
    const method = route.request().method();
    if (path.endsWith('/login'))
      return route.fulfill({
        json: { accessToken: 'test', expiresAt: new Date(Date.now() + 300000).toISOString() },
      });
    if (path.endsWith('/access'))
      return route.fulfill({
        json: { name: 'Operador', isRoot: true, permissions: [], userProfileId: 'test' },
      });
    if (path === '/leads/summary') return route.fulfill({ json: { total: 0, recent: 0 } });
    if (path === '/customers/summary')
      return route.fulfill({ json: { total: 2, onboarding: 0, active: 2, paused: 0, closed: 0 } });
    if (path === '/customers')
      return route.fulfill({ json: { page: 1, pageSize: 20, total: 2, items: customers } });
    const customer = customers.find((item) => path === `/customers/${item.externalId}`);
    if (customer)
      return route.fulfill({
        json: {
          ...customer,
          revision: 1,
          legalName: '',
          document: null,
          notes: '',
          sourceLeadExternalId: null,
          updatedAt: null,
          contacts: [{ name: 'Contato', role: '', email: 'contact@example.test', whatsapp: '' }],
          changes: [],
        },
      });
    if (path === '/services' && method === 'GET')
      return route.fulfill({
        json: {
          page: Number(url.searchParams.get('page') ?? 1),
          pageSize: 20,
          total: service ? 1 : 0,
          items: service ? [{ ...service, customerCount: service.customers.length }] : [],
        },
      });
    if (path === '/services' && method === 'POST') {
      const body = route.request().postDataJSON();
      writes.push(body);
      service = {
        ...body,
        externalId: id,
        revision: 1,
        customers: body.customerExternalIds.map((customerId: string) => ({
          customerExternalId: customerId,
          business: customers.find((item) => item.externalId === customerId)!.business,
          startedAt: '2026-10-22T12:00:00Z',
          endedAt: null,
          startedByName: 'Operador',
          endedByName: null,
        })),
        previousCustomers: [],
        changes: [],
      };
      service.changes = [
        {
          ...service,
          revision: 1,
          createdAt: '2026-10-22T12:00:00Z',
          actorName: 'Operador',
          previousStatus: 'planned',
        },
      ];
      return route.fulfill({ status: 201, json: { externalId: id, revision: 1 } });
    }
    if (path === `/services/${id}` && method === 'GET') return route.fulfill({ json: service });
    if (path === `/services/${id}` && method === 'PUT') {
      const body = route.request().postDataJSON();
      writes.push(body);
      if (conflict)
        return route.fulfill({
          status: 409,
          json: { detail: 'Este serviço mudou. Reabra a ficha antes de salvar.' },
        });
      const ended = service!.customers
        .filter((item) => !body.customerExternalIds.includes(item.customerExternalId))
        .map((item) => ({
          ...item,
          endedAt: '2026-10-23T12:00:00Z',
          endedByName: 'Operador',
        }));
      service = {
        ...service,
        ...body,
        revision: service!.revision + 1,
        previousCustomers: [...ended, ...service!.previousCustomers],
        customers: body.customerExternalIds.map((customerId: string) => ({
          customerExternalId: customerId,
          business: customers.find((item) => item.externalId === customerId)!.business,
          startedAt: '2026-10-22T12:00:00Z',
          endedAt: null,
          startedByName: 'Operador',
          endedByName: null,
        })),
      };
      return route.fulfill({ json: { externalId: id, revision: service.revision } });
    }
    throw new Error(`Requisição inesperada: ${method} ${path}`);
  });
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill('root');
  await page.getByLabel('Senha', { exact: true }).fill('test-password');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page).toHaveURL(/\/admin$/);
  return writes;
}
for (const mobile of [false, true]) {
  test(`serviços ${mobile ? 'mobile' : 'desktop'} cadastra aplicação compartilhada e preserva encerramento dos vínculos`, async ({
    page,
  }, testInfo) => {
    await page.setViewportSize({ width: mobile ? 390 : 1440, height: 1000 });
    const writes = await setup(page);
    await page.getByRole('link', { name: 'Serviços', exact: true }).click();
    await expect(page.getByText('Nenhum serviço neste recorte.')).toBeVisible();
    await page.getByRole('link', { name: 'Cadastrar serviço', exact: true }).click();
    await page.getByLabel('Nome de apresentação').fill('Aplicação compartilhada');
    await page.getByLabel('Identificador técnico').fill('ms-example-api');
    await page.getByRole('button', { name: 'Cadastrar serviço', exact: true }).click();
    await expect(page.getByRole('alert')).toContainText('ao menos um cliente');
    expect(writes).toHaveLength(0);
    await page.getByRole('button', { name: 'Vincular Empresa Aurora', exact: true }).click();
    await page.getByRole('button', { name: 'Vincular Empresa Horizonte', exact: true }).click();
    await page.getByRole('button', { name: 'Cadastrar serviço', exact: true }).click();
    await expect(page.getByRole('heading', { name: 'Ficha do serviço' })).toBeVisible();
    expect(writes[0]['customerExternalIds']).toHaveLength(2);
    await expect(page.getByLabel('Identificador técnico')).toBeDisabled();
    await page.getByLabel('Situação cadastral').selectOption('closed');
    page.once('dialog', (dialog) => dialog.accept());
    await page.getByRole('button', { name: 'Salvar alterações' }).click();
    await expect(page.getByText('Serviço atualizado.', { exact: true })).toBeVisible();
    await expect(page.getByText('Nenhum vínculo ativo.', { exact: true })).toBeVisible();
    expect(writes.at(-1)!['customerExternalIds']).toEqual([]);
    await expect(page.getByRole('heading', { name: 'Vínculos encerrados' })).toBeVisible();
    await page.screenshot({
      path: testInfo.outputPath(`services-${mobile ? 'mobile' : 'desktop'}.png`),
      fullPage: true,
    });
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(
      mobile ? 390 : 1440,
    );
  });
}
test('serviço interno não exige empresa fictícia e conflito conserva formulário', async ({
  page,
}) => {
  const writes = await setup(page, true);
  await page.goto('/admin/services/new');
  await page.getByLabel('Nome de apresentação').fill('Aplicação interna');
  await page.getByLabel('Identificador técnico').fill('ms-internal-worker');
  await page.getByLabel('Escopo').selectOption('internal');
  await page.getByRole('button', { name: 'Cadastrar serviço', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Ficha do serviço' })).toBeVisible();
  expect(writes[0]['customerExternalIds']).toEqual([]);
  await page.getByLabel('Descrição').fill('Texto mantido em conflito');
  await page.getByRole('button', { name: 'Salvar alterações' }).click();
  await expect(page.getByRole('alert')).toContainText('Este serviço mudou');
  await expect(page.getByLabel('Descrição')).toHaveValue('Texto mantido em conflito');
  await expect(page.getByRole('button', { name: 'Salvar alterações' })).toBeDisabled();
});
test('ficha do cliente prepara cadastro de serviço sem persistir automaticamente', async ({
  page,
}) => {
  const writes = await setup(page);
  await page.goto(`/admin/customers/${customers[0].externalId}`);
  await page.getByRole('link', { name: 'Cadastrar serviço para este cliente' }).click();
  await expect(
    page.getByRole('button', { name: 'Retirar Empresa Aurora', exact: true }),
  ).toBeVisible();
  expect(writes).toHaveLength(0);
});
