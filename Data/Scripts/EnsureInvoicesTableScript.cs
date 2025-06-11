using Microsoft.EntityFrameworkCore;
using QL_Spa.Data;
using System;
using System.Threading.Tasks;

namespace QL_Spa.Data.Scripts
{
    public class EnsureInvoicesTableScript
    {
        private readonly SpaDbContext _context;
        private readonly ILogger<EnsureInvoicesTableScript> _logger;

        public EnsureInvoicesTableScript(SpaDbContext context, ILogger<EnsureInvoicesTableScript> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                _logger.LogInformation("Checking if Invoices table exists...");
                
                // Check if table exists
                bool tableExists = await CheckIfTableExistsAsync("Invoices");
                
                if (!tableExists)
                {
                    _logger.LogWarning("Invoices table not found. Creating it...");
                    await CreateInvoicesTableAsync();
                    _logger.LogInformation("Invoices table created successfully.");
                }
                else
                {
                    _logger.LogInformation("Invoices table exists.");
                    
                    // Check if Status column exists
                    bool statusColumnExists = await CheckIfColumnExistsAsync("Invoices", "Status");
                    if (!statusColumnExists)
                    {
                        _logger.LogWarning("Status column not found in Invoices table. Adding it...");
                        await AddStatusColumnAsync();
                        _logger.LogInformation("Status column added successfully.");
                    }
                }

                // Check if InvoiceServices table exists
                bool invoiceServicesTableExists = await CheckIfTableExistsAsync("InvoiceServices");
                if (!invoiceServicesTableExists)
                {
                    _logger.LogWarning("InvoiceServices table not found. Creating it...");
                    await CreateInvoiceServicesTableAsync();
                    _logger.LogInformation("InvoiceServices table created successfully.");
                }
                else
                {
                    // Check if Price column exists in InvoiceServices
                    bool priceColumnExists = await CheckIfColumnExistsAsync("InvoiceServices", "Price");
                    if (!priceColumnExists)
                    {
                        _logger.LogWarning("Price column not found in InvoiceServices table. Adding it...");
                        await AddPriceColumnToInvoiceServicesAsync();
                        _logger.LogInformation("Price column added successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring Invoices table exists");
            }
        }

        private async Task<bool> CheckIfTableExistsAsync(string tableName)
        {
            try 
            {
                // Truy vấn đơn giản hơn sử dụng văn bản SQL thuần túy
                string sql = $@"
                    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}')
                        SELECT 1
                    ELSE
                        SELECT 0";

                // Thực thi SQL thuần không sử dụng tham số
                var result = await _context.Database.ExecuteSqlRawAsync(sql);
                _logger.LogInformation($"Kiểm tra bảng {tableName}: {result == 1}");
                return result == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi kiểm tra tồn tại bảng {tableName}");
                // Trong trường hợp lỗi, mặc định trả về true để tránh việc tạo lại bảng
                return true;
            }
        }

        private async Task<bool> CheckIfColumnExistsAsync(string tableName, string columnName)
        {
            try 
            {
                string sql = $@"
                    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}')
                        SELECT 1
                    ELSE
                        SELECT 0";

                var result = await _context.Database.ExecuteSqlRawAsync(sql);
                _logger.LogInformation($"Kiểm tra cột {columnName} trong bảng {tableName}: {result == 1}");
                return result == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi kiểm tra tồn tại cột {columnName} trong bảng {tableName}");
                return false;
            }
        }

        private async Task CreateInvoicesTableAsync()
        {
            string sql = @"
                CREATE TABLE [dbo].[Invoices](
                    [InvoiceId] [int] IDENTITY(1,1) NOT NULL,
                    [CreatedDate] [datetime2](7) NOT NULL,
                    [TotalAmount] [decimal](18, 2) NOT NULL,
                    [Discount] [decimal](18, 2) NOT NULL,
                    [FinalAmount] AS ([TotalAmount]-([TotalAmount]*[Discount])/(100)),
                    [PaidAmount] [decimal](18, 2) NOT NULL,
                    [CustomerId] [int] NOT NULL,
                    [Status] [nvarchar](50) NOT NULL,
                    CONSTRAINT [PK_Invoices] PRIMARY KEY CLUSTERED ([InvoiceId] ASC)
                );
                
                ALTER TABLE [dbo].[Invoices] WITH CHECK ADD CONSTRAINT [FK_Invoices_Customers_CustomerId] 
                FOREIGN KEY([CustomerId]) REFERENCES [dbo].[Customers] ([CustomerId]) ON DELETE CASCADE;
                
                ALTER TABLE [dbo].[Invoices] CHECK CONSTRAINT [FK_Invoices_Customers_CustomerId];";

            await _context.Database.ExecuteSqlRawAsync(sql);
        }

        private async Task AddStatusColumnAsync()
        {
            string sql = @"
                ALTER TABLE [dbo].[Invoices]
                ADD [Status] [nvarchar](50) NOT NULL DEFAULT N'Chờ thanh toán';";

            await _context.Database.ExecuteSqlRawAsync(sql);
        }

        private async Task CreateInvoiceServicesTableAsync()
        {
            string sql = @"
                CREATE TABLE [dbo].[InvoiceServices](
                    [InvoiceId] [int] NOT NULL,
                    [ServiceId] [int] NOT NULL,
                    [Quantity] [int] NOT NULL,
                    [Price] [decimal](18, 2) NOT NULL,
                    CONSTRAINT [PK_InvoiceServices] PRIMARY KEY CLUSTERED ([InvoiceId] ASC, [ServiceId] ASC)
                );
                
                ALTER TABLE [dbo].[InvoiceServices] WITH CHECK ADD CONSTRAINT [FK_InvoiceServices_Invoices_InvoiceId] 
                FOREIGN KEY([InvoiceId]) REFERENCES [dbo].[Invoices] ([InvoiceId]) ON DELETE CASCADE;
                
                ALTER TABLE [dbo].[InvoiceServices] CHECK CONSTRAINT [FK_InvoiceServices_Invoices_InvoiceId];
                
                ALTER TABLE [dbo].[InvoiceServices] WITH CHECK ADD CONSTRAINT [FK_InvoiceServices_Services_ServiceId] 
                FOREIGN KEY([ServiceId]) REFERENCES [dbo].[Services] ([ServiceId]) ON DELETE CASCADE;
                
                ALTER TABLE [dbo].[InvoiceServices] CHECK CONSTRAINT [FK_InvoiceServices_Services_ServiceId];";

            await _context.Database.ExecuteSqlRawAsync(sql);
        }

        private async Task AddPriceColumnToInvoiceServicesAsync()
        {
            string sql = @"
                ALTER TABLE [dbo].[InvoiceServices]
                ADD [Price] [decimal](18, 2) NOT NULL DEFAULT 0;";

            await _context.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
