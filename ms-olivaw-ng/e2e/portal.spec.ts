import { expect, test, type Page } from '@playwright/test';
const customer = '40000000-0000-0000-0000-000000000020';
const service = '50000000-0000-0000-0000-000000000020';
async function setup(page: Page) {
  const writes: Record<string, unknown>[] = [];
  await page.route('https://api.olivaw.com.br/**', async (route) => {
    const request = route.request();
    const path = new URL(request.url()).pathname;
    if (path === '/portal/sessions' && request.method() === 'POST')
      return route.fulfill({
        json: {
          accessToken: 'portal-test',
          expiresAt: new Date(Date.now() + 300000).toISOString(),
        },
      });
    if (path.startsWith('/portal/'))
      expect(request.headers()['authorization']).toBe('Bearer portal-test');
    if (path === '/portal/companies')
      return route.fulfill({
        json: {
          items: [{ externalId: customer, business: 'Empresa de teste', role: 'requester' }],
          nextBeforeId: null,
        },
      });
    if (path === '/portal/dashboard')
      return route.fulfill({
        json: {
          customerExternalId: customer,
          business: 'Empresa de teste',
          preview: false,
          canRequest: true,
          canReadBilling: false,
          services: [
            {
              externalId: service,
              serviceExternalId: service,
              name: 'Site da empresa',
              publicUrl: 'https://example.test',
              state: 'unknown',
              observedAt: null,
            },
          ],
          currentPlan: null,
          nextBeforeId: null,
        },
      });
    if (path === '/portal/home')
      return route.fulfill({
        json: {
          openRequests: 0,
          waitingRequests: 0,
          completedRequests: 0,
          workingImprovements: 0,
          queuedImprovements: 0,
          completedImprovements: 0,
          storageUsedMiB: null,
          storageLimitMiB: null,
          storagePercent: null,
          observedAt: null,
          from: '2026-09-25T00:00:00Z',
          to: '2026-10-25T00:00:00Z',
          observedDays: 0,
          expectedDays: 30,
          pageViews: null,
          series: [],
        },
      });
    if (path === '/portal/requests/messages')
      return route.fulfill({ json: { items: [], nextBeforeId: null } });
    if (path === '/portal/deliveries')
      return route.fulfill({
        json: {
          items: [
            {
              externalId: service,
              title: 'Catálogo publicado',
              summary: 'Busca por produtos disponível.',
              version: '1.0',
              category: 'feature',
              serviceLabel: 'Site',
              deliveredAt: '2026-10-23T12:00:00Z',
              publishedAt: '2026-10-23T12:01:00Z',
              updatedAt: null,
              revision: 1,
            },
          ],
          nextBeforeId: null,
          nextBeforeAt: null,
        },
      });
    if (path === '/portal/requests' && request.method() === 'POST') {
      writes.push(request.postDataJSON());
      return route.fulfill({ status: 201, json: { externalId: service, protocol: 'OW-EXAMPLE' } });
    }
    if (path === '/portal/requests')
      return route.fulfill({
        json: {
          items: writes.length
            ? [
                {
                  externalId: service,
                  protocol: 'OW-EXAMPLE',
                  title: 'Mais espaço',
                  status: 'received',
                  requestType: 'question',
                  publicSummary: 'Recebemos seu pedido.',
                  serviceLabel: '',
                  createdAt: '2026-10-25T12:00:00Z',
                  revision: 1,
                },
              ]
            : [],
          nextBeforeId: null,
        },
      });
    return route.fulfill({ status: 404, json: { detail: 'Rota não simulada' } });
  });
  await page.goto('/meu-negocio/entrar');
  await page.getByLabel('E-mail', { exact: true }).fill('cliente@example.test');
  await page.getByLabel('Senha', { exact: true }).fill('test-password');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Meu negócio', exact: true })).toBeVisible();
  return writes;
}
for (const width of [1440, 390]) {
  test(`portal mostra fontes desconhecidas, entregas e pedidos reais ${width}`, async ({
    page,
  }) => {
    await page.setViewportSize({ width, height: 900 });
    const writes = await setup(page);
    await expect(page.getByRole('heading', { name: 'Site da empresa' })).toBeVisible();
    await expect(
      page.getByText('Estamos atualizando a verificação', { exact: true }),
    ).toBeVisible();
    await page.getByRole('link', { name: 'Entregas', exact: true }).click();
    await expect(page.getByRole('heading', { name: 'Catálogo publicado' })).toBeVisible();
    await page.getByRole('link', { name: 'Solicitações', exact: true }).click();
    await page.getByRole('link', { name: 'Novo pedido', exact: true }).click();
    await page.getByLabel('Título', { exact: true }).fill('Mais espaço');
    const raw = '  Preciso de espaço.\nQuero avaliar o preço.  ';
    await page.getByLabel('Descreva seu pedido').fill(raw);
    await page.getByRole('button', { name: 'Enviar pedido', exact: true }).click();
    await expect.poll(() => writes.length).toBe(1);
    await expect(page).toHaveURL(new RegExp('request=' + service));
    await expect(page.getByRole('heading', { name: 'Conversa · OW-EXAMPLE' })).toBeVisible();
    await expect(page.locator('dialog')).toHaveCount(0);
    expect(writes[0]['description']).toBe(raw);
    expect(writes[0]['customerExternalId']).toBe(customer);
    expect(writes[0]['requestKey']).toMatch(/^[0-9a-f-]{36}$/);
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(
      true,
    );
  });
}
