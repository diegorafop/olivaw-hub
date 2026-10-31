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

test('catálogo separa propostas e navega para detalhes com composição de custo', async ({
  page,
}) => {
  await setup(page);
  await page.goto('/admin/plans/catalog');
  await expect(page).toHaveURL(/admin\/plans$/);
  await expect(page.getByRole('heading', { name: 'Um plano para cada momento' })).toBeVisible();
  const cards = page.locator('.catalog article');
  await expect(cards).toHaveCount(4);
  await expect(cards.first()).toContainText('49');
  await expect(cards.first()).toContainText('Não cadastrado');
  await expect(cards.nth(1)).toContainText('Domínio próprio permitido');
  await page.getByRole('link', { name: 'Conhecer Bronze' }).click();
  await expect(page).toHaveURL(/admin\/plans\/bronze$/);
  await expect(page.getByRole('heading', { name: 'Composição do custo mensal' })).toBeVisible();
  const crumb = page.getByRole('navigation', { name: 'Caminho de navegação' });
  await expect(crumb.locator('[aria-current=page]')).toHaveText('Bronze');
  await page.setViewportSize({ width: 390, height: 844 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
  await crumb.getByRole('link', { name: 'Planos', exact: true }).click();
  await expect(cards).toHaveCount(4);
});

test('fatura preserva JSON desconhecido e navega ao detalhe sem criar despesas', async ({
  page,
}) => {
  const expenses = await setup(page);
  const id = '20000000-0000-0000-0000-000000000021';
  let document: Record<string, unknown> | null = null;
  await page.route('https://api.olivaw.com.br/aws-invoices**', (route) => {
    if (route.request().method() === 'POST') {
      document = route.request().postDataJSON();
      return route.fulfill({ status: 201, json: { externalId: id } });
    }
    if (new URL(route.request().url()).pathname.endsWith(id))
      return route.fulfill({
        json: {
          invoice: { ...document, externalId: id },
          rawData: document?.['rawData'],
          expenses: [],
        },
      });
    return route.fulfill({ json: { items: [], nextBeforeId: null } });
  });
  await page.goto('/admin/invoices');
  await expect(page.getByText('Nenhum documento registrado.')).toBeVisible();
  await page.getByRole('link', { name: 'Registrar documento AWS' }).click();
  await page.getByLabel('Emissor *', { exact: true }).fill('AWS Brasil');
  await page.getByLabel('Referência da conta').fill('Conta de teste');
  await page.getByLabel('Fonte', { exact: false }).fill('CSV recebido — provisório');
  await page
    .getByLabel('Dados originais em JSON')
    .fill('{"schemaVersion":1,"campoFuturo":{"itens":[1,2]}}');
  await page.getByRole('button', { name: 'Registrar documento', exact: true }).click();
  await expect(page).toHaveURL(new RegExp('/admin/invoices/' + id + '$'));
  await expect(page.getByRole('heading', { name: 'AWS Brasil' })).toBeVisible();
  await expect(page.locator('pre')).toContainText('campoFuturo');
  expect(expenses).toHaveLength(0);
  const crumb = page.getByRole('navigation', { name: 'Caminho de navegação' });
  await expect(crumb.locator('[aria-current=page]')).toHaveText('Detalhes da fatura');
  await crumb.getByRole('link', { name: 'Faturas AWS', exact: true }).click();
  await expect(page).toHaveURL(/admin\/invoices$/);
});

test('plano publica versão e vincula competência sem criar despesa ou deploy', async ({ page }) => {
  const expenses = await setup(page);
  const versionId = '20000000-0000-0000-0000-000000000022';
  let version: Record<string, unknown> | null = null;
  const assignments: Record<string, unknown>[] = [];
  await page.route('https://api.olivaw.com.br/plans/versions**', (route) => {
    if (route.request().method() === 'POST') {
      version = {
        ...route.request().postDataJSON(),
        externalId: versionId,
        version: 1,
        estimatedMonthlyCost: null,
      };
      return route.fulfill({ status: 201, json: { externalId: versionId, version: 1 } });
    }
    const own = new URL(route.request().url()).searchParams.get('planKey') === 'bronze';
    return route.fulfill({
      json: {
        items: own && version ? [version] : [],
        current: own ? version : null,
        nextBeforeId: null,
      },
    });
  });
  await page.route('https://api.olivaw.com.br/plans/assignments', (route) => {
    assignments.push(route.request().postDataJSON());
    return route.fulfill({
      status: 201,
      json: { externalId: '20000000-0000-0000-0000-000000000023' },
    });
  });
  await page.route('https://api.olivaw.com.br/plans/conditions**', (route) => {
    const condition = assignments.length
      ? {
          externalId: 'condition-test',
          name: 'Bronze',
          monthlyPrice: 49,
          version: 1,
          startsAt: new Date().toISOString().slice(0, 7) + '-01T00:00:00Z',
          approvalReference: 'Aceite de teste',
        }
      : null;
    return route.fulfill({
      json: { items: condition ? [condition] : [], current: condition, nextBeforeId: null },
    });
  });
  await page.goto('/admin/plans/bronze');
  await page.getByLabel('Mensalidade aprovada').fill('49');
  await page.getByLabel('Referência da aprovação').fill('Aprovação de teste');
  await page.getByRole('button', { name: 'Publicar versão aprovada' }).click();
  await expect(page.getByRole('status').filter({ hasText: 'Nova versão publicada' })).toBeVisible();
  await page.getByLabel('Cliente do plano', { exact: true }).selectOption('client-1');
  await page.getByLabel('Versão', { exact: false }).selectOption(versionId);
  await page.getByLabel('Aprovação do cliente').fill('Aceite de teste');
  await page.getByRole('button', { name: 'Registrar condição aprovada' }).click();
  await expect(page.getByRole('status')).toContainText('Condição do cliente registrada');
  await expect(
    page.getByRole('heading', { name: 'Histórico do cliente', exact: true }),
  ).toBeVisible();
  await expect(page.getByText('Aprovação: Aceite de teste', { exact: true })).toBeVisible();
  expect(assignments).toHaveLength(1);
  expect(assignments[0]['planVersionExternalId']).toBe(versionId);
  expect(expenses).toHaveLength(0);
});

test('controle financeiro registra baixa parcial sem duplicar despesas', async ({ page }) => {
  const expenses = await setup(page);
  const accountId = '20000000-0000-0000-0000-000000000031';
  const entryId = '20000000-0000-0000-0000-000000000032';
  const accounts: Record<string, unknown>[] = [];
  let entry: Record<string, unknown> | null = null;
  const settlements: Record<string, unknown>[] = [];
  await page.route('https://api.olivaw.com.br/finance/**', (route) => {
    const path = new URL(route.request().url()).pathname;
    if (path === '/finance/accounts') {
      if (route.request().method() === 'POST') {
        accounts.push({ ...route.request().postDataJSON(), externalId: accountId });
        return route.fulfill({ status: 201, json: { externalId: accountId } });
      }
      return route.fulfill({ json: { items: accounts, nextBeforeId: null } });
    }
    if (path === '/finance/entries' && route.request().method() === 'POST') {
      entry = {
        ...route.request().postDataJSON(),
        externalId: entryId,
        settledAmount: 0,
        status: 'open',
        revision: 1,
      };
      return route.fulfill({ status: 201, json: { externalId: entryId } });
    }
    if (path === '/finance/settlements') {
      const body = route.request().postDataJSON();
      settlements.push({
        ...body,
        externalId: 'settlement-test',
        accountName: 'Conta operacional',
      });
      entry = { ...entry, settledAmount: 20, status: 'partial', revision: 2 };
      return route.fulfill({ status: 201, json: { externalId: 'settlement-test', revision: 2 } });
    }
    if (path.endsWith(entryId)) return route.fulfill({ json: { entry, settlements } });
    return route.fulfill({ json: { items: [], nextBeforeId: null } });
  });
  await page.goto('/admin/finance/accounts');
  await page.getByLabel('Nome da conta').fill('Conta operacional');
  await page.getByLabel('Saldo inicial (R$)').fill('100');
  await page.getByRole('button', { name: 'Registrar conta', exact: true }).click();
  await expect(page.getByText('Conta registrada, sem movimentar dinheiro.')).toBeVisible();
  await page.getByRole('link', { name: 'Nova obrigação', exact: true }).click();
  await page.getByLabel('Descrição', { exact: false }).fill('Mensalidade de teste');
  await page.getByLabel('Direção').selectOption('receivable');
  await page.getByLabel('Valor da obrigação').fill('49');
  await page.getByLabel('Fonte da obrigação').fill('Contrato de teste');
  await page.getByRole('button', { name: 'Registrar obrigação' }).click();
  await expect(page).toHaveURL(new RegExp('/finance/entries/' + entryId + '$'));
  await page.getByLabel('Conta da baixa').selectOption(accountId);
  await page.getByLabel('Valor da baixa').fill('20');
  await page.getByLabel('Comprovante da baixa').fill('Comprovante de teste');
  await page.getByRole('button', { name: 'Registrar baixa / estorno' }).click();
  await expect(page.getByText('Baixa registrada no histórico;', { exact: false })).toBeVisible();
  await expect(
    page.getByRole('button', { name: 'Cancelar obrigação', exact: true }),
  ).toBeDisabled();
  expect(settlements).toHaveLength(1);
  expect(expenses).toHaveLength(0);
});

test('composição do plano inclui domínio anual, ferramentas e trabalho antes de publicar', async ({
  page,
}) => {
  await setup(page);
  const id = '20000000-0000-0000-0000-000000000040';
  let sheet: Record<string, unknown> | null = null;
  await page.route('https://api.olivaw.com.br/plans/cost-sheets', (route) => {
    sheet = route.request().postDataJSON();
    return route.fulfill({
      status: 201,
      json: {
        externalId: id,
        monthlyCostWithReserve: 32.58,
        suggestedMonthlyPrice: 44.03,
        unallocatedMonthlyCost: 183,
      },
    });
  });
  await page.goto('/admin/plans/bronze');
  const labels = [
    'VM / processamento',
    'Armazenamento',
    'Rede e IP público',
    'Backups',
    'Domínio',
    'IA e ferramentas',
    'Licenças e serviços',
    'Outros custos',
  ];
  for (const label of labels) {
    await page
      .getByLabel(label + ' — valor da base', { exact: true })
      .fill(
        label === 'Domínio'
          ? '40'
          : label === 'VM / processamento' || label === 'IA e ferramentas'
            ? '100'
            : '0',
      );
    await page.getByLabel(label + ' — clientes que dividem', { exact: true }).fill('10');
  }
  await page.getByLabel('Operação e suporte (horas/mês)').fill('0.10');
  await page.getByLabel('Custo da hora de trabalho').fill('80');
  await page
    .getByLabel('Fonte e hipóteses do estudo')
    .fill('Bases hipotéticas de teste — dez clientes projetados');
  await expect(page.getByLabel('Prévia local da conta')).toContainText('32,58');
  await page.getByRole('button', { name: 'Salvar estudo e vincular à próxima versão' }).click();
  await expect(page.getByText('Estudo salvo. Custo mensal:', { exact: false })).toBeVisible();
  await expect(page.getByLabel('Estudo de custos', { exact: true })).toHaveValue(id);
  expect(sheet?.['planKey']).toBe('bronze');
  expect((sheet?.['lines'] as unknown[]).length).toBe(8);
});
