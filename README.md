# Fadak Payment Gateway

درگاه پرداخت آزمایشی مبتنی بر سه Microservice مستقل با ASP.NET Core، EF Core، SQL Server، RabbitMQ، Quartz.NET، HttpClient و MediatR.

## معماری و ساختار Solution

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

tests/
  PaymentService/
  GatewayService/
  NotificationService/
```

جهت وابستگی‌ها:

- `Api -> Application / Infrastructure`
- `Infrastructure -> Application -> Domain`
- `Domain` به هیچ لایه‌ای وابسته نیست.

Payment API درخواست‌ها را از طریق MediatR Command/Query و Handlerهای لایه Application پردازش می‌کند.

## سرویس‌ها

- Payment Service: `http://localhost:5001`
- Gateway Service: `http://localhost:5002`
- Notification Service: `http://localhost:5003`

Swagger:

- `http://localhost:5001/swagger`
- `http://localhost:5002/swagger`
- `http://localhost:5003/swagger`

## دیتابیس‌های Code First

هر سرویس DbContext و دیتابیس مستقل دارد:

- `FadakPayments` — جدول `Transactions`
- `FadakGateway` — جدول `PaymentLogs`
- `FadakNotifications` — جدول `NotificationLogs`

Migration اولیه برای هر سه DbContext در پروژه Infrastructure قرار دارد و هنگام Startup با `MigrateAsync` اجرا می‌شود. Initializer در صورت آماده نبودن SQL Server چند بار Retry می‌کند.

Connection String توسعه در `appsettings.Development.json` هر API قرار دارد.

## اجرای محلی

نیازمندی‌ها:

- .NET 9 SDK
- SQL Server
- RabbitMQ

```bash
dotnet restore
dotnet build fadak-task.sln

dotnet run --project src/PaymentService/PaymentService.Api --urls http://localhost:5001
dotnet run --project src/GatewayService/GatewayService.Api --urls http://localhost:5002
dotnet run --project src/NotificationService/NotificationService.Api --urls http://localhost:5003
```

## اجرای Docker

```bash
docker compose up --build
```

- RabbitMQ Management: `http://localhost:15672` با `guest / guest`
- SQL Server و RabbitMQ دارای Health Check هستند.
- سرویس‌ها پس از آماده شدن وابستگی‌ها اجرا می‌شوند.

## جریان پرداخت

1. `POST /api/payment/get-token`
2. انتقال کاربر به `GET /api/gateway/pay/{token}`
3. Gateway توکن را از Payment Service اعتبارسنجی می‌کند.
4. Gateway نتیجه ۸۰٪ موفق / ۲۰٪ ناموفق را شبیه‌سازی می‌کند.
5. Gateway با `POST /api/payment/update-status` وضعیت را ثبت می‌کند.
6. Payment Service رویداد RabbitMQ منتشر می‌کند.
7. Notification Service Callback را ارسال و نتیجه را Log می‌کند.
8. `POST /api/payment/verify` نتیجه نهایی را برمی‌گرداند.

Quartz Job هر ۳۰ ثانیه تراکنش‌های Pending قدیمی‌تر از دو دقیقه را Expired می‌کند.

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

## نمونه Verify

```http
POST http://localhost:5001/api/payment/verify
Content-Type: application/json

{
  "token": "TOKEN_FROM_GET_TOKEN",
  "appCode": "CUSTOMER-APP-01"
}
```

`appCode` الزامی است و در Transaction ذخیره می‌شود. اگر پرداخت هنوز در وضعیت Pending باشد، Verify پاسخ Pending برمی‌گرداند تا کلاینت بعداً دوباره وضعیت را بررسی کند.

## RabbitMQ و Callback

- پیام‌ها Persistent هستند.
- Publisher از Publisher Confirm و Retry محدود استفاده می‌کند.
- Notification Consumer از Manual Ack/Nack استفاده می‌کند.
- پیام‌های ناموفق به Dead Letter Queue منتقل می‌شوند.
- Callback دارای Retry است و موفقیت یا شکست آن در `NotificationLogs` ذخیره می‌شود.

## مدیریت خطا و Validation

- Payment و Gateway دارای Global Exception Middleware هستند.
- ورودی‌ها با Data Annotation اعتبارسنجی می‌شوند.
- Amount باید بزرگ‌تر از صفر باشد.
- PhoneNumber، URL، Token، RRN و AppCode اعتبارسنجی می‌شوند.

## تصمیمات معماری

- تفکیک هر سرویس به چهار لایه، وابستگی Domain به Frameworkها را حذف می‌کند.
- MediatR برای جدا کردن Endpointها از منطق Use Case در Payment Service استفاده شده است.
- HttpClient برای ارتباط همزمان Gateway با Payment و RabbitMQ برای رویدادهای ناهمزمان استفاده شده است.
- هر سرویس دیتابیس مستقل دارد تا مستقل اجرا و تست شود.
- Quartz.NET برای Job تکرارشونده انقضای پرداخت انتخاب شده است.

## چالش‌های پیاده‌سازی

- جلوگیری از تغییر دوباره تراکنشی که به وضعیت نهایی رسیده است.
- هماهنگی Update دیتابیس و انتشار رویداد RabbitMQ.
- مدیریت آماده نبودن SQL Server و RabbitMQ هنگام Startup در Docker.
- ثبت و Retry کردن Callbackهای ناموفق بدون Auto Ack کردن پیام.

## بهبودهای پیشنهادی با زمان بیشتر

- Outbox Pattern برای تضمین اتمیک بودن تغییر دیتابیس و انتشار Event
- Integration Test کامل با Testcontainers
- Authentication برای Internal APIها
- Correlation ID و Distributed Tracing
- Handle Idempotency and Race Condition
- محدودسازی Callback URLها برای محیط Production

## تست

```bash
dotnet test fadak-task.sln
```
