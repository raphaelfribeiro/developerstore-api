# DeveloperStore API

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-13-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)

![Entity Framework](https://img.shields.io/badge/Entity_Framework-Core_8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![MediatR](https://img.shields.io/badge/MediatR-12.4-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![AutoMapper](https://img.shields.io/badge/AutoMapper-15.1.3-BE3C28?style=for-the-badge&logo=dotnet&logoColor=white)
![FluentValidation](https://img.shields.io/badge/FluentValidation-11.x-00B4AB?style=for-the-badge&logo=dotnet&logoColor=white)

![xUnit](https://img.shields.io/badge/xUnit-2.9-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Coverage](https://img.shields.io/badge/Coverage-91%25-brightgreen?style=for-the-badge&logo=dotnet&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI_3.0-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)
![Serilog](https://img.shields.io/badge/Serilog-8.x-EF5B25?style=for-the-badge&logo=dotnet&logoColor=white)

</div>

---

<div align="center">

> **Desafio Técnico — Developer Evaluation**
>
> API RESTful para gestão de vendas, carrinhos e produtos com arquitetura DDD e CQRS.

</div>

---

## Índice

- [Visão Geral](#visão-geral)
- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Regras de Negócio](#regras-de-negócio)
- [Como Executar](#como-executar)
- [Endpoints](#endpoints)
- [Testes](#testes)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Decisões Técnicas](#decisões-técnicas)

---

## Visão Geral

O **DeveloperStore API** é uma API RESTful completa para gestão de um sistema de vendas, implementando:

- **CRUD completo** de Vendas, Carrinhos e Produtos
- **Regras de negócio de desconto** por quantidade de itens
- **Eventos de domínio** publicados a cada operação relevante
- **Autenticação JWT** em todos os endpoints protegidos
- **Paginação e filtros** em todos os endpoints de listagem
- **~91% de cobertura** nos testes unitários (208 testes)

---

## Tecnologias

### Backend
| Tecnologia | Versão | Uso |
|---|---|---|
| ![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet) | 8.0 | Framework principal |
| ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-13-4169E1?style=flat&logo=postgresql) | 13 | Banco de dados relacional |
| ![EF Core](https://img.shields.io/badge/EF_Core-8.0-512BD4?style=flat&logo=dotnet) | 8.0 | ORM + Migrations |
| ![MediatR](https://img.shields.io/badge/MediatR-12.4-512BD4?style=flat&logo=dotnet) | 12.4 | CQRS / Mediator pattern |
| ![AutoMapper](https://img.shields.io/badge/AutoMapper-15.1.3-BE3C28?style=flat&logo=dotnet) | 15.1.3 | Mapeamento de objetos |
| ![FluentValidation](https://img.shields.io/badge/FluentValidation-11.x-00B4AB?style=flat&logo=dotnet) | 11.x | Validação de comandos |
| ![Serilog](https://img.shields.io/badge/Serilog-8.x-EF5B25?style=flat&logo=dotnet) | 8.x | Logging estruturado |
| ![Polly](https://img.shields.io/badge/Polly-8.x-FF6B35?style=flat&logo=dotnet) | 8.x | Retry policy nos eventos |
| ![Swagger](https://img.shields.io/badge/Swagger-OpenAPI_3.0-85EA2D?style=flat&logo=swagger&logoColor=black) | 6.x | Documentação da API |

### Testes
| Tecnologia | Versão | Uso |
|---|---|---|
| ![xUnit](https://img.shields.io/badge/xUnit-2.9-512BD4?style=flat&logo=dotnet) | 2.9 | Framework de testes (Unit, Integration, Functional) |
| ![Bogus](https://img.shields.io/badge/Bogus-35.6-512BD4?style=flat&logo=dotnet) | 35.6 | Geração de dados falsos (Unit) |
| ![NSubstitute](https://img.shields.io/badge/NSubstitute-5.1-512BD4?style=flat&logo=dotnet) | 5.1 | Mocking (Unit) |
| ![FluentAssertions](https://img.shields.io/badge/FluentAssertions-6.12-00B4AB?style=flat&logo=dotnet) | 6.12 | Asserções expressivas (todos os projetos) |
| ![MockQueryable](https://img.shields.io/badge/MockQueryable-7.0-512BD4?style=flat&logo=dotnet) | 7.0 | Mock de IQueryable async (Unit) |
| ![Testcontainers](https://img.shields.io/badge/Testcontainers-3.10-2496ED?style=flat&logo=docker) | 3.10 | PostgreSQL real nos testes Integration e Functional |

### Infraestrutura
| Tecnologia | Versão | Uso |
|---|---|---|
| ![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat&logo=docker) | - | Containerização |
| ![Redis](https://img.shields.io/badge/Redis-7.4-DC382D?style=flat&logo=redis&logoColor=white) | 7.4 | Cache |
| ![MongoDB](https://img.shields.io/badge/MongoDB-8.0-47A248?style=flat&logo=mongodb&logoColor=white) | 8.0 | NoSQL |

---

## Arquitetura

O projeto segue **Domain-Driven Design (DDD)** com **CQRS** via MediatR:

```
src/
├── Ambev.DeveloperEvaluation.Domain        # Entidades, Eventos, Repositórios, Validators
├── Ambev.DeveloperEvaluation.Application   # Commands, Queries, Handlers, Profiles
├── Ambev.DeveloperEvaluation.ORM           # DbContext, Repositórios, Migrations
├── Ambev.DeveloperEvaluation.WebApi        # Controllers, Middleware, Program.cs
└── Ambev.DeveloperEvaluation.Common        # Utilitários compartilhados

tests/
├── Ambev.DeveloperEvaluation.Unit          # Testes unitários (~91% cobertura, 208 testes)
├── Ambev.DeveloperEvaluation.Integration   # Testes de integração com Testcontainers
└── Ambev.DeveloperEvaluation.Functional    # Testes funcionais
```

### Fluxo de uma requisição

```
HTTP Request
     │
     ▼
Controller  (WebApi)
     │
     ▼
MediatR  →  Command / Query
     │
     ▼
Handler  (Application)
     │                         │  (operações de escrita)
     ▼                         ▼
Repository  (ORM)        Domain Events
     │                         │
     ▼                         ▼
EF Core → PostgreSQL    EventPublisher
                          (Polly retry 3×)
```

### Padrões utilizados

| Padrão | Implementação |
|---|---|
| **CQRS** | Commands e Queries separados via MediatR |
| **Repository Pattern** | Abstração do acesso a dados |
| **Domain Events** | `SaleCreatedEvent`, `SaleModifiedEvent`, `SaleCancelledEvent`, `ItemCancelledEvent` |
| **Specification Pattern** | `ActiveUserSpecification` |
| **External Identity** | Cart referencia Product apenas pelo Id (domínios desacoplados) |
| **Retry Pattern** | Polly com 3 tentativas e exponential backoff nos eventos |
| **GitFlow** | Feature branches a partir de `develop`; merge `--no-ff`; tipos: `feature/`, `fix/`, `docs/`, `refactor/`, `chore/` |
| **Conventional Commits** | Prefixos semânticos: `feat:`, `fix:`, `test:`, `refactor:`, `chore:`, `docs:` — histórico auditável e compatível com ferramentas de changelog |

---

## Regras de Negócio

### Descontos por quantidade de itens idênticos

| Quantidade | Desconto | Exemplo (R$ 100/un) |
|---|---|---|
| Abaixo de 4 itens | Sem desconto | 3 × R$100 = **R$300** |
| 4 a 9 itens | **10%** | 5 × R$100 × 0.90 = **R$450** |
| 10 a 20 itens | **20%** | 10 × R$100 × 0.80 = **R$800** |
| Acima de 20 itens | ❌ Não permitido | — |

### Eventos de domínio publicados

| Evento | Quando é publicado |
|---|---|
| `SaleCreatedEvent` | Ao criar uma venda |
| `SaleModifiedEvent` | Ao atualizar uma venda |
| `SaleCancelledEvent` | Ao deletar/cancelar uma venda |
| `ItemCancelledEvent` | Ao cancelar um item da venda |

---

## Como Executar

### Pré-requisitos

![Docker](https://img.shields.io/badge/Docker_Desktop-Required-2496ED?style=flat&logo=docker)
![.NET](https://img.shields.io/badge/.NET_8_SDK-Optional-512BD4?style=flat&logo=dotnet)

### 1. Clone o repositório

```bash
git clone https://github.com/raphaelfernandesribeiro/desafio-tecnico.git
cd desafio-tecnico/template/backend
# O código-fonte está em template/backend — estrutura pré-configurada fornecida pelo desafio
```

### 2. Suba os containers

```bash
docker-compose up --build -d
```

> As migrations do EF Core são aplicadas automaticamente no startup — nenhum comando `dotnet ef database update` é necessário.

| Serviço | URL |
|---|---|
| **API / Swagger UI** | http://localhost:8080 (Swagger na raiz `/`) |
| **Health Check** | http://localhost:8080/health |
| **PostgreSQL** | localhost:5432 |
| **MongoDB** | localhost:27017 |
| **Redis** | localhost:6379 |

### 3. Verifique os logs

```bash
docker logs ambev_developer_evaluation_webapi -f
```

### 4. Pare os containers

```bash
docker-compose down
```

### Variáveis de ambiente

O projeto vem com valores padrão funcionais. Para personalizar, copie o arquivo de exemplo e edite conforme seu ambiente:

```bash
cp .env.example .env
# edite .env com suas credenciais
docker-compose up --build -d
```

| Variável | Padrão | Descrição |
|---|---|---|
| `POSTGRES_DB` | `developer_evaluation` | Nome do banco PostgreSQL |
| `POSTGRES_USER` | `developer` | Usuário do PostgreSQL |
| `POSTGRES_PASSWORD` | `ev@luAt10n` | Senha do PostgreSQL |
| `MONGO_USER` | `developer` | Usuário do MongoDB |
| `MONGO_PASSWORD` | `ev@luAt10n` | Senha do MongoDB |
| `REDIS_PASSWORD` | `ev@luAt10n` | Senha do Redis |
| `JWT_SECRET_KEY` | `YourSuperSecretKey...` | Chave de assinatura JWT (mín. 32 chars) ⚠️ |
| `JWT_ISSUER` | `AmbevDeveloperEvaluation` | Issuer do token JWT |
| `JWT_AUDIENCE` | `AmbevDeveloperEvaluationUsers` | Audience do token JWT |
| `JWT_EXPIRY_MINUTES` | `60` | Validade do token em minutos |

> ⚠️ **Produção:** Substitua `JWT_SECRET_KEY` por uma chave forte de pelo menos 32 caracteres gerada aleatoriamente. O app valida o tamanho mínimo na inicialização e rejeita chaves curtas.

> Para rodar sem Docker, copie `appsettings.json`, ajuste `DefaultConnection` para `Host=localhost` e execute `dotnet run` na pasta `src/Ambev.DeveloperEvaluation.WebApi`.

---

## Endpoints

### 🔐 Auth

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `POST` | `/api/auth/login` | Autenticar usuário (campo: `username`) | ❌ |

### 👤 Users

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `POST` | `/api/users` | Criar usuário (schema aninhado: `name`, `address`) | ❌ |
| `GET` | `/api/users` | Listar usuários (paginado: `_page`, `_size`, `_order`) | ✅ |
| `GET` | `/api/users/{id}` | Buscar usuário por ID | ✅ |
| `PUT` | `/api/users/{id}` | Atualizar perfil (owner ou Admin) | ✅ |
| `PATCH` | `/api/users/{id}/role` | Alterar role/status do usuário | ✅ Admin |
| `DELETE` | `/api/users/{id}` | Deletar usuário | ✅ |

### 📦 Products

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `POST` | `/api/products` | Criar produto | ✅ |
| `GET` | `/api/products` | Listar produtos (paginado) | ✅ |
| `GET` | `/api/products/{id}` | Buscar produto por ID | ✅ |
| `GET` | `/api/products/categories` | Listar categorias | ✅ |
| `GET` | `/api/products/category/{category}` | Listar por categoria | ✅ |
| `PUT` | `/api/products/{id}` | Atualizar produto | ✅ |
| `DELETE` | `/api/products/{id}` | Deletar produto | ✅ |

### 🛒 Carts

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `POST` | `/api/carts` | Criar carrinho | ✅ |
| `GET` | `/api/carts` | Listar carrinhos (paginado) | ✅ |
| `GET` | `/api/carts/{id}` | Buscar carrinho por ID | ✅ |
| `PUT` | `/api/carts/{id}` | Atualizar carrinho | ✅ |
| `DELETE` | `/api/carts/{id}` | Deletar carrinho | ✅ |

### 💰 Sales

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `POST` | `/api/sales` | Criar venda | ✅ |
| `GET` | `/api/sales` | Listar vendas (paginado) | ✅ |
| `GET` | `/api/sales/{id}` | Buscar venda por ID | ✅ |
| `PUT` | `/api/sales/{id}` | Atualizar venda | ✅ |
| `PATCH` | `/api/sales/{id}/items/{itemId}/cancel` | Cancelar item | ✅ |
| `DELETE` | `/api/sales/{id}` | Cancelar e deletar venda | ✅ |

### Formato de resposta padrão (sucesso)

```json
{
  "data": { },
  "success": true,
  "message": "Operation successful"
}
```

### Formato de resposta de erro

```json
{
  "type": "ValidationError",
  "error": "Invalid request",
  "detail": "Username must be between 3 and 50 characters"
}
```

### Paginação e Ordenação

Parâmetros de query com prefixo `_`:
- `_page` (padrão: 1), `_size` (padrão: 10), `_order` (ex: `username desc, email asc`)

```json
{
  "currentPage": 1,
  "totalPages": 3,
  "totalItems": 30,
  "data": []
}
```

### Filtros

Os endpoints de listagem suportam três tipos de filtro, combináveis com `&` (AND):

**Valor exato por campo**
```
GET /api/products?category=electronics
```

**Correspondência parcial — wildcard `*`**
```
GET /api/products?title=Samsung*        # começa com
GET /api/products?category=*clothing    # termina com
```

**Faixa numérica e de data — prefixos `_min` / `_max`**
```
GET /api/products?_minPrice=50&_maxPrice=200
GET /api/carts?_minDate=2024-01-01&_maxDate=2024-12-31
GET /api/sales?_minDate=2024-06-01
```

**Exemplo combinado**
```
GET /api/products?category=electronics&_minPrice=100&title=Samsung*
```

---

## Testes

> **Estratégia:** Testes de **integração** verificam endpoints HTTP completos com banco real (Testcontainers) — validam contrato da API, autenticação e regras de negócio via HTTP. Testes **funcionais** rodam num container PostgreSQL **isolado** (sem estado compartilhado com integração) e focam exclusivamente nas regras de negócio críticas do domínio (tiers de desconto, limites de quantidade, cancelamento de item), garantindo que mudanças no código nunca quebrem a lógica central independentemente de detalhes de transporte.

### Testes Unitários

```bash
cd template/backend
dotnet test Ambev.DeveloperEvaluation.sln --filter "FullyQualifiedName~Unit"
```

<div align="center">

![Coverage](https://img.shields.io/badge/Line_Coverage-91%25-brightgreen?style=for-the-badge)
![Tests](https://img.shields.io/badge/Tests-208_passing-brightgreen?style=for-the-badge)
![Failures](https://img.shields.io/badge/Failures-0-brightgreen?style=for-the-badge)

</div>

### Gerar relatório de cobertura

```powershell
# Instalar ferramenta (apenas uma vez)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Executar no Windows
.\coverage-report.ps1

# Executar no Linux/Mac
./coverage-report.sh
```

Relatório HTML gerado em `coverage-report/index.html`.

### Testes de Integração

```bash
dotnet test Ambev.DeveloperEvaluation.sln --filter "FullyQualifiedName~Integration"
```

<div align="center">

![Tests](https://img.shields.io/badge/Integration_Tests-40_passing-brightgreen?style=for-the-badge)
![Failures](https://img.shields.io/badge/Failures-0-brightgreen?style=for-the-badge)

</div>

| Suite | Testes | Cobertura |
|---|---|---|
| Auth / Users | 7 | Registro, autenticação, JWT, 401 |
| Users (ownership + admin role) | 8 | PUT ownership check (200/403/401), PUT role change (owner→403, admin→200), PATCH role (200/403/401/404) |
| Products | 7 | CRUD completo, listagem paginada, 401 |
| Sales | 8 | CRUD, regras de desconto (10%/20%), cancelamento de item, 401 |
| Carts | 7 | CRUD completo, listagem paginada, 401 |
| E2E | 3 | Fluxo completo: produto → carrinho → venda com desconto |

> ⚠️ Docker deve estar rodando — os testes sobem automaticamente um container PostgreSQL via **Testcontainers**.
> Um único container é compartilhado entre todas as suites (`ICollectionFixture`) — inicialização rápida (~15s).

### Testes Funcionais

```bash
dotnet test Ambev.DeveloperEvaluation.sln --filter "FullyQualifiedName~Functional"
```

<div align="center">

![Tests](https://img.shields.io/badge/Functional_Tests-3_passing-brightgreen?style=for-the-badge)
![Failures](https://img.shields.io/badge/Failures-0-brightgreen?style=for-the-badge)

</div>

| Cenário | O que verifica |
|---|---|
| Discount tiers across all brackets | Sem desconto (3 itens), 10% (5 itens), 20% (15 itens) — todos em um único fluxo autenticado |
| Maximum quantity enforcement | Venda com 21 itens é rejeitada com 400 (regra de negócio crítica) |
| Item cancellation state | Cancelar item via PATCH marca o item como cancelado sem cancelar a venda |

> ⚠️ Docker deve estar rodando — os testes funcionais sobem um container PostgreSQL **isolado** do container de integração.

---

## Estrutura do Projeto

```
template/backend/
├── src/
│   ├── Ambev.DeveloperEvaluation.Domain/
│   │   ├── Entities/          # Sale, SaleItem, Cart, CartItem, Product, User
│   │   ├── Events/            # SaleCreatedEvent, SaleModifiedEvent, etc.
│   │   ├── Repositories/      # ISaleRepository, ICartRepository, IProductRepository
│   │   ├── Validation/        # SaleValidator, CartValidator, ProductValidator
│   │   └── Specifications/    # ActiveUserSpecification
│   ├── Ambev.DeveloperEvaluation.Application/
│   │   ├── Sales/             # Create, Get, GetList, Update, Delete, CancelItem
│   │   ├── Carts/             # Create, Get, GetList, Update, Delete
│   │   ├── Products/          # Create, Get, GetList, Categories, Update, Delete
│   │   ├── Users/             # Create, Get, GetList, Update, PatchUserRole, Delete
│   │   └── Auth/              # AuthenticateUser (login por username)
│   ├── Ambev.DeveloperEvaluation.ORM/
│   │   ├── Repositories/      # SaleRepository, CartRepository, ProductRepository
│   │   ├── Services/          # LoggingEventPublisher (Polly retry)
│   │   └── Migrations/        # InitialMigrations, AddSaleCartProduct, AddUserProfileFields
│   └── Ambev.DeveloperEvaluation.WebApi/
│       ├── Features/          # Controllers, Requests, Responses, Profiles
│       └── Middleware/        # ValidationExceptionMiddleware
└── tests/
    ├── Ambev.DeveloperEvaluation.Unit/
    │   ├── Domain/            # Entity tests, Validator tests, Event tests
    │   ├── Application/       # Handler tests, Mapping tests
    │   └── Infrastructure/    # LoggingEventPublisher tests
    ├── Ambev.DeveloperEvaluation.Integration/
    │   ├── Fixtures/          # IntegrationTestFactory (Testcontainers), BaseIntegrationTest
    │   └── Features/          # Auth, Users, Products, Sales, Carts, E2E — 40 testes
    └── Ambev.DeveloperEvaluation.Functional/
        ├── Fixtures/          # FunctionalTestFactory (container isolado), BaseFunctionalTest
        └── Scenarios/         # SalesBusinessRulesTests — 3 cenários de negócio
```

---

## Pendências e Próximos Passos

O que seria evoluído com mais tempo:

| Item | Descrição |
|---|---|
| **Message Broker real** | Substituir o `LoggingEventPublisher` por integração com RabbitMQ ou Azure Service Bus via Rebus, mantendo o Polly retry já implementado |
| **Testes unitários de Cart** | Handlers e validators do domínio Cart cobertos da mesma forma que Sale e Product |
| ~~**Testes funcionais**~~ | ✅ Implementado: `Ambev.DeveloperEvaluation.Functional` com 3 cenários de negócio (tiers de desconto, limite de quantidade, cancelamento de item) |
| **API versioning** | Versionamento de rotas (`/api/v1/`) para suportar evolução sem quebrar clientes |
| **Rate limiting** | Throttling por IP/usuário nos endpoints públicos (auth, criação de usuário) |
| **CI/CD pipeline** | GitHub Actions definido em `.github/workflows/ci.yml` com build, testes, relatório de cobertura e push de imagem Docker |

---

## Importar no Postman

1. Abra o **Postman**
2. Clique em **Import**
3. Selecione `template/backend/docs/DeveloperStore.postman_collection.json`
4. Execute na ordem: **Auth → Users → Products → Carts → Sales**

| Pasta | Requests | Scripts |
|---|---|---|
| Auth | Create User, Authenticate | Login por `username`; salva `userId` e `token` automaticamente |
| Users | Get User, Delete User | Valida status 200 |
| Products | Create, Get All, Get by ID, Categories, By Category, Update, Delete | Salva `productId`; valida status e paginação |
| Carts | Create, Get All, Get by ID, Update, Delete | Data dinâmica via Pre-request; salva `cartId` |
| Sales | Create, Get All, Get by ID, Update, Cancel Item, Delete | `saleNumber` único por execução via `SALE-{timestamp}`; salva `saleId` e `saleItemId`; valida desconto de 10% no Create |

> Todos os 22 requests têm scripts de teste com asserções de status. Os Pre-request Scripts em Create/Update de Cart e Sale geram data e saleNumber dinamicamente — a collection pode ser executada múltiplas vezes sem conflito.

---

## Decisões Técnicas

| Decisão | Justificativa |
|---|---|
| **AutoMapper 15.1.3** | Versão sem vulnerabilidades de segurança conhecidas |
| **MockQueryable.NSubstitute** | Necessário para testar handlers com `CountAsync`/`ToListAsync` do EF em testes unitários |
| **Testcontainers** | Testes de integração com banco real — maior confiabilidade sem mocks de repositório |
| **Polly retry nos eventos** | Garante resiliência na publicação de eventos de domínio (3 tentativas, exponential backoff) |
| **External Identity no Cart** | Cart e Product são domínios independentes — desacoplamento intencional (DDD) |
| **SuppressModelStateInvalidFilter** | Garante que todas as validações passem pelo middleware customizado |
| **Production no Docker** | Ambiente correto para containers — `Development` causa instabilidade com DataProtection no Linux |
| **Kestrel explícito na porta 8080** | Garante binding correto independente de variáveis de ambiente |
| **`[Authorize]` nos controllers** | Products, Carts e Sales exigem JWT Bearer em todos os endpoints; Users expõe somente `POST /api/users` como público (registro) |
| **`ICollectionFixture` nos testes de integração** | Um único container PostgreSQL compartilhado entre todas as suites — elimina o anti-pattern `BuildServiceProvider()` e reduz o tempo de setup de minutos para ~15s |
| **JWT Issuer + Audience validados** | `ValidateIssuer` e `ValidateAudience` habilitados; token gerado inclui `iss` e `aud` — tokens de outros sistemas ou chaves diferentes são rejeitados |
| **JWT expiry lido do config** | `JwtTokenGenerator` lê `Jwt:ExpiryMinutes` (padrão 60 min) — sem valor hardcoded; alterável por variável de ambiente sem rebuild |
| **JWT secret com mínimo 32 bytes** | `AddJwtAuthentication` valida comprimento mínimo da chave (requisito HS256 / RFC 7518) na inicialização da aplicação |
| **Ownership check em `PUT /api/users/{id}`** | OWASP A01:2021 — qualquer usuário autenticado poderia atualizar o perfil de outro. O controller extrai o GUID do `ClaimTypes.NameIdentifier` e retorna 403 se não for o dono nem Admin |
| **`role` e `status` no body de `PUT /api/users/{id}` com guard no handler** | Spec exige esses campos no request body. O handler verifica: se o caller não for Admin e o valor enviado diferir do atual, lança `ForbiddenException` (403). Não-admins podem omitir ou repetir o valor atual sem restrição. Admins podem alterar livremente. Spec compliance ✅ + segurança ✅ |
| **`PATCH /api/users/{id}/role` — admin only** | Endpoint dedicado com `[Authorize(Roles = "Admin")]` para administradores gerenciarem role e status de qualquer usuário sem precisar do endpoint de perfil |
| **GitFlow + Conventional Commits** | Critério explícito do desafio (spec overview, item #16). Feature branches criadas a partir de `develop`, merge com `--no-ff` para preservar histórico, prefixos semânticos (`feat:`, `fix:`, `test:`, `refactor:`, `chore:`, `docs:`). Histórico auditável e legível por ferramentas de changelog automatizado. |
| **MongoDB provisionado, sem integração ativa** | Presente no `docker-compose` conforme especificado no tech stack do desafio. Não há repositório ativo apontando para ele na versão atual — o PostgreSQL via EF Core cobre todos os casos de uso implementados. Reservado para evoluções futuras: auditoria de eventos, catálogo de produtos desnormalizado, ou log de domínio. |

---

<div align="center">

*Desenvolvido por **Raphael Ribeiro** — Desafio Técnico Developer Evaluation 2026*

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-13-4169E1?style=flat&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white)
![Coverage](https://img.shields.io/badge/Coverage-91%25-brightgreen?style=flat)

</div>