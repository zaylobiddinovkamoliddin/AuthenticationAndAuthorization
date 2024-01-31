using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Part_2_CreateUser.Migrations
{
    /// <inheritdoc />
    public partial class RoleAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "22bd7ed9-cbb5-4f8a-8c03-a7606f82d2f1", "3", "HR", "HR" },
                    { "56ec2caa-71d8-4ecb-bde1-179d5b668167", "1", "Admin", "Admin" },
                    { "be828f87-8004-4260-b16a-15cbc2d0b6dc", "2", "User", "User" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "22bd7ed9-cbb5-4f8a-8c03-a7606f82d2f1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "56ec2caa-71d8-4ecb-bde1-179d5b668167");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "be828f87-8004-4260-b16a-15cbc2d0b6dc");
        }
    }
}
