# Korp_Teste_WagnerDaniel

Sistema de emissão de Notas Fiscais desenvolvido como desafio técnico para a **Korp ERP (Viasoft)**.

---

## Tecnologias utilizadas

| Camada | Tecnologia |
|---|---|
| Backend | C# .NET 8 — ASP.NET Core Web API |
| Banco de dados | PostgreSQL 16 |
| ORM | Entity Framework Core 8 + Npgsql |
| Validação | FluentValidation |
| UUID | UUIDNext (v7 database-friendly) |
| Containerização | Docker + Docker Compose |

---

## Como rodar o projeto

### Pré-requisitos

- [Docker](https://www.docker.com/) instalado
- [Docker Compose](https://docs.docker.com/compose/) instalado

### Subir o ambiente completo

```bash
git clone https://github.com/seu-usuario/Korp_Teste_SeuNome.git
cd Korp_Teste_SeuNome
cp .env.example .env
docker-compose up --build
```

Após o build, os serviços estarão disponíveis em:

| Serviço | URL |
|---|---|
| Frontend Angular | http://localhost:4200 |
| EstoqueApi | http://localhost:5001 |
| FaturamentoApi | http://localhost:5002 |
| Swagger — EstoqueApi | http://localhost:5001/swagger |
| Swagger — FaturamentoApi | http://localhost:5002/swagger |

> As migrations são aplicadas automaticamente ao subir os containers via `database-migrator`.

---

## Arquitetura

O sistema é estruturado em **dois microsserviços independentes**, cada um com seu próprio banco de dados PostgreSQL, seguindo o princípio de isolamento de dados entre serviços.

```
┌─────────────────────────────────────────────────────┐
│                  Angular (Frontend)                  │
│                   localhost:4200                     │
└──────────────────┬──────────────────────────────────┘
                   │ HTTP
        ┌──────────┴──────────┐
        │                     │
┌───────▼────────┐   ┌────────▼────────┐
│ FaturamentoApi │   │   EstoqueApi    │
│ localhost:5002 ├──►│ localhost:5001  │
│                │   │                 │
│ db_faturamento │   │   db_estoque    │
│  (PostgreSQL)  │   │  (PostgreSQL)   │
└────────────────┘   └─────────────────┘
```

O **FaturamentoApi** se comunica com o **EstoqueApi** via HTTP para:
- Validar se os produtos existem antes de criar uma nota fiscal
- Realizar a baixa de estoque ao imprimir uma nota fiscal

A comunicação entre serviços é protegida por um **secret compartilhado** via header `X-Internal-Secret`, impedindo que endpoints internos sejam acessados externamente.

---

## Estrutura de pastas — Clean Architecture

Ambos os serviços seguem a mesma estrutura em camadas:

```
Korp.[Servico].Api/              # Controllers, Middleware, Filters
Korp.[Servico].Application/      # UseCases, DTOs, Validators, Services (interfaces)
Korp.[Servico].Domain/           # Entities, Repositories (interfaces), Exceptions
Korp.[Servico].Infrastructure/   # DbContext, Repositories (impl), Clients, Migrations
```

**Regra de dependência:** as camadas internas nunca referenciam as externas.

```
Api → Infrastructure → Application → Domain
```

---

## Serviço de Estoque

Responsável pelo controle de produtos e saldos.

### Endpoints

| Método | Rota | Descrição | Acesso |
|---|---|---|---|
| GET | `/api/v1/produtos` | Listar todos os produtos | Público |
| GET | `/api/v1/produtos/{codigo}` | Buscar produto por código | Público |
| POST | `/api/v1/produtos` | Criar produto | Público |
| PUT | `/api/v1/produtos/{id}` | Atualizar descrição | Público |
| DELETE | `/api/v1/produtos/{id}` | Remover produto | Público |
| POST | `/api/v1/produtos/validar` | Validar existência de produtos | Interno |
| POST | `/api/v1/produtos/baixa` | Baixar estoque de múltiplos produtos | Interno |

### Entidade Produto

```
Id          — UUID v7 (gerado no backend)
Codigo      — string único, obrigatório
Descricao   — string, obrigatório
Saldo       — int, >= 0
CreatedAt   — datetime UTC
```

### Uso de LINQ

```csharp
// Ordenação na listagem
_context.Produtos.OrderByDescending(p => p.CreatedAt)

// Validação de código único
_context.Produtos.AnyAsync(p => p.Codigo == request.Codigo)

// Validação de existência em lote (uma única query)
_context.Produtos
    .Where(p => codigos.Contains(p.Codigo))
    .Select(p => p.Codigo)
    .ToListAsync()
```

### Controle de Concorrência — SELECT FOR UPDATE

No endpoint de baixa de estoque, é utilizado **controle de concorrência pessimista** via `SELECT FOR UPDATE` do PostgreSQL. Isso garante que dois processos simultâneos não consigam descontar o mesmo saldo ao mesmo tempo.

```csharp
// SQL raw com lock de linha
SELECT * FROM produtos WHERE "Codigo" = ANY(@codigos) ORDER BY "Codigo" FOR UPDATE
```

A ordenação por `"Codigo"` antes do lock é intencional — garante que múltiplas transações concorrentes sempre bloqueiem as linhas na mesma ordem, **evitando deadlock**.

O `FOR UPDATE` é executado dentro do `EfTransactionExecutor`, que utiliza `CreateExecutionStrategy()` do EF Core para compatibilidade com o `EnableRetryOnFailure`:

```csharp
var strategy = _context.Database.CreateExecutionStrategy();
await strategy.ExecuteAsync(async () =>
{
    await using var transaction = await _context.Database.BeginTransactionAsync();
    // operações com FOR UPDATE aqui
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
});
```

### Validação de baixa

O `BaixaEstoqueUseCase` valida em dois passos antes de alterar qualquer estado:

1. Verifica se todos os produtos existem — retorna 404 com lista dos não encontrados
2. Verifica se todos têm saldo suficiente — retorna 409 com detalhes de cada produto

```json
// Exemplo de resposta 409
{
  "status": 409,
  "detail": "Saldo insuficiente para: ABC-1 (disponível: 3, solicitado: 10) | ABC-2 (disponível: 1, solicitado: 5)"
}
```

---

## Serviço de Faturamento

Responsável pela gestão de notas fiscais.

### Endpoints

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/v1/notas` | Listar todas as notas fiscais |
| GET | `/api/v1/notas/{id}` | Buscar nota por ID |
| POST | `/api/v1/notas` | Criar nova nota fiscal |
| POST | `/api/v1/notas/{id}/imprimir` | Imprimir nota fiscal |

### Entidades

**NotaFiscal**
```
Id           — UUID v7 (gerado no backend)
Numero       — int sequencial (PostgreSQL Sequence)
Status       — enum: Aberta | Fechada
ImpressaoId  — UUID (idempotência)
CreatedAt    — datetime UTC
Itens        — coleção de ItemNotaFiscal
```

**ItemNotaFiscal**
```
Id             — UUID v7
NotaFiscalId   — FK para NotaFiscal
CodigoProduto  — string
Quantidade     — int
```

### Uso de LINQ

```csharp
// Numeração sequencial via Sequence do PostgreSQL
HasDefaultValueSql("nextval('notas_fiscais_numero_seq')")

// Listagem com itens ordenada
_context.NotasFiscais
    .Include(n => n.Itens)
    .OrderByDescending(n => n.Numero)
    .ToListAsync()

// Projeção dos itens no response
nota.Itens.Select(i => new ItemNotaResponse(i.Id, i.CodigoProduto, i.Quantidade)).ToList()
```

### Fluxo de impressão

```
POST /api/v1/notas/{id}/imprimir
    │
    ├── 1. Busca nota por ID → 404 se não encontrar
    ├── 2. Verifica ImpressaoId → retorna 200 sem reprocessar (idempotência)
    ├── 3. Valida status = Aberta → 409 se já fechada
    ├── 4. Chama EstoqueApi/baixa → 503 se indisponível
    ├── 5. Atualiza status para Fechada
    └── 6. Persiste ImpressaoId + SaveChanges
```

### Validação ao criar nota

Antes de persistir qualquer dado, o `CreateNotaUseCase` chama o **EstoqueApi** para validar se todos os produtos informados existem. Isso evita criar notas fiscais com produtos inválidos que só falhariam na impressão.

```csharp
var codigos = request.Itens.Select(i => i.CodigoProduto).Distinct().ToList();
await _estoqueService.ValidarProdutosAsync(codigos);
```

---

## Idempotência na impressão

O Angular gera um **UUID v4 único** (`ImpressaoId`) por intenção de impressão e o envia no body da requisição. O backend armazena esse UUID na nota fiscal após processá-la com sucesso.

Se a mesma requisição chegar novamente — por clique duplo, retry automático ou instabilidade de rede — o backend identifica o `ImpressaoId` já processado e retorna 200 sem reprocessar, sem descontar o estoque duas vezes.

```csharp
// Na entidade
public bool JaProcessada(Guid impressaoId) => ImpressaoId == impressaoId;

// No use case
if (nota.JaProcessada(impressaoId))
    return NotaFiscalResponse(...); // retorna sem reprocessar
```

---

## Tratamento de falhas

Ao chamar o EstoqueApi, o FaturamentoApi utiliza `HttpClient` com **timeout de 5 segundos**. Qualquer falha de conexão ou timeout lança `ServiceUnavailableException`, que o middleware converte em 503 com mensagem clara para o frontend.

```csharp
// EstoqueServiceClient
catch (HttpRequestException ex)
{
    throw new ServiceUnavailableException("Serviço de estoque indisponível. Tente novamente.", ex);
}
catch (TaskCanceledException)
{
    throw new ServiceUnavailableException("Serviço de estoque não respondeu a tempo.");
}
```

Para demonstrar o cenário de falha: basta derrubar o container do EstoqueApi e tentar imprimir uma nota — o FaturamentoApi retorna 503 e o Angular exibe o feedback ao usuário.

---

## Tratamento de erros e exceções

Ambas as APIs possuem um `ExceptionMiddleware` global que intercepta todas as exceções não tratadas e retorna respostas padronizadas:

| Exceção | Status HTTP | Situação |
|---|---|---|
| `ValidationException` | 400 | Falha na validação FluentValidation |
| `UnauthorizedException` | 401 | Acesso não autorizado |
| `NotFoundException` | 404 | Recurso não encontrado |
| `ConflictException` | 409 | Conflito (saldo insuficiente, nota já fechada) |
| `BusinessException` | 422 | Violação de regra de negócio |
| `ServiceUnavailableException` | 503 | Microsserviço dependente indisponível |
| `Exception` | 500 | Erro inesperado — logado via `ILogger` |

---

## Decisões técnicas

### UUID v7 como chave primária

Utilizei `UUIDNext` para gerar UUIDs v7 (database-friendly) no backend. UUIDs v7 são sequenciais no tempo, o que evita fragmentação de índice no PostgreSQL — diferente de UUIDs v4 que são completamente aleatórios e causam page splits frequentes em tabelas grandes.

### Sequence do PostgreSQL para numeração de notas

A numeração sequencial das notas fiscais utiliza uma **Sequence nativa do PostgreSQL** em vez de `MAX + 1` via LINQ. Isso garante que mesmo sob alta concorrência, dois processos simultâneos nunca gerem o mesmo número de nota.

```sql
CREATE SEQUENCE notas_fiscais_numero_seq START 1 INCREMENT 1;
```

### ITransactionExecutor

Para compatibilizar o `SELECT FOR UPDATE` (que exige transação manual) com o `EnableRetryOnFailure` do EF Core (que não permite transações manuais abertas fora do retry), foi criada a abstração `ITransactionExecutor` na camada de Domain. A implementação em Infrastructure usa `CreateExecutionStrategy()` para envolver a transação dentro do mecanismo de retry do EF Core.

### Persistência sem UnitOfWork

Após análise, o `IUnitOfWork` foi removido por ser apenas um wrapper desnecessário de `SaveChangesAsync`. Cada método de repositório que persiste dados chama `SaveChangesAsync` diretamente. O único caso que requer transação explícita (`BaixaEstoque`) utiliza o `ITransactionExecutor`.

### Comunicação interna protegida

Endpoints internos (baixa de estoque, validação de produtos) são protegidos pelo filtro `[InternalServiceOnly]`, que verifica o header `X-Internal-Secret`. O secret é compartilhado via variável de ambiente no Docker Compose e nunca exposto no código.

---

## Variáveis de ambiente

Crie um arquivo `.env` na raiz baseado no `.env.example`:

```env
# Configurações do Banco de Dados
DB_USER=admin
DB_PASSWORD=senhaaqui
DB_HOST=db-postgres
DB_PORT=5432

# Configurações das apis
DB_APIUSER=apiuser
DB_APIPASSWORD=senhaaqui
ASPNETCORE_ENV=Development

#Senha para comunicação interna das apis
INTERNAL_SHARED_SECRET=senhaaqui
```

---

## Autor

Desenvolvido por **wagner Daniel Malta Rodrigues**  
Contato: **wagnerdaniell@email.com**