import { expect, test, type Page } from '@playwright/test';
const reportId = 'a0000000-0000-0000-0000-000000000001';
async function setup(page: Page, root = true, failure = false, mixed = false) {
  const rawData = JSON.stringify({
    ai: {
      status: 'completed',
      summary: 'Resumo das evidências observadas.',
      recommendations: ['Revisar permissões.'],
    },
    result: {
      findings: [
        {
          ruleId: 'docker.socket',
          title: 'Socket Docker acessível',
          severity: 'high',
          evidence: 'Proxy',
          recommendation: 'Revisar acesso mínimo.',
        },
        ...(mixed
          ? [
              {
                ruleId: 'host.firewall',
                title: 'Revisar firewall',
                severity: 'medium',
                evidence: 'INPUT ACCEPT',
                recommendation: 'Conferir regras.',
              },
              {
                ruleId: 'host.permissions',
                title: 'Permissões adicionais',
                severity: 'low',
                evidence: 'Metadados',
                recommendation: 'Revisar permissões.',
              },
            ]
          : []),
      ],
      coverage: { docker: 'collected' },
      limitations: ['Amostra limitada.'],
    },
  });
  let approvals = JSON.parse(rawData).result.findings.map((_finding: unknown, index: number) => ({
    externalId: '',
    findingIndex: index,
    status: 'review',
    message: '',
    supported: index === 0,
    planTitle:
      index === 0
        ? 'Restringir permissões e validar a infraestrutura.'
        : 'Procedimento seguro indisponível; revisão manual.',
  }));
  const report = {
    externalId: reportId,
    runId: reportId,
    agentId: 'baley',
    hostId: 'Servidor principal',
    completedAt: '2026-10-25T11:00:00Z',
    createdAt: '2026-10-25T11:01:00Z',
    executionStatus: 'completed',
    rawData,
  };
  await page.route('https://api.olivaw.com.br/**', async (route) => {
    const path = new URL(route.request().url()).pathname;
    if (path.endsWith('/login'))
      return route.fulfill({
        json: { accessToken: 'test', expiresAt: new Date(Date.now() + 300000).toISOString() },
      });
    if (path.endsWith('/access'))
      return route.fulfill({
        json: {
          name: 'Operador',
          isRoot: root,
          permissions: ['leads.manage'],
          userProfileId: 'test',
        },
      });
    if (path === '/security-remediations' && route.request().method() === 'GET')
      return route.fulfill({ json: { items: approvals } });
    if (path === '/security-remediations' && route.request().method() === 'POST') {
      const index = route.request().postDataJSON().findingIndex;
      approvals = approvals.map(
        (item: { findingIndex: number; status: string; supported: boolean }) =>
          item.findingIndex === index
            ? { ...item, status: item.supported ? 'authorized' : 'intervention' }
            : item,
      );
      return route.fulfill({ json: { externalId: reportId, status: approvals[index].status } });
    }
    if (path === '/security-remediations/batch') {
      approvals = approvals.map((item: { status: string; supported: boolean }) =>
        item.status === 'review'
          ? { ...item, status: item.supported ? 'authorized' : 'intervention' }
          : item,
      );
      return route.fulfill({
        json: {
          authorizedCount: 1,
          interventionCount: approvals.length - 1,
          alreadyHandledCount: 0,
        },
      });
    }
    if (path === '/security-reports')
      return route.fulfill(
        failure ? { status: 503, json: {} } : { json: { items: [report], nextBeforeId: null } },
      );
    if (path === '/security-reports/' + reportId) return route.fulfill({ json: { item: report } });
    if (path.endsWith('/summary'))
      return route.fulfill({ json: { total: 0, recent: 0, active: 0, byStatus: {} } });
    return route.fulfill({ json: { items: [], total: 0 } });
  });
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill('operator');
  await page.getByLabel('Senha', { exact: true }).fill('test-password');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
}
test('root navega por Agents, relatórios e detalhes com breadcrumb', async ({ page }) => {
  await setup(page);
  await page.getByRole('link', { name: 'Agents', exact: true }).click();
  await page.getByRole('link', { name: /SEGURANÇA Baley/ }).click();
  await expect(page.getByText('Danger', { exact: true })).toBeVisible();
  await page.getByRole('link', { name: /Servidor principal/ }).click();
  await expect(page.getByRole('heading', { name: 'Detalhes da auditoria' })).toBeVisible();
  await expect(page.getByText('Socket Docker acessível', { exact: true })).toBeVisible();
  await expect(page.getByText('Resumo das evidências observadas.', { exact: true })).toBeVisible();
  await page
    .getByRole('navigation', { name: /breadcrumb|caminho/i })
    .getByRole('link', { name: 'Baley', exact: true })
    .click();
  await expect(page.getByRole('heading', { name: 'Baley', exact: true })).toBeVisible();
});
test('operador sem root não vê nem acessa Agents', async ({ page }) => {
  await setup(page, false);
  await expect(page.getByRole('link', { name: 'Agents', exact: true })).toHaveCount(0);
  await page.goto('/admin/agents/baley');
  await expect(page).toHaveURL(/acesso-negado/);
});
test('falha de consulta não aparece como servidor seguro e permite tentar novamente', async ({
  page,
}) => {
  await setup(page, true, true);
  await page.goto('/admin/agents/baley');
  await expect(page.getByRole('alert')).toContainText('Não foi possível consultar');
  await expect(page.getByRole('button', { name: 'Atualizar', exact: true })).toBeEnabled();
  await expect(page.getByText('Sem achados nos controles avaliados')).toHaveCount(0);
});

test('lista de relatórios fica compacta e sem overflow no celular', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await setup(page);
  await expect(page).toHaveURL(/\/admin$/);
  await page.goto('/admin/agents/baley');
  const card = page.getByRole('link', { name: /Servidor principal/ });
  await expect(card).toBeVisible();
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(
    true,
  );
  expect(await card.evaluate((element) => getComputedStyle(element).backgroundColor)).toBe(
    'rgb(255, 255, 255)',
  );
  await page.screenshot({ path: test.info().outputPath('baley-mobile.png'), fullPage: true });
});

test('filtros por gravidade preservam contagens e recolhem textos complementares', async ({
  page,
}) => {
  await setup(page, true, false, true);
  await expect(page).toHaveURL(/\/admin$/);
  await page.goto('/admin/agents/baley');
  await expect(page.getByText('1 warning', { exact: true })).toBeVisible();
  await page.getByRole('link', { name: /Servidor principal/ }).click();
  await expect(page.locator('article.finding')).toHaveCount(3);
  await expect(page.getByText('Resumo das evidências observadas.', { exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Warning 1', exact: true }).click();
  await expect(page.locator('article.finding')).toHaveCount(1);
  await expect(page.getByRole('heading', { name: 'Revisar firewall', exact: true })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Warning 1', exact: true })).toHaveAttribute(
    'aria-pressed',
    'true',
  );
  await page.getByRole('button', { name: 'Danger 1', exact: true }).click();
  await expect(
    page.getByRole('heading', { name: 'Socket Docker acessível', exact: true }),
  ).toBeVisible();
  await page.getByRole('button', { name: 'Todos 3', exact: true }).click();
  await expect(page.locator('article.finding')).toHaveCount(3);
  await page.screenshot({ path: test.info().outputPath('baley-details.png'), fullPage: true });
});

test('aprovação em lote abrange todo o relatório e evidência fica visível sem clique', async ({
  page,
}) => {
  await setup(page, true, false, true);
  await expect(page).toHaveURL(/\/admin$/);
  await page.goto('/admin/agents/baley/' + reportId);
  await expect(page.getByText('Proxy', { exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Danger 1', exact: true }).click();
  await page.getByRole('button', { name: 'Autorizar todas', exact: true }).click();
  await expect(
    page.getByText('Autorizar as 3 verificações pendentes deste relatório?'),
  ).toBeVisible();
  await page.getByRole('button', { name: 'Confirmar autorizações', exact: true }).click();
  await expect(page.getByRole('status')).toContainText('1 autorizadas');
  await expect(page.getByText('Autorizado', { exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Todos 3', exact: true }).click();
  await expect(page.getByText('Requer intervenção', { exact: true })).toHaveCount(2);
});
test('autorização individual exige confirmação do procedimento', async ({ page }) => {
  await setup(page);
  await expect(page).toHaveURL(/\/admin$/);
  await page.goto('/admin/agents/baley/' + reportId);
  await page.getByRole('button', { name: 'Autorizar correção', exact: true }).click();
  await expect(
    page.getByText('Restringir permissões e validar a infraestrutura.', { exact: true }),
  ).toBeVisible();
  await page.getByRole('button', { name: 'Confirmar autorização', exact: true }).click();
  await expect(page.getByText('Autorizado', { exact: true })).toBeVisible();
});
