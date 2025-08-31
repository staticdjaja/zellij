using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace zellij.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 29, 15, 35, 16, 422, DateTimeKind.Local).AddTicks(5481));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 29, 15, 35, 16, 422, DateTimeKind.Local).AddTicks(5485));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 29, 15, 35, 16, 422, DateTimeKind.Local).AddTicks(5488));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 29, 15, 35, 16, 422, DateTimeKind.Local).AddTicks(5491));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 29, 15, 35, 16, 422, DateTimeKind.Local).AddTicks(5493));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 29, 15, 9, 0, 846, DateTimeKind.Local).AddTicks(81));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 29, 15, 9, 0, 846, DateTimeKind.Local).AddTicks(85));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 29, 15, 9, 0, 846, DateTimeKind.Local).AddTicks(89));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 29, 15, 9, 0, 846, DateTimeKind.Local).AddTicks(91));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 8, 29, 15, 9, 0, 846, DateTimeKind.Local).AddTicks(94));
        }
    }
}
