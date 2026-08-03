using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GatewayService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(GatewayDbContext))]
[Migration("20260802120100_InitialCreate")]
public sealed class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PaymentLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                Rrn = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PaymentLogs", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_PaymentLogs_ProcessedAt", table: "PaymentLogs", column: "ProcessedAt");
        migrationBuilder.CreateIndex(name: "IX_PaymentLogs_Token", table: "PaymentLogs", column: "Token");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "PaymentLogs");
}
