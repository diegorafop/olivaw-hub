# Docker do frontend

Execute os comandos da [documentação do template](../README.md#docker) a partir da raiz do monorepo. Docker Compose v2 é necessário.

- `Dockerfile`: build do Angular e runtime Nginx sem root.
- `nginx.conf`: arquivos públicos e fallback para o shell CSR; não contém proxy de API.
- `docker-compose.yml`: serviço exclusivo do frontend, porta padrão 8081.
- `.env.example`: variáveis públicas de exemplo, sem segredos.

Frontend e API têm containers, builds e publicações independentes. Não precisam compartilhar redes Docker. As chamadas HTTP são feitas pelo navegador à URL absoluta configurada em `public/config.json`. Nunca use hostname interno de container como endereço público da API.

```bash
docker compose -f ms-limpo-ng/docker/docker-compose.yml up --build -d
```

Para alterar a porta sem criar arquivo local:

```bash
FRONTEND_PORT=8082 docker compose -f ms-limpo-ng/docker/docker-compose.yml up --build -d
```

Para produção, crie um JSON público próprio com a URL HTTPS da API:

```json
{
  "apiUrl": "https://api.cliente.example.com"
}
```

Monte esse arquivo em `/usr/share/nginx/html/config.json:ro` por um override local de Compose:

```yaml
services:
  ms-limpo-ng:
    volumes:
      - /caminho/absoluto/settings-publicos.json:/usr/share/nginx/html/config.json:ro
```

O padrão `http://localhost:5137` atende somente desenvolvimento no próprio computador. `localhost` no navegador de um visitante aponta para a máquina desse visitante. A API deve ser publicada separadamente e permitir a origem do frontend via CORS. TLS do frontend fica no proxy de entrada da infraestrutura. Não inclua segredos no JSON público.

Valide a configuração sem subir serviços:

```bash
docker compose -f ms-limpo-ng/docker/docker-compose.yml config --quiet
```

`GET /health` deve retornar 200. A raiz deve conter o HTML público prerenderizado; uma rota interna deve receber `index.csr.html`; um arquivo `.js` inexistente deve retornar 404. HTML e configuração não têm cache persistente; assets usam cache de uma hora.

As imagens base usam tags de manutenção (`node:24-alpine` e `nginxinc/nginx-unprivileged:stable-alpine`). Fixe digests revisados na esteira de produção se precisar reproduzir também as camadas do sistema operacional. Nenhuma imagem é publicada por este template.
