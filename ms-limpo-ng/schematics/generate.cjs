const path = require('node:path');
const { spawnSync } = require('node:child_process');
const args = process.argv.slice(2);
if (args.some((arg) => /^--(?:force|allow-private)(?:=|$)/.test(arg))) {
  console.error('O gerador não permite forçar sobrescrita.');
  process.exit(1);
}
const cli = require.resolve('@angular-devkit/schematics-cli/bin/schematics.js');
const collection = path.join(__dirname, 'collection.json');
if (!args.some((arg) => /^--dry-run(?:=|$)/.test(arg))) args.push('--dry-run=false');
const result = spawnSync(process.execPath, [cli, `${collection}:application`, ...args], {
  cwd: path.resolve(__dirname, '../..'),
  stdio: 'inherit',
});
process.exit(result.status ?? 1);
