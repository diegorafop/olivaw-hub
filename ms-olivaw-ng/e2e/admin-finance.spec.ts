import { expect, test, type Page } from '@playwright/test';
const client = '11111111-1111-1111-1111-111111111111';
async function setup(page: Page, root = true, fail = false) {
  const reads: URL[] = [];
  await page.route('https://api.olivaw.com.br/**', (route) => {
    const u = new URL(route.request().url()),
      p = u.pathname;
    if (p.endsWith('/login'))
      return route.fulfill({
        json: { accessToken: 'test', expiresAt: new Date(Date.now() + 300000).toISOString() },
      });
    if (p.endsWith('/access'))
      return route.fulfill({
        json: {
          name: 'Operador',
          isRoot: root,
          permissions: ['leads.manage'],
          userProfileId: 'test',
        },
      });
    if (p === '/customers')
      return route.fulfill({
        json: {
          page: 1,
          pageSize: 20,
          total: 1,
          items: [{ externalId: client, business: 'Empresa Aurora' }],
        },
      });
    if (p === '/expenses') {
      reads.push(u);
      if (fail) return route.fulfill({ status: 503, json: { detail: 'Indisponível' } });
      const month = u.searchParams.get('from')?.startsWith('2026-09');
      const hasData = month && !u.searchParams.get('customerExternalId');
      return route.fulfill({
        json: {
          grossBrl: hasData ? 24.7 : 0,
          creditsBrl: 0,
          netBrl: hasData ? 24.7 : 0,
          unallocatedBrl: month ? 0 : 0,
          total: hasData ? 2 : 0,
          items: hasData
            ? [
                {
                  externalId: 'entry-1',
                  description: 'AWS setembro/2026 — EC2 e EBS',
                  kind: 'cost',
                  amountBrl: 18.1,
                  occurredAt: '2026-09-30T00:00:00Z',
                  customerName: 'Compartilhada / sem rateio',
                  source: 'AWS Bills',
                },
              ]
            : [],
          series: Array.from({ length: 30 }, (_, i) => ({
            date: `2026-09-${String(i + 1).padStart(2, '0')}T00:00:00Z`,
            costBrl: hasData && i === 29 ? 24.7 : 0,
            creditsBrl: hasData && i === 29 ? 24.7 : 0,
            netBrl: hasData ? 24.7 : 0,
          })),
          byCategory: hasData
            ? [
                {
                  key: 'infrastructure',
                  label: 'infrastructure',
                  grossBrl: 24.7,
                  creditsBrl: 24.7,
                  netBrl: hasData ? 24.7 : 0,
                  entries: 2,
                },
              ]
            : [],
          byCustomer: hasData
            ? [
                {
                  key: 'shared',
                  label: 'Compartilhada / sem rateio',
                  grossBrl: 24.7,
                  creditsBrl: 24.7,
                  netBrl: hasData ? 24.7 : 0,
                  entries: 2,
                },
              ]
            : [],
        },
      });
    }
    return route.fulfill({ json: { total: 0, recent: 0, active: 0, items: [], byStatus: {} } });
  });
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill('operator');
  await page.getByLabel('Senha', { exact: true }).fill('password');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page).toHaveURL(/admin/);
  await page.goto('/admin/finance?period=month&date=2026-09-30');
  return reads;
}
for (const width of [1440, 390])
  test(`financeiro navega por período, cliente e despesas preservando o recorte ${width}`, async ({
    page,
  }) => {
    await page.setViewportSize({ width, height: 1000 });
    const reads = await setup(page);
    const nav = page.getByRole('navigation', { name: 'Navegação da operação' });
    await expect(nav.getByRole('link', { name: 'Financeiro', exact: true })).toHaveAttribute(
      'aria-current',
      'page',
    );
    await expect(page.getByRole('heading', { name: 'Clareza sobre cada gasto.' })).toBeVisible();
    await expect(page.locator('.metric').nth(0)).toContainText('24,70');
    await expect(page.locator('.metric').nth(1)).toContainText('2');
    await expect(page.locator('.metric.net')).toContainText('24,70');
    await page.screenshot({ path: `test-results/finance-${width}.png`, fullPage: true });
    await page.getByRole('link', { name: 'Semana', exact: true }).click();
    await expect
      .poll(() => reads.at(-1)?.searchParams.get('from'))
      .toBe('2026-09-28T00:00:00.000Z');
    await expect.poll(() => reads.at(-1)?.searchParams.get('to')).toBe('2026-10-05T00:00:00.000Z');
    await page.getByRole('link', { name: 'Ano', exact: true }).click();
    await expect.poll(() => reads.at(-1)?.searchParams.get('groupBy')).toBe('month');
    await page.goBack();
    await expect(page.getByRole('link', { name: 'Semana', exact: true })).toHaveAttribute(
      'aria-current',
      'true',
    );
    await page.getByRole('link', { name: 'Mês', exact: true }).click();
    await page.getByLabel('Cliente', { exact: true }).selectOption('shared');
    await page.getByRole('button', { name: 'Aplicar', exact: true }).click();
    await expect.poll(() => reads.at(-1)?.searchParams.get('unallocatedOnly')).toBe('true');
    await page.getByRole('link', { name: 'Consultar despesas', exact: false }).click();
    await expect(page).toHaveURL(/admin\/expenses/);
    const filters = page.getByRole('form', { name: 'Filtrar despesas' });
    await expect(filters.getByLabel('De', { exact: true })).toHaveValue('2026-09-01');
    await expect(filters.getByLabel('Até', { exact: true })).toHaveValue('2026-09-30');
    await expect(filters.getByLabel('Cliente', { exact: true })).toHaveValue('shared');
    await expect(nav.getByRole('link', { name: 'Financeiro', exact: true })).toHaveAttribute(
      'aria-current',
      'page',
    );
    const crumbs = page.getByRole('navigation', { name: 'Caminho de navegação' });
    await expect(crumbs.locator('[aria-current=page]')).toHaveText('Despesas');
    await crumbs.getByRole('link', { name: 'Financeiro', exact: true }).click();
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(
      true,
    );
    await page.goto('/admin/configuration');
    await expect(
      page
        .getByRole('navigation', { name: 'Cadastros e ferramentas' })
        .getByRole('link', { name: 'Despesas', exact: true }),
    ).toHaveCount(0);
  });
test('financeiro diferencia falha e ausência de lançamentos', async ({ page }) => {
  await setup(page, true, true);
  await expect(page.getByRole('alert')).toContainText('Não foi possível consultar o financeiro');
  await expect(page.locator('.metrics')).toHaveCount(0);
  await expect(page.getByText('Ainda não há gastos neste recorte.')).toHaveCount(0);
});
test('operador sem root não vê nem acessa financeiro', async ({ page }) => {
  await setup(page, false);
  await expect(page).toHaveURL(/acesso-negado/);
  await expect(
    page
      .getByRole('navigation', { name: 'Navegação da operação' })
      .getByRole('link', { name: 'Financeiro', exact: true }),
  ).toHaveCount(0);
});
