using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ecommerce.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreSeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Shirts");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Sweaters");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 4, "Hoodies" },
                    { 5, "Jeans" },
                    { 6, "Shorts" }
                });

            migrationBuilder.InsertData(
                table: "Customer",
                columns: new[] { "Id", "Email", "Name" },
                values: new object[,]
                {
                    { 3, "paco@example.com", "Paco Sanchez" },
                    { 4, "ana@example.com", "Ana Lopez" }
                });

            migrationBuilder.UpdateData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 2,
                column: "CurrentStock",
                value: 18);

            migrationBuilder.UpdateData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 3,
                column: "CurrentStock",
                value: 15);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CategoryId", "Color", "Description", "Name", "Price", "Size", "Sku" },
                values: new object[] { 1, "White", "Plain everyday t-shirt", "Basic Tee", 300m, 1, "TS-002" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CategoryId", "Description", "Name", "Price", "Size", "Sku" },
                values: new object[] { 2, "Casual button-up shirt", "Button-Up Shirt", 550m, 2, "SH-001" });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Color", "Description", "Name", "Price", "Size", "Sku" },
                values: new object[,]
                {
                    { 4, 2, "Beige", "Lightweight linen shirt", "Linen Shirt", 650m, 3, "SH-002" },
                    { 5, 3, "Cream", "Soft knit crewneck sweater", "Crewneck Sweater", 750m, 2, "SW-001" },
                    { 6, 3, "Brown", "Warm turtleneck sweater", "Turtleneck Sweater", 850m, 3, "SW-002" }
                });

            migrationBuilder.InsertData(
                table: "Inventory",
                columns: new[] { "Id", "CurrentStock", "ProductId" },
                values: new object[,]
                {
                    { 4, 10, 4 },
                    { 5, 12, 5 },
                    { 6, 9, 6 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Color", "Description", "Name", "Price", "Size", "Sku" },
                values: new object[,]
                {
                    { 7, 4, "Gray", "Cotton blend oversized hoodie", "Oversized Hoodie", 800m, 3, "HD-001" },
                    { 8, 4, "Navy", "Comfortable zip-up hoodie", "Zip-Up Hoodie", 850m, 2, "HD-002" },
                    { 9, 5, "Blue", "Classic dark denim jeans", "Dark Denim Jeans", 600m, 4, "JN-001" },
                    { 10, 6, "Khaki", "Lightweight casual shorts", "Casual Shorts", 450m, 2, "ST-001" }
                });

            migrationBuilder.InsertData(
                table: "Inventory",
                columns: new[] { "Id", "CurrentStock", "ProductId" },
                values: new object[,]
                {
                    { 7, 12, 7 },
                    { 8, 11, 8 },
                    { 9, 8, 9 },
                    { 10, 14, 10 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Customer",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Customer",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Hoodies");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Jeans");

            migrationBuilder.UpdateData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 2,
                column: "CurrentStock",
                value: 12);

            migrationBuilder.UpdateData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 3,
                column: "CurrentStock",
                value: 8);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CategoryId", "Color", "Description", "Name", "Price", "Size", "Sku" },
                values: new object[] { 2, "Gray", "80% cotton and 20% polyester oversized hoodie", "Oversized Hoodie", 800.00m, 3, "HD-001" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CategoryId", "Description", "Name", "Price", "Size", "Sku" },
                values: new object[] { 3, "95% cotton and 5% polyester jean", "Dark Denim Jeans", 600.00m, 4, "JN-001" });
        }
    }
}
