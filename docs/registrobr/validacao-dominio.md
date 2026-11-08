# Validação de olivaw.com.br

## Compra confirmada

Em 18/10/2026, o fundador confirmou a compra no Registro.br e informou o status publicado. Os documentos recebidos permanecem nesta pasta. Os arquivos sensíveis ficam em `../secrets/`; seus conteúdos não devem ser reproduzidos na documentação ou nos logs.

## Resultado observado

- `olivaw.com.br` possui delegação para `a.auto.dns.br` e `b.auto.dns.br`.
- Consultas A e AAAA retornaram `NOERROR`, sem endereços.
- A consulta direta ao servidor autoritativo `a.auto.dns.br` também confirmou ausência do registro A. Portanto, não é apenas uma questão de cache do resolvedor consultado.
- `www.olivaw.com.br` retornou `NXDOMAIN`.
- HTTP e HTTPS falharam na resolução do nome; ainda não foi possível avaliar o certificado ou a aplicação pelo novo domínio.
- O endereço antigo `olivaw-hub.duckdns.org` retornou `3.88.110.125`. Esse IP precisa ser confirmado como destino estável da infraestrutura antes de configurar o novo domínio.

Publicado indica que a delegação existe; o site ainda precisa de apontamento e configuração da hospedagem. Nenhuma alteração remota foi feita nesta verificação.

## Próximos passos

1. Confirmar o IP público estável da VM, idealmente Elastic IP.
2. No DNS do Registro.br, criar A para a raiz apontando ao IP confirmado e CNAME de `www` para `olivaw.com.br`.
3. Definir e configurar os subdomínios das APIs.
4. Atualizar Traefik/TLS, URLs públicas e de API, CORS, canonical, sitemap e robots na implantação coordenada. DNS sozinho não configura o site nem o certificado.
5. Repetir as consultas e confirmar HTTPS e conteúdo da página.

```bash
dig olivaw.com.br NS +short
dig @a.auto.dns.br olivaw.com.br A +noall +answer +authority
dig olivaw.com.br A +short
dig www.olivaw.com.br A +short
curl -I --max-time 15 https://olivaw.com.br
```

Após configurar o apontamento, caches negativos podem manter temporariamente a resposta anterior. A consulta direta ao servidor autoritativo ajuda a distinguir publicação de propagação nos resolvedores.
