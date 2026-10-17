FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y libgssapi-krb5-2
ARG HTTP_PORT
ARG HTTPS_PORT
COPY ["certificates/*", "https/"]
WORKDIR /app
EXPOSE ${HTTP_PORT} ${HTTPS_PORT}

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIG
ARG GITEA_API_KEY
RUN wget -qO- http://ac-mps.mps.com.br/ac-mps-v5.crt >> /usr/local/share/ca-certificates/ac-mps-v5.crt
ENV NODE_EXTRA_CA_CERTS=/ac-mps-v5.crt
RUN update-ca-certificates
WORKDIR /src
COPY . .

RUN dotnet nuget update source "gitea-xjp" \
    --username "holos-no-reply@mps.com.br" \
    --password "${GITEA_API_KEY}" \
    --configfile ./nuget.config \
    --store-password-in-clear-text \
    --valid-authentication-types "basic"

WORKDIR "/src/src/Mps.Auth.Api"
RUN dotnet restore "Mps.Auth.Api.csproj"
RUN dotnet build "Mps.Auth.Api.csproj" -c ${BUILD_CONFIG} -o /app/build

FROM build AS tests
ARG SKIP_UNITARY_TESTS
ARG SKIP_EF_SCHEMA_SCRIPT
ARG BUILD_CONFIG
WORKDIR /src/tests/Mps.Auth.Tests
RUN if [ "$SKIP_UNITARY_TESTS" = "false" ]; then \
    echo -e "========= STARTING TESTS =============\n" ; \
    \
    # Restore
    dotnet tool install dotnet-reportgenerator-globaltool --tool-path /dotnetglobaltools --version 5.5.3 && \
    dotnet add package coverlet.msbuild --version 10.0.1 && \
    dotnet restore "Mps.Auth.Tests.csproj" && \
    \
    # Run
    dotnet test \
    /p:CollectCoverage=true \
    /p:Threshold=0 \
    /p:ThresholdType=line \
    /p:ThresholdType=line \
    /p:ThresholdStat=total \
    /p:CoverletOutputFormat=cobertura \
    /p:Exclude="[*]Mps.Auth.Dados.*" \
    /p:CoverletOutput='./results/' && \
    \
    dotnet test --logger:"xunit;LogFilePath=./results/xunit-result.xml" && \
    \
    # Report
    /dotnetglobaltools/reportgenerator "-reports:results/coverage.cobertura.xml" "-targetdir:./results/reports" "-reporttypes:HTMLInline;HTMLChart" && \
    \
    echo -e "========== FINISH TESTS ==============\n" ; \
    else \
    mkdir results && \
    echo -e "=========== SKIP TESTS ===============\n" ; \
    fi

RUN if [ "$SKIP_EF_SCHEMA_SCRIPT" = "false" ]; then \
    echo -e "========== SCHEMA SCRIPT =============\n" ; \
    \
    mkdir -p /src/src/Mps.Auth.Api/bin/Release/net10.0/ && \
    \
    cp -r /app/build/. /src/src/Mps.Auth.Api/bin/Release/net10.0/ && \
    \
    dotnet tool install --global dotnet-ef --version 10.0.3 && \
    \
    export PATH="$PATH:/root/.dotnet/tools" && \
    \
    dotnet ef migrations script --output ./schema.sql \
    --startup-project /src/src/Mps.Auth.Api \
    --project /src/src/Mps.Auth.Dados.PostgreSql \
    --configuration ${BUILD_CONFIG} \
    --no-build && \
    \
    echo -e "========== FINISH SCRIPT =============\n" ; \
    else \
    touch schema.sql && \
    echo -e "========== SKIP SCRIPT ===============\n" ; \
    fi

FROM build AS publish
ARG BUILD_CONFIG
WORKDIR "/src/src/Mps.Auth.Api"
RUN dotnet publish "Mps.Auth.Api.csproj" -c ${BUILD_CONFIG} -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=tests /src/tests/Mps.Auth.Tests/results ./results
COPY --from=tests /src/tests/Mps.Auth.Tests/schema.sql ./results/

ENTRYPOINT ["dotnet", "Mps.Auth.Api.dll"]