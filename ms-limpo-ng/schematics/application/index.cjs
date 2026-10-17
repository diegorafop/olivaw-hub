const fs = require('node:fs');
const path = require('node:path');
const { SchematicsException } = require('@angular-devkit/schematics');
const templateRoot = path.resolve(__dirname, '../..');
const roots = ['src', 'public', 'docker', '.vscode'];
const rootFiles = [
  '.dockerignore',
  '.editorconfig',
  '.gitignore',
  '.nvmrc',
  '.postcssrc.json',
  '.prettierignore',
  '.prettierrc',
  'AGENTS.md',
  'angular.json',
  'eslint.config.mjs',
  'package.json',
  'package-lock.json',
  'playwright.config.ts',
  'tsconfig.json',
  'tsconfig.app.json',
  'tsconfig.spec.json',
];
function collect(directory, prefix = '') {
  return fs.readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const publicEnvs = ['.env', '.env.dev', '.env.qas', '.env.rls', '.env.stg', '.env.prd'];
    if (entry.name.startsWith('.env') && !(prefix === 'docker' && publicEnvs.includes(entry.name)))
      return [];
    if (entry.name === '__pycache__' || entry.name.endsWith('.pyc')) return [];
    const relative = path.posix.join(prefix, entry.name);
    if (entry.isSymbolicLink())
      throw new SchematicsException(`Link simbólico não permitido: ${relative}`);
    return entry.isDirectory() ? collect(path.join(directory, entry.name), relative) : [relative];
  });
}
function environmentHost(host, environment) {
  if (environment === 'prd') return host;
  if (host === 'olivaw-hub.duckdns.org' || host.split('.').length === 2)
    return `${environment}.${host}`;
  const [label, ...base] = host.split('.');
  return `${label}-${environment}.${base.join('.')}`;
}
function application(options) {
  return (tree, context) => {
    const { name, apiUrl = 'http://localhost:5137', siteDomain } = options;
    if (!/^ms-[a-z0-9]+(?:-[a-z0-9]+)*-ng$/.test(name) || name === 'ms-limpo-ng') {
      throw new SchematicsException(
        'Use um nome de cliente como ms-cliente-ng, diferente do template.',
      );
    }
    let api;
    try {
      api = new URL(apiUrl);
    } catch {
      throw new SchematicsException('apiUrl deve ser uma URL absoluta HTTP(S).');
    }
    if (
      !['http:', 'https:'].includes(api.protocol) ||
      api.username ||
      api.password ||
      api.search ||
      api.hash
    ) {
      throw new SchematicsException(
        'apiUrl deve usar HTTP(S), sem credenciais, query ou fragmento.',
      );
    }
    if (siteDomain && !/^(?=.{1,253}$)[a-z0-9]+(?:[.-][a-z0-9]+)*\.[a-z]{2,}$/.test(siteDomain)) {
      throw new SchematicsException('siteDomain deve ser um DNS sem protocolo.');
    }
    const publicHost = siteDomain ?? `${name.slice(3, -3)}.olivaw-hub.duckdns.org`;
    const cloudApi = api.hostname === 'localhost' ? new URL(`https://api.${publicHost}`) : api;
    const target = path.resolve(process.cwd(), name);
    if (fs.existsSync(target) && fs.lstatSync(target).isSymbolicLink())
      throw new SchematicsException('Destino não pode ser link simbólico.');
    const paths = [
      ...rootFiles,
      ...roots.flatMap((root) => collect(path.join(templateRoot, root), root)),
      ...collect(path.join(templateRoot, 'e2e'), 'e2e'),
    ];
    const output = new Map();
    for (const relative of paths) {
      const buffer = fs.readFileSync(path.join(templateRoot, relative));
      const decoded = buffer.toString('utf8');
      if (buffer.includes(0) || !Buffer.from(decoded).equals(buffer)) {
        output.set(`${name}/${relative}`, buffer);
        continue;
      }
      let content = decoded.replaceAll('ms-limpo-ng', name);
      if (relative.startsWith('docker/')) {
        for (const environment of ['dev', 'qas', 'rls', 'stg', 'prd']) {
          const oldHost =
            environment === 'prd'
              ? 'limpo.olivaw-hub.duckdns.org'
              : `limpo-${environment}.olivaw-hub.duckdns.org`;
          const newHost = environmentHost(publicHost, environment);
          const apiForEnvironment = new URL(cloudApi.href);
          if (environment !== 'prd')
            apiForEnvironment.hostname = cloudApi.hostname.startsWith('api.')
              ? `api.${environmentHost(cloudApi.hostname.slice(4), environment)}`
              : environmentHost(cloudApi.hostname, environment);
          content = content.replaceAll(
            `https://api.${oldHost}`,
            apiForEnvironment.href.replace(/\/$/, ''),
          );
          content = content.replaceAll(oldHost, newHost);
        }
      }
      if (relative.startsWith('e2e/'))
        content = content.replaceAll('http://localhost:5137', api.href.replace(/\/$/, ''));
      if (relative === 'package.json' || relative === 'package-lock.json') {
        const data = JSON.parse(content);
        const pkg = relative === 'package.json' ? data : data.packages[''];
        for (const dependency of ['@angular-devkit/schematics', '@angular-devkit/schematics-cli'])
          delete pkg.devDependencies[dependency];
        if (pkg.scripts)
          for (const script of ['new:client', 'test:schematics']) delete pkg.scripts[script];
        delete pkg.schematics;
        content = JSON.stringify(data, null, 2) + '\n';
      }
      if (relative === 'angular.json') {
        const data = JSON.parse(content);
        delete data.cli.analytics; // Preferência local do template não é transportada.
        content = JSON.stringify(data, null, 2) + '\n';
      }
      if (relative === 'public/config.json')
        content = JSON.stringify({ apiUrl: api.href.replace(/\/$/, '') }, null, 2) + '\n';
      if (relative === 'src/index.html' && siteDomain)
        content = content.replace(
          '</head>',
          `  <link rel="canonical" href="https://${siteDomain}/">\n</head>`,
        );
      if (relative === 'docker/nginx.conf' && siteDomain)
        content = content.replace('server_name _;', `server_name ${siteDomain};`);
      output.set(`${name}/${relative}`, content);
    }
    if (siteDomain) {
      output.set(
        `${name}/public/robots.txt`,
        `User-agent: *\nAllow: /\nDisallow: /admin\nDisallow: /login\nSitemap: https://${siteDomain}/sitemap.xml\n`,
      );
      output.set(
        `${name}/public/sitemap.xml`,
        `<?xml version="1.0" encoding="UTF-8"?>\n<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"><url><loc>https://${siteDomain}/</loc></url></urlset>\n`,
      );
    }
    output.set(
      `${name}/README.md`,
      `# ${name}\n\nFrontend gerado do template **Olivaw Hub**, com build e publicação independentes da API.\n\n## Desenvolvimento\n\nNode 24 e npm 11.\n\n\`\`\`bash\nnpm ci\nnpm start\nnpm run build\nnpm test\nnpm run lint\nnpm run format:check\nnpx playwright install chromium\nnpm run e2e\n\`\`\`\n\nSettings públicos: \`public/config.json\`. Tema: \`src/theme/_brand.scss\`. ${siteDomain ? `Site: https://${siteDomain}. ` : ''}API: ${api.href.replace(/\/$/, '')}.\n\nDocker: \`docker compose -f docker/docker-compose.yml -f docker/docker-compose.override.yml up --build -d\`. Configure CORS na API para a origem do frontend. Não há proxy de API, refresh token nem persistência da sessão. Consulte o [template](../ms-limpo-ng/README.md) para os padrões compartilhados. Arquivos existentes no destino são preservados; a geração recusa conflitos e não atualiza projetos já gerados.\n`,
    );
    // Valida todos os conflitos antes de registrar qualquer criação. Nunca usa overwrite/delete.
    const conflicts = [...output.keys()].filter((file) => tree.exists(file));
    if (conflicts.length)
      throw new SchematicsException(
        `Destino contém arquivos conflitantes; nada foi sobrescrito: ${conflicts.join(', ')}`,
      );
    for (const [file, content] of output) tree.create(file, content);
    context.logger.info(
      `Criado ${name}. Arquivos preexistentes preservados. Execute npm ci dentro do novo projeto.`,
    );
    return tree;
  };
}
module.exports = { application, collect };
