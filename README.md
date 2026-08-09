# Seatly — Full-Stack Event Booking System (.NET 10 & React)

> Author / Student: Mirza Hodžić  
> Course Project: Event Management & Ticket Booking System  
> Tech Stack: .NET 10 C#, Entity Framework Core, PostgreSQL, Supabase Auth, React (Vite), xUnit  

---

## 1. Project Overview

Seatly is a high-performance full-stack web application developed for managing events (concerts, conferences) and booking seats in real-time.

Key Features:
* Dynamic ticket pricing using seat category multipliers and discount strategies.
* Base64 QR code generation for digital event tickets.
* Automated background service for releasing expired pending bookings.
* High-performance reads via In-Memory Caching and PostgreSQL B-Tree database indexes.
* Fully containerized Docker deployment on Render, Vercel frontend hosting, and Cloudflare DNS management.

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
 ├── Frontend/                  (React 18 + Vite + TailwindCSS SPA)
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

* ExpiredBookingCleanupService: Inherits from BackgroundService. Runs periodically every 2 minutes to scan for Pending bookings older than 15 minutes, automatically updating their status to Cancelled and freeing allocated seats.

---

## 6. Production Infrastructure & Cloud Deployment

### A. Backend Deployment (Render & Docker)
* **Containerization:** The .NET 10 Web API is containerized using a multi-stage Dockerfile (`mcr.microsoft.com/dotnet/sdk:10.0` for build, `aspnet:10.0` for runtime) exposing port `8080`.
* **Database Connectivity (IPv4 Session Pooling):** Render free instances support outbound IPv4 traffic. Connection to Supabase PostgreSQL is established via Supabase Session Pooler to resolve IPv6 routing limitations.

### B. Frontend Deployment (Vercel)
* **Framework:** React 18 + Vite SPA deployed on Vercel Edge Network.
* **Single Page Application Rewrites:** `vercel.json` maps all incoming routes `/(.*)` to `/index.html` to allow client-side routing via React Router DOM.

### C. Custom Domain & Cloudflare DNS Management
* **Custom Domain:** Managed via Cloudflare DNS.
* **DNS Records:** CNAME Flattening pointing subdomain traffic to Vercel production edge servers (`cname.vercel-dns.com`).
* **Supabase OAuth URL Configuration:**
  - `Site URL` and `Redirect URLs` configured to ensure seamless OAuth (Google/GitHub) authentication flows.

---

## 7. How to Run & Test Locally

### Running Backend API:
```bash
cd src/Seatly.API
dotnet run
```
API available at: http://localhost:5051 (Swagger UI at http://localhost:5051/swagger).

### Running Frontend App:
```bash
cd Frontend
npm install
npm run dev
```
Frontend available at: http://localhost:5173.

### Running Unit Tests:
```bash
cd tests/Seatly.Tests
dotnet test
```
