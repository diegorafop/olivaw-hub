FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIG
RUN wget -qO- http://ac-mps.mps.com.br/ac-mps-v5.crt >> /usr/local/share/ca-certificates/ac-mps-v5.crt
ENV NODE_EXTRA_CA_CERTS=/ac-mps-v5.crt
RUN update-ca-certificates
WORKDIR /src
COPY . .

WORKDIR "/src/src/Mps.OlivawCore.Api"
RUN dotnet restore "Mps.OlivawCore.Api.csproj"
RUN dotnet build "Mps.OlivawCore.Api.csproj" -c ${BUILD_CONFIG} -o /app/build

FROM build AS tests
ARG SKIP_UNITARY_TESTS
WORKDIR /src/tests/Mps.OlivawCore.Tests
RUN if [ "$SKIP_UNITARY_TESTS" = "false" ]; then \
    echo -e "========= STARTING TESTS =============\n" ; \
    \
    # Restore
    dotnet tool install dotnet-reportgenerator-globaltool --tool-path /dotnetglobaltools --version 5.5.3 && \
    dotnet add package coverlet.msbuild --version 10.0.1 && \
    dotnet restore "Mps.OlivawCore.Tests.csproj" && \
    \
    # Run
    dotnet test \
        /p:CollectCoverage=true \
        /p:Threshold=0 \
        /p:ThresholdType=line \
        /p:ThresholdStat=total \
        /p:CoverletOutputFormat=cobertura \
        /p:Include="[Mps.OlivawCore*]*" \
        /p:ExcludeByFile="**/*.generated.cs" \
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

FROM build AS pack
ARG BUILD_CONFIG
WORKDIR /src
RUN dotnet pack "OlivawCore.sln" -c ${BUILD_CONFIG} -o /app/packages

FROM scratch AS final
COPY --from=pack /app/packages ./packages
COPY --from=tests /src/tests/Mps.OlivawCore.Tests/results ./results
