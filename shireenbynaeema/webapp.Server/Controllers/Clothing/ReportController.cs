namespace Server
{
    using System.Collections;
    using System.Dynamic;
    using Application;
    using Domain;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using QuestPDF.Fluent;
    using QuestPDF.Helpers;
    using QuestPDF.Infrastructure;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService reportService;

        public ReportController(IReportService reportService) { this.reportService = reportService; }

        [HttpPost("Sales")]
        public async Task<ActionResult> GetSales(ReportFilterRequest filter)
        {
            var response = await reportService.GetSalesReport(filter);
            return Ok(response);
        }

        [HttpPost("Category")]
        public async Task<ActionResult> GetCategory(ReportFilterRequest filter)
        {
            var response = await reportService.GetCategoryReport(filter);
            return Ok(response);
        }

        [HttpPost("Product")]
        public async Task<ActionResult> GetProduct(ReportFilterRequest filter)
        {
            var response = await reportService.GetProductReport(filter);
            return Ok(response);
        }

        [HttpPost("Export/Sales")]
        public async Task<ActionResult> ExportSales(ReportFilterRequest filter)
        {
            var response = await reportService.GetSalesReport(filter);
            if (!response.IsSuccess) return BadRequest(response);

            var result = (SalesReportResult)response.Data!;
            var pdf = GenerateSalesPdf(result.Items, result.Summary);
            return File(pdf, "application/pdf", "sales-report.pdf");
        }

        [HttpPost("Export/Category")]
        public async Task<ActionResult> ExportCategory(ReportFilterRequest filter)
        {
            var response = await reportService.GetCategoryReport(filter);
            if (!response.IsSuccess) return BadRequest(response);

            var result = (CategoryReportResult)response.Data!;
            var pdf = GenerateCategoryPdf(result.Items);
            return File(pdf, "application/pdf", "category-report.pdf");
        }

        [HttpPost("Export/Product")]
        public async Task<ActionResult> ExportProduct(ReportFilterRequest filter)
        {
            var response = await reportService.GetProductReport(filter);
            if (!response.IsSuccess) return BadRequest(response);

            var result = (ProductReportResult)response.Data!;
            var pdf = GenerateProductPdf(result.Items);
            return File(pdf, "application/pdf", "product-report.pdf");
        }

        private static byte[] GenerateSalesPdf(List<ReportSalesRow> items, ReportSummary summary)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.MarginHorizontal(40);
                    page.MarginVertical(30);

                    page.Header().Element(header =>
                    {
                        header.Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("SHIREEN BY NAEEMA").FontSize(18).FontColor("#1a1a1a").SemiBold();
                                col.Item().Text("Sales Report").FontSize(12).FontColor("#666666");
                                col.Item().Text($"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC").FontSize(9).FontColor("#999999");
                            });
                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().Text($"Rs. {summary.TotalRevenue:N0}").FontSize(14).FontColor("#1a1a1a").SemiBold();
                                col.Item().Text($"Revenue").FontSize(9).FontColor("#999999");
                                col.Item().Text($"{summary.TotalOrders} orders  |  {summary.TotalItems} items").FontSize(9).FontColor("#666666");
                            });
                        });
                    });

                    page.Content().Element(content =>
                    {
                        content.PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(0.7f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1f);
                            });

                            table.Header(header =>
                            {
                                foreach (var h in new[] { "Order #", "Date", "Product", "Category", "Size", "Color", "Qty", "Unit Price", "Total", "Status" })
                                    header.Cell().Background("#f5f5f5").Padding(6).Text(h).FontSize(8).FontColor("#666666").SemiBold();
                            });

                            foreach (var item in items)
                            {
                                table.Cell().Padding(5).Text(item.OrderNumber).FontSize(8);
                                table.Cell().Padding(5).Text(item.OrderDate).FontSize(8).FontColor("#666666");
                                table.Cell().Padding(5).Text(item.ProductName).FontSize(8);
                                table.Cell().Padding(5).Text(item.Category).FontSize(8).FontColor("#666666");
                                table.Cell().Padding(5).Text(item.Size).FontSize(8).FontColor("#666666");
                                table.Cell().Padding(5).Text(item.Color).FontSize(8).FontColor("#666666");
                                table.Cell().Padding(5).Text(item.Quantity.ToString()).FontSize(8);
                                table.Cell().Padding(5).Text($"Rs. {item.UnitPrice:N0}").FontSize(8);
                                table.Cell().Padding(5).Text($"Rs. {item.Total:N0}").FontSize(8).SemiBold();
                                table.Cell().Padding(5).Text(item.Status).FontSize(8).FontColor("#666666");
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ").FontSize(8).FontColor("#999999");
                        text.CurrentPageNumber().FontSize(8).FontColor("#999999");
                    });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        private static byte[] GenerateCategoryPdf(List<ReportCategoryRow> items)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginHorizontal(50);
                    page.MarginVertical(40);

                    page.Header().Element(header =>
                    {
                        header.Column(col =>
                        {
                            col.Item().Text("SHIREEN BY NAEEMA").FontSize(18).FontColor("#1a1a1a").SemiBold();
                            col.Item().Text("Sales by Category").FontSize(12).FontColor("#666666");
                            col.Item().Text($"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC").FontSize(9).FontColor("#999999");
                        });
                    });

                    page.Content().Element(content =>
                    {
                        content.PaddingVertical(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                foreach (var h in new[] { "Category", "Orders", "Items Sold", "Revenue" })
                                    header.Cell().Background("#f5f5f5").Padding(8).Text(h).FontSize(9).FontColor("#666666").SemiBold();
                            });

                            foreach (var item in items)
                            {
                                table.Cell().Padding(8).Text(item.Category).FontSize(9);
                                table.Cell().Padding(8).Text(item.OrderCount.ToString()).FontSize(9);
                                table.Cell().Padding(8).Text(item.TotalQuantity.ToString()).FontSize(9);
                                table.Cell().Padding(8).Text($"Rs. {item.TotalRevenue:N0}").FontSize(9).SemiBold();
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ").FontSize(8).FontColor("#999999");
                        text.CurrentPageNumber().FontSize(8).FontColor("#999999");
                    });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        private static byte[] GenerateProductPdf(List<ReportProductRow> items)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.MarginHorizontal(40);
                    page.MarginVertical(30);

                    page.Header().Element(header =>
                    {
                        header.Column(col =>
                        {
                            col.Item().Text("SHIREEN BY NAEEMA").FontSize(18).FontColor("#1a1a1a").SemiBold();
                            col.Item().Text("Sales by Product").FontSize(12).FontColor("#666666");
                            col.Item().Text($"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC").FontSize(9).FontColor("#999999");
                        });
                    });

                    page.Content().Element(content =>
                    {
                        content.PaddingVertical(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                foreach (var h in new[] { "Product", "Orders", "Items Sold", "Revenue", "Avg Price" })
                                    header.Cell().Background("#f5f5f5").Padding(8).Text(h).FontSize(9).FontColor("#666666").SemiBold();
                            });

                            foreach (var item in items)
                            {
                                table.Cell().Padding(8).Text(item.ProductName).FontSize(9);
                                table.Cell().Padding(8).Text(item.OrderCount.ToString()).FontSize(9);
                                table.Cell().Padding(8).Text(item.TotalQuantity.ToString()).FontSize(9);
                                table.Cell().Padding(8).Text($"Rs. {item.TotalRevenue:N0}").FontSize(9).SemiBold();
                                table.Cell().Padding(8).Text($"Rs. {item.AveragePrice:N0}").FontSize(9).FontColor("#666666");
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ").FontSize(8).FontColor("#999999");
                        text.CurrentPageNumber().FontSize(8).FontColor("#999999");
                    });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }
    }
}
