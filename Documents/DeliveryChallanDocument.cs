using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SSProjectSolution.Request;
using System.Linq;
using System.Collections.Generic;

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
                    // A5 Landscape
                    page.Size(211, 142, Unit.Millimetre);
                    page.Margin(3, Unit.Millimetre);
                    page.PageColor(Colors.White);
                    // Increased base font size from 7 to 9
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial")); 

                    page.Header().AlignCenter().Text("DELIVERY CHALLAN (Not for Sale)").FontSize(10).Bold().FontColor("#111111");

                    page.Content().Element(ComposeMain);
                });
        }

        void ComposeMain(IContainer container)
        {
            container.Border(1).Padding(2).Column(column =>
            {
                column.Item().Element(ComposeHeader);
                column.Item().Element(ComposeContent);

                column.Item()
                      .ExtendVertical()
                      .AlignBottom()
                      .Element(ComposeFooter);
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    // Left Logo/Company details
                    row.RelativeItem().Row(innerRow =>
                    {
                        var logoPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.jpeg");
                        if (System.IO.File.Exists(logoPath))
                        {
                            innerRow.AutoItem().Height(40).Image(logoPath);
                        }
                        else
                        {
                            innerRow.ConstantItem(40).Height(40).Placeholder(); // Placeholder for Logo
                        }
                        
                        innerRow.RelativeItem().PaddingLeft(8).Column(c =>
                        {
                            c.Item().Text("S.S.EMBROIDERY").FontSize(15).Bold().FontColor("#1e3a8a"); 
                            c.Item().Text("No:12, 2nd Street, Deeivega Nagar").FontSize(9).Medium(); 
                            c.Item().Text("Ranganathapuram").FontSize(9).Medium(); 
                            c.Item().Text("TIRUPUR - 641 603, Tamil Nadu India").FontSize(9).SemiBold(); 
                            c.Item().PaddingTop(2).Text(t => {
                                t.Span("GST: 33AEMFS9121J1ZF").Bold().FontSize(8); 
                            });
                        });
                    });

                    // Right Document Title
                    row.ConstantItem(180).PaddingTop(10).Column(c =>
                    {
                        c.Item().Text($"DC No: {_model.DcNo}").FontSize(10); 
                           c.Item().Height(5);
                        c.Item().Text($"Date: {_model.Date}").FontSize(10); 
                    });
                });

                column.Item().PaddingTop(4).Height(1.5f).Background("#1e3a8a");
            });
        }

        void ComposeContent(IContainer container)
        {
            var colorsText = _model.ColourBreakdowns != null && _model.ColourBreakdowns.Any()
                ? string.Join(", ", _model.ColourBreakdowns.Select(c => c.ColourName))
                : string.Empty;

            container.PaddingVertical(1).Column(column =>
            {
                // Party Details & Job details side-by-side
                column.Item().PaddingBottom(2).Border(0.5f).BorderColor(Colors.Grey.Medium).Row(row =>
                {
                    row.RelativeItem()
                        .Padding(2)
                        .Column(c =>
                        {
                            c.Item().Text(_model.CompanyName).FontSize(10).Bold();
                            c.Item().Text(_model.Address).FontSize(7);
                        });

                    row.AutoItem().LineVertical(0.5f).LineColor(Colors.Grey.Medium);

                    row.RelativeItem().PaddingTop(5)
                        .Padding(2)
                        .Column(c =>
                        {
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(45).Text("Design").FontSize(9);
                                r.RelativeItem().Text($": {_model.DesignReference}").FontSize(9);
                            });

                            c.Item().Row(r =>
                            {
                                r.ConstantItem(45).Text("Style").FontSize(9);
                                r.RelativeItem().Text($": {_model.Style}").FontSize(9);
                            });

                            c.Item().Row(r =>
                            {
                                r.ConstantItem(45).Text("Color").FontSize(9);
                                r.RelativeItem().Text($": {colorsText}").FontSize(9);
                            });
                        });
                });

                // Get distinct sizes
                var distinctSizes = new List<string>();
                if (_model.ColourBreakdowns != null)
                {
                    // To keep them somewhat logically ordered if possible, but we'll preserve appearance order or natural sort
                    distinctSizes = _model.ColourBreakdowns
                        .SelectMany(c => c.Sizes)
                        .Select(s => s.SizeName)
                        .Distinct()
                        .ToList();
                }

                // Table
                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);     // Style
                        columns.RelativeColumn(2);     // Color
                        
                        foreach (var size in distinctSizes)
                        {
                            columns.RelativeColumn(1); // Dynamic Size
                        }
                        
                        columns.RelativeColumn(1.5f);  // Total Qty
                    });

                    // Header
                    table.Header(header =>
                    {
                        void HeaderCell(string text)
                        {
                            header.Cell()
                                .Border(0.5f)
                                .Background("#e5e7eb")
                                .AlignCenter()
                                .AlignMiddle()
                                .Padding(2)
                                .PaddingVertical(3)
                                .Text(text)
                                .Bold()
                                .FontColor(Colors.Black)
                                .FontSize(10);
                        }

                        HeaderCell("Style");
                        HeaderCell("Color");

                        foreach (var size in distinctSizes)
                        {
                            HeaderCell(size);
                        }

                        HeaderCell("Total Qty");
                    });

                    var rowCount = _model.ColourBreakdowns?.Count ?? 0;
                    var rowSpan = (uint)(rowCount > 0 ? rowCount : 1);

                    if (rowCount == 0)
                    {
                         table.Cell().Border(0.5f).AlignCenter().AlignMiddle().Padding(2).PaddingVertical(3).Text(_model.Style).FontSize(9).Bold();
                         table.Cell().Border(0.5f).AlignCenter().AlignMiddle().Padding(2).PaddingVertical(3).Text("").FontSize(9);
                         foreach (var size in distinctSizes)
                         {
                             table.Cell().Border(0.5f).AlignCenter().AlignMiddle().PaddingVertical(3).Text("0").FontSize(9);
                         }
                         table.Cell().Border(0.5f).AlignCenter().AlignMiddle().Padding(2).PaddingVertical(3).Text("0").Bold().FontSize(9);
                    }
                    else
                    {
                        table.Cell().RowSpan(rowSpan).Column(1)
                            .Border(0.5f)
                            .AlignCenter()
                            .AlignMiddle()
                            .Padding(2)
                            .PaddingVertical(3)
                            .Text(_model.Style)
                            .FontSize(9)
                            .Bold();

                        uint currentRow = 1;
                        foreach (var breakdown in _model.ColourBreakdowns)
                        {
                            // Color
                            table.Cell().Row(currentRow).Column(2)
                                .Border(0.5f)
                                .AlignCenter()
                                .AlignMiddle()
                                .Padding(2)
                                .PaddingVertical(3)
                                .Text(breakdown.ColourName)
                                .FontSize(9);

                            uint colIndex = 3;
                            int totalQtyForRow = 0;

                            foreach (var size in distinctSizes)
                            {
                                var sizeObj = breakdown.Sizes?.FirstOrDefault(s => s.SizeName == size);
                                int qty = sizeObj?.Quantity ?? 0;
                                totalQtyForRow += qty;

                                table.Cell().Row(currentRow).Column(colIndex)
                                    .Border(0.5f)
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .PaddingVertical(3)
                                    .Text(qty > 0 ? qty.ToString() : "-")
                                    .FontSize(9);
                                
                                colIndex++;
                            }

                            // Total Qty for the row
                            table.Cell().Row(currentRow).Column(colIndex)
                                .Border(0.5f)
                                .AlignCenter()
                                .AlignMiddle()
                                .Padding(2)
                                .PaddingVertical(3)
                                .Text(totalQtyForRow.ToString())
                                .Bold()
                                .FontSize(9);

                            currentRow++;
                        }
                    }
                });

                var grandTotalQty = _model.ColourBreakdowns?
                    .SelectMany(c => c.Sizes)
                    .Sum(s => s.Quantity) ?? 0;

                // Grand Total Row
                column.Item()
                    .PaddingTop(10)
                    .Border(0.5f)
                    .AlignCenter()
                    .AlignMiddle()
                    .Padding(3)
                    .Text($"GRAND TOTAL QUANTITY : {grandTotalQty} PCS")
                    .FontSize(9)
                    .Bold();

                // Remarks
                column.Item()
                    .PaddingTop(10)
                    .Border(0.5f)
                    .MinHeight(20)
                    .Padding(3)
                    .Column(c =>
                    {
                        c.Item().Text("Remarks")
                            .FontSize(9) 
                            .Bold();

                        c.Item().Text(
                            string.IsNullOrWhiteSpace(_model.Remarks)
                                ? "-"
                                : _model.Remarks)
                            .FontSize(9); 
                    });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.PaddingTop(15).PaddingBottom(5).Row(row =>
            {
                row.RelativeItem().AlignCenter().Column(col =>
                {
                    col.Item().LineHorizontal(0.5f);
                    col.Item().Text("Receiver Signature")
                        .FontSize(8)
                        .AlignCenter();
                });

                row.RelativeItem().AlignCenter().Column(col =>
                {
                    col.Item().LineHorizontal(0.5f);
                    col.Item().Text("Prepared By")
                        .FontSize(8)
                        .AlignCenter();
                });

                row.RelativeItem().AlignCenter().Column(col =>
                {
                    col.Item().LineHorizontal(0.5f);
                    col.Item().Text("Authorised Signatory")
                        .FontSize(8)
                        .AlignCenter();
                });
            });
        }
    }
}
