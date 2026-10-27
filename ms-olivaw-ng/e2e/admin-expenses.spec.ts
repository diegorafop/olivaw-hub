import { expect, test, type Page } from '@playwright/test';
async function setup(page: Page, root = true) {
  const entries: Record<string, unknown>[] = [];
  await page.route('https://api.olivaw.com.br/**', (route) => {
    const req = route.request(),
      u = new URL(req.url()),
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
          items: [{ externalId: 'client-1', business: 'Empresa Aurora' }],
        },
      });
    if (p === '/expenses' && req.method() === 'POST') {
      const body = req.postDataJSON();
      entries.push({
        ...body,
        externalId: 'expense-' + entries.length,
        amountBrl: body.amount * body.exchangeRateToBrl,
        customerName: body.customerExternalId ? 'Empresa Aurora' : 'Compartilhada / sem rateio',
      });
      return route.fulfill({ status: 201, json: { externalId: 'expense-1' } });
    }
    if (p === '/expenses') {
      const filtered = entries.filter(
        (x) =>
          !u.searchParams.get('customerExternalId') ||
          x['customerExternalId'] === u.searchParams.get('customerExternalId'),
      );
      const grossBrl = filtered
        .filter((x) => x['kind'] === 'cost')
        .reduce((t, x) => t + Number(x['amountBrl']), 0);
      return route.fulfill({
        json: {
          grossBrl,
          creditsBrl: 0,
          netBrl: grossBrl,
          unallocatedBrl: entries
            .filter((x) => !x['customerExternalId'])
            .reduce((t, x) => t + Number(x['amountBrl']), 0),
          total: filtered.length,
          items: filtered,
          series: [
            { date: new Date().toISOString(), costBrl: grossBrl, creditsBrl: 0, netBrl: grossBrl },
          ],
        },
      });
    }
    return route.fulfill({ json: { total: 0, recent: 0, active: 0, byStatus: {}, items: [] } });
  });
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill('operator');
  await page.getByLabel('Senha', { exact: true }).fill('password');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page).toHaveURL(/admin/);
  await page.goto('/admin/expenses');
  return entries;
}
test('despesas registra conversão explícita e filtra sem atribuir compartilhadas ao cliente', async ({
  page,
}) => {
  await page.setViewportSize({ width: 1440, height: 1000 });
  const entries = await setup(page);
  await expect(page.getByText('Nenhuma despesa registrada neste recorte.')).toBeVisible();
  await page.getByRole('button', { name: 'Nova despesa' }).click();
  const form = page.getByRole('form', { name: 'Lançar despesa' });
  await form.getByLabel('Descrição', { exact: true }).fill('Infraestrutura AWS');
  await form.getByLabel('Valor original').fill('10');
  await form.getByLabel('Moeda', { exact: true }).selectOption('USD');
  await form.getByLabel('Cotação em reais por US$ 1').fill('5.5');
  await form.getByLabel('Origem / comprovante').fill('AWS Bills · mês corrente');
  await form.getByRole('button', { name: 'Registrar despesa' }).click();
  await expect(page.getByRole('status').filter({ hasText: 'Despesa registrada' })).toBeVisible();
  expect(entries[0]['amountBrl']).toBe(55);
  expect(entries[0]['customerExternalId']).toBeNull();
  await page.screenshot({ path: 'test-results/expenses-desktop.png', fullPage: true });
  await page
    .getByRole('form', { name: 'Filtrar despesas' })
    .getByLabel('Cliente', { exact: true })
    .selectOption('client-1');
  await page.getByRole('button', { name: 'Aplicar' }).click();
  await expect(page.locator('.expense-row')).toHaveCount(0);
  await expect(
    page.getByText('Esse valor não está incluído no total do cliente.', { exact: false }),
  ).toBeVisible();
  await page.setViewportSize({ width: 390, height: 844 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
  await page.screenshot({ path: 'test-results/expenses-mobile.png', fullPage: true });
  const breadcrumb = page.getByRole('navigation', { name: 'Caminho de navegação' });
  await expect(breadcrumb.locator('[aria-current=page]')).toHaveText('Despesas');
  await breadcrumb.getByRole('link', { name: 'Financeiro' }).click();
  await expect(page).toHaveURL(/admin\/finance/);
  await page.goto('/admin/configuration');
  const cards = page.getByRole('navigation', { name: 'Cadastros e ferramentas' });
  for (const name of ['Saúde', 'Incidentes', 'Consumo', 'Despesas'])
    await expect(cards.getByRole('link', { name, exact: true })).toHaveCount(0);
});
test('despesas rejeita navegação de operador sem root', async ({ page }) => {
  await setup(page, false);
  await expect(page).toHaveURL(/acesso-negado/);
});

test('catálogo mostra domínio por plano e recalcula viabilidade sem gravar contratos', async ({
  page,
}) => {
  await setup(page);
  await page.goto('/admin/plans/catalog');
  await expect(page.getByRole('heading', { name: 'Planos que fazem sentido.' })).toBeVisible();
  const cards = page.locator('.catalog article');
  await expect(cards).toHaveCount(3);
  await expect(cards.first()).toContainText('Endereço Olivaw');
  await expect(cards.nth(1)).toContainText('Domínio próprio permitido');
  await expect(page.locator('.negative')).toHaveCount(0);
  await page.getByLabel('Clientes pagantes', { exact: true }).fill('2');
  await expect(page.locator('.negative').first()).toBeVisible();
  await page.screenshot({ path: 'test-results/plan-catalog-desktop.png', fullPage: true });
  await page.setViewportSize({ width: 390, height: 844 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
  const crumb = page.getByRole('navigation', { name: 'Caminho de navegação' });
  await expect(crumb.locator('[aria-current=page]')).toHaveText('Proposta comercial');
  await crumb.getByRole('link', { name: 'Planos', exact: true }).click();
  await expect(page).toHaveURL(/admin\/plans$/);
});
