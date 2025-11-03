# Order System - Architecture

Poc sistema di ordine prodotti basato su servizi con .NET 9 e .NET Aspire

---

## Quick Start

### Prerequisiti
- **.NET 9 SDK** → [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Docker Desktop** → [Download](https://www.docker.com/products/docker-desktop)

> Per evitare problemi di compatibilità con Aspire installare la versione 9.0.306 del .NET SDK.

### Avvio del Sistema

```bash
# Clone del repository
git clone <repository-url>
cd order-poc

# Avvio con .NET Aspire (avvia tutto l'infrastruttura)
cd OrderSystem.AppHost
dotnet run
```

Eseguito il comando, Aspire avvierà:
- 4 database PostgreSQL (uno per servizio)
- 4 servizi gRPC: Product, User, Address, Order
- 4 Database Migrator per inizializzare gli schemi e seed dei dati iniziali. Giusto per non avere prodotti e categorie vuote all'inizio.
- API Gateway REST con Swagger
- Dashboard Aspire per observability

### Accesso Rapido

| Servizio | URL |
|----------|-----|
| **API Gateway (Swagger)** | http://localhost:5046/swagger |
| **Aspire Dashboard** | http://localhost:15066 |

> Per accedere alla dashboard è necessario cliccare nella url con token di accesso fornito nella console di Aspire al momento dell'avvio.

### Test del Sistema

Per semplicita di testing nella cartella `http/` sono presenti file `.http` con http client integrato in Rider.


1. **00_create_product.http** - Crud su prodotti
2. **01_create_user.http** - Crud su utenti
3. **02_create_address.http** - Crud su indirizzi
4. **03_order_lifecycle.http** - Gestione completa ciclo di vita di un ordine
5. **04_delete_user.http** - Cleanup dati

I file utilizzano il concetto di variabili globali per riutilizzare gli ID creati nei passaggi precedenti.

In alternativa si possono eseguire le richieste utilizzando la Swagger UI all'indirizzo http://localhost:5046/swagger.

## Struttura del Progetto

Ogni servizio segue questa struttura:

* Service.Core (Domain Layer)
* Service.Application (Application Layer)
* Service.Infrastructure (Infrastructure Layer)
* Service.GrpcService (Presentation Layer)
* Service.Proto (Contract Layer)

## Architettura del Sistema

Il client può comunicare con l'API Gateway tramite REST (HTTP/1.1).
L'API Gateway a sua volta comunica con i servizi tramite gRPC (HTTP/2).


### Responsabilità dei Microservizi

| Servizio | Responsabilità | Funzionalità Chiave                   |
|----------|----------------|---------------------------------------|
| **Product Service** | Catalogo prodotti | Stock Management (lock/release)       |
| **User Service** | Anagrafica utenti | CRUD completo, ricerca, paginazione   |
| **Address Service** | Indirizzi | Indirizzi Utente, default address     |
| **Order Service** | Gestione ordini | Stati ordine, items, calcolo totali   |
| **API Gateway** | Punto d'accesso unificato | REST API, orchestrazione, validazione |

---

## Stack Tecnologico

### Core
- **.NET 9.0** - Framework
- **ASP.NET Core** - Web & gRPC hosting
- **gRPC** - Comunicazione interna tra gatewat e service (HTTP/2, type-safe)
- **Entity Framework Core 9** - ORM
- **PostgreSQL** - Database (uno per servizio)
- **.NET Aspire** - Orchestrazione, Service Discovery, Resilience

### Patterns
- **Clean Architecture** - Separazione Domain/Application/Infrastructure
- **CQRS** - Repository separati per Read/Write
- **API Gateway Pattern** - Punto d'accesso unificato
- **Database per Servizio** - Isolamento dei dati

### Librerie Principali
- **FluentValidation** - Validazione DTO
-  **Entity Framework Core** - ORM
- **Polly** (via Aspire) - Retry, Circuit Breaker, Timeout
- **OpenTelemetry** - Distributed tracing & metrics

### Testing
- **xUnit** - Testing framework
- **Testcontainers** - Integration test con DB reali
- **Shouldly** - Assertion library

---

## Come Usare le api

### 1. Workflow Completo - Creazione Ordine

Seguire i file `.http` in sequenza o usare Swagger:

#### Step 1: Crea un Prodotto
```http
POST http://localhost:5046/api/products
Content-Type: application/json

{
  "name": "iPhone 100",
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

# Risposta: { "orderId": "..."}
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
- **Logs**: Log strutturati con correlazione con traces
- **Health**: Stato di salute di ogni servizio
- **Resources**: Database, container Docker attivi

> Dalla dashboard è possibile stoppare/avviare servizi e monitorare lo stato in tempo reale.
> Se un servizio non dovesse partire allo startup del progetto, è possibile avviarlo manualmente dalla dashboard, tramite il tasto play

---

## Testing

Il progetto include sia unit test che integration test.
i primi servono a testare le classi di dominio,i secondi testano direttamente l'esecuzione del metodo grpc. 
### Unit Test
```bash

# Test di un servizio specifico
cd test/{Service}/Product.UnitTest
dotnet test
```

### Integration Test

I test di integrazione usano **Testcontainers** per creare database PostgreSQL reali:

Assicurarsi che Docker sia in esecuzione prima di eseguire i test.

```bash


# Test di un servizio specifico
cd test/{Service}/Product.IntegrationTest
dotnet test
```

In alternativa potete eseguire tutti i test simultaneamente utilizzando Rider o Visual Studio,Visual Studio Code.

---


