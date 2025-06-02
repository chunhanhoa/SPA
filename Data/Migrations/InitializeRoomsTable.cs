using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace QL_Spa.Data.Migrations
{
    public partial class InitializeRoomsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if the Rooms table exists
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Rooms')
                BEGIN
                    CREATE TABLE [dbo].[Rooms] (
                        [RoomId] INT IDENTITY(1, 1) NOT NULL,
                        [RoomName] NVARCHAR(100) NOT NULL,
                        [RoomType] NVARCHAR(50) NULL DEFAULT 'Standard',
                        [Price] DECIMAL(18, 2) NOT NULL DEFAULT 0,
                        [IsAvailable] BIT NOT NULL DEFAULT 1,
                        CONSTRAINT [PK_Rooms] PRIMARY KEY ([RoomId])
                    );
                END
            ");

            // Check if RoomType column exists and add it if it doesn't
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Rooms' AND COLUMN_NAME = 'RoomType')
                BEGIN
                    ALTER TABLE [dbo].[Rooms] ADD [RoomType] NVARCHAR(50) NULL DEFAULT 'Standard';
                END
            ");

            // Check if Price column exists and add it if it doesn't
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Rooms' AND COLUMN_NAME = 'Price')
                BEGIN
                    ALTER TABLE [dbo].[Rooms] ADD [Price] DECIMAL(18, 2) NOT NULL DEFAULT 0;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // We don't want to drop these columns or tables if rolling back
        }
    }
}
