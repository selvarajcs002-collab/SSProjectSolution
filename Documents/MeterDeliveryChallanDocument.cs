using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SSProjectSolution.Request;
using System.Linq;
using System.Collections.Generic;

namespace SSProjectSolution.Documents
{
    public class MeterDeliveryChallanDocument : IDocument
    {
        private readonly GenerateMeterDcRequest _model;

        public MeterDeliveryChallanDocument(GenerateMeterDcRequest model)
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
                            c.Item().Text("S.S.EMBROIDERY").FontSize(22).Bold().FontColor("#1e3a8a"); 
                            c.Item().Text("No:12, Discovery Nagar").FontSize(9).Medium(); 
                            c.Item().Text("2nd Street, Kangarainagaram").FontSize(9).Medium(); 
                            c.Item().Text("TIRUPUR - 641 666, Tamil Nadu India").FontSize(9).SemiBold(); 
                            c.Item().PaddingTop(2).Text(t => {
                                t.Span("GST: 33AEMFS9121J1ZF").Bold().FontSize(8); 
                            });
                        });
                    });

                    // Right Document Title
                    row.ConstantItem(180).Column(c =>
                    {
                        c.Item().Text("DELIVERY CHALLAN").FontSize(18).Bold().FontColor("#111111"); 
                        c.Item().Text($"DC No: {_model.DcNo}").FontSize(10); 
                        c.Item().Text($"Date: {_model.Date}").FontSize(10); 
                        c.Item().PaddingTop(2).Background("#bbf7d0").AlignCenter().PaddingVertical(2).Text("ORIGINAL").FontSize(9).Bold().FontColor("#166534"); 
                    });
                });

                column.Item().PaddingTop(4).Height(1.5f).Background("#1e3a8a");
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(1).Column(column =>
            {
                // Party Details & Job details side-by-side
                column.Item().PaddingBottom(2).Border(0.5f).BorderColor(Colors.Grey.Medium).Row(row =>
                {
                    row.RelativeItem()
                        .Padding(2)
                        .Column(c =>
                        {
                            c.Item().Text("Party Name").FontSize(9);
                            c.Item().Text(_model.CompanyName).FontSize(10).Bold();
                            c.Item().Text(_model.Address).FontSize(9);
                        });

                    row.AutoItem().LineVertical(0.5f).LineColor(Colors.Grey.Medium);

                    row.RelativeItem()
                        .Padding(2)
                        .Column(c =>
                        {
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(45).Text("Design").FontSize(9);
                                r.RelativeItem().Text($": {_model.Design}").FontSize(9);
                            });

                            c.Item().Row(r =>
                            {
                                r.ConstantItem(45).Text("Style").FontSize(9);
                                r.RelativeItem().Text($": {_model.Style}").FontSize(9);
                            });

                            c.Item().Row(r =>
                            {
                                r.ConstantItem(45).Text("Color").FontSize(9);
                                r.RelativeItem().Text($": {_model.Color}").FontSize(9);
                            });
                        });
                });

                // Compute total bits sum for Qty column
                var totalBitsSum = _model.Items.Sum(i => i.BitsCount);

                // Meter Details Table
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);     // Style
                        columns.RelativeColumn(2);     // Design
                        columns.RelativeColumn(2);     // Color
                        columns.RelativeColumn(1.5f);  // Meter / Bit
                        columns.RelativeColumn(1);     // Bits
                        columns.RelativeColumn(1);     // Pieces
                        columns.RelativeColumn(1.5f);  // Total Meter
                        columns.RelativeColumn(1);     // Qty
                    });

                    // Header
                    string[] headers =
                                         {
                                            "Style",
                                            "Design",
                                            "Color",
                                            "Meter / Bit",
                                            "Bits Count",
                                            "No of Pieces",
                                            "Total Meter",
                                            "Qty"
                                        };

                    table.Header(header =>
                    {
                        foreach (var h in headers)
                        {
                            header.Cell()
                                .Border(0.5f)
                                .Background("#e5e7eb")
                                .AlignCenter()
                                .AlignMiddle()
                                .Padding(2)
                                .Text(h)
                                .Bold()
                                .FontColor(Colors.Black)
                                .FontSize(9); 
                        }
                    });

                    var rowSpan = _model.Items.Count + 1;

                    // Left fixed columns
                    table.Cell().RowSpan((uint)rowSpan).Column(1)
                        .Border(0.5f)
                        .AlignCenter()
                        .AlignMiddle()
                        .Padding(1)
                        .Text(_model.Style)
                        .FontSize(9)
                        .Bold();

                    table.Cell().RowSpan((uint)rowSpan).Column(2)
                        .Border(0.5f)
                        .AlignCenter()
                        .AlignMiddle()
                        .Padding(1)
                        .Text(_model.Design)
                        .FontSize(9);

                    table.Cell().RowSpan((uint)rowSpan).Column(3)
                        .Border(0.5f)
                        .AlignCenter()
                        .AlignMiddle()
                        .Padding(1)
                        .Text(_model.Color)
                        .FontSize(9);

                    // Qty column (right side)
                    table.Cell().RowSpan((uint)rowSpan).Column(8)
                        .Border(0.5f)
                        .AlignCenter()
                        .AlignMiddle()
                        .Padding(1)
                        .Text(totalBitsSum.ToString())
                        .Bold()
                        .FontSize(9);

                    // Detail rows
                    uint currentRow = 1;
                    foreach (var item in _model.Items)
                    {
                        table.Cell().Row(currentRow).Column(4)
                            .Border(0.5f)
                            .AlignCenter()
                            .Text($"{item.MeterPerBit:0.00} Mtr")
                            .FontSize(9);

                        table.Cell().Row(currentRow).Column(5)
                            .Border(0.5f)
                            .AlignCenter()
                            .Text(item.BitsCount.ToString())
                            .FontSize(9);

                        table.Cell().Row(currentRow).Column(6)
                            .Border(0.5f)
                            .AlignCenter()
                            .Text(item.PiecesCount.ToString())
                            .FontSize(9);

                        table.Cell().Row(currentRow).Column(7)
                            .Border(0.5f)
                            .AlignCenter()
                            .Text(item.TotalMeter.ToString("0.00"))
                            .FontSize(9);
                            
                        currentRow++;
                    }

                    // Grand Total Row
                    table.Cell().Row(currentRow).Column(4).ColumnSpan(2)
                      .Border(0.5f)
                      .AlignCenter()
                      .Text("Grand Total ( Meter )")
                      .FontSize(9)
                      .Bold();

                    table.Cell().Row(currentRow).Column(6)
                        .Border(0.5f)
                        .AlignCenter()
                        .Text(_model.Items.Sum(x => x.PiecesCount).ToString())
                        .FontSize(9)
                        .Bold();

                    table.Cell().Row(currentRow).Column(7)
                        .Border(0.5f)
                        .AlignCenter()
                        .Text(_model.TotalMeterSum.ToString("0.00"))
                        .FontSize(9)
                        .Bold();
                });


                column.Item()
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
