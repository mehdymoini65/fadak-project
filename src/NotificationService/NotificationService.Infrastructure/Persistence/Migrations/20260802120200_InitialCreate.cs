using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NotificationService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(NotificationDbContext))]
[Migration("20260802120200_InitialCreate")]
public sealed class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "NotificationLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                CallbackUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CallbackSucceeded = table.Column<bool>(type: "bit", nullable: false),
                AttemptCount = table.Column<int>(type: "int", nullable: false),
                ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_NotificationLogs", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_NotificationLogs_CreatedAt", table: "NotificationLogs", column: "CreatedAt");
        migrationBuilder.CreateIndex(name: "IX_NotificationLogs_Token", table: "NotificationLogs", column: "Token");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "NotificationLogs");
}
