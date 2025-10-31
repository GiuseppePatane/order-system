# Order System - Microservices Architecture

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![gRPC](https://img.shields.io/badge/gRPC-1.66-00ADD8?logo=grpc)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql)
![.NET Aspire](https://img.shields.io/badge/.NET%20Aspire-9.5-512BD4)

Sistema distribuito di gestione ordini basato su architettura a microservizi, sviluppato con .NET 9 e .NET Aspire per la gestione della Service Discovery, resilienza e telemetria.

## 📋 Indice

- [Overview](#overview)
- [Architettura](#architettura)
- [Tecnologie](#tecnologie)
- [Struttura del Progetto](#struttura-del-progetto)
- [Prerequisiti](#prerequisiti)
- [Quick Start](#quick-start)
- [Testing](#testing)
- [API Documentation](#api-documentation)
- [Funzionalità Principali](#funzionalità-principali)

## 🎯 Overview

Questo progetto è un sistema di gestione ordini progettato secondo i principi dei microservizi, dove ogni servizio ha la propria responsabilità e database. Il sistema implementa:

- ✅ **Clean Architecture** con separazione tra Domain, Application e Infrastructure
- ✅ **CQRS Pattern** con repository separati per lettura e scrittura
- ✅ **gRPC** per comunicazione inter-service performante e type-safe
- ✅ **API Gateway** come punto di accesso unificato
- ✅ **Service Discovery** automatica tramite .NET Aspire
- ✅ **Resilience Patterns** (retry, circuit breaker, timeout)
- ✅ **OpenTelemetry** per observability completa
- ✅ **Database per Microservizio** (PostgreSQL)
- ✅ **Integration Testing** con Testcontainers

## 🏗️ Architettura

### Diagramma ad Alto Livello

```
┌─────────────────┐
│   HTTP Client   │
└────────┬────────┘
         │
         ▼
┌─────────────────────────────┐
│      API Gateway            │
│   (REST Endpoints)          │
│   - Swagger UI              │
│   - Validation              │
│   - Orchestration           │
└──┬────┬────┬────┬───────────┘
   │    │    │    │
   │ gRPC    │    │
   │    │    │    │
   ▼    ▼    ▼    ▼
┌──────┐ ┌──────┐ ┌────────┐ ┌───────┐
│Product│ │ User │ │Address │ │ Order │
│Service│ │Service│ │Service│ │Service│
└───┬──┘ └──┬───┘ └───┬────┘ └───┬───┘
    │       │         │           │
    ▼       ▼         ▼           ▼
  ┌────┐ ┌────┐   ┌────┐      ┌────┐
  │ DB │ │ DB │   │ DB │      │ DB │
  └────┘ └────┘   └────┘      └────┘
```

### Microservizi

#### 1. **Product Service**
- Gestione del catalogo prodotti e categorie
- Gestione stock con operazioni atomiche (lock/release)
- Supporto per ricerche e paginazione
- Validazione SKU unici

#### 2. **User Service**
- Gestione anagrafica utenti
- CRUD completo
- Ricerca e paginazione

#### 3. **Address Service**
- Gestione indirizzi utente (spedizione e fatturazione)
- Supporto per indirizzo predefinito
- Validazione per paese e formato

#### 4. **Order Service**
- Gestione ciclo di vita ordini
- Stati ordine (Pending, Confirmed, Shipped, Delivered, Cancelled)
- Gestione item dell'ordine
- Calcolo totali automatico

#### 5. **API Gateway**
- Espone API REST unificate
- Orchestrazione di operazioni complesse multi-service
- Validation con FluentValidation
- Swagger UI per documentazione

## 🛠️ Tecnologie

### Backend
- **.NET 9.0** - Framework principale
- **ASP.NET Core** - Web API & gRPC hosting
- **gRPC** - Comunicazione inter-service
- **Entity Framework Core 9** - ORM
- **PostgreSQL** - Database relazionale
- **.NET Aspire** - Orchestrazione e Service Discovery

### Patterns & Libraries
- **Clean Architecture** - Separazione delle responsabilità
- **CQRS** - Command Query Responsibility Segregation
- **FluentValidation** - Validazione DTO
- **Mapster** - Object mapping
- **Polly** (via Aspire) - Resilience patterns

### Observability
- **OpenTelemetry** - Distributed tracing
- **Prometheus** (metrics)
- **Health Checks** - Endpoint di health per ogni servizio

### Testing
- **xUnit** - Framework di testing
- **Testcontainers** - Integration testing con database reali
- **FluentAssertions** - Assertion library

## 📁 Struttura del Progetto

```
ProductionSystem/
├── OrderSystem.AppHost/              # .NET Aspire Orchestrator
├── OrderSystem.ServiceDefaults/      # Configurazione condivisa
├── src/
│   ├── ApiGateway/
│   │   ├── ApiGateway.Api/          # REST Controllers
│   │   ├── ApiGateway.Core/          # DTOs e Interfaces
│   │   └── ApiGateway.infrastructure.GrcpClient/  # gRPC Clients
│   ├── Product/
│   │   ├── Product.Core/             # Domain entities
│   │   ├── Product.Application/      # Business logic
│   │   ├── Product.Infrastructure/   # EF Core, Repositories
│   │   ├── Product.GrpcService/      # gRPC Service
│   │   ├── Product.Proto/            # Protobuf definitions
│   │   └── Product.DataMigrator/     # DB Migrations & Seeding
│   ├── User/                         # Stessa struttura
│   ├── Address/                      # Stessa struttura
│   ├── Order/                        # Stessa struttura
│   └── Shared/
│       └── Shared.Proto/             # Proto comuni (es. common.proto)
├── test/
│   ├── Product/
│   │   ├── Product.UnitTest/
│   │   └── Product.IntegrationTest/
│   ├── User/
│   ├── Address/
│   ├── Order/
│   └── Test.Shared/                  # Utilities comuni
└── http/                             # HTTP Request files
    ├── 00_create_product.http
    ├── 01_create_user.http
    ├── 02_create_address.http
    └── 03_order_lifecycle.http
```

### Architettura per Microservizio

Ogni microservizio segue Clean Architecture:

```
Service/
├── Service.Core/              # Domain Layer
│   ├── Entities/             # Domain entities
│   └── Enums/                # Enumerations
├── Service.Application/       # Application Layer
│   ├── Dto/                  # Data Transfer Objects
│   ├── Interfaces/           # Repository & Service interfaces
│   ├── Mappers/              # Mapster mappers
│   └── Services/             # Business logic
├── Service.Infrastructure/    # Infrastructure Layer
│   ├── EF/                   # DbContext, Configurations
│   ├── Repositories/         # Repository implementations
│   └── Migrations/           # EF Migrations
├── Service.GrpcService/       # Presentation Layer
│   └── Services/             # gRPC service implementations
├── Service.Proto/             # Contract Layer
│   └── service.proto         # Protobuf definitions
└── Service.DataMigrator/      # Data seeding
```

## 📦 Prerequisiti

- **.NET 9 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- **Docker Desktop** ([Download](https://www.docker.com/products/docker-desktop))
- **JetBrains Rider** / **Visual Studio 2022** (opzionale ma consigliato)

## 🚀 Quick Start

### 1. Clone del Repository

```bash
git clone <repository-url>
cd ProductionSystem
```

### 2. Avvio con .NET Aspire

Il modo più semplice per avviare l'intero sistema è utilizzare .NET Aspire:

```bash
cd OrderSystem.AppHost
dotnet run
```

Questo comando:
- 🐘 Avvia i container PostgreSQL per ogni servizio
- 🚀 Avvia tutti i microservizi (Product, User, Address, Order)
- 🌐 Avvia l'API Gateway
- 📊 Configura Service Discovery e Telemetry
- 🔍 Apre la dashboard Aspire su `http://localhost:15066`

### 3. Accesso ai Servizi

- **API Gateway (Swagger)**: http://localhost:5046/swagger
- **Aspire Dashboard**: http://localhost:15066

### 4. Test del Sistema

Utilizza i file `.http` nella cartella `http/` per testare il sistema in sequenza:

1. `00_create_product.http` - Crea prodotti e categorie
2. `01_create_user.http` - Crea utenti
3. `02_create_address.http` - Crea indirizzi
4. `03_order_lifecycle.http` - Gestione completa ordine

Oppure usa Swagger UI per esplorare e testare le API interattivamente.

## 🧪 Testing

### Unit Test

```bash
dotnet test --filter Category=Unit
```

### Integration Test

I test di integrazione utilizzano Testcontainers per creare database PostgreSQL reali:

```bash
dotnet test --filter Category=Integration
```

### Test di uno Specifico Servizio

```bash
cd test/Product/Product.IntegrationTest
dotnet test
```

## 📖 API Documentation

### Swagger UI

Una volta avviato il sistema, accedi a Swagger UI:

**http://localhost:5046/swagger**

### Workflow Completo di un Ordine

#### 1. Creazione Prodotto
```http
POST /api/products
{
  "name": "iPhone 15",
  "description": "Latest iPhone",
  "categoryId": "<category-id>",
  "price": 999.99,
  "stock": 100,
  "sku": "IPH15-001"
}
```

#### 2. Creazione Utente
```http
POST /api/users
{
  "firstName": "Mario",
  "lastName": "Rossi",
  "email": "mario.rossi@example.com"
}
```

#### 3. Creazione Indirizzo
```http
POST /api/addresses
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
```

#### 4. Creazione Ordine
```http
POST /api/orders
{
  "userId": "<user-id>",
  "shippingAddressId": "<address-id>",
  "billingAddressId": "<address-id>",
  "firstItem": {
    "productId": "<product-id>",
    "quantity": 2
  }
}
```

#### 5. Gestione Ordine
```http
# Aggiungere item
POST /api/orders/{orderId}/items

# Modificare quantità
PATCH /api/orders/{orderId}/items/{itemId}/quantity

# Rimuovere item
DELETE /api/orders/{orderId}/items/{itemId}

# Cambiare stato
PATCH /api/orders/{orderId}/status

# Cancellare ordine
DELETE /api/orders/{orderId}/cancel
```

## ✨ Funzionalità Principali

### 1. Service Discovery
Tutti i servizi sono registrati automaticamente e si scoprono a vicenda tramite .NET Aspire. Non ci sono IP hardcoded.

### 2. Resilience Patterns
- **Retry automatico** per chiamate gRPC fallite
- **Circuit breaker** per prevenire cascading failures
- **Timeout** configurabili per ogni chiamata

### 3. Gestione Stock Transazionale
Il Product Service implementa operazioni di lock/release dello stock atomiche per garantire consistenza durante la creazione ordini.

```csharp
// Lock stock durante creazione ordine
await productClient.LockProductStock(productId, quantity);

// Release stock in caso di rollback
await productClient.ReleaseProductStock(productId, quantity);
```

### 4. Orchestrazione Complessa
L'API Gateway orchestra operazioni multi-service, ad esempio la creazione di un ordine richiede:
- Validazione utente (User Service)
- Validazione indirizzi (Address Service)
- Lock stock (Product Service)
- Recupero prezzi (Product Service)
- Creazione ordine (Order Service)

Con gestione automatica di rollback in caso di errore.

### 5. Observability
- **Distributed Tracing**: Traccia le richieste attraverso tutti i microservizi
- **Metrics**: Metriche di performance per ogni endpoint
- **Logs**: Logging strutturato con correlazione automatica
- **Health Checks**: Endpoint `/health` su ogni servizio

### 6. Database per Microservizio
Ogni servizio ha il proprio database PostgreSQL, garantendo:
- Isolamento dei dati
- Scalabilità indipendente
- Libertà tecnologica (schema diverso per ogni servizio)

### 7. Validazione Completa
- Validazione dei DTO con FluentValidation
- Validazione business logic a livello di Application
- Response standardizzate con ServiceResult<T>

### 8. Integration Testing
Test di integrazione completi con:
- Database PostgreSQL reale (Testcontainers)
- Test del ciclo di vita completo
- Setup e teardown automatici

## 🎓 Concetti Dimostrati

Questo progetto dimostra competenze in:

### Architettura
- ✅ Microservices Architecture
- ✅ Clean Architecture
- ✅ CQRS Pattern
- ✅ API Gateway Pattern
- ✅ Service Discovery
- ✅ Database per Microservizio

### Best Practices
- ✅ Separation of Concerns
- ✅ Dependency Injection
- ✅ Repository Pattern
- ✅ DTO Pattern
- ✅ Validation Layer
- ✅ Error Handling standardizzato

### DevOps & Observability
- ✅ Containerization (Docker)
- ✅ Orchestration (.NET Aspire)
- ✅ Distributed Tracing
- ✅ Health Checks
- ✅ Structured Logging

### Testing
- ✅ Unit Testing
- ✅ Integration Testing con Testcontainers
- ✅ Test Isolation
- ✅ Arrange-Act-Assert Pattern

### Performance & Resilience
- ✅ gRPC per comunicazione performante
- ✅ Connection pooling
- ✅ Retry policies
- ✅ Circuit breaker
- ✅ Timeout handling

## 🔧 Configurazione

### Variabili d'Ambiente

Ogni servizio supporta le seguenti variabili:

```bash
# Database
ConnectionStrings__DefaultConnection=Host=localhost;Database=productdb;Username=postgres;Password=postgres

# Logging
Logging__LogLevel__Default=Information

# Kestrel
ASPNETCORE_URLS=http://+:5000;https://+:5001
```

### Porte di Default

| Servizio | HTTP | HTTPS | gRPC |
|----------|------|-------|------|
| API Gateway | 5046 | 7046 | - |
| Product Service | 5001 | 7001 | 5001 |
| User Service | 5002 | 7002 | 5002 |
| Address Service | 5003 | 7003 | 5003 |
| Order Service | 5004 | 7004 | 5004 |

## 📝 Note di Sviluppo

### Aggiunta di un Nuovo Microservizio

1. Creare la struttura Clean Architecture
2. Definire i contract gRPC in `.proto`
3. Implementare le entities e repository
4. Implementare il servizio gRPC
5. Aggiungere il client nell'API Gateway
6. Registrare in `OrderSystem.AppHost`
7. Aggiungere i test

### Migrazione Database

```bash
cd src/<Service>/<Service>.Infrastructure
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

### Troubleshooting

**Problema: I servizi non si trovano**
- Verifica che .NET Aspire sia in esecuzione
- Controlla i log nella Aspire Dashboard

**Problema: Database connection error**
- Assicurati che Docker sia in esecuzione
- Verifica che i container PostgreSQL siano avviati

**Problema: Porta già in uso**
- Modifica le porte in `launchSettings.json`
- Oppure termina il processo che usa la porta

## 📄 Licenza

Questo progetto è sviluppato a scopo didattico per colloqui tecnici.

---

**Sviluppato con** ❤️ **usando .NET 9 e .NET Aspire**

