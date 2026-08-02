# تطبیق پروژه با Interview test 20251011

## انجام‌شده

- سه سرویس مستقل Payment، Gateway و Notification
- ساختار یکسان چهارلایه برای هر سرویس: Api، Application، Domain، Infrastructure
- APIهای GetToken، Verify، UpdateStatus و Gateway Pay
- وضعیت‌های Pending، Success، Failed و Expired
- RabbitMQ publisher/consumer و رویدادهای پردازش و انقضا
- Quartz Job هر ۳۰ ثانیه با مهلت دو دقیقه
- SQL Server و EF Core Code First برای هر سه سرویس
- دیتابیس‌های مستقل `FadakPayments`، `FadakGateway` و `FadakNotifications`
- جداول `Transactions`، `PaymentLogs` و `NotificationLogs`
- Connection String توسعه مبتنی بر SQL Server لوکال و Integrated Security
- Docker Compose برای SQL Server، RabbitMQ و هر سه سرویس
- Global Exception Middleware و Data Annotation Validation در Payment Service
- HttpClient برای ارتباط Gateway با Payment Service
- Callback همراه Retry در Notification Service

## مواردی که هنوز باقی مانده یا اختیاری است

- Unit Test و Integration Test در مستند اختیاری است و هنوز پروژه تست اضافه نشده است.
- MediatR در فهرست فناوری‌های مستند آمده، اما جریان فعلی بر Application Service و Repository استوار است و هنوز Command/Query Handlerهای MediatR پیاده‌سازی نشده‌اند.
- Migrationهای نسخه‌بندی‌شده EF Core در Repository قرار نگرفته‌اند؛ دیتابیس‌ها در Startup با `EnsureCreatedAsync` به روش Code First ایجاد می‌شوند. برای محیط Production بهتر است Migration تولید و `MigrateAsync` استفاده شود.
- Authentication/Authorization برای Internal APIهای Payment Service در مستند الزام نشده و پیاده‌سازی نشده است.
- Outbox Pattern و Dead-letter topology کامل برای RabbitMQ در مستند الزام نشده و پیاده‌سازی نشده است.
- Gateway در مستند می‌تواند صفحه بانکی را شبیه‌سازی کند؛ خروجی فعلی JSON است و UI HTML پرداخت ندارد.

## دستور ساخت Migration پیشنهادی

```bash
dotnet ef migrations add InitialPayment \
  --project PaymentService/PaymentService.Infrastructure \
  --startup-project PaymentService/PaymentService.Api \
  --context PaymentDbContext

dotnet ef migrations add InitialGateway \
  --project GatewayService/GatewayService.Infrastructure \
  --startup-project GatewayService/GatewayService.Api \
  --context GatewayDbContext

dotnet ef migrations add InitialNotification \
  --project NotificationService/NotificationService.Infrastructure \
  --startup-project NotificationService/NotificationService.Api \
  --context NotificationDbContext
```
