using Microsoft.EntityFrameworkCore.Migrations;

namespace QL_Spa.Data.Migrations
{
    public partial class AddStatusToInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First check if Status column already exists
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Invoices' 
                    AND COLUMN_NAME = 'Status'
                )
                BEGIN
                    ALTER TABLE Invoices
                    ADD Status nvarchar(50) NOT NULL DEFAULT N'Chờ thanh toán'
                END
            ");
            
            // Then check if Price column exists in InvoiceServices
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'InvoicesServices' 
                    AND COLUMN_NAME = 'Price'
                )
                BEGIN
                    ALTER TABLE InvoicesServices
                    ADD Price decimal(18, 2) NOT NULL DEFAULT 0
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No down migration needed as these are required columns
        }
    }
}
