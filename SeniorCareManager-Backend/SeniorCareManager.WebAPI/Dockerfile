FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

# Versão/commit do release, injetados pelo CI (release.yml). Default = dev.
ARG VERSION=0.0.0
ARG GIT_SHA=unknown

# Restaura/publica só o .csproj da API, nunca o .sln — o .sln também lista os projetos
# de teste (SeniorCareManager.UnitTests/IntegrationTests), que não fazem parte da
# imagem e, nesta etapa (antes do `COPY . .`), nem têm seus arquivos copiados ainda.
COPY SeniorCareManager.WebAPI.csproj ./
RUN dotnet restore SeniorCareManager.WebAPI.csproj

COPY . .
RUN dotnet publish SeniorCareManager.WebAPI.csproj -c Release -o /out \
    -p:Version=${VERSION} -p:SourceRevisionId=${GIT_SHA}

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app

RUN addgroup -S seniorcare && adduser -S seniorcare -G seniorcare

COPY --from=build /out .
RUN chown -R seniorcare:seniorcare /app

USER seniorcare

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=30s --timeout=10s --retries=5 --start-period=60s \
    CMD wget -qO- http://localhost:8080/health/ready || exit 1

ENTRYPOINT ["dotnet", "SeniorCareManager.WebAPI.dll"]
