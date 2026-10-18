FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIG=Release
WORKDIR /src
COPY . .
RUN dotnet restore ms-ltmj-api/src/Mps.Ltmj.Api/Mps.Ltmj.Api.csproj
RUN dotnet publish ms-ltmj-api/src/Mps.Ltmj.Api/Mps.Ltmj.Api.csproj -c ${BUILD_CONFIG} --no-restore -o /app/publish

FROM build AS tests
ARG BUILD_CONFIG=Release
RUN dotnet test olivaw-identity/tests/Olivaw.Identity.Tests/Olivaw.Identity.Tests.csproj -c ${BUILD_CONFIG}
RUN dotnet test ms-olivaw-core-api/tests/Mps.OlivawCore.Tests/Mps.OlivawCore.Tests.csproj -c ${BUILD_CONFIG}
RUN dotnet test ms-ltmj-api/tests/Mps.Ltmj.Tests/Mps.Ltmj.Tests.csproj -c ${BUILD_CONFIG}

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=tests --chown=app:app /app/publish .
USER app
ENTRYPOINT ["dotnet", "Mps.Ltmj.Api.dll"]
