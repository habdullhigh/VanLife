using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanLife.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCautionFeeToRentals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CautionFee",
                table: "Rentals",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "Rentals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Days",
                table: "Rentals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Destination",
                table: "Rentals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Rentals",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Rentals",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPaid",
                table: "Rentals",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CautionFee",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Contact",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Days",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "Destination",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "TotalPaid",
                table: "Rentals");
        }
    }
}
