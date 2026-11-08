import { expect, test, type Page } from '@playwright/test';
import type {
  ResourceDetail,
  DeploymentDetail,
  InventoryLink,
} from '../src/app/features/admin/inventory/inventory-api';
const resourceId = '40000000-0000-0000-0000-000000000020';
const otherId = '40000000-0000-0000-0000-000000000021';
const customerId = '10000000-0000-0000-0000-000000000020';
const serviceId = '30000000-0000-0000-0000-000000000020';
const deploymentId = '50000000-0000-0000-0000-000000000020';
const at = '2026-10-22T12:00:00Z';
function resource(): ResourceDetail {
  return {
    externalId: resourceId,
    technicalId: 'vm-shared',
    name: 'VM compartilhada',
    type: 'compute',
    purpose: '',
    provider: '',
    accountReference: '',
    region: '',
    providerResourceId: '',
    owner: '',
    payer: '',
    operator: '',
    scope: 'platform',
    status: 'reported',
    source: '',
    monitoring: 'none',
    referenceUrl: '',
    cpuCapacity: null,
    memoryCapacityGiB: null,
    storageCapacityGiB: null,
    customerCount: 0,
    originCount: 0,
    origins: [],
    revision: 1,
    providerIdentityLocked: false,
    verifiedAt: null,
    customers: [],
    reservedCpu: null,
    reservedMemoryGiB: null,
    reservedStorageGiB: null,
    incompleteReservations: 0,
    history: [],
  };
}
async function setup(
  page: Page,
  options: { conflict?: boolean; root?: boolean; seed?: boolean } = {},
) {
  const writes: { path: string; body: Record<string, unknown> }[] = [];
  const reads: URL[] = [];
  let current = resource();
  let exists = options.seed ?? false;
  let deployment: DeploymentDetail | null = null;
  let links: (InventoryLink & { kind: string })[] = [];
  await page.route('https://api.olivaw.com.br/**', async (route) => {
    const request = route.request(),
      url = new URL(request.url()),
      path = url.pathname,
      method = request.method();
    if (path.endsWith('/login'))
      return route.fulfill({
        json: { accessToken: 'test', expiresAt: new Date(Date.now() + 300000).toISOString() },
      });
    if (path.endsWith('/access'))
      return route.fulfill({
        json: {
          name: 'Operador',
          isRoot: options.root !== false,
          permissions: ['leads.manage'],
          userProfileId: 'test',
        },
      });
    if (method === 'GET') reads.push(url);
    if (path === '/leads/summary') return route.fulfill({ json: { total: 0, recent: 0 } });
    if (path === '/customers/summary')
      return route.fulfill({ json: { total: 1, onboarding: 0, active: 1, paused: 0, closed: 0 } });
    if (path === '/leads')
      return route.fulfill({ json: { page: 1, pageSize: 20, total: 0, items: [] } });
    if (path === '/customers')
      return route.fulfill({
        json: {
          page: 1,
          pageSize: 20,
          total: 1,
          items: [{ externalId: customerId, business: 'Empresa Aurora', status: 'active' }],
        },
      });
    if (path === `/customers/${customerId}`)
      return route.fulfill({
        json: {
          externalId: customerId,
          business: 'Empresa Aurora',
          legalName: '',
          document: null,
          owner: '',
          notes: '',
          contacts: [],
          status: 'active',
          revision: 1,
          createdAt: at,
          changes: [],
          sourceLeadExternalId: null,
          updatedAt: null,
        },
      });
    if (path === '/services')
      return route.fulfill({
        json: {
          page: 1,
          pageSize: 20,
          total: 1,
          items: [
            {
              externalId: serviceId,
              technicalId: 'ms-example-api',
              name: 'API Aurora',
              type: 'api',
              status: 'active',
              scope: 'customer',
              customerCount: 1,
            },
          ],
        },
      });
    if (path === `/services/${serviceId}`)
      return route.fulfill({
        json: {
          externalId: serviceId,
          technicalId: 'ms-example-api',
          name: 'API Aurora',
          type: 'api',
          status: 'active',
          scope: 'customer',
          description: '',
          owner: '',
          revision: 1,
          customers: [
            {
              customerExternalId: customerId,
              business: 'Empresa Aurora',
              startedAt: at,
              startedByName: 'Operador',
            },
          ],
          previousCustomers: [],
          changes: [],
        },
      });
    if (path === '/resources' && method === 'GET') {
      let items: ResourceDetail[] = exists ? [current] : [];
      if (url.searchParams.get('search') === 'Banco')
        items = [
          {
            ...resource(),
            externalId: otherId,
            technicalId: 'pg-shared',
            name: 'Banco compartilhado',
            type: 'database-instance',
          },
        ];
      if (url.searchParams.get('customerExternalId'))
        items = [
          {
            ...current,
            customerCount: 2,
            originCount: 2,
            origins: [
              {
                kind: 'deployment',
                fromResourceExternalId: null,
                fromResourceName: '',
                deploymentExternalId: deploymentId,
                serviceName: 'API Aurora',
                environment: 'production',
                relationshipKind: '',
                purpose: 'Execução da API',
              },
              {
                kind: 'dependency',
                fromResourceExternalId: otherId,
                fromResourceName: 'Banco compartilhado',
                deploymentExternalId: null,
                serviceName: '',
                environment: '',
                relationshipKind: 'runs-on',
                purpose: 'Banco na VM',
              },
            ],
          },
        ];
      return route.fulfill({
        json: {
          page: Number(url.searchParams.get('page') ?? 1),
          pageSize: 20,
          total: items.length,
          items,
        },
      });
    }
    if (path === '/resources' && method === 'POST') {
      const body = request.postDataJSON();
      writes.push({ path, body });
      current = {
        ...resource(),
        ...body,
        history: [{ ...body, revision: 1, actorName: 'Operador', createdAt: at }],
      };
      exists = true;
      return route.fulfill({ status: 201, json: { externalId: resourceId, revision: 1 } });
    }
    if (path === `/resources/${resourceId}` && method === 'GET')
      return route.fulfill({ json: current });
    if (path === `/resources/${resourceId}` && method === 'PUT') {
      const body = request.postDataJSON();
      writes.push({ path, body });
      if (options.conflict)
        return route.fulfill({
          status: 409,
          json: { detail: 'Este recurso mudou. Reabra a ficha.' },
        });
      current = { ...current, ...body, revision: current.revision + 1 };
      return route.fulfill({ json: { externalId: resourceId, revision: current.revision } });
    }
    if (path === '/deployments' && method === 'GET')
      return route.fulfill({
        json: {
          page: 1,
          pageSize: 20,
          total: deployment ? 1 : 0,
          items: deployment ? [deployment] : [],
        },
      });
    if (path === '/deployments' && method === 'POST') {
      const body = request.postDataJSON();
      writes.push({ path, body });
      deployment = {
        ...body,
        externalId: deploymentId,
        serviceExternalId: serviceId,
        serviceName: 'API Aurora',
        environment: String(body.environment),
        name: String(body.name),
        location: '',
        endpoint: '',
        status: String(body.status),
        endedAt: null,
        revision: 1,
        history: [],
      };
      return route.fulfill({ status: 201, json: { externalId: deploymentId, revision: 1 } });
    }
    if (path === `/deployments/${deploymentId}` && method === 'GET')
      return route.fulfill({ json: deployment });
    if (path === `/deployments/${deploymentId}` && method === 'PUT') {
      const body = request.postDataJSON();
      writes.push({ path, body });
      deployment = {
        ...deployment!,
        ...body,
        revision: deployment!.revision + 1,
        endedAt: body.status === 'closed' ? at : null,
      };
      if (body.status === 'closed')
        links = links.map((link) => ({
          ...link,
          endedAt: at,
          endedByName: 'Operador',
          revision: link.revision + 1,
        }));
      return route.fulfill({ json: { externalId: deploymentId, revision: deployment.revision } });
    }
    if (path === '/inventory-links') {
      const kind = url.searchParams.get('kind');
      const items = links.filter(
        (link) =>
          link.kind === kind && (url.searchParams.get('includeEnded') === 'true' || !link.endedAt),
      );
      return route.fulfill({ json: { page: 1, pageSize: 20, total: items.length, items } });
    }
    const kind = path.startsWith('/resource-customers')
      ? 'customer'
      : path.startsWith('/resource-dependencies')
        ? 'dependency'
        : path.startsWith('/deployment-resources')
          ? 'deployment'
          : '';
    if (kind && method === 'POST') {
      const body = request.postDataJSON();
      writes.push({ path, body });
      links.push({
        externalId: `60000000-0000-0000-0000-${String(links.length + 1).padStart(12, '0')}`,
        resourceExternalId: resourceId,
        resourceName: current.name,
        targetExternalId: String(body.targetExternalId),
        targetName:
          kind === 'customer'
            ? 'Empresa Aurora'
            : kind === 'dependency'
              ? 'Banco compartilhado'
              : deployment!.name,
        purpose: String(body.purpose),
        relationshipKind: String(body.kind ?? ''),
        environment: 'production',
        revision: 1,
        startedAt: at,
        endedAt: null,
        startedByName: 'Operador',
        endedByName: null,
        reservedCpu: body.reservedCpu as number | null,
        reservedMemoryGiB: body.reservedMemoryGiB as number | null,
        reservedStorageGiB: body.reservedStorageGiB as number | null,
        kind,
      });
      if (kind === 'customer') {
        current.customerCount = 1;
        current.customers = [{ externalId: customerId, business: 'Empresa Aurora' }];
      }
      return route.fulfill({
        status: 201,
        json: { externalId: links.at(-1)!.externalId, revision: 1 },
      });
    }
    if (kind && method === 'PUT') {
      const body = request.postDataJSON();
      writes.push({ path, body });
      links = links.map((link) =>
        path.endsWith(link.externalId)
          ? { ...link, endedAt: at, endedByName: 'Operador', revision: link.revision + 1 }
          : link,
      );
      return route.fulfill({ json: { externalId: path.split('/').at(-1), revision: 2 } });
    }
    throw new Error(`Requisição inesperada: ${method} ${path}`);
  });
  await page.goto('/login');
  await page.getByLabel('Login', { exact: true }).fill('root');
  await page.getByLabel('Senha', { exact: true }).fill('test-password');
  await page.getByRole('button', { name: 'Entrar', exact: true }).click();
  await expect(page).toHaveURL(options.root === false ? /\/admin\/leads$/ : /\/admin$/);
  return { writes, reads };
}
for (const mobile of [false, true]) {
  test(`inventário ${mobile ? 'mobile' : 'desktop'} cadastra recurso e preserva períodos e autores`, async ({
    page,
  }, testInfo) => {
    await page.setViewportSize({ width: mobile ? 390 : 1440, height: 1000 });
    const { writes } = await setup(page);
    await page
      .getByRole('navigation', { name: 'Navegação da operação' })
      .getByRole('link', { name: 'Configuração', exact: true })
      .click();
    await page
      .getByRole('navigation', { name: 'Cadastros e ferramentas' })
      .getByRole('link', { name: 'Recursos', exact: true })
      .click();
    await page.getByRole('link', { name: 'Cadastrar recurso' }).click();
    await page.getByLabel('Nome *', { exact: true }).fill('VM compartilhada');
    await page.getByLabel('Identificador técnico').fill('vm-shared');
    await page.getByRole('combobox', { name: 'Situação', exact: true }).selectOption('verified');
    await page.getByRole('button', { name: 'Salvar recurso' }).click();
    await expect(page.getByRole('alert')).toContainText('fonte da conferência');
    expect(writes).toHaveLength(0);
    await page.getByLabel('Fonte da informação / conferência').fill('Conferência manual');
    await page.getByLabel('Memória (GiB)', { exact: true }).fill('8');
    await page.getByRole('button', { name: 'Salvar recurso' }).click();
    await expect(page.getByRole('heading', { name: 'Ficha do recurso' })).toBeVisible();
    expect(writes[0].body['memoryCapacityGiB']).toBe(8);
    expect(writes[0].body['cpuCapacity']).toBeNull();
    await expect(page.getByLabel('Identificador técnico')).toBeDisabled();
    const panel = page.getByRole('region', { name: 'Vínculos do inventário' });
    await panel.getByText('Vincular cliente existente', { exact: true }).click();
    await panel.getByRole('button', { name: 'Empresa Aurora', exact: true }).click();
    await panel.getByLabel('Finalidade do vínculo').fill('Infraestrutura da empresa');
    await panel.getByRole('button', { name: 'Registrar vínculo' }).click();
    await expect(panel.getByText('Infraestrutura da empresa', { exact: true })).toBeVisible();
    page.once('dialog', (dialog) => dialog.accept());
    await panel.getByRole('button', { name: 'Encerrar vínculo' }).click();
    await expect(panel.getByText('Nenhum vínculo neste recorte.')).toBeVisible();
    await panel.getByLabel('Incluir vínculos encerrados').check();
    await expect(panel.getByText(/Encerrado em .*Operador/)).toBeVisible();
    await page.screenshot({
      path: testInfo.outputPath(`inventory-${mobile ? 'mobile' : 'desktop'}.png`),
      fullPage: true,
    });
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(
      mobile ? 390 : 1440,
    );
  });
}
test('implantação usa recurso com reservas e encerra preservando histórico', async ({ page }) => {
  const { writes } = await setup(page, { seed: true });
  await page.goto(`/admin/services/${serviceId}`);
  await page.getByRole('link', { name: 'Cadastrar ambiente' }).click();
  await page.getByLabel('Nome *').fill('API em produção');
  await page.getByRole('combobox', { name: 'Situação', exact: true }).selectOption('active');
  await page.getByRole('button', { name: 'Salvar implantação' }).click();
  await expect(page.getByRole('heading', { name: 'Ficha da implantação' })).toBeVisible();
  const panel = page.getByRole('region', { name: 'Vínculos do inventário' });
  await panel.getByText('Vincular recurso existente', { exact: true }).click();
  await panel.getByRole('button', { name: 'VM compartilhada', exact: true }).click();
  await panel.getByLabel('Finalidade do vínculo').fill('Executa a API');
  await panel.getByLabel('CPU reservada (vCPU)').fill('0.5');
  await panel.getByRole('button', { name: 'Registrar vínculo' }).click();
  await expect(panel.getByText('Executa a API', { exact: true })).toBeVisible();
  expect(writes.find((write) => write.path === '/deployment-resources')!.body['reservedCpu']).toBe(
    0.5,
  );
  await page.getByRole('combobox', { name: 'Situação', exact: true }).selectOption('closed');
  page.once('dialog', (dialog) => dialog.accept());
  await page.getByRole('button', { name: 'Salvar implantação' }).click();
  await expect(page.getByText(/Período encerrado em/)).toBeVisible();
  await panel.getByLabel('Incluir vínculos encerrados').check();
  await expect(panel.getByText(/Encerrado em .*Operador/)).toBeVisible();
  await expect(panel.getByText('Vincular recurso existente', { exact: true })).toHaveCount(0);
});
test('conflito conserva formulário e exige reabrir antes de sobrescrever', async ({ page }) => {
  await setup(page, { seed: true, conflict: true });
  await page.goto(`/admin/resources/${resourceId}`);
  await page.getByLabel('Nome *', { exact: true }).fill('Nome local');
  await page.getByRole('button', { name: 'Salvar recurso' }).click();
  await expect(page.getByRole('alert')).toContainText('Este recurso mudou');
  await expect(page.getByLabel('Nome *', { exact: true })).toHaveValue('Nome local');
  await expect(page.getByRole('button', { name: 'Salvar recurso' })).toBeDisabled();
  await page.getByRole('button', { name: 'Reabrir ficha e descartar alterações locais' }).click();
  await expect(page.getByLabel('Nome *', { exact: true })).toHaveValue('VM compartilhada');
});
test('cliente reúne duas origens sem duplicar recurso compartilhado', async ({ page }) => {
  await setup(page, { seed: true });
  await page.goto(`/admin/customers/${customerId}`);
  const panel = page.getByRole('region', { name: 'Recursos do cliente' });
  await expect(panel.getByRole('link', { name: 'VM compartilhada', exact: true })).toHaveCount(1);
  await expect(panel.getByText(/Compartilhado por 2 clientes/)).toBeVisible();
  await expect(panel.getByRole('link', { name: 'API Aurora / Produção' })).toBeVisible();
  await expect(panel.getByRole('link', { name: 'Banco compartilhado' })).toBeVisible();
});
test('filtros persistem na URL e em reload sem enviar campos vazios', async ({ page }) => {
  const { reads } = await setup(page, { seed: true });
  await page.goto('/admin/resources');
  await page.getByRole('combobox', { name: 'Tipo', exact: true }).selectOption('compute');
  await page.getByRole('combobox', { name: 'Compartilhamento', exact: true }).selectOption('true');
  await page.getByRole('combobox', { name: 'Ambiente', exact: true }).selectOption('production');
  await page.getByRole('button', { name: 'Filtrar', exact: true }).click();
  await expect(page).toHaveURL(/type=compute/);
  await expect
    .poll(() =>
      reads
        .filter((url) => url.pathname === '/resources')
        .at(-1)
        ?.searchParams.get('shared'),
    )
    .toBe('true');
  const url = reads.filter((url) => url.pathname === '/resources').at(-1)!;
  expect(url.searchParams.has('search')).toBe(false);
  await page.reload();
  await expect(page.getByRole('combobox', { name: 'Tipo', exact: true })).toHaveValue('compute');
});
test('operador sem root não vê inventário nem acessa rota direta', async ({ page }) => {
  await setup(page, { root: false });
  await expect(
    page
      .getByRole('navigation', { name: 'Navegação da operação' })
      .getByRole('link', { name: 'Recursos', exact: true }),
  ).toHaveCount(0);
  await page.goto('/admin/resources');
  await expect(page.getByRole('heading', { name: 'Acesso negado' })).toBeVisible();
});

test('relações técnicas escolhem recurso existente e preservam finalidade', async ({ page }) => {
  const { writes } = await setup(page, { seed: true });
  await page.goto(`/admin/resources/${resourceId}`);
  const panel = page.getByRole('region', { name: 'Vínculos do inventário' });
  await panel.getByRole('button', { name: 'Relações técnicas', exact: true }).click();
  await panel.getByText('Relacionar outro recurso', { exact: true }).click();
  await panel.getByLabel('Buscar recurso', { exact: true }).fill('Banco');
  await panel.getByRole('button', { name: 'Banco compartilhado', exact: true }).click();
  await panel.getByLabel('Finalidade do vínculo').fill('Persistência da aplicação');
  await panel.getByRole('button', { name: 'Registrar vínculo' }).click();
  await expect(panel.getByText('Persistência da aplicação', { exact: true })).toBeVisible();
  const write = writes.find((write) => write.path === '/resource-dependencies')!;
  expect(write.body['resourceExternalId']).toBe(resourceId);
  expect(write.body['targetExternalId']).toBe(otherId);
  expect(write.body['kind']).toBe('depends-on');
});

test('recurso com alterações pede confirmação antes de sair', async ({ page }) => {
  await setup(page, { seed: true });
  await page.goto(`/admin/resources/${resourceId}`);
  await page.getByLabel('Nome *', { exact: true }).fill('Alteração pendente');
  page.once('dialog', (dialog) => dialog.dismiss());
  await page.getByRole('link', { name: 'Voltar aos recursos', exact: true }).click();
  await expect(page).toHaveURL(new RegExp(resourceId));
  await expect(page.getByLabel('Nome *', { exact: true })).toHaveValue('Alteração pendente');
});

test('novo recurso simplifica tipo e salva escopo sem selecionar cliente', async ({ page }) => {
  const { writes } = await setup(page);
  await page.goto('/admin/resources/new');
  const type = page.getByRole('combobox', { name: 'Tipo', exact: true });
  await expect(type.getByRole('option')).toHaveCount(1);
  await expect(type).toHaveValue('compute');
  await expect(page.getByRole('combobox', { name: 'Escopo', exact: true })).toHaveValue('shared');
  await page.getByRole('combobox', { name: 'Escopo', exact: true }).selectOption('individual');
  await page.getByLabel('Nome *', { exact: true }).fill('Servidor dedicado');
  await page.getByLabel('Identificador técnico').fill('vm-dedicated');
  await page.getByLabel('Provedor', { exact: true }).fill('AWS');
  await page.getByLabel('Identificador no provedor', { exact: true }).fill('i-0123456789abcdef0');
  await page.getByRole('button', { name: 'Salvar recurso' }).click();
  await expect(page.getByRole('heading', { name: 'Ficha do recurso' })).toBeVisible();
  expect(writes[0].body['scope']).toBe('individual');
  expect(writes[0].body['type']).toBe('compute');
  expect(writes[0].body['provider']).toBe('AWS');
});
