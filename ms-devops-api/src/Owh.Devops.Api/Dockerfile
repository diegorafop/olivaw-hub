FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIG=Release
WORKDIR /src
COPY . .
RUN dotnet restore ms-devops-api/src/Owh.Devops.Api/Owh.Devops.Api.csproj
RUN dotnet publish ms-devops-api/src/Owh.Devops.Api/Owh.Devops.Api.csproj -c ${BUILD_CONFIG} --no-restore -o /app/publish

FROM build AS tests
ARG BUILD_CONFIG=Release
RUN dotnet test olivaw-identity/tests/Olivaw.Identity.Tests/Olivaw.Identity.Tests.csproj -c ${BUILD_CONFIG}
RUN dotnet test ms-olivaw-core-api/tests/Owh.OlivawCore.Tests/Owh.OlivawCore.Tests.csproj -c ${BUILD_CONFIG}
RUN dotnet test ms-devops-api/tests/Owh.Devops.Tests/Owh.Devops.Tests.csproj -c ${BUILD_CONFIG}

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
RUN apt-get update && apt-get install -y --no-install-recommends openssh-client && rm -rf /var/lib/apt/lists/*
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=tests --chown=app:app /app/publish .
USER app
ENTRYPOINT ["dotnet", "Owh.Devops.Api.dll"]
