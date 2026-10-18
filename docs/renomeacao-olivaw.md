# Renomeação para Olivaw

## Decisão e escopo

Em 18/10/2026, o fundador definiu **Olivaw** como nome da marca. A solução principal passa a ser `Olivaw.sln`; a API administrativa fica em `ms-olivaw-api`, com projetos `Owh.Olivaw.*`, e o site em `ms-olivaw-ng`. O prefixo compartilhado `Owh` continua conforme a convenção das APIs.

Também acompanham a mudança os nomes Angular/npm, Docker/Traefik, prefixos de mensageria, identificadores internos de JWT, exemplos, scripts e referências entre projetos. As contas de implantação usam `deployment/olivaw-credentials.env`, com `OLIVAW_LOGIN` e `OLIVAW_PASSWORD`; o cliente aceita `OLIVAW_API_URL`. Os valores secretos permanecem preservados e o arquivo continua excluído dos contextos Docker.

## Transição operacional pendente

A alteração é local. Antes de publicar, preparar uma janela de manutenção e backup, conferir o estado real de cada ambiente e coordenar:

- O checkout da VM e a allowlist do executor passam a usar `olivaw/` e as novas pastas de serviços. Atualizar também permissões `devops.deploy:<folder>` das contas técnicas existentes; alterações no código não atualizam registros já persistidos.
- A pasta deste clone e o repositório remoto conservam seus endereços atuais. Links GitHub publicados nos metadados continuam apontando para o repositório existente. Renomear a pasta de trabalho e o repositório remoto exige uma etapa própria; a pasta raiz não é um caminho versionado pelo Git.
- As configurações de banco compartilhadas passam de `olivaw_hub_<ambiente>` para `olivaw_<ambiente>` em todos os serviços que as usam. A API administrativa passa do schema `ms_olivaw_hub` para `ms_olivaw`, incluindo o histórico de migrations. Em bancos existentes, renomear ou migrar os recursos preservando dados, permissões e a tabela de histórico antes de iniciar os novos binários. Não apontar para um banco vazio nem executar as migrations sobre um schema novo como substituto da migração dos dados. Nenhum banco foi acessado ou alterado nesta tarefa.
- Os arquivos de migrations existentes acompanham o novo namespace/schema; não foi criada uma migration de transição de dados. Uma instalação existente precisa da preparação de banco descrita acima.
- Publicar a autoridade administrativa e a DevOps com emissor/audiência correspondentes por ambiente. Os novos identificadores internos invalidam tokens antigos; autenticar novamente após a atualização coordenada. As chaves de assinatura não foram rotacionadas.
- Atualizar o executor e os scripts de implantação com os novos caminhos e variáveis. O checkout externo do Lisierp também precisa adaptar o arquivo de credenciais e seus scripts próprios; ele não foi modificado nesta tarefa. Conferir filas e prefixos de mensageria e remover containers antigos somente na implantação autorizada, evitando duas autoridades ativas sobre o mesmo banco.

## Domínio ao final

A compra de `olivaw.com.br` ainda aguarda confirmação do fundador. Até essa confirmação, ficam preservados hosts DuckDNS, regras de roteamento por hostname, URLs públicas, CORS, canonical, sitemap, robots e URLs de API que usam esses hosts. Nenhuma configuração de DNS, certificado, hospedagem ou serviço remoto foi alterada.

Após a compra, definir os subdomínios do site, da autoridade, da DevOps e dos clientes; então atualizar em conjunto DNS, TLS/Traefik, URLs de runtime, CORS e metadados públicos. O gerador Angular também conserva o domínio padrão atual até essa etapa.

## Validação local

Comandos usados para validar a renomeação, sem implantação e sem bancos reais:

```bash
dotnet build Olivaw.sln -c Release -m:1
dotnet test Olivaw.sln -c Release --no-build
npm --prefix ms-olivaw-ng run build
python3 -m unittest discover -s scripts/deployment/tests -v
```

Os resultados observados são informados na entrega da tarefa.
