#!/bin/bash
set -e

echo "--- Instalando Ferramentas ---"
export PATH="$PATH:/root/.dotnet/tools"
dotnet tool install --global dotnet-ef || echo "dotnet-ef já instalado"

# Entra na raiz do mapeamento
cd /src

echo "--- Restaurando Dependências ---"
# Note que adicionei a subpasta repetida no caminho
dotnet restore services/Korp.Estoque.Api/Korp.Estoque.Api/Korp.Estoque.Api.csproj
dotnet restore services/Korp.Faturamento.Api/Korp.Faturamento.Api/Korp.Faturamento.Api.csproj

echo "--- Rodando Migrations: ESTOQUE ---"
dotnet ef database update \
    --project services/Korp.Estoque.Api/Korp.Estoque.Infrastructure/Korp.Estoque.Infrastructure.csproj \
    --startup-project services/Korp.Estoque.Api/Korp.Estoque.Api/Korp.Estoque.Api.csproj \
    --connection "$CONN_ESTOQUE"

echo "--- Rodando Migrations: FATURAMENTO ---"
dotnet ef database update \
    --project services/Korp.Faturamento.Api/Korp.Faturamento.Infrastructure/Korp.Faturamento.Infrastructure.csproj \
    --startup-project services/Korp.Faturamento.Api/Korp.Faturamento.Api/Korp.Faturamento.Api.csproj \
    --connection "$CONN_FATURAMENTO"

echo "--- Migrations concluídas com sucesso! ---"