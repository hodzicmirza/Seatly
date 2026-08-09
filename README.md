# Seatly — Backend Documentation (.NET 10 Web API)

> Author / Student: Mirza Hodžić  
> Course Project: Event Management & Ticket Booking System  
> Tech Stack: .NET 10 C#, Entity Framework Core, PostgreSQL, Supabase Auth, xUnit  

---

## 1. Project Overview

Seatly is a backend Web API system developed using .NET 10 C# for managing events (concerts, conferences) and booking seats in real-time.

Key Features:
* Dynamic ticket pricing using seat category multipliers and discount strategies.
* QR code generation in Base64 for digital event tickets.
* Automated background service for releasing expired pending bookings.
* High-performance reads via In-Memory Caching and database indexes.

---

## 2. Architecture (Clean Architecture / Onion Pattern)

The project follows Clean Architecture principles with strict boundary separation:

```
Seatly Solution Structure:
 ├── src/
 │    ├── Seatly.Domain         (Core: Entities, Value Objects, Enums, Factories)
 │    ├── Seatly.Application    (Business Logic: DTOs, Services, Discount Strategies)
 │    ├── Seatly.Infrastructure (Data: AppDbContext, Repositories, QrCode, Email)
 │    └── Seatly.API            (Entry Point: Controllers, Middleware, BackgroundServices)
 └── tests/
      └── Seatly.Tests          (xUnit Unit Tests)
```

### Layer Responsibilities:

#### 1. Seatly.Domain
* Pure C# domain logic with zero external framework dependencies.
* Entities: Event (abstract base class), Concert, Conference, Booking, User.
* Value Objects: Money (amount + currency BAM), Address (street, city, country), SeatCategory (name + multiplier).
* Enums: UserRole (Customer, Admin, Organizer), BookingStatus (Pending, Confirmed, Cancelled, Used), EventType.

#### 2. Seatly.Application
* Use case implementations via service interfaces (IEventService, IBookingService, IUserService) and DTO records.
* Discount calculation algorithm via Strategy Pattern.
* In-Memory Caching in EventService.

#### 3. Seatly.Infrastructure
* Database access via Entity Framework Core 10 and Npgsql PostgreSQL provider.
* AppDbContext: Model configuration, value object mappings, and database indexes.
* Repositories: EventRepository, BookingRepository, UserRepository.
* Infrastructure Services: QrCodeService (QR code image generation), EmailService.

#### 4. Seatly.API
* REST Controllers (EventsController, BookingsController, UsersController).
* ExceptionMiddleware for global error handling and standard JSON error responses.
* ExpiredBookingCleanupService (IHostedService background process).

---

## 3. Design Patterns Applied

### A. Strategy Pattern (IDiscountStrategy)
Encapsulates discount logic to satisfy the Open/Closed Principle. Adding a new discount type does not require modifying existing booking code.

* Interface: IDiscountStrategy with CalculateDiscount(...) method.
* Concrete Strategies:
  1. EarlyBirdDiscount: 10% discount if booked more than 30 days before event date.
  2. VIPDiscount: 20% discount for users with VIP role.
  3. BulkDiscount: 15% discount for reservations of 5 or more seats.

### B. Factory Pattern (EventFactory)
Encapsulates object instantiation logic for Concert and Conference types based on EventType.

### C. Repository & Unit of Work Pattern
* Repository Pattern: Abstracts database queries away from business services. Read queries utilize .AsNoTracking() for high throughput.
* Unit of Work Pattern (IUnitOfWork): Ensures atomic transactions across multiple repositories via SaveChangesAsync().

---

## 4. Performance & Caching (IMemoryCache & Indexing)

1. In-Memory Caching (IMemoryCache):
   - EventService caches the full list of events and individual event details for 30 seconds.
   - Cache invalidation occurs automatically when an Admin creates, updates, or deletes an event.
2. PostgreSQL Database Indexes:
   - Indexes configured on high-cardinality search/filter fields (Booking.UserId, Booking.EventId, Booking.Status, Event.Date).

---

## 5. Background Services

* ExpiredBookingCleanupService: Inherits from BackgroundService. Runs periodically every 5 minutes to scan for Pending bookings older than 15 minutes, automatically updating their status to Cancelled and freeing allocated seats.

---

## 6. How to Run & Test

### Running Backend API:
```bash
cd src/Seatly.API
dotnet run
```
API available at: http://localhost:5051 (Swagger UI at http://localhost:5051/swagger).

### Running Unit Tests:
```bash
cd tests/Seatly.Tests
dotnet test
```

