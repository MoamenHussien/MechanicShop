using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MechanicShop.Infrastructure.Services;

public sealed class InvoicePdfGenerator : IInvoicePdfGenerator
{
    private static readonly CultureInfo UsCulture = new("en-US");

    public byte[] Generate(Invoice invoice)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor("#0F172A"));

                page.Header().Element(BuildHeader(invoice));
                page.Content().Element(BuildInvoiceContent(invoice));
                page.Footer().Element(BuildFooter());
            });
        })
        .GeneratePdf();
    }

    private Action<IContainer> BuildHeader(Invoice invoice) => header =>
    {
        header.Column(col =>
        {
            // Top Header Row: Shop Logo & Info (Left) vs Invoice Details (Right)
            col.Item().Row(row =>
            {
                // Left: Company Logo & Brand Name
                row.RelativeItem(1).Column(companyCol =>
                {
                    companyCol.Item().Row(logoRow =>
                    {
                        // Logo Icon Container
                        logoRow.ConstantItem(42).Height(42)
                            .Background("#0F172A")
                            .CornerRadius(8)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text("🔧")
                            .FontSize(20)
                            .FontColor(Colors.White);

                        // Company Brand Name & Subtitle
                        logoRow.RelativeItem().PaddingLeft(12).AlignMiddle().Column(brandCol =>
                        {
                            brandCol.Item().Text(text =>
                            {
                                text.Span("MECHANIC").FontSize(18).Bold().FontColor("#0F172A");
                                text.Span("SHOP").FontSize(18).Bold().FontColor("#2563EB");
                            });

                            brandCol.Item().Text("AUTO REPAIR & WORKSHOP MANAGEMENT")
                                .FontSize(7)
                                .SemiBold()
                                .FontColor("#64748B")
                                .LetterSpacing(0.08f);
                        });
                    });

                    companyCol.Item().PaddingTop(8).Text("100 Auto Care Blvd, Suite 400 • Phone: +1 (555) 019-2831")
                        .FontSize(8)
                        .FontColor("#64748B");

                    companyCol.Item().Text("support@mechanicshop.com • www.mechanicshop.com")
                        .FontSize(8)
                        .FontColor("#64748B");
                });

                // Right: Invoice Title, ID, Date & Status
                row.RelativeItem(1).AlignRight().Column(detailsCol =>
                {
                    detailsCol.Item().Text("INVOICE")
                        .FontSize(26)
                        .ExtraBold()
                        .FontColor("#0F172A");

                    detailsCol.Item().PaddingTop(2).Text($"#INV-{invoice.Id.ToString().Substring(0, 8).ToUpper()}")
                        .FontSize(11)
                        .Bold()
                        .FontColor("#2563EB");

                    detailsCol.Item().PaddingTop(4).Text($"Date: {invoice.IssuedAtUtc.ToString("MMMM dd, yyyy", UsCulture)}")
                        .FontSize(9)
                        .FontColor("#475569");

                    // Status Badge Pill
                    detailsCol.Item().PaddingTop(6).AlignRight().Element(container =>
                    {
                        var status = invoice.Status.ToString().ToUpper();
                        var (bgColor, textColor) = GetStatusBadgeColors(status);

                        container
                            .Background(bgColor)
                            .CornerRadius(4)
                            .PaddingHorizontal(10)
                            .PaddingVertical(4)
                            .Text(status)
                            .FontSize(9)
                            .Bold()
                            .FontColor(textColor);
                    });
                });
            });

            // Divider line
            col.Item().PaddingVertical(16).LineHorizontal(1).LineColor("#E2E8F0");
        });
    };

    private Action<IContainer> BuildInvoiceContent(Invoice invoice) => content =>
    {
        content.Column(col =>
        {
            // Table of Repair Services & Line Items
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(5); // Description
                    columns.RelativeColumn(1); // Qty
                    columns.RelativeColumn(2); // Unit Price
                    columns.RelativeColumn(2); // Line Total
                });

                // Table Header Row
                table.Header(header =>
                {
                    header.Cell()
                        .Background("#1E293B")
                        .Padding(10)
                        .Text("DESCRIPTION / REPAIR SERVICE")
                        .Bold()
                        .FontColor(Colors.White)
                        .FontSize(9);

                    header.Cell()
                        .Background("#1E293B")
                        .Padding(10)
                        .AlignCenter()
                        .Text("QTY")
                        .Bold()
                        .FontColor(Colors.White)
                        .FontSize(9);

                    header.Cell()
                        .Background("#1E293B")
                        .Padding(10)
                        .AlignRight()
                        .Text("UNIT PRICE")
                        .Bold()
                        .FontColor(Colors.White)
                        .FontSize(9);

                    header.Cell()
                        .Background("#1E293B")
                        .Padding(10)
                        .AlignRight()
                        .Text("LINE TOTAL")
                        .Bold()
                        .FontColor(Colors.White)
                        .FontSize(9);
                });

                // Line Items Rows
                var isEvenRow = false;
                foreach (var item in invoice.InvoiceLineItems)
                {
                    var backgroundColor = isEvenRow ? "#F8FAFC" : "#FFFFFF";

                    table.Cell()
                        .Background(backgroundColor)
                        .Padding(10)
                        .BorderBottom(1)
                        .BorderColor("#E2E8F0")
                        .Text(item.Description)
                        .FontSize(9)
                        .FontColor("#1E293B");

                    table.Cell()
                        .Background(backgroundColor)
                        .Padding(10)
                        .BorderBottom(1)
                        .BorderColor("#E2E8F0")
                        .AlignCenter()
                        .Text(item.Quantity.ToString(UsCulture))
                        .FontSize(9)
                        .FontColor("#1E293B");

                    table.Cell()
                        .Background(backgroundColor)
                        .Padding(10)
                        .BorderBottom(1)
                        .BorderColor("#E2E8F0")
                        .AlignRight()
                        .Text(item.UnitPrice.ToString("C", UsCulture))
                        .FontSize(9)
                        .FontColor("#1E293B");

                    table.Cell()
                        .Background(backgroundColor)
                        .Padding(10)
                        .BorderBottom(1)
                        .BorderColor("#E2E8F0")
                        .AlignRight()
                        .Text(item.LineTotal.ToString("C", UsCulture))
                        .FontSize(9)
                        .FontColor("#0F172A")
                        .Bold();

                    isEvenRow = !isEvenRow;
                }
            });

            // Summary & Totals Box
            col.Item().PaddingTop(20).Row(row =>
            {
                row.RelativeItem(2); // Empty left spacing

                row.RelativeItem(1.5f).Column(totalsCol =>
                {
                    totalsCol.Item().PaddingVertical(3).Row(totalRow =>
                    {
                        totalRow.RelativeItem().Text("Subtotal:").FontSize(10).FontColor("#64748B");
                        totalRow.RelativeItem().AlignRight().Text(invoice.Subtotal.ToString("C", UsCulture)).FontSize(10).FontColor("#1E293B").SemiBold();
                    });

                    totalsCol.Item().PaddingVertical(3).Row(totalRow =>
                    {
                        totalRow.RelativeItem().Text("Tax (15%):").FontSize(10).FontColor("#64748B");
                        totalRow.RelativeItem().AlignRight().Text(invoice.TaxAmount.ToString("C", UsCulture)).FontSize(10).FontColor("#1E293B").SemiBold();
                    });

                    if (invoice.DiscountAmount > 0)
                    {
                        totalsCol.Item().PaddingVertical(3).Row(totalRow =>
                        {
                            totalRow.RelativeItem().Text("Discount:").FontSize(10).FontColor("#DC2626");
                            totalRow.RelativeItem().AlignRight().Text($"-{invoice.DiscountAmount.ToString("C", UsCulture)}").FontSize(10).FontColor("#DC2626").SemiBold();
                        });
                    }

                    // Total Highlight Box
                    totalsCol.Item().PaddingTop(8).Element(container =>
                    {
                        container
                            .Background("#0F172A")
                            .CornerRadius(6)
                            .Padding(10)
                            .Row(totalRow =>
                            {
                                totalRow.RelativeItem().AlignMiddle().Text("TOTAL").FontSize(12).Bold().FontColor(Colors.White);
                                totalRow.RelativeItem().AlignRight().AlignMiddle().Text(invoice.Total.ToString("C", UsCulture)).FontSize(14).ExtraBold().FontColor("#4ADE80");
                            });
                    });
                });
            });
        });
    };

    private Action<IContainer> BuildFooter() => footer =>
    {
        footer.Column(col =>
        {
            col.Item().PaddingBottom(8).LineHorizontal(1).LineColor("#E2E8F0");

            col.Item().Row(row =>
            {
                row.RelativeItem()
                    .AlignLeft()
                    .Text("Thank you for choosing MechanicShop! Drive safely and see you next time.")
                    .FontSize(8)
                    .FontColor("#64748B")
                    .Italic();

                row.RelativeItem()
                    .AlignRight()
                    .Text(text =>
                    {
                        text.Span("Generated on ").FontSize(8).FontColor("#94A3B8");
                        text.Span($"{DateTime.UtcNow.ToString("MMMM dd, yyyy 'at' HH:mm", UsCulture)} UTC")
                            .FontSize(8)
                            .FontColor("#64748B")
                            .SemiBold();
                    });
            });
        });
    };

    private static (string BgColor, string TextColor) GetStatusBadgeColors(string status)
    {
        return status switch
        {
            "PAID" => ("#DCFCE7", "#166534"),      // Soft Green Bg, Dark Green Text
            "UNPAID" => ("#FEF3C7", "#92400E"),    // Soft Amber Bg, Dark Amber Text
            "OVERDUE" => ("#FEE2E2", "#991B1B"),   // Soft Red Bg, Dark Red Text
            "CANCELLED" => ("#F1F5F9", "#475569"), // Soft Gray Bg, Dark Gray Text
            _ => ("#F1F5F9", "#475569"),
        };
    }

}
