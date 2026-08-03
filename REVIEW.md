# تطبیق پروژه با Interview test 20251011

## وضعیت نهایی

نیازمندی‌های اصلی مستند در پروژه پیاده‌سازی شده‌اند:

- سه سرویس مستقل Payment، Gateway و Notification
- APIهای GetToken، Verify، UpdateStatus و Gateway Pay
- وضعیت‌های Pending، Success، Failed و Expired
- EF Core Code First با Migration اولیه و `MigrateAsync`
- SQL Server و دیتابیس مستقل هر سرویس
- RabbitMQ Publisher/Consumer، Publisher Confirm، Retry و Dead Letter Queue
- Quartz Job هر ۳۰ ثانیه و انقضای دو دقیقه‌ای
- HttpClient برای ارتباط Gateway با Payment Service
- MediatR Command/Query Handler در Payment Service
- Data Annotation Validation و Global Exception Handling
- Callback و ثبت موفقیت/شکست Notification
- Swagger، Docker Compose و README کامل

## موارد اختیاری یا خارج از الزام مستند

- تست‌ها در مستند اختیاری هستند؛ تست‌های Domain پایه وجود دارند ولی Integration Test کامل اضافه نشده است.
- Notification Service در مستند اختیاری است اما در این پروژه پیاده‌سازی شده است.
- Outbox Pattern، Authentication داخلی و Distributed Tracing الزام مستند نیستند و به‌عنوان بهبود آینده در README آمده‌اند.
- Gateway مطابق نمونه مستند JSON برمی‌گرداند و صفحه HTML بانکی جداگانه ندارد.

## ساخت Migration جدید در تغییرات بعدی

```bash
dotnet ef migrations add MigrationName \
  --project src/PaymentService/PaymentService.Infrastructure \
  --startup-project src/PaymentService/PaymentService.Api \
  --context PaymentDbContext

dotnet ef migrations add MigrationName \
  --project src/GatewayService/GatewayService.Infrastructure \
  --startup-project src/GatewayService/GatewayService.Api \
  --context GatewayDbContext

dotnet ef migrations add MigrationName \
  --project src/NotificationService/NotificationService.Infrastructure \
  --startup-project src/NotificationService/NotificationService.Api \
  --context NotificationDbContext
```
