// Servidor exclusivo dos testes: reproduz HTML público e fallback CSR do Nginx.
import { createServer } from 'node:http';
import { readFile, stat } from 'node:fs/promises';
import { resolve, extname, sep } from 'node:path';
import { spawnSync } from 'node:child_process';

const build = spawnSync(process.execPath, ['node_modules/@angular/cli/bin/ng.js', 'build'], {
  stdio: 'inherit',
  env: { ...process.env, NG_BUILD_MAX_WORKERS: '2' },
});
if (build.status !== 0) process.exit(build.status ?? 1);
const root = resolve('dist/ms-ltmj-ng/browser');
const types = {
  '.html': 'text/html',
  '.js': 'text/javascript',
  '.css': 'text/css',
  '.json': 'application/json',
  '.jpg': 'image/jpeg',
  '.jpeg': 'image/jpeg',
  '.png': 'image/png',
  '.svg': 'image/svg+xml',
  '.webp': 'image/webp',
  '.woff2': 'font/woff2',
  '.ico': 'image/x-icon',
  '.txt': 'text/plain',
  '.xml': 'application/xml',
};
createServer(async (request, response) => {
  try {
    const path = decodeURIComponent(new URL(request.url, 'http://localhost').pathname);
    let file = resolve(root, `.${path}`);
    if (!file.startsWith(root + sep) && file !== root) {
      response.writeHead(403).end();
      return;
    }
    if (path === '/') file = resolve(root, 'index.html');
    else {
      try {
        if (!(await stat(file)).isFile()) throw new Error('Not a file');
      } catch {
        if (extname(path)) {
          response.writeHead(404).end();
          return;
        }
        file = resolve(root, 'index.csr.html');
      }
    }
    response.writeHead(200, {
      'Content-Type': types[extname(file)] ?? 'application/octet-stream',
      'Cache-Control': 'no-store',
    });
    response.end(await readFile(file));
  } catch {
    response.writeHead(500).end();
  }
}).listen(4317, '127.0.0.1');
