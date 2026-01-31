# Insurance Proposal Manager

Sistema de Gerenciamento de Propostas de Seguros desenvolvido com **Arquitetura Hexagonal**, **Domain-Driven Design (DDD)** e **Microservices** utilizando **.NET 8**.

---

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Arquitetura](#-arquitetura)
- [Tecnologias](#-tecnologias)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Pré-requisitos](#-pré-requisitos)
- [Como Executar](#-como-executar)
- [Endpoints da API](#-endpoints-da-api)
- [Testes](#-testes)
- [Decisões de Design](#-decisões-de-design)
- [Padrões Utilizados](#-padrões-utilizados)

---

## 🎯 Visão Geral

O **Insurance Proposal Manager** é uma solução para gerenciamento de propostas de seguros composta por dois microserviços:

1. **PropostaService**: Gerencia o ciclo de vida das propostas de seguro
2. **ContratacaoService**: Responsável pela contratação/efetivação das propostas aprovadas

### Fluxo de Negócio

```
┌─────────────┐     ┌─────────────────┐     ┌───────────────────┐
│   Cliente   │────>│ PropostaService │────>│ ContratacaoService│
└─────────────┘     └─────────────────┘     └───────────────────┘
                            │                         │
                            │    RabbitMQ (Async)     │
                            └─────────────────────────┘
```

1. Cliente cria uma proposta via PropostaService
2. Proposta passa por análise e pode ser aprovada ou rejeitada
3. Proposta aprovada pode ser contratada via ContratacaoService
4. Serviços comunicam via HTTP (síncrono) e RabbitMQ (assíncrono)

---

## 🏗️ Arquitetura

### Arquitetura Hexagonal (Ports & Adapters)

```
┌─────────────────────────────────────────────────────────────────┐
│                         ADAPTERS (Input)                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │  REST API    │  │  Consumers   │  │  gRPC (extensível)   │  │
│  │  (Minimal)   │  │  (RabbitMQ)  │  │                      │  │
│  └──────┬───────┘  └──────┬───────┘  └──────────┬───────────┘  │
├─────────┼─────────────────┼─────────────────────┼──────────────┤
│         │                 │                     │              │
│         ▼                 ▼                     ▼              │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                  APPLICATION LAYER                      │   │
│  │  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌───────────┐  │   │
│  │  │Commands │  │ Queries │  │Handlers │  │Validators │  │   │
│  │  └─────────┘  └─────────┘  └─────────┘  └───────────┘  │   │
│  │                     │                                   │   │
│  │                     │ PORTS (Interfaces)                │   │
│  │                     ▼                                   │   │
│  │  ┌──────────────────────────────────────────────────┐  │   │
│  │  │ IProposalRepository │ IProposalService (Port)    │  │   │
│  │  └──────────────────────────────────────────────────┘  │   │
│  └─────────────────────────────────────────────────────────┘   │
│                           │                                    │
│                           ▼                                    │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                    DOMAIN LAYER                         │   │
│  │  ┌──────────┐  ┌──────────────┐  ┌─────────────────┐   │   │
│  │  │ Entities │  │ Value Objects│  │ Domain Events   │   │   │
│  │  │ (Rich)   │  │ (Immutable)  │  │                 │   │   │
│  │  └──────────┘  └──────────────┘  └─────────────────┘   │   │
│  └─────────────────────────────────────────────────────────┘   │
│                           │                                    │
├───────────────────────────┼────────────────────────────────────┤
│                           ▼                                    │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                 ADAPTERS (Output)                       │   │
│  │  ┌────────────┐  ┌────────────┐  ┌──────────────────┐  │   │
│  │  │ PostgreSQL │  │ RabbitMQ   │  │ HTTP Client      │  │   │
│  │  │ (EF Core)  │  │ (MassTransit│  │ (External APIs) │  │   │
│  │  └────────────┘  └────────────┘  └──────────────────┘  │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

### Princípios Aplicados

- **Dependency Inversion Principle (DIP)**: Camadas internas não dependem de externas
- **Single Responsibility Principle (SRP)**: Cada classe tem uma única responsabilidade
- **Rich Domain Model**: Entidades com comportamento, não apenas dados
- **CQRS**: Separação de comandos (escrita) e queries (leitura)

---

## 🛠️ Tecnologias

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| .NET | 8.0 | Framework principal |
| Entity Framework Core | 8.0.2 | ORM e persistência |
| PostgreSQL | 16 | Banco de dados |
| MediatR | 14.0.0 | Mediador para CQRS |
| FluentValidation | 11.9.0 | Validação de comandos |
| MassTransit | 8.2.2 | Abstração de mensageria |
| RabbitMQ | 3.x | Message broker |
| **Refit** | 8.0.0 | HTTP Client tipado (comunicação entre serviços) |
| **Polly** | 8.3.0 | Resiliência (Retry, Circuit Breaker) |
| Serilog | 8.0.1 | Logging estruturado |
| xUnit | 2.7.0 | Framework de testes |
| NSubstitute | 5.1.0 | Mocking |
| FluentAssertions | 6.12.0 | Assertivas fluentes |
| Bogus | 35.4.0 | Geração de dados fake |

---

## 📁 Estrutura do Projeto

```
InsuranceProposalManager/
├── src/
│   ├── BuildingBlocks/                    # Componentes compartilhados
│   │   ├── BuildingBlocks.Domain/         # Primitivos de domínio
│   │   │   ├── Primitives/
│   │   │   │   ├── Entity.cs              # Classe base para entidades
│   │   │   │   ├── AggregateRoot.cs       # Raiz de agregado
│   │   │   │   ├── ValueObject.cs         # Objeto de valor
│   │   │   │   ├── DomainEvent.cs         # Evento de domínio
│   │   │   │   └── Result.cs              # Padrão Result para erros
│   │   │   └── Errors/
│   │   │       └── Error.cs               # Estrutura de erro
│   │   ├── BuildingBlocks.Application/    # Abstrações de aplicação
│   │   │   ├── Messaging/                 # Interfaces CQRS
│   │   │   └── Behaviors/                 # Pipeline behaviors
│   │   └── BuildingBlocks.Infrastructure/ # Infraestrutura base
│   │       └── Persistence/               # UnitOfWork base
│   │
│   └── Services/
│       ├── PropostaService/               # Serviço de Propostas
│       │   ├── PropostaService.Domain/
│       │   │   ├── Entities/
│       │   │   │   └── Proposal.cs        # Aggregate Root
│       │   │   ├── ValueObjects/
│       │   │   │   ├── Cpf.cs             # CPF validado
│       │   │   │   ├── Email.cs           # Email validado
│       │   │   │   ├── Money.cs           # Valor monetário
│       │   │   │   └── CoveragePeriod.cs  # Período de cobertura
│       │   │   ├── Enums/
│       │   │   │   └── ProposalStatus.cs
│       │   │   ├── Events/
│       │   │   │   ├── ProposalCreatedEvent.cs
│       │   │   │   └── ProposalApprovedEvent.cs
│       │   │   ├── Errors/
│       │   │   │   └── ProposalErrors.cs
│       │   │   └── Repositories/
│       │   │       └── IProposalRepository.cs
│       │   │
│       │   ├── PropostaService.Application/
│       │   │   ├── Commands/
│       │   │   │   ├── CreateProposal/
│       │   │   │   ├── ApproveProposal/
│       │   │   │   └── RejectProposal/
│       │   │   ├── Queries/
│       │   │   │   ├── GetProposalById/
│       │   │   │   └── GetAllProposals/
│       │   │   └── DTOs/
│       │   │       └── ProposalDto.cs
│       │   │
│       │   ├── PropostaService.Infrastructure/
│       │   │   ├── Persistence/
│       │   │   │   ├── PropostaDbContext.cs
│       │   │   │   ├── Configurations/
│       │   │   │   └── Repositories/
│       │   │   └── Messaging/
│       │   │       └── EventHandlers/
│       │   │
│       │   └── PropostaService.Api/
│       │       ├── Endpoints/
│       │       │   └── ProposalEndpoints.cs
│       │       ├── Program.cs
│       │       └── Dockerfile
│       │
│       └── ContratacaoService/            # Serviço de Contratação
│           ├── ContratacaoService.Domain/
│           │   ├── Entities/
│           │   │   └── Contract.cs        # Aggregate Root
│           │   ├── Enums/
│           │   │   └── ContractStatus.cs
│           │   ├── Events/
│           │   │   └── ContractCreatedEvent.cs
│           │   └── Repositories/
│           │       └── IContractRepository.cs
│           │
│           ├── ContratacaoService.Application/
│           │   ├── Commands/
│           │   │   ├── CreateContract/
│           │   │   └── CancelContract/
│           │   ├── Queries/
│           │   │   └── GetContractById/
│           │   └── Ports/
│           │       └── IProposalService.cs  # Port para serviço externo
│           │
│           ├── ContratacaoService.Infrastructure/
│           │   ├── Persistence/
│           │   ├── Messaging/
│           │   └── ExternalServices/
│           │       └── HttpProposalService.cs  # Adapter HTTP
│           │
│           └── ContratacaoService.Api/
│               ├── Endpoints/
│               │   └── ContractEndpoints.cs
│               ├── Program.cs
│               └── Dockerfile
│
├── tests/
│   ├── PropostaService.UnitTests/
│   ├── ContratacaoService.UnitTests/
│   └── IntegrationTests/
│
├── docker-compose.yml
├── docker-compose.override.yml
├── Directory.Build.props
├── Directory.Packages.props
└── InsuranceProposalManager.sln
```

---

## ✅ Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

---

## 🚀 Como Executar

### Opção 1: Docker Compose (Recomendado)

```bash
# Clone o repositório
git clone https://github.com/seu-usuario/InsuranceProposalManager.git
cd InsuranceProposalManager

# Inicie todos os serviços
docker-compose up -d

# Verifique os logs
docker-compose logs -f
```

**Serviços disponíveis:**
- PropostaService API: http://localhost:5010
- ContratacaoService API: http://localhost:5020
- RabbitMQ Management: http://localhost:15672 (guest/guest)
- PostgreSQL PropostaService: localhost:5432
- PostgreSQL ContratacaoService: localhost:5433

### Opção 2: Execução Local

```bash
# Restaure as dependências
dotnet restore

# Inicie a infraestrutura
docker-compose up -d proposta-db contratacao-db rabbitmq

# Execute o PropostaService
cd src/Services/PropostaService/PropostaService.Api
dotnet run

# Em outro terminal, execute o ContratacaoService
cd src/Services/ContratacaoService/ContratacaoService.Api
dotnet run
```

### Opção 3: Modo Desenvolvimento (Hot Reload)

```bash
docker-compose -f docker-compose.yml -f docker-compose.override.yml up
```

---

## 📡 Endpoints da API

### PropostaService (http://localhost:5010)

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/proposals` | Lista todas as propostas |
| GET | `/api/proposals/{id}` | Obtém proposta por ID |
| POST | `/api/proposals` | Cria nova proposta |
| PUT | `/api/proposals/{id}/approve` | Aprova uma proposta |
| PUT | `/api/proposals/{id}/reject` | Rejeita uma proposta |
| GET | `/health` | Health check |

#### Criar Proposta
```bash
curl -X POST http://localhost:5010/api/proposals \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "João Silva",
    "customerCpf": "12345678909",
    "customerEmail": "joao@email.com",
    "premiumAmount": 1500.00,
    "coverageAmount": 100000.00,
    "coverageStartDate": "2024-02-01",
    "coverageEndDate": "2025-02-01"
  }'
```

#### Aprovar Proposta
```bash
curl -X PUT http://localhost:5010/api/proposals/{id}/approve
```

### ContratacaoService (http://localhost:5020)

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/contracts` | Lista todos os contratos |
| GET | `/api/contracts/{id}` | Obtém contrato por ID |
| POST | `/api/contracts` | Cria novo contrato |
| PUT | `/api/contracts/{id}/cancel` | Cancela um contrato |
| GET | `/health` | Health check |

#### Criar Contrato
```bash
curl -X POST http://localhost:5020/api/contracts \
  -H "Content-Type: application/json" \
  -d '{
    "proposalId": "guid-da-proposta-aprovada"
  }'
```

---

## 🧪 Testes

A suíte de testes inclui **4 categorias** de testes automatizados:

| Categoria | Quantidade | Descrição |
|-----------|------------|-----------|
| **Unit Tests** | 46 | Testes de domínio e aplicação |
| **Architecture Tests** | 25 | Validação de regras arquiteturais com NetArchTest |
| **Contract Tests** | 10 | Testes de contrato Consumer-Driven (WireMock) |
| **Integration Tests** | 13 | Testes de integração com Testcontainers (PostgreSQL) |

### Executar Testes (Sem Docker)

```bash
# Executar todos os testes que NÃO requerem Docker
dotnet test --filter "Category!=RequiresDocker"
```

### Executar Todos os Testes (Requer Docker)

```bash
# Requer Docker Desktop rodando
dotnet test
```

### Executar Testes com Cobertura

```bash
dotnet test --collect:"XPlat Code Coverage" --filter "Category!=RequiresDocker"
```

### Executar Testes por Categoria

```bash
# Unit Tests do PropostaService
dotnet test tests/PropostaService.UnitTests

# Unit Tests do ContratacaoService
dotnet test tests/ContratacaoService.UnitTests

# Architecture Tests (validação de camadas com NetArchTest)
dotnet test tests/ArchitectureTests

# Contract Tests - Consumer (não requer Docker)
dotnet test tests/ContractTests --filter "Category!=RequiresDocker"

# Contract Tests - Provider (requer Docker)
dotnet test tests/ContractTests --filter "Category=RequiresDocker"

# Integration Tests (requer Docker)
dotnet test tests/IntegrationTests
```

### Estrutura de Testes

```
tests/
├── PropostaService.UnitTests/          # Unit Tests - PropostaService
│   ├── Domain/
│   │   ├── ProposalTests.cs            # Testes da entidade Proposal
│   │   ├── CpfTests.cs                 # Testes do Value Object CPF
│   │   └── MoneyTests.cs               # Testes do Value Object Money
│   └── Application/
│       ├── CreateProposalCommandHandlerTests.cs
│       └── ApproveProposalCommandHandlerTests.cs
│
├── ContratacaoService.UnitTests/       # Unit Tests - ContratacaoService
│   ├── Domain/
│   │   └── ContractTests.cs
│   └── Application/
│       └── CreateContractCommandHandlerTests.cs
│
├── ArchitectureTests/                  # Architecture Tests (NetArchTest)
│   ├── HexagonalArchitectureTests.cs   # Validação de camadas hexagonais
│   ├── DomainPersistenceIgnoranceTests.cs  # Persistência ignorada no Domain
│   └── NamingConventionTests.cs        # Convenções de nomenclatura
│
├── ContractTests/                      # Consumer-Driven Contract Tests
│   ├── Contracts/
│   │   └── ProposalContract.cs         # Definição do contrato
│   ├── Consumer/
│   │   └── ContratacaoServiceConsumerContractTests.cs  # Consumer tests (WireMock)
│   └── Provider/
│       └── PropostaServiceProviderContractTests.cs     # Provider tests (Docker)
│
└── IntegrationTests/                   # Integration Tests (Testcontainers)
    ├── Fixtures/
    │   └── PostgresContainerFixture.cs # Container PostgreSQL compartilhado
    ├── Factories/
    │   ├── PropostaServiceWebApplicationFactory.cs
    │   └── ContratacaoServiceWebApplicationFactory.cs
    └── PropostaService/
        └── PropostaServiceIntegrationTests.cs
```

### Tecnologias de Teste

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| xUnit | 2.7.0 | Framework de testes |
| FluentAssertions | 6.12.0 | Assertivas fluentes |
| NSubstitute | 5.1.0 | Mocking |
| Bogus | 35.4.0 | Geração de dados fake |
| **NetArchTest** | 1.3.2 | Validação de arquitetura |
| **Testcontainers** | 3.7.0 | Containers Docker para testes |
| **WireMock.Net** | 1.5.51 | Mock HTTP para contract tests |
| **Respawn** | 6.2.1 | Reset de banco entre testes |

---

## 🎨 Decisões de Design

### 1. Rich Domain Model

Ao invés de modelos anêmicos, as entidades encapsulam comportamento:

```csharp
public sealed class Proposal : AggregateRoot<Guid>
{
    // Métodos de negócio na própria entidade
    public Result Approve()
    {
        if (Status != ProposalStatus.Pending)
            return Result.Failure(ProposalErrors.InvalidStatusForApproval);

        Status = ProposalStatus.Approved;
        RaiseDomainEvent(new ProposalApprovedEvent(Id));
        return Result.Success();
    }
}
```

### 2. Value Objects para Validação

Valores de negócio são representados como Value Objects imutáveis:

```csharp
public sealed class Cpf : ValueObject
{
    public string Value { get; }

    private Cpf(string value) => Value = value;

    public static Result<Cpf> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Cpf>(CpfErrors.Empty);

        if (!IsValid(value))
            return Result.Failure<Cpf>(CpfErrors.Invalid);

        return new Cpf(FormatCpf(value));
    }
}
```

### 3. Result Pattern para Erros

Erros são tratados explicitamente, sem exceções para fluxo de controle:

```csharp
public Result Handle(CreateProposalCommand command)
{
    var cpfResult = Cpf.Create(command.CustomerCpf);
    if (cpfResult.IsFailure)
        return Result.Failure(cpfResult.Error);

    var proposal = Proposal.Create(...);
    // ...
}
```

### 4. Ports & Adapters

O ContratacaoService define um **Port** para acessar o PropostaService:

```csharp
// Port (Application Layer)
public interface IProposalService
{
    Task<ProposalDto?> GetProposalByIdAsync(Guid id, CancellationToken ct);
    Task<Result> MarkAsContractedAsync(Guid id, CancellationToken ct);
}

// Adapter (Infrastructure Layer)
public class HttpProposalService : IProposalService
{
    private readonly HttpClient _httpClient;
    
    public async Task<ProposalDto?> GetProposalByIdAsync(Guid id, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync($"/api/proposals/{id}", ct);
        // ...
    }
}
```

### 5. Eventos de Domínio vs Eventos de Integração

- **Domain Events**: Internos ao bounded context
- **Integration Events**: Comunicação entre microserviços

```csharp
// Domain Event (interno)
public sealed record ProposalApprovedEvent(Guid ProposalId) : DomainEvent;

// Integration Event (externo via RabbitMQ)
public sealed record ProposalApprovedIntegrationEvent
{
    public Guid ProposalId { get; init; }
    public DateTime ApprovedAt { get; init; }
}
```

---

## 📐 Padrões Utilizados

| Padrão | Aplicação |
|--------|-----------|
| **Hexagonal Architecture** | Estrutura geral dos microserviços |
| **DDD (Domain-Driven Design)** | Modelagem do domínio |
| **CQRS** | Separação de Commands e Queries |
| **Aggregate Root** | Proposal e Contract como raízes |
| **Value Object** | CPF, Email, Money, CoveragePeriod |
| **Repository Pattern** | Abstração de persistência |
| **Unit of Work** | Transações atômicas |
| **Result Pattern** | Tratamento de erros sem exceções |
| **Mediator Pattern** | Desacoplamento via MediatR |
| **Event-Driven Architecture** | Comunicação assíncrona |
| **Outbox Pattern** | (Extensível) Garantia de entrega |

---

## 📊 Diagrama de Sequência

### Fluxo de Contratação

```
┌────────┐     ┌──────────────┐     ┌──────────────────┐     ┌──────────┐
│ Client │     │ContratacaoAPI│     │ PropostaService  │     │ RabbitMQ │
└────┬───┘     └──────┬───────┘     └────────┬─────────┘     └────┬─────┘
     │                │                      │                    │
     │ POST /contracts│                      │                    │
     │ {proposalId}   │                      │                    │
     │───────────────>│                      │                    │
     │                │                      │                    │
     │                │ GET /proposals/{id}  │                    │
     │                │─────────────────────>│                    │
     │                │                      │                    │
     │                │ ProposalDto (Approved)                    │
     │                │<─────────────────────│                    │
     │                │                      │                    │
     │                │ Create Contract      │                    │
     │                │ (local)              │                    │
     │                │                      │                    │
     │                │ PUT /mark-contracted │                    │
     │                │─────────────────────>│                    │
     │                │                      │                    │
     │                │         OK           │                    │
     │                │<─────────────────────│                    │
     │                │                      │                    │
     │                │                      │ Publish Event      │
     │                │                      │───────────────────>│
     │                │                      │                    │
     │   201 Created  │                      │                    │
     │<───────────────│                      │                    │
     │                │                      │                    │
```

---

## 🔒 Segurança (Extensões Futuras)

- [ ] Autenticação JWT
- [ ] Rate Limiting
- [ ] Validação de entrada (já implementado com FluentValidation)
- [ ] Logs de auditoria
- [ ] Secrets management

---

## 📈 Observabilidade (Extensões Futuras)

- [ ] OpenTelemetry para tracing distribuído
- [ ] Métricas com Prometheus
- [ ] Dashboards com Grafana
- [ ] Alertas

---

## 🤝 Contribuindo

1. Fork o projeto
2. Crie uma feature branch (`git checkout -b feature/nova-funcionalidade`)
3. Commit suas mudanças (`git commit -m 'Adiciona nova funcionalidade'`)
4. Push para a branch (`git push origin feature/nova-funcionalidade`)
5. Abra um Pull Request

---

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

## 👤 Autor

Desenvolvido como solução para desafio técnico de arquitetura de software.

---

## 📚 Referências

- [Hexagonal Architecture - Alistair Cockburn](https://alistair.cockburn.us/hexagonal-architecture/)
- [Domain-Driven Design - Eric Evans](https://domainlanguage.com/ddd/)
- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS - Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
