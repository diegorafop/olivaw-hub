FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIG=Release
WORKDIR /src
COPY . .
RUN dotnet restore ms-olivaw-hub-api/src/Owh.OlivawHub.Api/Owh.OlivawHub.Api.csproj
RUN dotnet publish ms-olivaw-hub-api/src/Owh.OlivawHub.Api/Owh.OlivawHub.Api.csproj -c ${BUILD_CONFIG} --no-restore -o /app/publish

FROM build AS tests
ARG BUILD_CONFIG=Release
RUN dotnet test olivaw-identity/tests/Olivaw.Identity.Tests/Olivaw.Identity.Tests.csproj -c ${BUILD_CONFIG}
RUN dotnet test ms-olivaw-core-api/tests/Owh.OlivawCore.Tests/Owh.OlivawCore.Tests.csproj -c ${BUILD_CONFIG}
RUN dotnet test ms-olivaw-hub-api/tests/Owh.OlivawHub.Tests/Owh.OlivawHub.Tests.csproj -c ${BUILD_CONFIG}

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=tests --chown=app:app /app/publish .
USER app
ENTRYPOINT ["dotnet", "Owh.OlivawHub.Api.dll"]
