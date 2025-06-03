using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QL_Spa.Migrations
{
    /// <inheritdoc />
    public partial class AddModifiedDateToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "Appointments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "Appointments");
        }
    }
}
