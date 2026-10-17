# certificates

-- Certificados necessários para o service.

-- Gerar cerfitificado auto assinado.
	
	dotnet dev-certs https --clean
	
	dotnet dev-certs https -t
	
	dotnet dev-certs https --trust -ep $env:USERPROFILE\.aspnet\https\ms-fluxo-evento-api.pfx -p mps2021*