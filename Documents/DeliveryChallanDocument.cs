using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SSProjectSolution.Request;
using System.Linq;

namespace SSProjectSolution.Documents
{
    public class DeliveryChallanDocument : IDocument
    {
        private readonly GenerateDcRequest _model;

        public DeliveryChallanDocument(GenerateDcRequest model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    // A5 Portrait
                    page.Size(PageSizes.A5);
                    page.Margin(5, Unit.Millimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(7).FontFamily(Fonts.Arial));

                   page.Content().Element(ComposeContent);
                });
        }

        void ComposeContent(IContainer container)
        {
            container.Border(1).BorderColor(Colors.Black).Column(column =>
            {
                // 1. Header
                column.Item().BorderBottom(1).BorderColor(Colors.Black).Row(row =>
                {
                    // Left - Company
                    row.RelativeItem().Padding(4).Row(innerRow =>
                    {
                        innerRow.RelativeItem().Column(c =>
                        {
                            c.Item().Text(string.IsNullOrEmpty(_model.CompanyName) ? "Jai Vishnu Clothings" : _model.CompanyName).FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
                            c.Item().Text("123, Textile Market,").FontSize(7);
                            c.Item().Text("Ring Road, Surat - 395002").FontSize(7);
                            c.Item().Text("Gujarat, India").FontSize(7);
                            c.Item().Text("Phone: 0261-1234567").FontSize(7);
                        });
                    });

                    // Middle - Title
                    row.RelativeItem().Padding(4).AlignCenter().AlignMiddle().Column(c =>
                    {
                        c.Item().Text("DELIVERY CHALLAN").FontSize(13).Bold().FontFamily(Fonts.TimesNewRoman);
                        c.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Black);
                    });

                    // Right - Details
                    row.ConstantItem(90).BorderLeft(1).BorderColor(Colors.Black).Column(c =>
                    {
                        c.Item().Padding(5).BorderBottom(1).BorderColor(Colors.Black).Row(r =>
                        {
                            r.RelativeItem().Text("DC No.").FontSize(7);
                            r.ConstantItem(10).Text(":");
                            r.RelativeItem().Text(_model.DcNo).FontSize(7);
                        });
                        c.Item().Padding(5).BorderBottom(1).BorderColor(Colors.Black).Row(r =>
                        {
                            r.RelativeItem().Text("DC Date").FontSize(7);
                            r.ConstantItem(10).Text(":");
                            r.RelativeItem().Text(_model.Date).FontSize(7);
                        });
                        c.Item().Padding(5).Row(r =>
                        {
                            r.RelativeItem().Text("Page No.").FontSize(7);
                            r.ConstantItem(10).Text(":");
                            r.RelativeItem().Text(text =>
                            {
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });
                        });
                    });
                });

                // 2. Info Panels
                column.Item().BorderBottom(1).BorderColor(Colors.Black).Row(row =>
                {
                    // COMPANY DETAILS
                    row.RelativeItem().BorderRight(1).BorderColor(Colors.Black).Padding(4).Column(c =>
                    {
                        c.Item().AlignCenter().Text("COMPANY DETAILS").Bold().FontSize(7);
                        c.Item().PaddingTop(5).PaddingBottom(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);

                        c.Item().PaddingTop(10).Row(r => { r.ConstantItem(55).Text("Company Name"); r.ConstantItem(10).Text(":"); r.RelativeItem().Text(_model.CompanyName); });
                        c.Item().PaddingTop(10).Row(r => { r.ConstantItem(55).Text("Style Number"); r.ConstantItem(10).Text(":"); r.RelativeItem().Text(_model.Style); });
                        c.Item().PaddingTop(10).Row(r => { r.ConstantItem(55).Text("Design Reference"); r.ConstantItem(10).Text(":"); r.RelativeItem().Text(_model.DesignReference); });
                    });

                    // ITEM DETAILS
                    row.RelativeItem().BorderRight(1).BorderColor(Colors.Black).Padding(4).Column(c =>
                    {
                        c.Item().AlignCenter().Text("ITEM DETAILS").Bold().FontSize(7);
                        c.Item().PaddingTop(5).PaddingBottom(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);

                        c.Item().PaddingTop(10).Row(r => { r.ConstantItem(55).Text("Item Type"); r.ConstantItem(10).Text(":"); r.RelativeItem().Text(_model.ItemType); });
                        c.Item().PaddingTop(10).Row(r =>
                        {
                            r.RelativeItem().Text("Lot Completed");
                            r.ConstantItem(10).Text(":");
                            r.RelativeItem()
                                .Border(1)
                                .BorderColor(Colors.Red.Medium)
                                .PaddingHorizontal(5)
                                .PaddingVertical(1)
                                .Text(_model.LotCompleted ? "Yes" : "No")
                                .FontColor(Colors.Red.Medium)
                                .Bold();
                        });
                    });

                    // CHALLAN DETAILS
                    row.RelativeItem().Padding(4).Column(c =>
                    {
                        c.Item().AlignCenter().Text("CHALLAN DETAILS").Bold().FontSize(7);
                        c.Item().PaddingTop(5).PaddingBottom(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                    });
                });

                // 3. Remarks
                column.Item().BorderBottom(1).BorderColor(Colors.Black).Padding(4).Row(row =>
                {
                    row.ConstantItem(50).Text("Remarks").Bold();
                    row.ConstantItem(10).Text(":");
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Medium).Padding(5).Text(_model.Remarks);
                });

                // 4. Size Breakdown Title
                column.Item().BorderBottom(1).BorderColor(Colors.Black).Padding(4).AlignCenter().Text("MULTI COLOUR SIZE BREAKDOWN (SIZE BASED)").Bold().FontSize(10);

                int sumTotal = 0;

                // 5. Size Breakdown Table
                column.Item().BorderBottom(1).BorderColor(Colors.Black).Table(table =>
                {
                    var allSizes = _model.ColourBreakdowns
                                                        .SelectMany(x => x.Sizes)
                                                        .Select(x => x.SizeName)
                                                        .Distinct()
                                                        .Take(10)
                                                        .ToList();

                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);
                        columns.RelativeColumn(3);

                        foreach (var size in allSizes)
                            columns.ConstantColumn(30);

                        columns.ConstantColumn(45);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Border(1).PaddingVertical(4).PaddingHorizontal(3).AlignCenter().Text("S.No.").FontSize(8).Bold();
                        header.Cell().Border(1).PaddingVertical(4).PaddingHorizontal(3).Text("Colour").FontSize(8).Bold();

                        foreach (var size in allSizes)
                        {
                            header.Cell()
                                .Border(1)
                                .PaddingVertical(4).PaddingHorizontal(3)
                                .AlignCenter()
                                .Text(size)
                                .FontSize(8)
                                .Bold();
                        }

                        header.Cell()
                            .Border(1)
                            .PaddingVertical(4).PaddingHorizontal(3)
                            .AlignCenter()
                            .Text("TOTAL")
                            .FontSize(8)
                            .Bold();
                    });

                    int rowNo = 1;
                    int grandTotal = 0;

                    var sizeTotals = allSizes.ToDictionary(x => x, x => 0);

                    foreach (var colour in _model.ColourBreakdowns)
                    {
                        table.Cell().Border(1).PaddingVertical(4).PaddingHorizontal(3).AlignCenter().Text(rowNo.ToString()).FontSize(8);

                        table.Cell().Border(1).PaddingVertical(4).PaddingHorizontal(3).Text(colour.ColourName).FontSize(8);

                        int rowTotal = 0;

                        foreach (var size in allSizes)
                        {
                            var qty = colour.Sizes
                                .FirstOrDefault(x => x.SizeName == size)?.Quantity ?? 0;

                            table.Cell()
                                .Border(1)
                                .PaddingVertical(4).PaddingHorizontal(3)
                                .AlignCenter()
                                .Text(qty > 0 ? qty.ToString() : "")
                                .FontSize(8);

                            rowTotal += qty;
                            sizeTotals[size] += qty;
                        }

                        table.Cell()
                            .Border(1)
                            .PaddingVertical(4).PaddingHorizontal(3)
                            .AlignCenter()
                            .Text(rowTotal.ToString())
                            .FontSize(8)
                            .Bold();

                        grandTotal += rowTotal;
                        rowNo++;
                    }

                    table.Cell()
                        .ColumnSpan(2)
                        .Border(1)
                        .PaddingVertical(4).PaddingHorizontal(3)
                        .AlignRight()
                        .Text("TOTAL (QTY)")
                        .FontSize(8)
                        .Bold();

                    foreach (var size in allSizes)
                    {
                        table.Cell()
                            .Border(1)
                            .PaddingVertical(4).PaddingHorizontal(3)
                            .AlignCenter()
                            .Text(sizeTotals[size].ToString())
                            .FontSize(8)
                            .Bold();
                    }

                    table.Cell()
                        .Border(1)
                        .PaddingVertical(4).PaddingHorizontal(3)
                        .AlignCenter()
                        .Text(grandTotal.ToString())
                        .FontSize(8)
                        .Bold();

                    sumTotal = grandTotal;
                });

                // 6. Grand Total Box
                column.Item().BorderBottom(1).BorderColor(Colors.Black).Padding(4).AlignCenter().Row(r =>
                {
                    r.AutoItem().Border(1).BorderColor(Colors.Grey.Medium).Padding(4).Row(inner =>
                    {
                        inner.AutoItem().AlignMiddle().Text("GRAND TOTAL QUANTITY : ").Bold().FontSize(10);
                        inner.AutoItem().AlignMiddle().Text(sumTotal.ToString()).FontColor(Colors.Green.Darken2).Bold().FontSize(10);
                        inner.AutoItem().AlignMiddle().PaddingLeft(5).Text(" PCS").FontSize(10);
                    });
                });

                // 7. Signatures
                column.Item().BorderBottom(1).BorderColor(Colors.Black).Row(row =>
                {
                    void DrawSignBox(IContainer c, string title)
                    {
                        c.Padding(4).Column(col =>
                        {
                            col.Item().AlignCenter().Text(title).Bold();
                            col.Item().PaddingTop(15).Text("Date: __________________").FontSize(5);
                        });
                    }

                    row.RelativeItem().BorderRight(1).BorderColor(Colors.Black).Element(c => DrawSignBox(c, "Prepared By"));
                    row.RelativeItem().BorderRight(1).BorderColor(Colors.Black).Element(c => DrawSignBox(c, "Checked By"));
                    row.RelativeItem().BorderRight(1).BorderColor(Colors.Black).Element(c => DrawSignBox(c, "Received By"));
                    row.RelativeItem().Element(c => DrawSignBox(c, "Authorized Sign"));
                });

                // 8. Notes
                column.Item().Padding(4).Column(c =>
                {
                    c.Item().Text("Notes:").Bold().FontSize(7);
                    c.Item().Text("1. Goods once sold will not be taken back.").FontSize(5);
                    c.Item().Text("2. Please verify all details carefully before accepting the challan.").FontSize(5);
                });
            });
        }
    }
}
