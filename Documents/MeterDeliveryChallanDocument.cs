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
                    page.Size(210, 148, Unit.Millimetre);
                    page.Margin(6, Unit.Millimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
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
                        innerRow.ConstantItem(40).Height(40).Placeholder(); // Placeholder for Logo
                        innerRow.RelativeItem().PaddingLeft(8).Column(c =>
                        {
                            c.Item().Text("S.S.EMBROIDERY").FontSize(14).Bold().FontColor("#1e3a8a");
                            c.Item().Text("No:12, Discovery Nagar").FontSize(8);
                            c.Item().Text("2nd Street, Kangarainagaram").FontSize(8);
                            c.Item().Text("TIRUPUR - 641 666, Tamil Nadu India").FontSize(8);
                            c.Item().Text(t => {
                                t.Span("GST: 33AEMFS9121J1ZF").Bold().FontSize(8);
                                t.Span("  Phone: -").FontSize(8);
                            });
                        });
                    });

                    // Right Document Title
                    row.ConstantItem(150).AlignRight().Column(c =>
                    {
                        c.Item().Text("DELIVERY CHALLAN").FontSize(12).Bold().FontColor("#111111");
                        c.Item().Text($"DC No: {_model.DcNo}").FontSize(9);
                        c.Item().Text($"Date: {_model.Date}").FontSize(9);
                        c.Item().PaddingTop(2).Background("#bbf7d0").PaddingHorizontal(10).PaddingVertical(2).Text("ORIGINAL").FontSize(8).Bold().FontColor("#166534").AlignCenter();
                    });
                });

                column.Item().PaddingTop(4).Height(1.5f).Background("#1e3a8a");
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(2).Column(column =>
            {
                // Party Details & Job details side-by-side
                column.Item().PaddingBottom(6).Row(row =>
                {
                    // Party Details (Left)
                    row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4).Column(c =>
                    {
                        c.Item().Text("Party Name").FontSize(7).FontColor(Colors.Grey.Darken1);
                        c.Item().Text(_model.CompanyName).FontSize(10).Bold();
                        c.Item().Text(_model.Address).FontSize(8).LineHeight(1.2f);
                    });

                    row.ConstantItem(8); // spacing

                    // Job Details (Right)
                    row.ConstantItem(180).Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4).Column(c =>
                    {
                        c.Item().Row(r => { r.ConstantItem(50).Text("Design").FontSize(8); r.RelativeItem().Text($": {_model.Design}").FontSize(8).Bold(); });
                        c.Item().Row(r => { r.ConstantItem(50).Text("Style").FontSize(8); r.RelativeItem().Text($": {_model.Style}").FontSize(8).Bold(); });
                        c.Item().Row(r => { r.ConstantItem(50).Text("Color").FontSize(8); r.RelativeItem().Text($": {_model.Color}").FontSize(8).Bold(); });
                        c.Item().Row(r => { r.ConstantItem(50).Text("Machine").FontSize(8); r.RelativeItem().Text($": {_model.Machine}").FontSize(8); });
                        c.Item().Row(r => { r.ConstantItem(50).Text("DC Type").FontSize(8); r.RelativeItem().Text($": {_model.DcType}").FontSize(8).Bold().FontColor("#2563eb"); });
                    });
                });

                // Compute total bits sum for Qty column
                var totalBitsSum = _model.Items.Sum(i => i.BitsCount);

                // Meter Details Table
               column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50); // Style
                            columns.ConstantColumn(55); // Design
                            columns.ConstantColumn(60); // Color
                            columns.ConstantColumn(45); // Meter / Bit (equal width)
                            columns.ConstantColumn(45); // Bits Count (equal width)
                            columns.ConstantColumn(55); // Total Meter
                            columns.ConstantColumn(60); // Qty
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell()
                            .Border(0.5f)
                            .Background("#e5e7eb")
                            .Padding(4)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text("Style")
                            .Bold()
                            .FontSize(8);
                        header.Cell()
                            .Border(0.5f)
                            .Background("#e5e7eb")
                            .Padding(4)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text("Design")
                            .Bold()
                            .FontSize(8);
                        header.Cell()
                            .Border(0.5f)
                            .Background("#e5e7eb")
                            .Padding(4)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text("Color")
                            .Bold()
                            .FontSize(8);
                        header.Cell()
                            .Border(0.5f)
                            .Background("#e5e7eb")
                            .Padding(4)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text("Meter / Bit")
                            .Bold()
                            .FontSize(8);
                        header.Cell()
                            .Border(0.5f)
                            .Background("#e5e7eb")
                            .Padding(4)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text("Bits Count")
                            .Bold()
                            .FontSize(8);
                        header.Cell()
                            .Border(0.5f)
                            .Background("#e5e7eb")
                            .Padding(4)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text("Total Meter")
                            .Bold()
                            .FontSize(8);
                        header.Cell()
                            .Border(0.5f)
                            .Background("#e5e7eb")
                            .Padding(4)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text("Qty")
                            .Bold()
                            .FontSize(8);
                        });

                        var rowSpan = _model.Items.Count + 1;

                        // Left fixed columns
                        table.Cell().RowSpan((uint)rowSpan)
                            .Border(0.5f)
                            .AlignCenter()
                            .AlignMiddle()
                            .Padding(3)
                            .Text(_model.Style)
                            .Bold()
                            .FontSize(8);

                        table.Cell().RowSpan((uint)rowSpan)
                            .Border(0.5f)
                            .AlignCenter()
                            .AlignMiddle()
                            .Padding(3)
                            .Text(_model.Design)
                            .FontSize(8);

                        table.Cell().RowSpan((uint)rowSpan)
                            .Border(0.5f)
                            .AlignCenter()
                            .AlignMiddle()
                            .Padding(3)
                            .Text(_model.Color)
                            .FontSize(8);

                        // Qty column (right side)
                        table.Cell().RowSpan((uint)rowSpan)
                            .Border(0.5f)
                            .AlignCenter()
                            .AlignMiddle()
                            .Padding(3)
                            .Text(totalBitsSum.ToString())
                            .Bold()
                            .FontSize(8);

                        // Detail rows
                        foreach (var item in _model.Items)
                        {
                            table.Cell()
                                .Border(0.5f)
                                .Padding(3)
                                .AlignCenter()
                                .Text($"{item.MeterPerBit:0.00} Mtr")
                                .FontSize(8);

                            table.Cell()
                                .Border(0.5f)
                                .Padding(3)
                                .AlignCenter()
                                .Text(item.BitsCount.ToString())
                                .FontSize(8);

                            table.Cell()
                                .Border(0.5f)
                                .Padding(3)
                                .AlignCenter()
                                .Text(item.TotalMeter.ToString("0.00"))
                                .FontSize(8);
                        }

                        // Grand Total Row
                        table.Cell()
                            .ColumnSpan(2)
                            .Border(0.5f)
                            .Background("#f8fafc")
                            .AlignCenter()
                            .Padding(3)
                            .Text("Grand Total ( Meter )")
                            .Bold()
                            .FontSize(8);

                        table.Cell()
                            .Border(0.5f)
                            .Background("#f8fafc")
                            .AlignCenter()
                            .Padding(3)
                            .Text(_model.TotalMeterSum.ToString("0.00"))
                            .Bold()
                            .FontSize(8);
                    });

                if (!string.IsNullOrEmpty(_model.Remarks))
                {
                    column.Item().PaddingTop(5).Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4).Column(c =>
                    {
                        c.Item().Text("Remarks").FontSize(7).FontColor(Colors.Grey.Darken1);
                        c.Item().Text(_model.Remarks).FontSize(8);
                    });
                }
            });
        }

        // Removed HeaderCell method; header cells are defined inline in the Header block.

        void ComposeFooter(IContainer container)
        {
            container.AlignBottom().PaddingBottom(4).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Height(0.8f).Background(Colors.Black);
                    c.Item().PaddingTop(2).AlignCenter().Text("Receiver Signature").FontSize(8).SemiBold();
                });
                row.ConstantItem(25); // spacing
                row.RelativeItem().Column(c =>
                {
                    c.Item().Height(0.8f).Background(Colors.Black);
                    c.Item().PaddingTop(2).AlignCenter().Text("Prepared By").FontSize(8).SemiBold();
                });
                row.ConstantItem(25); // spacing
                row.RelativeItem().Column(c =>
                {
                    c.Item().Height(0.8f).Background(Colors.Black);
                    c.Item().PaddingTop(2).AlignCenter().Text("Authorised Signatory").FontSize(8).SemiBold();
                });
            });
        }

    }
}
