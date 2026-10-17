# Gerar um microservice

No diretório deste repositório, execute:

```bash
./scripts/new-microservice.sh precatorio-calculo
```

O resultado será `../ms-precatorio-calculo-api`, com solução, projetos e namespaces em `PrecatorioCalculo` e identificadores de serviço em `precatorio-calculo`. O script instala ou atualiza o template local do `dotnet new`. O diretório de destino não pode existir. Para escolher outro destino, passe-o como segundo argumento.

O arquivo `certificates/https/ms-limpo-api.pfx` é copiado e renomeado para o novo serviço. No novo serviço, crie `docker/.env` a partir de `docker/.env.example` e informe a senha do certificado. O template não copia o `.env`.

Informe também `GITEA_API_KEY` no ambiente do Docker Compose para restaurar os pacotes privados. O projeto `Mps.<Nome>.Client` é gerado como biblioteca NuGet base, incluída na solução e no estágio `pack` do Dockerfile; seus contratos e operações devem ser implementados conforme as regras reais do novo serviço.
