# Fadak Payment Gateway

درگاه پرداخت آزمایشی مبتنی بر سه Microservice مستقل و ساختار یکسان Clean/Layered Architecture.

## ساختار Solution

هر سرویس از چهار پروژه تشکیل شده است:

```text
<Service>.Api             Controllers, Program, appsettings
<Service>.Application     DTOs, Contracts, Options, Application services
<Service>.Domain          Entities, Enums, Events, Abstractions
<Service>.Infrastructure  EF Core, SQL Server, Repository, RabbitMQ, HttpClient, Jobs
```

سرویس‌ها:

- Payment Service روی پورت 5001
- Gateway Service روی پورت 5002
- Notification Service روی پورت 5003

## دیتابیس‌های مستقل Code First

- `FadakPayments` با جدول `Transactions`
- `FadakGateway` با جدول `PaymentLogs`
- `FadakNotifications` با جدول `NotificationLogs`

هر سرویس DbContext و Repository مستقل دارد. در Startup، `EnsureCreatedAsync` مدل EF Core را به دیتابیس تبدیل می‌کند. Connection Stringها در `appsettings.json` و `appsettings.Development.json` هر Api قرار دارند.

Connection String توسعه از SQL Server لوکال با Integrated Security استفاده می‌کند و برای هر سرویس `Initial Catalog` مستقل دارد.

## اجرای محلی

نیازمندی‌ها: .NET 9 SDK، SQL Server و RabbitMQ.

```bash
dotnet restore
dotnet build fadak-task.sln

dotnet run --project PaymentService/PaymentService.Api --urls http://localhost:5001
dotnet run --project GatewayService/GatewayService.Api --urls http://localhost:5002
dotnet run --project NotificationService/NotificationService.Api --urls http://localhost:5003
```

## اجرای Docker

```bash
docker compose up --build
```

- RabbitMQ Management: `http://localhost:15672` (`guest` / `guest`)
- Payment API: `http://localhost:5001`
- Gateway API: `http://localhost:5002`
- Notification Health: `http://localhost:5003/health`

## جریان پرداخت

1. `POST /api/payment/get-token`
2. `GET /api/gateway/pay/{token}`
3. Gateway نتیجه را با `POST /api/payment/update-status` ثبت می‌کند.
4. Payment Service رویداد RabbitMQ منتشر می‌کند.
5. Notification Service رویداد را Log کرده و Callback می‌فرستد.
6. `POST /api/payment/verify` نتیجه نهایی را برمی‌گرداند.

## نمونه GetToken

```http
POST http://localhost:5001/api/payment/get-token
Content-Type: application/json

{
  "terminalNo": "10001",
  "amount": 250000,
  "redirectUrl": "https://webhook.site/your-id",
  "reservationNumber": "RES-1001",
  "phoneNumber": "09121234567"
}
```

جزئیات تطبیق با مستند و موارد باقی‌مانده در [REVIEW.md](REVIEW.md) آمده است.

## Solution structure

The solution follows the same Clean Architecture layout for all three services:

```text
src/
  PaymentService/
    PaymentService.Api
    PaymentService.Application
    PaymentService.Domain
    PaymentService.Infrastructure
  GatewayService/
    GatewayService.Api
    GatewayService.Application
    GatewayService.Domain
    GatewayService.Infrastructure
  NotificationService/
    NotificationService.Api
    NotificationService.Application
    NotificationService.Domain
    NotificationService.Infrastructure
```

Dependencies follow this direction: `Api -> Infrastructure/Application`, `Infrastructure -> Application`, and `Application -> Domain`.

## Swagger

After running the services, Swagger UI is available at:

- Payment Service: `http://localhost:5001/swagger`
- Gateway Service: `http://localhost:5002/swagger`
- Notification Service: `http://localhost:5003/swagger`

## Local SQL Server Code First

Each service has its own SQL Server database and initializes its schema from the EF Core model on startup:

- `FadakPayments`
- `FadakGateway`
- `FadakNotifications`

Development connection strings are stored in each API project's `appsettings.Development.json`.
