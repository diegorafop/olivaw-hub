import { expect, test, type Page } from '@playwright/test';
const id = '40000000-0000-0000-0000-000000000020';
const costId = '60000000-0000-0000-0000-000000000020';
async function setup(page: Page, root = true) {
  const writes: { path: string; body: Record<string, unknown> }[] = [];
  const costs: Record<string, unknown>[] = [];
  const studies: Record<string, unknown>[] = [];
  const incidents: Record<string, unknown>[] = [];
  const history: Record<string, unknown>[] = [];
  await page.route('https://api.olivaw.com.br/**', async (route) => {
    const request = route.request(),
      path = new URL(request.url()).pathname;
    if (path.endsWith('/login'))
      return route.fulfill({
        json: { accessToken: 'test', expiresAt: new Date(Date.now() + 300000).toISOString() },
      });
    if (path.endsWith('/access'))
      return route.fulfill({
        json: { name: 'Operador', isRoot: root, permissions: ['leads.manage'], userProfileId: id },
      });
    if (path.endsWith('/summary'))
      return route.fulfill({
        json: { total: 0, recent: 0, active: 0, onboarding: 0, paused: 0, closed: 0 },
      });
    if (path === '/resources' || path === '/customers')
      return route.fulfill({
        json: {
          page: 1,
          pageSize: 20,
          total: 1,
          items: [{ externalId: id, name: 'VM de teste', business: 'Empresa de teste' }],
        },
      });
    if (request.method() === 'GET') {
      if (path === '/plans/costs')
        return route.fulfill({ json: { items: costs, nextBeforeId: null } });
      if (path === '/plans/studies')
        return route.fulfill({ json: { items: studies, nextBeforeId: null } });
      if (path === '/incidents')
        return route.fulfill({ json: { items: incidents, nextBeforeId: null } });
      if (path.endsWith('/history'))
        return route.fulfill({ json: { items: history, nextBeforeId: null } });
      return route.fulfill({
        json: { items: [], nextBeforeId: null, total: 0, state: 'unmonitored' },
      });
    }
    const body = request.postDataJSON() as Record<string, unknown>;
    writes.push({ path, body });
    if (path === '/plans/costs') costs.unshift({ ...body, externalId: costId });
    if (path === '/plans/estimate')
      return route.fulfill({
        json: {
          knownMonthlyCost: 20,
          unallocatedMonthlyCost: 80,
          suggestedMonthlyPrice: 68.75,
          missingCosts: 0,
        },
      });
    if (path === '/plans/studies')
      studies.unshift({
        ...body,
        externalId: id,
        createdAt: new Date().toISOString(),
        knownMonthlyCost: 20,
        unallocatedMonthlyCost: 80,
        suggestedMonthlyPrice: 68.75,
        missingCosts: 0,
        costs: [{ ...costs[0], costReferenceExternalId: costId, allocationPercent: 20 }],
      });
    if (path === '/incidents' && request.method() === 'POST') {
      incidents.push({ ...body, externalId: id, revision: 1, createdAt: new Date().toISOString() });
      history.unshift({
        ...body,
        revision: 1,
        actorName: 'Operador',
        createdAt: new Date().toISOString(),
      });
    }
    if (path === `/incidents/${id}`) {
      if (body['status'] === 'resolved' && incidents[0]['status'] !== 'observation')
        return route.fulfill({
          status: 409,
          json: { detail: 'Confirme a recuperação em observação antes de resolver.' },
        });
      incidents[0] = { ...incidents[0], ...body, revision: Number(body['revision']) + 1 };
      history.unshift({
        ...body,
        revision: Number(body['revision']) + 1,
        actorName: 'Operador',
        createdAt: new Date().toISOString(),
      });
    }
    return route.fulfill({
      status: 201,
      json: { externalId: id, revision: incidents[0]?.['revision'] ?? 1 },
    });
  });
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill('root');
  await page.getByLabel('Senha', { exact: true }).fill('test-password');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page).toHaveURL(root ? /\/admin$/ : /\/admin\/leads$/);
  return { writes };
}
for (const width of [1440, 390]) {
  test(`configuração reúne os cadastros em cards com navegação compacta ${width}`, async ({
    page,
  }) => {
    await page.setViewportSize({ width, height: 1000 });
    await setup(page);
    const nav = page.getByRole('navigation', { name: 'Navegação da operação' });
    await expect(nav.getByRole('link')).toHaveCount(5);
    await nav.getByRole('link', { name: 'Configuração', exact: true }).click();
    const cards = page.getByRole('navigation', { name: 'Cadastros e ferramentas' });
    await expect(cards.getByRole('link')).toHaveCount(8);
    for (const name of ['Saúde', 'Incidentes', 'Consumo'])
      await expect(cards.getByRole('link', { name, exact: true })).toHaveCount(0);
    await cards.getByRole('link', { name: 'Planos', exact: true }).click();
    await expect(
      page.getByRole('heading', { name: 'Um plano para cada momento', exact: true }),
    ).toBeVisible();
    await expect(nav.getByRole('link', { name: 'Configuração', exact: true })).toHaveAttribute(
      'aria-current',
      'page',
    );
    const breadcrumb = page.getByRole('navigation', { name: 'Caminho de navegação' });
    await expect(breadcrumb.locator('[aria-current="page"]')).toHaveText('Planos');
    await expect(
      breadcrumb.getByRole('link', { name: 'Visão geral', exact: true }),
    ).toHaveAttribute('href', '/admin');
    await breadcrumb.getByRole('link', { name: 'Configuração', exact: true }).click();
    await expect(breadcrumb.locator('[aria-current="page"]')).toHaveText('Configuração');
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(
      width,
    );
  });
}
test('estudo preserva funcionalidades e 2 GB como hipótese, sem alterar contrato', async ({
  page,
}) => {
  const { writes } = await setup(page);
  await page.goto('/admin/plans/simulator');
  await page.getByLabel('Nome da origem').fill('VM de teste');
  await page.getByLabel('Custo mensal (R$)').fill('100');
  await page.getByLabel('Fonte / referência da versão').fill('Valor fictício para teste');
  await page.getByRole('button', { name: 'Registrar versão do custo' }).click();
  await page.getByRole('checkbox', { name: /VM de teste/ }).check();
  await page.getByLabel('Parcela desta origem no plano (%)').fill('20');
  await page.getByLabel('Nome do cenário').fill('Bronze — hipótese');
  await page.getByLabel('Armazenamento incluído (GB)').fill('2');
  await page
    .getByLabel('Funcionalidades e cuidados incluídos')
    .fill('Site simples\nManutenção combinada');
  await page.getByLabel('Horas mensais de suporte').fill('1');
  await page.getByLabel('Custo interno por hora').fill('30');
  await page.getByLabel('Reserva operacional (%)').fill('10');
  await page.getByLabel('Margem sobre o preço (%)').fill('20');
  await page.getByRole('button', { name: 'Calcular estudo', exact: true }).click();
  await expect(page.getByRole('region', { name: 'Resultado do estudo' })).toContainText('68,75');
  await page.getByRole('button', { name: 'Salvar cenário', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Bronze — hipótese' })).toBeVisible();
  const study = writes.find((item) => item.path === '/plans/studies')!.body;
  expect(study['storageLimitGB']).toBe(2);
  expect(study['includedFeatures']).toBe('Site simples\nManutenção combinada');
  expect(study['memoryLimitMiB']).toBeNull();
  expect(study['customerExternalId']).toBeUndefined();
  expect(writes.some((item) => item.path === '/plans/conditions')).toBe(false);
  await page.getByRole('button', { name: 'Usar como base', exact: true }).click();
  await expect(page.getByLabel('Funcionalidades e cuidados incluídos')).toHaveValue(
    'Site simples\nManutenção combinada',
  );
  await page.getByLabel('Nome do cenário').fill('Prata — hipótese');
  await page.getByLabel('Armazenamento incluído (GB)').fill('5');
  await page.getByRole('button', { name: 'Salvar cenário', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Prata — hipótese' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Bronze — hipótese' })).toBeVisible();
});
test('incidente exige confirmação e preserva edição quando há conflito', async ({ page }) => {
  await setup(page);
  await page.goto('/admin/incidents');
  await page.getByLabel('Cadastro vinculado').selectOption(id);
  await page.getByLabel('Título', { exact: false }).fill('Falha compartilhada');
  await page.getByLabel('Impacto', { exact: false }).fill('Sites afetados');
  await page.getByLabel('Responsável', { exact: false }).fill('Operador');
  await page.getByLabel('Próxima ação', { exact: false }).fill('Investigar origem');
  await page.getByRole('button', { name: 'Salvar registro', exact: true }).click();
  await page.getByRole('button', { name: 'Abrir registro', exact: true }).click();
  await page.getByLabel('Estado', { exact: false }).selectOption('resolved');
  await page.getByLabel('Nota interna / justificativa').fill('Texto preservado');
  await page.getByRole('button', { name: 'Salvar registro', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('observação');
  await expect(page.getByLabel('Nota interna / justificativa')).toHaveValue('Texto preservado');
  await page.getByLabel('Estado', { exact: false }).selectOption('observation');
  await page.getByRole('button', { name: 'Salvar registro', exact: true }).click();
  await expect(page.getByRole('status').filter({ hasText: 'Registro salvo' })).toBeVisible();
});
test('configuração permanece protegida para operador sem root', async ({ page }) => {
  await setup(page, false);
  await expect(
    page
      .getByRole('navigation', { name: 'Navegação da operação' })
      .getByRole('link', { name: 'Configuração' }),
  ).toHaveCount(0);
  await page.goto('/admin/configuration');
  await expect(page).toHaveURL(/acesso-negado/);
});

for (const scenario of [
  { path: 'health', endpoint: '/health-readings', observation: true },
  { path: 'usage', endpoint: '/usage-readings', observation: true },
  { path: 'care', endpoint: '/care-evidence', observation: true },
  { path: 'support', endpoint: '/support-requests', observation: false },
]) {
  test(`configuração registra ${scenario.path} com fonte ou responsável explícito`, async ({
    page,
  }) => {
    const { writes } = await setup(page);
    await page.goto(`/admin/${scenario.path}`);
    await page.getByLabel('Cadastro vinculado').selectOption(id);
    if (scenario.observation) {
      await page.getByLabel('Fonte da informação').fill('Conferência manual de teste');
      await page.getByLabel('Instante da observação').fill('2026-10-22T12:00');
      if (scenario.path === 'usage') await page.getByLabel('Valor (GiB)').fill('2');
    } else {
      await page.getByLabel('Título', { exact: false }).fill('Pedido de melhoria');
      await page.getByLabel('Responsável', { exact: false }).fill('Operador');
      await page.getByLabel('Próxima ação', { exact: false }).fill('Avaliar escopo');
      await page.getByLabel('Solicitante', { exact: false }).fill('Cliente');
    }
    await page.getByRole('button', { name: 'Salvar registro', exact: true }).click();
    await expect(page.getByText('Registro salvo.', { exact: true })).toBeVisible();
    const body = writes.find((item) => item.path === scenario.endpoint)!.body;
    if (scenario.observation) {
      expect(body['source']).toBe('Conferência manual de teste');
      expect(body['sampleKey']).toMatch(/^[0-9a-f-]{36}$/);
      expect(body['observedAt']).toMatch(/Z$/);
    } else {
      expect(body['customerExternalId']).toBe(id);
      expect(body['owner']).toBe('Operador');
      expect(body['status']).toBe('received');
    }
  });
}

test('breadcrumb acompanha formulário de cadastro e volta pela hierarquia', async ({ page }) => {
  await setup(page);
  await page.goto('/admin/resources/new?customer=' + id);
  const breadcrumb = page.getByRole('navigation', { name: 'Caminho de navegação' });
  await expect(breadcrumb.getByRole('listitem')).toHaveCount(4);
  await expect(breadcrumb.locator('[aria-current="page"]')).toHaveText('Novo recurso');
  await expect(breadcrumb.getByRole('link', { name: 'Recursos', exact: true })).toHaveAttribute(
    'href',
    '/admin/resources',
  );
  await breadcrumb.getByRole('link', { name: 'Configuração', exact: true }).click();
  await expect(page).toHaveURL(/\/admin\/configuration$/);
  await breadcrumb.getByRole('link', { name: 'Visão geral', exact: true }).click();
  await expect(page).toHaveURL(/\/admin$/);
  await expect(breadcrumb.locator('[aria-current="page"]')).toHaveText('Visão geral');
});
