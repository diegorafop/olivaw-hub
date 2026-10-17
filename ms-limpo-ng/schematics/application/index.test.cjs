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
    tree.files.some((file) => /node_modules|\.angular|schematics\//.test(file)),
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
