import { expect, test, type Page } from '@playwright/test';
const id = '00000000-0000-0000-0000-000000000001';
const createdAt = '2026-10-21T10:00:00Z';
const item = {
  externalId: id,
  createdAt,
  updatedAt: null,
  business: 'Pastelaria Aurora',
  title: 'Vender meus produtos',
  entry: 'studio',
  email: '',
  whatsapp: '11987654321',
};
const snapshot = {
  schemaVersion: 1,
  revision: 1,
  startedAt: createdAt,
  entry: 'studio',
  stage: 'submitted',
  business: 'Pastelaria Aurora',
  requirements: 'Quero organizar pedidos e entregas.',
  email: '',
  whatsapp: '11987654321',
  project: {
    source: 'studio',
    title: 'Vender meus produtos',
    choices: ['Categorias', 'Detalhes do produto'],
    context: 'Catálogo da pastelaria',
  },
  excluded: ['Detalhes do produto'],
  plan: 'Bronze',
  events: [{ name: 'project-opened', value: 'Vender meus produtos', at: createdAt }],
};
async function setup(
  page: Page,
  options: { failFirst?: boolean; paginated?: boolean; detailFails?: boolean } = {},
) {
  let listCount = 0;
  const reads: URL[] = [];
  await page.route('https://api.olivaw.com.br/**', (route) => {
    const url = new URL(route.request().url());
    if (url.pathname.endsWith('/login'))
      return route.fulfill({
        json: { accessToken: 'test', expiresAt: new Date(Date.now() + 120000).toISOString() },
      });
    if (url.pathname.endsWith('/access'))
      return route.fulfill({
        json: { name: 'Operador', isRoot: true, permissions: [], userProfileId: 'test' },
      });
    expect(route.request().headers()['authorization']).toBe('Bearer test');
    if (url.pathname === '/leads/summary')
      return route.fulfill({
        json: { total: 2, recent: 1, since: '2026-10-14T10:00:00Z', asOf: createdAt },
      });
    if (url.pathname === '/leads') {
      reads.push(url);
      listCount++;
      if (options.failFirst && listCount === 1) return route.fulfill({ status: 503, json: {} });
      const pageNumber = Number(url.searchParams.get('page') ?? 1);
      const empty = url.searchParams.get('search') === 'semresultado';
      let items = [
        item,
        {
          ...item,
          externalId: '00000000-0000-0000-0000-000000000002',
          business: '',
          title: 'Quero conversar com a Olivaw',
          email: 'contato@example.test',
          whatsapp: '',
          entry: 'discovery',
        },
      ];
      if (options.paginated)
        items = Array.from({ length: pageNumber === 1 ? 20 : 1 }, (_, index) => ({
          ...item,
          externalId: `00000000-0000-0000-0000-${String((pageNumber - 1) * 20 + index + 1).padStart(12, '0')}`,
          business: `Empresa ${index + 1}`,
        }));
      return route.fulfill({
        json: {
          page: pageNumber,
          pageSize: 20,
          total: empty ? 0 : options.paginated ? 21 : 2,
          asOf: createdAt,
          items: empty ? [] : items,
        },
      });
    }
    if (url.pathname === `/leads/${id}`) {
      if (options.detailFails) return route.fulfill({ status: 404, json: {} });
      return route.fulfill({
        json: { externalId: id, createdAt, updatedAt: null, snapshot, contentUnavailable: false },
      });
    }
    return route.abort();
  });
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill('root');
  await page.getByLabel('Senha', { exact: true }).fill('test-password');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page).toHaveURL(/\/admin$/);
  return reads;
}
for (const mobile of [false, true]) {
  test(`leads ${mobile ? 'mobile' : 'desktop'} consulta ficha e filtros com contatos antigos e WhatsApp`, async ({
    page,
  }, testInfo) => {
    await page.setViewportSize({ width: mobile ? 390 : 1440, height: 1000 });
    const reads = await setup(page);
    await expect(
      page.getByRole('heading', { name: 'Ideias que chegaram à Olivaw.' }),
    ).toBeVisible();
    await page.getByRole('link', { name: 'Ver leads' }).click();
    await expect(page.getByRole('heading', { level: 1, name: 'Leads' })).toBeVisible();
    await expect(
      page
        .getByRole('navigation', { name: 'Navegação da operação' })
        .getByRole('link', { name: 'Leads', exact: true }),
    ).toHaveAttribute('aria-current', 'page');
    await expect(page.getByText('contato@example.test', { exact: true })).toBeVisible();
    await page.getByRole('link', { name: /Pastelaria Aurora/ }).click();
    const detail = page.getByRole('complementary', { name: 'Detalhes do lead' });
    await expect(detail.getByRole('heading', { name: 'Pastelaria Aurora' })).toBeVisible();
    await expect(detail.getByText('Quero organizar pedidos e entregas.')).toBeVisible();
    await expect(detail.getByRole('link', { name: /WhatsApp/ })).toHaveAttribute(
      'href',
      'https://wa.me/5511987654321',
    );
    await expect(detail.getByText('Categorias', { exact: true })).toBeVisible();
    await expect(detail.getByText('Detalhes do produto', { exact: true })).toBeHidden();
    await page.screenshot({
      path: testInfo.outputPath(`leads-${mobile ? 'mobile' : 'desktop'}.png`),
      fullPage: true,
    });
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(
      mobile ? 390 : 1440,
    );
    await page.getByRole('link', { name: 'Voltar à lista' }).click();
    await page.getByLabel('Buscar contato ou negócio').fill('semresultado');
    await page.getByLabel('Origem', { exact: true }).selectOption('studio');
    await page.getByLabel('Recebido de').fill('2026-10-01');
    await page.getByLabel('Até', { exact: true }).fill('2026-10-21');
    await page.getByRole('button', { name: 'Filtrar', exact: true }).click();
    await expect(page.getByRole('heading', { name: 'Nenhum lead neste recorte.' })).toBeVisible();
    expect(reads.at(-1)?.searchParams.get('entry')).toBe('studio');
    expect(reads.at(-1)?.searchParams.get('from')).toBeTruthy();
    await page.getByRole('button', { name: 'Limpar', exact: true }).click();
    await expect(page.getByRole('link', { name: /Pastelaria Aurora/ })).toBeVisible();
  });
}
test('falha de lista tem nova tentativa e paginação é enviada à API', async ({ page }) => {
  const reads = await setup(page, { failFirst: true, paginated: true });
  await page.getByRole('link', { name: 'Ver leads' }).click();
  await expect(page.getByRole('status')).toContainText('Não foi possível carregar');
  await page.getByRole('button', { name: 'Tentar novamente' }).click();
  await expect(page.getByText('Página 1 de 2', { exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Próxima página' }).click();
  await expect(page.getByText('Página 2 de 2', { exact: true })).toBeVisible();
  expect(reads.at(-1)?.searchParams.get('page')).toBe('2');
  await expect(page.getByRole('button', { name: 'Próxima página' })).toBeDisabled();
});
test('lead não encontrado preserva lista e permite voltar', async ({ page }) => {
  await setup(page, { detailFails: true });
  await page.getByRole('link', { name: 'Ver leads' }).click();
  await page.getByRole('link', { name: /Pastelaria Aurora/ }).click();
  await expect(
    page.getByRole('complementary', { name: 'Detalhes do lead' }).getByRole('status'),
  ).toContainText('Não foi possível abrir este lead');
  await expect(page.getByRole('link', { name: /Pastelaria Aurora/ })).toBeVisible();
  await page.getByRole('link', { name: 'Voltar à lista' }).click();
  await expect(page.getByRole('complementary', { name: 'Detalhes do lead' })).toHaveCount(0);
});
