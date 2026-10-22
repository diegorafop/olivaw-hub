import { expect, test, type Page } from '@playwright/test';
import type { CustomerDetail } from '../src/app/features/admin/customers/customers-api';
const id = '10000000-0000-0000-0000-000000000001';
const leadId = '20000000-0000-0000-0000-000000000002';
async function setup(page: Page, options: { conflict?: boolean; failList?: boolean } = {}) {
  const writes: { method: string; body: Record<string, unknown> }[] = [];
  let failed = false;
  let customer: CustomerDetail = {
    externalId: id,
    revision: 1,
    business: 'Empresa Aurora',
    legalName: '',
    document: null,
    owner: '',
    notes: '',
    status: 'onboarding',
    sourceLeadExternalId: null,
    createdAt: '2026-10-21T10:00:00Z',
    updatedAt: null,
    contacts: [{ name: 'Maria', role: 'Gerente', email: '', whatsapp: '11987654321' }],
    changes: [
      {
        at: '2026-10-21T10:00:00Z',
        actor: 'Operador',
        action: 'created',
        previousStatus: 'onboarding',
        status: 'onboarding',
      },
    ],
  };
  await page.route('https://api.olivaw.com.br/**', (route) => {
    const url = new URL(route.request().url()),
      path = url.pathname,
      method = route.request().method();
    if (path.endsWith('/login'))
      return route.fulfill({
        json: { accessToken: 'test', expiresAt: new Date(Date.now() + 300000).toISOString() },
      });
    if (path.endsWith('/access'))
      return route.fulfill({
        json: { name: 'Operador', isRoot: true, permissions: [], userProfileId: 'test' },
      });
    expect(route.request().headers()['authorization']).toBe('Bearer test');
    if (path === '/leads/summary')
      return route.fulfill({
        json: { total: 1, recent: 1, since: '2026-10-14T00:00:00Z', asOf: '2026-10-21T00:00:00Z' },
      });
    if (path === '/customers/summary')
      return route.fulfill({
        json: {
          total: 1,
          onboarding: 1,
          active: 0,
          paused: 0,
          closed: 0,
          asOf: '2026-10-21T00:00:00Z',
        },
      });
    if (path === `/leads/${leadId}/history`)
      return route.fulfill({ json: { items: [], nextBeforeRevision: null } });
    if (path === `/leads/${leadId}`)
      return route.fulfill({
        json: {
          externalId: leadId,
          createdAt: '2026-10-21T00:00:00Z',
          updatedAt: null,
          contentUnavailable: false,
          snapshot: {
            business: 'Pastelaria Aurora',
            requirements: 'Organizar pedidos',
            email: '',
            whatsapp: '11987654321',
            project: {
              source: 'studio',
              title: 'Vender produtos',
              context: '',
              choices: [],
              blockIds: [],
            },
            excluded: [],
            events: [],
            plan: null,
          },
        },
      });
    if (path === '/leads')
      return route.fulfill({
        json: {
          page: 1,
          pageSize: 20,
          total: 1,
          asOf: '2026-10-21T00:00:00Z',
          items: [
            {
              externalId: leadId,
              business: 'Pastelaria Aurora',
              title: 'Vender produtos',
              entry: 'studio',
              email: '',
              whatsapp: '11987654321',
              createdAt: '2026-10-21T00:00:00Z',
              updatedAt: null,
            },
          ],
        },
      });
    if (path === '/customers' && method === 'GET') {
      if (options.failList && !failed) {
        failed = true;
        return route.fulfill({ status: 503, json: {} });
      }
      return route.fulfill({
        json: {
          page: Number(url.searchParams.get('page') ?? 1),
          pageSize: 20,
          total: url.searchParams.get('search') === 'inexistente' ? 0 : 1,
          items: url.searchParams.get('search') === 'inexistente' ? [] : [customer],
        },
      });
    }
    if (path === '/customers' && method === 'POST') {
      const body = route.request().postDataJSON();
      writes.push({ method, body });
      customer = { ...customer, ...body };
      return route.fulfill({ status: 201, json: { externalId: id, revision: 1 } });
    }
    if (path === `/customers/${id}` && method === 'PUT') {
      const body = route.request().postDataJSON();
      writes.push({ method, body });
      if (options.conflict)
        return route.fulfill({
          status: 409,
          json: { detail: 'Este cadastro mudou. Reabra a ficha antes de salvar.' },
        });
      customer = { ...customer, ...body, revision: customer.revision + 1 };
      return route.fulfill({ json: { externalId: id, revision: customer.revision } });
    }
    if (path === `/customers/${id}`) return route.fulfill({ json: customer });
    throw new Error(`Requisição não prevista: ${method} ${path}`);
  });
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill('operador');
  await page.getByLabel('Senha', { exact: true }).fill('password-test');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page).toHaveURL(/\/admin$/);
  return writes;
}
for (const mobile of [false, true]) {
  test(`clientes ${mobile ? 'mobile' : 'desktop'} cadastro valida contatos e mantém empresa sem login`, async ({
    page,
  }, testInfo) => {
    await page.setViewportSize({ width: mobile ? 390 : 1440, height: 1000 });
    const writes = await setup(page);
    const avatar = page.locator('.avatar-scene img');
    await expect(avatar).toHaveAttribute('src', /coordinator-v3.webp/);
    await expect
      .poll(() =>
        avatar.evaluate((image: HTMLImageElement) => image.complete && image.naturalWidth > 0),
      )
      .toBe(true);
    await page.screenshot({
      path: testInfo.outputPath(`overview-${mobile ? 'mobile' : 'desktop'}.png`),
      fullPage: true,
    });
    await page
      .getByRole('navigation', { name: 'Navegação da operação' })
      .getByRole('link', { name: 'Clientes', exact: true })
      .click();
    await expect(page.getByRole('heading', { name: 'Clientes', exact: true })).toBeVisible();
    await page.getByRole('link', { name: 'Novo cliente' }).click();
    await page.getByRole('button', { name: 'Cadastrar cliente' }).click();
    await expect(page.getByText('Informe o nome comercial da empresa.')).toBeVisible();
    expect(writes).toHaveLength(0);
    await page.getByLabel('Nome comercial').fill('   ');
    await page.getByRole('button', { name: 'Cadastrar cliente' }).click();
    expect(writes).toHaveLength(0);
    await page.getByLabel('Nome comercial').fill('Pastelaria Aurora');
    const contact = page.locator('fieldset.contact').first();
    await contact.getByLabel('Nome').fill('Maria');
    await contact.getByLabel('WhatsApp com DDD').fill('11987654321');
    await expect(contact.getByLabel('WhatsApp com DDD')).toHaveValue('(11) 98765-4321');
    await page.getByLabel('Observações internas').fill('Organizar pedidos');
    await page.getByRole('button', { name: 'Cadastrar cliente' }).click();
    await expect(page).toHaveURL(new RegExp(`/admin/customers/${id}$`));
    await expect(page.getByRole('heading', { level: 1, name: 'Pastelaria Aurora' })).toBeVisible();
    expect(writes).toHaveLength(1);
    expect(writes[0].body['contacts']).toEqual([
      { name: 'Maria', role: '', email: '', whatsapp: '11987654321' },
    ]);
    expect(writes[0].body).not.toHaveProperty('userProfileId');
    await expect(page.getByRole('heading', { name: 'Últimas alterações' })).toBeVisible();
    await page.screenshot({
      path: testInfo.outputPath(`customer-${mobile ? 'mobile' : 'desktop'}.png`),
      fullPage: true,
    });
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(
      mobile ? 390 : 1440,
    );
  });
}
test('clientes recupera erro da lista, filtra e protege edição concorrente', async ({ page }) => {
  const writes = await setup(page, { failList: true, conflict: true });
  await page.getByRole('link', { name: 'Ver clientes' }).click();
  await expect(page.getByText('Vamos tentar de novo?')).toBeVisible();
  await page.getByRole('button', { name: 'Tentar novamente' }).click();
  await page.getByLabel('Buscar cliente').fill('inexistente');
  await page.getByRole('button', { name: 'Filtrar' }).click();
  await expect(page.getByRole('heading', { name: 'Nenhum cliente por aqui.' })).toBeVisible();
  await page.getByRole('button', { name: 'Limpar', exact: true }).click();
  await page.getByRole('link', { name: 'Empresa Aurora', exact: true }).click();
  await page.getByLabel('Nome comercial').fill('Nome alterado');
  await page.getByRole('button', { name: 'Salvar alterações' }).click();
  await expect(page.getByRole('alert')).toContainText('Este cadastro mudou');
  await expect(page.getByLabel('Nome comercial')).toHaveValue('Nome alterado');
  expect(writes[0].body['revision']).toBe(1);
});
test('clientes prepara conversão de lead sem salvar automaticamente', async ({ page }) => {
  const writes = await setup(page);
  await page.getByRole('link', { name: 'Ver leads' }).click();
  await page.getByRole('link', { name: /Pastelaria Aurora/ }).click();
  await page.getByRole('link', { name: 'Cadastrar como cliente' }).click();
  await expect(page.getByLabel('Nome comercial')).toHaveValue('Pastelaria Aurora');
  await expect(page.getByLabel('Observações internas')).toHaveValue('Organizar pedidos');
  expect(writes).toHaveLength(0);
});

test('cliente com alterações não impede saída quando sessão expira', async ({ page }) => {
  await page.clock.install();
  await setup(page);
  await page.getByRole('link', { name: 'Ver clientes' }).click();
  await page.getByRole('link', { name: 'Empresa Aurora', exact: true }).click();
  await page.getByLabel('Nome comercial').fill('Alteração não salva');
  await page.clock.fastForward(301000);
  await expect(page).toHaveURL(/\/login$/);
  await expect(page.getByLabel('Nome comercial')).toHaveCount(0);
});
