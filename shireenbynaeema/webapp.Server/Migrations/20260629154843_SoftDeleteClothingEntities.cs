using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapp.Server.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteClothingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Return",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedOn",
                table: "Return",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Return",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Product",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedOn",
                table: "Product",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Product",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Order",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedOn",
                table: "Order",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Order",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Invoice",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedOn",
                table: "Invoice",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Invoice",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Delivery",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedOn",
                table: "Delivery",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Delivery",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Customer",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedOn",
                table: "Customer",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Customer",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Category",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedOn",
                table: "Category",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Category",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Return_IsDeleted",
                table: "Return",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Product_IsDeleted",
                table: "Product",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Order_IsDeleted",
                table: "Order",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_IsDeleted",
                table: "Invoice",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_IsDeleted",
                table: "Delivery",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_IsDeleted",
                table: "Customer",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Category_IsDeleted",
                table: "Category",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Return_IsDeleted",
                table: "Return");

            migrationBuilder.DropIndex(
                name: "IX_Product_IsDeleted",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Order_IsDeleted",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Invoice_IsDeleted",
                table: "Invoice");

            migrationBuilder.DropIndex(
                name: "IX_Delivery_IsDeleted",
                table: "Delivery");

            migrationBuilder.DropIndex(
                name: "IX_Customer_IsDeleted",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Category_IsDeleted",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Return");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Return");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Return");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Delivery");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Delivery");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Delivery");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Category");
        }
    }
}
