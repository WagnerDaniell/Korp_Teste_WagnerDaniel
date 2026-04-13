# KorpNF — Documentação do Frontend

> **Stack:** Angular 21 · Standalone Components · Signals · CSS Puro · RxJS

---

## Visão Geral

Frontend do sistema de gestão de estoque e emissão de notas fiscais da Korp ERP (Viasoft). Consome dois microsserviços .NET 8 independentes e entrega duas telas principais com sidebar de navegação, modais reativos e tratamento de erros centralizado.

---

## Estrutura de Pastas

```
src/
├── app/
|   ├── shared/
│   │   └── components/
│   │       └── sidebar/             # Componente Global de Navegação
│   ├── core/
│   │   ├── interceptors/
│   │   │   └── error.interceptor.ts
│   │   ├── models/
│   │   │   ├── produto.model.ts
│   │   │   └── nota-fiscal.model.ts
│   │   └── services/
│   │       ├── produto.service.ts
│   │       └── nota-fiscal.service.ts
├── pages/
│   │   ├── produtos/
│   │   │   ├── produto-table/       # Componente de Apresentação 
│   │   │   ├── produto-modal/       # Gerenciamento de Formulário 
|   |   |   ├── produtos.component.html
|   |   |   ├── produtos.component.css
│   │   │   └── produtos.component.ts   # Componente Orquestrador
│   │   └── notas-fiscais/
│   │       ├── nota-table/          # Lista e Detalhe Inline
│   │       ├── nota-modal/          # FormArray dinâmico
|   |       ├── notas-fiscais.component.html
|   |       ├── notas-fiscais.component.css
│   │       └── notas-fiscais.component.ts
│   ├── app.config.ts
│   └── app.routes.ts
└── environments/
    └── environment.ts
```

---

## Configuração da Aplicação

### `app.config.ts`

Ponto de entrada da aplicação. Usa `ApplicationConfig` do Angular 21 sem NgModules.

```typescript
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([errorInterceptor])),
  ],
};
```

**Providers registrados:**

- `provideRouter(routes)` — configura o roteamento com lazy loading
- `provideHttpClient(withInterceptors([errorInterceptor]))` — registra o cliente HTTP com o interceptor global de erros usando a API funcional (`HttpInterceptorFn`)

---

### `app.routes.ts`

Define as duas rotas principais com lazy loading:

| Rota | Componente |
|------|-----------|
| `/produtos` | `ProdutosComponent` |
| `/notas-fiscais` | `NotasFiscaisComponent` |
| `/` | Redireciona para `/produtos` |

---

### `environments/environment.ts`

```typescript
export const environment = {
  production: false,
  estoqueApiUrl:     'http://localhost:5001/api/v1',
  faturamentoApiUrl: 'http://localhost:5002/api/v1',
};
```

| Variável | Destino |
|----------|---------|
| `estoqueApiUrl` | EstoqueApi — microsserviço de produtos |
| `faturamentoApiUrl` | FaturamentoApi — microsserviço de notas fiscais |

---

## Core

### Models

#### `produto.model.ts`

```typescript
export interface Produto {
  id: string;
  codigo: string;
  descricao: string;
  saldo: number;
  createdAt: string;
}

export interface CreateProdutoDto {
  codigo: string;
  descricao: string;
  saldo: number;
}

export interface UpdateProdutoDto {
  descricao: string;
}
```

**Observações:**
- `id` é UUID gerado pelo backend.
- `codigo` é imutável após a criação — o campo é desabilitado no formulário de edição.
- `UpdateProdutoDto` aceita somente `descricao`. Saldo é gerenciado exclusivamente pelo fluxo de impressão de notas.

---

#### `nota-fiscal.model.ts`

```typescript
export interface ItemNotaFiscal {
  id: string;
  codigoProduto: string;
  quantidade: number;
}

export interface NotaFiscal {
  id: string;
  numero: number;
  status: 'Aberta' | 'Fechada';
  createdAt: string;
  itens: ItemNotaFiscal[];
}

export interface CreateItemNotaDto {
  codigoProduto: string;
  quantidade: number;
}

export interface CreateNotaFiscalDto {
  itens: CreateItemNotaDto[];
}

export interface ImprimirNotaDto {
  impressaoId: string; // UUID v4 gerado no frontend para idempotência
}
```

**Observações:**
- `status` é uma union type estrita: `'Aberta' | 'Fechada'`. Notas abertas podem ser impressas; após a impressão passam a `'Fechada'`.
- `impressaoId` é um UUID v4 gerado no frontend a cada intenção de impressão, garantindo idempotência no backend (reenvio da mesma requisição não duplica a operação).

---

### Services

#### `produto.service.ts`

Consome a `EstoqueApi` (`localhost:5001/api/v1/produtos`).

| Método | HTTP | Endpoint | Descrição |
|--------|------|----------|-----------|
| `listar()` | GET | `/produtos` | Retorna todos os produtos |
| `buscarPorCodigo(codigo)` | GET | `/produtos/{codigo}` | Busca produto por código |
| `criar(dto)` | POST | `/produtos` | Cria novo produto com saldo inicial |
| `atualizar(id, dto)` | PUT | `/produtos/{id}` | Atualiza descrição do produto |
| `remover(id)` | DELETE | `/produtos/{id}` | Remove produto |

**Injeção:** usa `inject(HttpClient)` — padrão Angular 21 sem construtor.

---

#### `nota-fiscal.service.ts`

Consome a `FaturamentoApi` (`localhost:5002/api/v1/notas`).

| Método | HTTP | Endpoint | Descrição |
|--------|------|----------|-----------|
| `listar()` | GET | `/notas` | Retorna todas as notas fiscais |
| `buscarPorId(id)` | GET | `/notas/{id}` | Busca nota com itens detalhados |
| `criar(dto)` | POST | `/notas` | Cria nova nota fiscal com itens |
| `imprimir(id, dto)` | POST | `/notas/{id}/imprimir` | Imprime nota, debita saldo do estoque e fecha |

---

### Interceptor

#### `error.interceptor.ts`

Interceptor funcional (`HttpInterceptorFn`) registrado globalmente via `withInterceptors`. Captura todos os erros HTTP e os normaliza em um objeto `{ status, mensagem }` propagado via `throwError`.

| Status HTTP | Mensagem exibida |
|-------------|-----------------|
| `0` | Não foi possível conectar ao servidor |
| `400` | `error.detail` ou "Dados inválidos" |
| `404` | `error.detail` ou "Recurso não encontrado" |
| `409` | `error.detail` ou "Conflito: operação não permitida" |
| `503` | Serviço de estoque indisponível. Tente novamente em instantes |
| `500` | Erro interno no servidor |
| Outros | Erro inesperado. Tente novamente |

**Fluxo de consumo nos componentes:**
```typescript
this.service.listar().subscribe({
  next:  d => { /* usa d */ },
  error: e => { this.erro.set(e.mensagem); } // e.mensagem já é string legível
});
```

---

## Pages

### Visual & Design System

Ambas as páginas compartilham o mesmo design system via variáveis CSS globais definidas em `styles.css`.

**Variáveis CSS:**

```css
:root {
  --bg:             #f0f2f5;   /* fundo da aplicação */
  --surface:        #ffffff;   /* cards e modais */
  --surface2:       #f7f8fa;   /* header da tabela, hover */

  --sidebar:        #13161c;   /* fundo da sidebar */
  --sidebar-border: #1f2330;
  --sidebar-txt:    #e6e8ec;
  --sidebar-muted:  #5a6175;
  --sidebar-hover:  #1e2230;

  --accent:         ##274b5b; 
  --accent-dk:      ##274b5b;

  --txt:            #111827;
  --txt2:           #374151;
  --muted:          #6b7280;

  --border:         #e5e7eb;
  --border2:        #d1d5db;
}
```

**Tipografia:** DM Sans (body) + DM Mono (chips/códigos) — importadas via Google Fonts.

**Layout:** cada page é autossuficiente — contém sidebar + conteúdo dentro do próprio template, sem dependência de um layout-shell externo.

---

### `ProdutosComponent`

**Arquivo:** `pages/produtos/`

**Responsabilidade:** CRUD completo de produtos com tabela, modais de criação/edição e confirmação de remoção.

#### Signals de estado

| Signal | Tipo | Descrição |
|--------|------|-----------|
| `produtos` | `signal<Produto[]>` | Lista carregada da API |
| `carregando` | `signal<boolean>` | Exibe spinner na tabela |
| `erro` | `signal<string>` | Alerta de erro na página |
| `erroModal` | `signal<string>` | Alerta de erro dentro do modal |
| `salvando` | `signal<boolean>` | Desabilita botão de submit |
| `removendoId` | `signal<string \| null>` | ID do produto em remoção (spinner inline) |
| `modalAberto` | `signal<boolean>` | Controla visibilidade do modal criar/editar |
| `editando` | `signal<Produto \| null>` | `null` = criar, objeto = editar |
| `modalRemocao` | `signal<boolean>` | Controla modal de confirmação |
| `produtoParaRemover` | `signal<Produto \| null>` | Produto alvo da remoção |

#### Formulário reativo

```typescript
form = this.fb.group({
  codigo:    ['', Validators.required],       // desabilitado no modo edição
  descricao: ['', Validators.required],
  saldo:     [0,  [Validators.required, Validators.min(0)]], // desabilitado no modo edição
});
```

- No modo **editar**: `codigo` e `saldo` são desabilitados com `form.get('campo')?.disable()`. Os valores são recuperados com `getRawValue()` para incluir campos desabilitados quando necessário.
- No modo **criar**: todos os campos habilitados.

#### Fluxo de criação

1. Usuário clica em **+ Novo Produto**
2. `abrirCriar()` reseta o form e abre o modal
3. Submit chama `salvar()` → `ProdutoService.criar(dto)`
4. Sucesso: fecha modal, recarrega lista
5. Erro: exibe `erroModal` dentro do modal

#### Fluxo de edição

1. Usuário clica em **✎** na linha do produto
2. `abrirEditar(p)` preenche `descricao`, desabilita `codigo` e `saldo`, abre modal
3. Submit chama `salvar()` → `ProdutoService.atualizar(id, { descricao })`
4. Sucesso: fecha modal, recarrega lista

#### Fluxo de remoção

1. Usuário clica em **✕** na linha do produto
2. `confirmarRemover(p)` abre modal de confirmação
3. Usuário confirma → `remover()` → `ProdutoService.remover(id)`
4. Spinner inline no botão da linha durante a operação

#### Badge de saldo

```typescript
saldoBadge(s: number): string {
  if (s === 0) return 'badge badge-zero';  // cinza
  if (s <= 5)  return 'badge badge-low';   // amarelo
  return 'badge badge-ok';                  // verde
}
```

---

### `NotasFiscaisComponent`

**Arquivo:** `pages/notas-fiscais/`

**Responsabilidade:** listagem de notas fiscais, criação com FormArray dinâmico de itens, detalhe inline e impressão com idempotência.

#### Signals de estado

| Signal | Tipo | Descrição |
|--------|------|-----------|
| `notas` | `signal<NotaFiscal[]>` | Lista da API |
| `produtos` | `signal<Produto[]>` | Produtos carregados ao abrir o modal |
| `carregando` | `signal<boolean>` | Spinner na tabela principal |
| `carregandoProdutos` | `signal<boolean>` | Estado de carregamento do select de produtos |
| `carregandoDetalhe` | `signal<boolean>` | Spinner no detalhe inline |
| `salvando` | `signal<boolean>` | Desabilita submit do modal |
| `imprimindoId` | `signal<string \| null>` | ID da nota em impressão |
| `erro` | `signal<string>` | Alerta de erro na página |
| `erroModal` | `signal<string>` | Alerta de erro no modal |
| `modalAberto` | `signal<boolean>` | Modal de nova nota |
| `feedbackOk` | `signal<boolean>` | Modal de sucesso após impressão |
| `detalheId` | `signal<string \| null>` | ID da nota com detalhe expandido |
| `notaDetalhe` | `signal<NotaFiscal \| null>` | Dados carregados do detalhe |

#### Formulário com FormArray

```typescript
form = this.fb.group({
  itens: this.fb.array([this.novoItem()])
});

novoItem() {
  return this.fb.group({
    codigoProduto: ['', Validators.required],
    quantidade:    [1,  [Validators.required, Validators.min(1)]],
  });
}
```

- Usuário pode adicionar itens com **+ Item** (`addItem()`) ou remover com **✕** (`removeItem(i)`)
- Mínimo de 1 item (botão remover oculto quando há apenas um)
- Select de produtos exibe: `CODIGO — Descrição (saldo: N)` para apoiar a decisão do usuário

#### Fluxo de criação de nota

1. Usuário clica em **+ Nova Nota**
2. `abrirCriar()`: reseta o FormArray com 1 item vazio, abre modal e carrega lista de produtos em paralelo
3. Submit → `NotaFiscalService.criar({ itens })` com array de `{ codigoProduto, quantidade }`
4. Sucesso: fecha modal, recarrega lista

#### Fluxo de impressão (com idempotência)

1. Usuário clica em **⎙ Imprimir** em uma nota `Aberta`
2. Frontend gera um `impressaoId` (UUID v4) único por intenção de impressão
3. `NotaFiscalService.imprimir(nota.id, { impressaoId })` via POST
4. Backend consome saldo do EstoqueApi e fecha a nota
5. Sucesso: exibe modal de feedback verde com ✓, recarrega lista
6. Erro `503`: EstoqueApi indisponível — alerta visível na página
7. Erro `409`: saldo insuficiente — alerta com `detail` do backend

**Por que UUID no frontend?** Garante que reenvios da mesma requisição (ex: duplo clique, retry de rede) não causem dupla dedução de estoque. O backend usa o `impressaoId` como chave de idempotência.

#### Detalhe inline

1. Usuário clica em **◉** na linha da nota
2. `toggleDetalhe(nota)`: se já expandido fecha; caso contrário faz GET em `buscarPorId(nota.id)` e exibe os itens em tabela aninhada
3. `fecharDetalhe()` remove o detalhe da view

#### Badge de status

```typescript
statusBadge(s: string) {
  return s === 'Aberta' ? 'badge badge-open' : 'badge badge-closed';
}
// badge-open:   fundo verde claro, texto verde escuro
// badge-closed: fundo cinza claro, texto cinza
```

Notas `Fechada` não exibem o botão Imprimir — exibem a tag estática `Impressa`.

---

## Bibliotecas Auxiliares

Além do Core do Angular, foram integradas bibliotecas específicas para garantir robustez e padronização:

- **uuid:** Utilizada para geração de UUID v4 no frontend, servindo como chave de idempotência nas requisições de impressão.
- **date-fns:** Utilizada para formatação de datas e horas seguindo o padrão brasileiro (`pt-BR`), garantindo consistência visual em diferentes navegadores.

---

## Padrões e Convenções

### Sem NgModules

Todos os componentes são `standalone: true`. Imports declarados diretamente no decorator:

```typescript
@Component({
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, RouterLinkActive],
  ...
})
```

### Signals para estado local

Nenhum `BehaviorSubject` ou propriedades mutáveis simples — todo estado reativo usa `signal()` do Angular Signals API. Isso permite reatividade fina sem `ChangeDetectionStrategy.OnPush` explícito.

### `inject()` ao invés de construtor

```typescript
// ✅ padrão usado no projeto
private service = inject(ProdutoService);
private fb      = inject(FormBuilder);

// ❌ não usado
constructor(private service: ProdutoService) {}
```

### `@if` / `@for` (template blocks)

Usa a nova sintaxe de template do Angular 17+ em todos os templates:

```html
@if (carregando()) {
  <div class="spinner"></div>
} @else if (lista().length === 0) {
  <div class="empty-state">...</div>
} @else {
  <table>...</table>
}

@for (item of lista(); track item.id) {
  <tr>...</tr>
}
```

### CSS por componente

Cada page tem seu próprio arquivo `.css` completo — sem CSS-in-JS, sem utility classes externas. O arquivo `styles.css` global define apenas variáveis CSS e reset. Os componentes consomem as variáveis via `var(--nome)`.

---

## Como Rodar

```bash
# Instalar dependências
npm install

# Rodar em desenvolvimento
ng serve

# Apis necessárias:
# EstoqueApi    → http://localhost:5001
# FaturamentoApi → http://localhost:5002
```

A aplicação sobe em `http://localhost:4200` e redireciona automaticamente para `/produtos`.