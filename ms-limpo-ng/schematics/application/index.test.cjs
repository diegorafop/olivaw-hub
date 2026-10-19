const { test } = require('node:test');
const assert = require('node:assert/strict');
const path = require('node:path');
const { Tree } = require('@angular-devkit/schematics');
const { SchematicTestRunner } = require('@angular-devkit/schematics/testing');
const options = {
  name: 'ms-exemplo-ng',
  apiUrl: 'https://api.exemplo.com',
  siteDomain: 'exemplo.com',
};
function runner() {
  return new SchematicTestRunner('olivaw', path.resolve(__dirname, '../collection.json'));
}
test('gera o padrão atual com nome, API, DNS e sem artefatos locais', async () => {
  const tree = await runner().runSchematic('application', options);
  assert.equal(JSON.parse(tree.readContent('/ms-exemplo-ng/package.json')).name, options.name);
  assert.equal(
    JSON.parse(tree.readContent('/ms-exemplo-ng/public/config.json')).apiUrl,
    options.apiUrl,
  );
  assert.match(tree.readContent('/ms-exemplo-ng/docker/nginx.conf'), /server_name exemplo.com;/);
  assert.match(tree.readContent('/ms-exemplo-ng/src/index.html'), /https:\/\/exemplo.com\//);
  assert.equal(
    tree.files.some((file) => /node_modules|__pycache__|\.angular|schematics\//.test(file)),
    false,
  );
  assert.equal(
    JSON.parse(tree.readContent('/ms-exemplo-ng/angular.json')).cli.analytics,
    undefined,
  );
  assert.equal(
    JSON.parse(tree.readContent('/ms-exemplo-ng/package-lock.json')).packages[''].name,
    options.name,
  );
});
test('preserva documentos na pasta existente', async () => {
  const initial = Tree.empty();
  initial.create('/ms-exemplo-ng/docs/briefing.md', 'Original');
  const tree = await runner().runSchematic('application', options, initial);
  assert.equal(tree.readContent('/ms-exemplo-ng/docs/briefing.md'), 'Original');
});
test('recusa conflitos sem sobrescrever ou criar arquivos', async () => {
  const initial = Tree.empty();
  initial.create('/ms-exemplo-ng/angular.json', 'Original');
  await assert.rejects(runner().runSchematic('application', options, initial), /conflitantes/);
  assert.equal(initial.read('/ms-exemplo-ng/angular.json').toString(), 'Original');
  assert.equal(initial.exists('/ms-exemplo-ng/package.json'), false);
});
test('recusa caminhos fora do padrão e URLs relativas', async () => {
  await assert.rejects(runner().runSchematic('application', { ...options, name: '../fora' }));
  await assert.rejects(
    runner().runSchematic('application', { ...options, apiUrl: '/api' }),
    /absoluta/,
  );
});

test('gera os cinco ambientes com domínio, API, isolamento e dotenv preenchidos', async () => {
  const tree = await runner().runSchematic('application', options);
  for (const env of ['dev', 'qas', 'rls', 'stg', 'prd']) {
    const compose = tree.readContent(`/ms-exemplo-ng/docker/docker-compose.${env}.yml`);
    const settings = tree.readContent(`/ms-exemplo-ng/docker/.env.${env}`);
    const host = env === 'prd' ? 'exemplo.com' : `${env}.exemplo.com`;
    assert.ok(compose.includes(`name: ms-exemplo-ng-${env}`));
    assert.ok(settings.includes(`PUBLIC_HOST=${host}`));
    assert.ok(settings.includes(`API_URL=https://api.${host}`));
    assert.equal(compose.includes('limpo.olivaw'), false);
    assert.equal(compose.includes('ports:'), false);
  }
});
test('transporta apenas dotenv públicos conhecidos, excluindo ajustes locais', () => {
  const fs = require('node:fs');
  const os = require('node:os');
  const { collect } = require('./index.cjs');
  const fixture = fs.mkdtempSync(path.join(os.tmpdir(), 'olivaw-schematic-'));
  try {
    for (const file of ['.env', '.env.prd', '.env.local', '.env.particular'])
      fs.writeFileSync(path.join(fixture, file), 'exemplo');
    assert.deepEqual(collect(fixture, 'docker').sort(), ['docker/.env', 'docker/.env.prd']);
  } finally {
    fs.rmSync(fixture, { recursive: true });
  }
});

test('gera ambientes sem alterar o nome de um domínio raiz com.br', async () => {
  const tree = await runner().runSchematic('application', {
    name: 'ms-empresa-ng',
    siteDomain: 'empresa.com.br',
  });
  assert.match(tree.readContent('/ms-empresa-ng/docker/.env.prd'), /PUBLIC_HOST=empresa\.com\.br/);
  assert.match(tree.readContent('/ms-empresa-ng/docker/.env.stg'), /PUBLIC_HOST=stg\.empresa\.com\.br/);
  assert.match(tree.readContent('/ms-empresa-ng/docker/.env.stg'), /API_URL=https:\/\/api\.stg\.empresa\.com\.br/);
});
