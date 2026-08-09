# Seatly — Full-Stack Event Booking System (.NET 10 & React)

> **Author / Student:** Mirza Hodžić  
> **Course Project:** Event Management & Ticket Booking System  
> **Tech Stack:** .NET 10 C#, Entity Framework Core, PostgreSQL (Supabase), Supabase Auth, React 18 (Vite), Resend API, GitHub Actions, xUnit  
> **Live Web App:** [https://seatly.hodzicmirza.com](https://seatly.hodzicmirza.com)  
> **Live API Backend:** [https://seatlybackend.onrender.com](https://seatlybackend.onrender.com)  

---

## 1. Project Overview

**Seatly** is a state-of-the-art, full-stack event management and real-time ticket booking web application. It enables organizers and administrators to create and update events, manage ticket categories and seat capacities, while providing customers with a seamless ticket purchasing experience featuring dynamic discount strategies and digital QR code email tickets.

### Key Features & UX Highlights:
* **Dynamic Seat Category Capacity Tracking:** Event creators can assign specific seat capacities per category (e.g., Standard: 50 seats, VIP: 20 seats). Server-side validation enforces that the category sum strictly equals the total event capacity.
* **Real-Time Unallocated Seats Banner:** Visual feedback status banner during event creation and editing that displays allocated capacity in real-time (`Total Allocated: X / Total seats`) with color-coded status indicators (🟢 All allocated, 🟡 Unallocated seats remaining, 🔴 Capacity exceeded).
* **Interactive Option Cards for Category Selection:** Booking modal lists categories as interactive option cards displaying category name, specific seat capacity, price in BAM, and radio selection state (eliminating plain `<select>` dropdowns).
* **Admin QR Code Ticket Validator (`/admin/validate-ticket`):** Dedicated admin tool to scan or verify ticket QR codes/Booking IDs in real-time. Automatically validates ticket authenticity, prevents duplicate or cancelled ticket entry, and updates ticket status to `Used`.
* **Smart Discount Calculation:** Automated discount evaluation using strategy patterns (15% Bulk Discount for 5+ seats, Early Bird 10%, VIP 20%).
* **Base64 Digital QR Ticket Generation:** Instant digital QR ticket creation upon booking confirmation, downloadable in PNG format.
* **Automated Email Dispatch (Resend API):** Production-ready HTML email notifications with embedded QR code tickets dispatched automatically upon booking confirmation, as well as instant cancellation notifications.
* **Interactive Frontend Workflows:**
  * **Automatic Navigation:** Successful ticket purchases immediately redirect users back to the main event overview.
  * **Confirmation Modal Dialogs:** Interactive modal prompt before cancelling any booking to prevent accidental cancellations.
  * **European Date & Time Formatting:** Displays and inputs date/time in standard European 24-hour format (`DD/MM/YYYY HH:mm`).
* **Automated Expired Booking Cleanup:** Background worker automatically cancels pending reservations older than 15 minutes, restoring seat availability.
* **Automatic Database Migrations:** Entity Framework Core auto-migrates database schemas (`db.Database.Migrate()`) on startup.
* **24/7 Keep-Alive Infrastructure:** GitHub Actions cron workflow (`.github/workflows/keep_alive.yml`) pings the backend API every 10 minutes to maintain 24/7 availability on Render free instances.

---

## 2. Architecture & Design Principles

Seatly strictly follows **Clean Architecture (Onion Architecture)** principles to decouple core domain rules from database infrastructure and API frameworks:

```
Seatly Solution Structure:
 ├── src/
 │    ├── Seatly.Domain         (Core: Entities, Value Objects, Enums, Exceptions)
 │    ├── Seatly.Application    (Business Logic: DTOs, Services, Discount Strategies)
 │    ├── Seatly.Infrastructure (Data: AppDbContext, Repositories, QrCode, Resend Email)
 │    └── Seatly.API            (Entry Point: Controllers, Middleware, BackgroundServices)
 ├── Frontend/                  (React 18 + Vite + TailwindCSS SPA)
 ├── .github/workflows/         (GitHub Actions: 24/7 Render Keep-Alive Workflow)
 └── tests/
      └── Seatly.Tests          (xUnit Unit Tests)
```

### Layer Breakdown:

#### 1. `Seatly.Domain`
* Pure C# domain model with zero external dependencies.
* **Entities:** `Event` (abstract base class), `Concert`, `Conference`, `Booking`, `User`.
* **Value Objects:** `Money` (amount + currency), `Address` (street, city, country), `SeatCategory` (name + multiplier + seatsCount).
* **Enums:** `UserRole` (`Customer`, `Admin`, `Organizer`), `BookingStatus` (`Pending`, `Confirmed`, `Cancelled`, `Used`), `EventType`.

#### 2. `Seatly.Application`
* Use-case orchestrations via service contracts (`IEventService`, `IBookingService`, `IUserService`) and record-based DTOs.
* **Discount System:** Strategy Pattern (`IDiscountStrategy`) for extensible price calculations.
* **In-Memory Caching:** High-throughput read performance via `IMemoryCache` with automatic cache invalidation upon mutation.

#### 3. `Seatly.Infrastructure`
* **Data Access:** Entity Framework Core 10 with Npgsql provider for Supabase PostgreSQL. Auto-applies schema migrations (`db.Database.Migrate()`).
* **Thread Safety & Entity Tracking:** Tracked query execution for domain entity updates/cancellations.
* **Repositories:** `EventRepository`, `BookingRepository`, `UserRepository`.
* **Services:** `QrCodeService` (SkiaSharp-based PNG generation), `EmailService` (Resend HTTP REST integration).

#### 4. `Seatly.API`
* REST controllers (`EventsController`, `BookingsController`, `UsersController`).
* Global exception handling middleware for uniform JSON responses.
* `ExpiredBookingCleanupService` (`IHostedService`) background process.

---

## 3. Design Patterns Applied

### A. Strategy Pattern (`IDiscountStrategy`)
Provides an open/closed structure for calculating ticket discounts without modifying core domain logic:
* `EarlyBirdDiscount`: 10% discount for bookings made >30 days in advance.
* `VIPDiscount`: 20% discount for VIP account holders.
* `BulkDiscount`: 15% discount applied automatically when reserving 5 or more seats.

### B. Factory Pattern (`EventFactory`)
Simplifies creation of specialized event types (`Concert`, `Conference`) based on `EventType`.

### C. Repository & Unit of Work Pattern
* **Repository Pattern:** Abstracts EF Core database interactions. Read queries utilize `.AsNoTracking()` for performance while write operations track entities.
* **Unit of Work Pattern (`IUnitOfWork`):** Guarantees transaction atomicity across multiple repository operations via `SaveChangesAsync()`.

---

## 4. Production Infrastructure & Cloud Deployment

### A. Backend Deployment (Render & Docker)
* **Multi-Stage Dockerfile:** Built on `mcr.microsoft.com/dotnet/sdk:10.0` and deployed on `aspnet:10.0` runtime on Render exposing port `8080`.
* **PostgreSQL Session Pooling:** Connects to Supabase PostgreSQL via IPv4 Session Pooler (`aws-0-eu-central-1.pooler.supabase.com:5432`).
* **Email Service:** Resend API integration for automated email notifications.

### B. 24/7 Availability (GitHub Actions Keep-Alive Workflow)
Render free instances spin down after 15 minutes of inactivity. To eliminate spin-up delays (up to 50 seconds), a GitHub Actions workflow (`.github/workflows/keep_alive.yml`) pings the backend every 10 minutes:

```yaml
name: Render Backend Keep-Alive

on:
  schedule:
    - cron: '*/10 * * * *'
  workflow_dispatch:

jobs:
  ping:
    runs-on: ubuntu-latest
    steps:
      - name: Ping Render Backend Endpoint
        run: |
          curl -s -o /dev/null -w "%{http_code}" https://seatlybackend.onrender.com/api/events
```

### C. Frontend Hosting (Vercel & Cloudflare)
* **Vercel SPA Hosting:** React 18 single-page application configured with `vercel.json` rewrite rules for client-side routing.
* **Cloudflare DNS:** SSL/TLS encryption and CNAME flattening pointing `seatly.hodzicmirza.com` to Vercel edge nodes.

---

## 5. Local Setup & Execution Instructions

### Prerequisites:
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [Node.js v18+](https://nodejs.org/)

### 1. Run Backend Web API:
```bash
cd src/Seatly.API
dotnet run
```
> API available at: `http://localhost:5051` (Swagger UI at `http://localhost:5051/swagger`).

### 2. Run Frontend Web App:
```bash
cd Frontend
npm install
npm run dev
```
> Web application available at: `http://localhost:5173`.

### 3. Execute Unit Test Suite:
```bash
cd tests/Seatly.Tests
dotnet test
```

---

*Developed by Mirza Hodžić — 2026*
