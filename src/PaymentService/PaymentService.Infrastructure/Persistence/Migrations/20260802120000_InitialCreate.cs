using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PaymentService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PaymentDbContext))]
[Migration("20260802120000_InitialCreate")]
public sealed class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Transactions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TerminalNo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                RedirectUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                ReservationNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Rrn = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                Status = table.Column<int>(type: "int", nullable: false),
                AppCode = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Transactions", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_Transactions_Status_CreatedAt", table: "Transactions", columns: new[] { "Status", "CreatedAt" });
        migrationBuilder.CreateIndex(name: "IX_Transactions_Token", table: "Transactions", column: "Token", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "Transactions");
}
