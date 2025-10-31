# Order System - Microservices Architecture

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![gRPC](https://img.shields.io/badge/gRPC-00ADD8?logo=grpc)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-336791?logo=postgresql)
![.NET Aspire](https://img.shields.io/badge/.NET%20Aspire-9.5-512BD4)

Sistema di gestione ordini basato su microservizi con **Clean Architecture**, **gRPC**, **API Gateway** e **.NET Aspire**.

---

## Quick Start

### Prerequisiti
- **.NET 9 SDK** → [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Docker Desktop** → [Download](https://www.docker.com/products/docker-desktop)

### Avvio del Sistema

```bash
# Clone del repository
git clone <repository-url>
cd ProductionSystem

# Avvio con .NET Aspire (avvia tutto automaticamente)
cd OrderSystem.AppHost
dotnet run
```

**Fatto!** Il sistema avvierà:
- 4 database PostgreSQL (uno per microservizio)
- 4 microservizi gRPC: Product, User, Address, Order
- API Gateway REST con Swagger
- Dashboard Aspire per observability

### Accesso Rapido

| Servizio | URL |
|----------|-----|
| **API Gateway (Swagger)** | http://localhost:5046/swagger |
| **Aspire Dashboard** | http://localhost:15066 |

### Test del Sistema

Usa i file `.http` nella cartella `http/` in questo ordine:

1. **00_create_product.http** - Crea categorie e prodotti
2. **01_create_user.http** - Crea utenti
3. **02_create_address.http** - Crea indirizzi
4. **03_order_lifecycle.http** - Gestione completa di un ordine
5. **04_delete_user.http** - Cleanup

Oppure usa **Swagger UI** per esplorare le API interattivamente.

---

## Struttura del Progetto

```
ProductionSystem/
│
├── OrderSystem.AppHost/              # .NET Aspire - Orchestrator
│   └── Program.cs                    # Configurazione Service Discovery
│
├── OrderSystem.ServiceDefaults/      # Configurazione condivisa
│   └── Extensions.cs                 # OpenTelemetry, Health Checks, Resilience
│
├── src/
│   │
│   ├── ApiGateway/                   # API Gateway (REST)
│   │   ├── ApiGateway.Api/          # Controllers REST
│   │   ├── ApiGateway.Core/          # DTOs e Interfaces
│   │   └── ApiGateway.infrastructure.GrcpClient/  # gRPC Clients
│   │
│   ├── Product/                      # Product Microservice
│   │   ├── Product.Core/            # ┐
│   │   ├── Product.Application/     # │ Clean Architecture
│   │   ├── Product.Infrastructure/  # │ (Domain, App, Infra)
│   │   ├── Product.GrpcService/     # ┘
│   │   ├── Product.Proto/           # Protobuf definitions
│   │   └── Product.DataMigrator/    # DB Migrations & Seeding
│   │
│   ├── User/                         # User Microservice
│   │   └── [stessa struttura]
│   │
│   ├── Address/                      # Address Microservice
│   │   └── [stessa struttura]
│   │
│   ├── Order/                        # Order Microservice
│   │   └── [stessa struttura]
│   │
│   └── Shared/
│       └── Shared.Proto/            # Proto condivisi (common.proto)
│
├── test/                            # Testing
│   ├── Product/
│   │   ├── Product.UnitTest/
│   │   └── Product.IntegrationTest/
│   ├── User/, Address/, Order/
│   └── Test.Shared/                 # Utilities comuni (Testcontainers)
│
└── http/                            # HTTP Request files
    ├── 00_create_product.http
    ├── 01_create_user.http
    ├── 02_create_address.http
    ├── 03_order_lifecycle.http
    └── 04_delete_user.http
```

### Architettura per Microservizio (Clean Architecture)

Ogni microservizio segue questa struttura:

```
Service/
│
├── Service.Core/              # DOMAIN LAYER
│   ├── Entities/             # Domain entities (Product, User, etc.)
│   └── Enums/                # Enumerations (OrderStatus, etc.)
│
├── Service.Application/       # APPLICATION LAYER
│   ├── Dto/                  # Data Transfer Objects
│   ├── Interfaces/           # Repository & Service interfaces
│   ├── Mappers/              # Mapster mappers
│   └── Services/             # Business logic
│
├── Service.Infrastructure/    # INFRASTRUCTURE LAYER
│   ├── EF/                   # DbContext, Configurations
│   ├── Repositories/         # Repository implementations (CQRS)
│   └── Migrations/           # EF Core Migrations
│
├── Service.GrpcService/       # PRESENTATION LAYER
│   ├── Services/             # gRPC service implementations
│   └── Program.cs            # Startup & DI
│
└── Service.Proto/             # CONTRACT LAYER
    └── service.proto         # Protobuf definitions (API contract)
```

---

## Architettura del Sistema

### Diagramma High-Level

```
┌─────────────┐
│ HTTP Client │
└──────┬──────┘
       │
       ▼
┌──────────────────┐
│   API Gateway    │  ← REST endpoints, Swagger, Validation
│   (Port 5046)    │  ← Orchestrazione multi-service
└─┬───┬───┬───┬───┘
  │   │   │   │
  │ gRPC (HTTP/2)
  │   │   │   │
  ▼   ▼   ▼   ▼
┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐
│ Product │ │  User   │ │ Address │ │  Order  │
│ Service │ │ Service │ │ Service │ │ Service │
└────┬────┘ └────┬────┘ └────┬────┘ └────┬────┘
     │           │           │           │
     ▼           ▼           ▼           ▼
   ┌──┐        ┌──┐        ┌──┐        ┌──┐
   │DB│        │DB│        │DB│        │DB│  ← PostgreSQL
   └──┘        └──┘        └──┘        └──┘
```

### Responsabilità dei Microservizi

| Servizio | Responsabilità | Funzionalità Chiave |
|----------|----------------|---------------------|
| **Product Service** | Catalogo prodotti | Categorie, Stock Management (lock/release), SKU unici |
| **User Service** | Anagrafica utenti | CRUD completo, ricerca, paginazione |
| **Address Service** | Indirizzi | Indirizzi spedizione/fatturazione, default address |
| **Order Service** | Gestione ordini | Stati ordine, items, calcolo totali |
| **API Gateway** | Punto d'accesso unificato | REST API, orchestrazione, validation |

---

## Stack Tecnologico

### Core
- **.NET 9.0** - Framework
- **ASP.NET Core** - Web & gRPC hosting
- **gRPC** - Comunicazione inter-service (HTTP/2, type-safe)
- **Entity Framework Core 9** - ORM
- **PostgreSQL** - Database (uno per servizio)
- **.NET Aspire** - Orchestrazione, Service Discovery, Resilience

### Patterns
- **Clean Architecture** - Separazione Domain/Application/Infrastructure
- **CQRS** - Repository separati per Read/Write
- **API Gateway Pattern** - Punto d'accesso unificato
- **Database per Microservizio** - Isolamento dei dati

### Libraries
- **FluentValidation** - Validazione DTO
- **Mapster** - Object mapping
- **Polly** (via Aspire) - Retry, Circuit Breaker, Timeout
- **OpenTelemetry** - Distributed tracing & metrics

### Testing
- **xUnit** - Testing framework
- **Testcontainers** - Integration test con DB reali
- **FluentAssertions** - Assertion library

---

## Come Usare il Sistema

### 1. Workflow Completo - Creazione Ordine

Seguire i file `.http` in sequenza o usare Swagger:

#### Step 1: Crea un Prodotto
```http
POST http://localhost:5046/api/products
Content-Type: application/json

{
  "name": "iPhone 15",
  "description": "Latest iPhone",
  "categoryId": "<category-id>",  # vedi 00_create_product.http
  "price": 999.99,
  "stock": 100,
  "sku": "IPH15-001"
}

# Risposta: { "productId": "..." }
```

#### Step 2: Crea un Utente
```http
POST http://localhost:5046/api/users
Content-Type: application/json

{
  "firstName": "Mario",
  "lastName": "Rossi",
  "email": "mario.rossi@example.com"
}

# Risposta: { "userId": "..." }
```

#### Step 3: Crea un Indirizzo
```http
POST http://localhost:5046/api/addresses
Content-Type: application/json

{
  "userId": "<user-id>",
  "street": "Via Roma 123",
  "city": "Milano",
  "state": "MI",
  "postalCode": "20100",
  "country": "IT",
  "label": "Casa",
  "isDefault": true
}

# Risposta: { "addressId": "..." }
```

#### Step 4: Crea un Ordine
```http
POST http://localhost:5046/api/orders
Content-Type: application/json

{
  "userId": "<user-id>",
  "shippingAddressId": "<address-id>",
  "billingAddressId": "<address-id>",
  "firstItem": {
    "productId": "<product-id>",
    "quantity": 2
  }
}

# Risposta: { "orderId": "...", "total": 1999.98, ... }
```

#### Step 5: Gestisci l'Ordine

```http
# Aggiungi item
POST http://localhost:5046/api/orders/{orderId}/items

# Modifica quantità
PATCH http://localhost:5046/api/orders/{orderId}/items/{itemId}/quantity

# Cambia stato
PATCH http://localhost:5046/api/orders/{orderId}/status
{ "newStatus": "Confirmed" }

# Cancella ordine
DELETE http://localhost:5046/api/orders/{orderId}/cancel
```

### 2. Esplorare la Dashboard Aspire

Visita http://localhost:15066 per:
- **Traces**: Visualizza richieste distribuite tra servizi
- **Metrics**: Performance e latenze
- **Logs**: Log strutturati con correlazione
- **Health**: Stato di salute di ogni servizio
- **Resources**: Database, container Docker attivi

---

## Testing

### Unit Test
```bash
# Tutti i unit test
dotnet test --filter Category=Unit

# Test di un servizio specifico
cd test/Product/Product.UnitTest
dotnet test
```

### Integration Test

I test di integrazione usano **Testcontainers** per creare database PostgreSQL reali:

```bash
# Tutti gli integration test
dotnet test --filter Category=Integration

# Test di un servizio specifico
cd test/Product/Product.IntegrationTest
dotnet test
```

### Coverage Report
```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## Funzionalità Chiave

### Service Discovery Automatico
I servizi si scoprono automaticamente tramite .NET Aspire. Nessun IP hardcoded.

### Resilience Patterns
- **Retry automatico** per chiamate fallite
- **Circuit breaker** per prevenire cascading failures  
- **Timeout** configurabili

### Gestione Stock Transazionale
```csharp
// Lock stock durante creazione ordine
await productClient.LockProductStock(productId, quantity);

// Release in caso di rollback
await productClient.ReleaseProductStock(productId, quantity);
```

### Orchestrazione Multi-Service
L'API Gateway orchestra operazioni complesse:
1. Valida utente (User Service)
2. Valida indirizzi (Address Service)
3. Lock stock + prezzi (Product Service)
4. Crea ordine (Order Service)
5. Rollback automatico in caso di errore

### Observability Completa
- **Distributed Tracing**: Traccia richieste tra servizi
- **Metrics**: Performance di ogni endpoint
- **Structured Logging**: Log correlati per request
- **Health Checks**: `/health` su ogni servizio

### Database Isolation
Ogni servizio ha il proprio PostgreSQL:
- Isolamento dei dati
- Scalabilità indipendente
- Schema diversificato

---

## Concetti Implementati

Questo progetto dimostra:

### Architettura
- Microservices Architecture  
- Clean Architecture (Domain, Application, Infrastructure)  
- CQRS Pattern  
- API Gateway Pattern  
- Service Discovery  
- Database per Microservizio  

### Best Practices
- Dependency Injection  
- Repository Pattern  
- DTO Pattern  
- Validation Layer (FluentValidation)  
- Error Handling standardizzato (ServiceResult<T>)  
- Separation of Concerns  

### DevOps & Observability
- Containerization (Docker)  
- Orchestration (.NET Aspire)  
- Distributed Tracing  
- Health Checks  
- Structured Logging  

### Performance & Resilience
- gRPC per comunicazione performante (HTTP/2, Protobuf)  
- Connection pooling  
- Retry policies  
- Circuit breaker  
- Timeout handling  

### Testing
- Unit Testing  
- Integration Testing con Testcontainers  
- Test Isolation  
- Arrange-Act-Assert Pattern  

---

## Configurazione Avanzata

### Variabili d'Ambiente

Ogni servizio supporta:

```bash
# Database
ConnectionStrings__DefaultConnection=Host=localhost;Database=db;Username=postgres;Password=postgres

# Logging
Logging__LogLevel__Default=Information

# Kestrel
ASPNETCORE_URLS=http://+:5000
```

### Porte di Default

| Servizio | HTTP | Descrizione |
|----------|------|-------------|
| API Gateway | 5046 | REST API + Swagger |
| Product Service | 5001 | gRPC (HTTP/2) |
| User Service | 5002 | gRPC (HTTP/2) |
| Address Service | 5003 | gRPC (HTTP/2) |
| Order Service | 5004 | gRPC (HTTP/2) |
| Aspire Dashboard | 15066 | Observability |

---

## Troubleshooting

### Problema: Servizi non si trovano
**Soluzione**: Verifica che .NET Aspire sia in esecuzione e controlla i log nella Dashboard.

### Problema: Database connection error
**Soluzione**: 
- Assicurati che Docker sia in esecuzione
- Verifica che i container PostgreSQL siano avviati (`docker ps`)

### Problema: Porta già in uso
**Soluzione**: 
- Modifica le porte in `launchSettings.json`
- Oppure: `lsof -ti:5046 | xargs kill` (macOS/Linux)

### Problema: Build errors
**Soluzione**: 
```bash
dotnet restore
dotnet build
```

---

## Note per Colloquio

Questo progetto dimostra competenze avanzate in:

- **Architetture distribuite**: Microservices con isolamento completo
- **Modern .NET**: .NET 9, gRPC, Aspire, EF Core 9
- **Design Patterns**: Clean Architecture, CQRS, Repository, Gateway
- **Resilience**: Retry, Circuit Breaker, Timeout policies
- **Observability**: OpenTelemetry, distributed tracing, metrics
- **Testing**: Unit + Integration con database reali (Testcontainers)
- **DevOps ready**: Docker, orchestrazione, health checks

---

**Sviluppato con .NET 9 e .NET Aspire**

