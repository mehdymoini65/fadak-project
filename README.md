# Fadak Payment Gateway

یک درگاه پرداخت آزمایشی مبتنی بر معماری Microservice شامل Payment Service، Gateway Service و Notification Service.

## معماری

- **Payment Service — port 5001**: ایجاد Token، Verify، نگهداری تراکنش‌ها، API داخلی Update Status، انتشار Event و Job انقضا.
- **Gateway Service — port 5002**: شبیه‌سازی پرداخت با احتمال موفقیت ۸۰٪، تولید RRN دوازده‌رقمی و اعلام نتیجه به Payment Service.
- **Notification Service — port 5003**: مصرف `PaymentProcessedEvent` و `PaymentExpiredEvent` از RabbitMQ، ثبت Log و ارسال Callback به `RedirectUrl`.
- **SQL Server**: دیتابیس مستقل Payment Service.
- **RabbitMQ**: ارتباط ناهمزمان مبتنی بر Topic Exchange.
- **Quartz.NET**: اجرای Job هر ۳۰ ثانیه و منقضی‌کردن Pendingهای قدیمی‌تر از ۲ دقیقه.

## تکنولوژی‌ها

.NET 9، ASP.NET Core Web API، Entity Framework Core، SQL Server، RabbitMQ، Quartz.NET و HttpClient.

## اجرای سریع با Docker

نیازمندی: Docker Desktop

```bash
docker compose up --build
```

آدرس‌ها:

- Payment Service: `http://localhost:5001`
- Gateway Service: `http://localhost:5002`
- Notification Service health: `http://localhost:5003/health`
- RabbitMQ Management: `http://localhost:15672` با نام کاربری و رمز `guest`

> در اجرای Docker مقدار `Gateway__BaseUrl` طوری تنظیم شده که URL برگشتی برای مرورگر کاربر `http://localhost:5002` باشد، در حالی که تماس داخلی Gateway با Payment Service از نام سرویس Docker استفاده می‌کند.

## اجرای محلی بدون Docker

1. SQL Server و RabbitMQ را اجرا کنید.
2. Connection String توسعه در `PaymentService/PaymentService.Api/appsettings.Development.json` قرار دارد و برای SQL Server لوکال با Integrated Security تنظیم شده است.
3. پروژه‌ها را در سه Terminal اجرا کنید:

```bash
dotnet run --project PaymentService/PaymentService.Api --urls http://localhost:5001
dotnet run --project GatewayService --urls http://localhost:5002
dotnet run --project NotificationService --urls http://localhost:5003
```

Database در شروع برنامه با `EnsureCreated` ساخته می‌شود.

## جریان تست

### 1. دریافت Token

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

### 2. شبیه‌سازی پرداخت

`gatewayUrl` پاسخ مرحله قبل را در مرورگر یا ابزار API فراخوانی کنید:

```http
GET http://localhost:5002/api/gateway/pay/{token}
```

### 3. Verify

```http
POST http://localhost:5001/api/payment/verify
Content-Type: application/json

{
  "token": "{token}",
  "appCode": "CLIENT-APP-01"
}
```

## Endpointها

### Payment Service

- `POST /api/payment/get-token`
- `POST /api/payment/verify`
- `POST /api/payment/update-status` — Internal
- `GET /api/payment/info/{token}` — Internal

### Gateway Service

- `GET /api/gateway/pay/{token}`

## تصمیمات طراحی

- Transition وضعیت با Conditional Update در دیتابیس انجام می‌شود تا درخواست‌های تکراری و Race Condition کنترل شوند.
- RabbitMQ به‌صورت Lazy متصل می‌شود تا نبود موقت Broker مانع Startup سرویس پرداخت نشود.
- Consumer با Manual Ack کار می‌کند؛ Callback ناموفق پس از Retry باعث Nack پیام می‌شود.
- Notification Service دیتابیس ندارد و فقط Eventها را پردازش می‌کند.

## بهبودهای پیشنهادی در زمان بیشتر

- استفاده از EF Core Migration به‌جای `EnsureCreated`.
- Outbox Pattern برای تضمین اتمیک‌بودن ذخیره تراکنش و انتشار Event.
- Dead-letter exchange و سیاست Retry کامل در RabbitMQ.
- Authentication برای APIهای داخلی و Rate Limiting.
- Integration Test با Testcontainers و Unit Test برای Transitionها.
- جایگزینی `RedirectUrl` با Callback URL مستقل برای جداسازی Redirect مرورگر از Server-to-server Callback.
