# Certificado HTTPS

O arquivo `https/ms-limpo-api.pfx` acompanha o template e é renomeado na geração de um novo serviço. Configure `Kestrel__Certificates__Default__Password` no `docker/.env` local ou em um gerenciador de segredos. Para substituir o certificado, gere um novo PFX com `dotnet dev-certs https -ep <caminho-do-arquivo>.pfx -p <senha>`.
