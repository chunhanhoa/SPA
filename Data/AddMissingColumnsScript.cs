using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace QL_Spa.Data
{
    public class AddMissingColumnsScript
    {
        private readonly SpaDbContext _context;
        private readonly ILogger<AddMissingColumnsScript> _logger;

        public AddMissingColumnsScript(SpaDbContext context, ILogger<AddMissingColumnsScript> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                _logger.LogInformation("Executing script to add missing columns");
                
                // Create the Rooms table if it doesn't exist
                string createTableScript = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Rooms')
                    BEGIN
                        CREATE TABLE [dbo].[Rooms] (
                            [RoomId] INT IDENTITY(1, 1) NOT NULL,
                            [RoomName] NVARCHAR(100) NOT NULL,
                            [IsAvailable] BIT NOT NULL DEFAULT 1,
                            [RoomType] NVARCHAR(50) NULL DEFAULT 'Standard',
                            [Price] DECIMAL(18, 2) NOT NULL DEFAULT 0,
                            CONSTRAINT [PK_Rooms] PRIMARY KEY ([RoomId])
                        );
                        PRINT 'Created Rooms table';
                    END
                ";
                
                await _context.Database.ExecuteSqlRawAsync(createTableScript);
                
                // Add the missing columns if they don't exist
                string alterTableScript = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Rooms' AND COLUMN_NAME = 'RoomType')
                    BEGIN
                        ALTER TABLE Rooms ADD RoomType NVARCHAR(50) NULL DEFAULT 'Standard';
                        PRINT 'Added RoomType column';
                    END

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Rooms' AND COLUMN_NAME = 'Price')
                    BEGIN
                        ALTER TABLE Rooms ADD Price DECIMAL(18, 2) NOT NULL DEFAULT 0;
                        PRINT 'Added Price column';
                    END

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Rooms' AND COLUMN_NAME = 'IsAvailable')
                    BEGIN
                        ALTER TABLE Rooms ADD IsAvailable BIT NOT NULL DEFAULT 1;
                        PRINT 'Added IsAvailable column';
                    END
                ";
                
                await _context.Database.ExecuteSqlRawAsync(alterTableScript);
                
                _logger.LogInformation("Successfully executed script to add missing columns");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing script to add missing columns");
                throw;
            }
        }
    }
}
