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
                    // A5 Landscape
                    page.Size(210, 148, Unit.Millimetre);
                    page.Margin(6, Unit.Millimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                // Company Header
                column.Item().AlignCenter().Text("SRI SASTHA TEXTILES").FontSize(16).SemiBold();
                column.Item().AlignCenter().Text("123, Main Road, Tirupur").FontSize(10);
                
                // Delivery Challan Header
                column.Item().PaddingTop(5).AlignCenter().Text("DELIVERY CHALLAN").FontSize(14).Bold().Underline();

                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"To: {_model.CompanyName}").Bold();
                        c.Item().Text(_model.Address);
                        c.Item().Text($"GST: {_model.GstNo}");
                    });
                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().Text($"DC No: {_model.DcNo}").Bold();
                        c.Item().Text($"Date: {_model.Date}");
                        c.Item().Text($"Ref: {_model.JobReference}");
                    });
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(5).Column(column =>
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);
                        columns.RelativeColumn();
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(50);
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).Padding(2).Text("S.No").SemiBold();
                        header.Cell().BorderBottom(1).Padding(2).Text("Description").SemiBold();
                        header.Cell().BorderBottom(1).Padding(2).AlignRight().Text("Quantity").SemiBold();
                        header.Cell().BorderBottom(1).Padding(2).AlignRight().Text("UOM").SemiBold();
                    });

                    int index = 1;
                    foreach (var item in _model.Items)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(index.ToString());
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(item.Description);
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).AlignRight().Text(item.Quantity.ToString("0.00"));
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).AlignRight().Text(item.Uom);
                        index++;
                    }
                });

                if (!string.IsNullOrEmpty(_model.Remarks))
                {
                    column.Item().PaddingTop(5).Text($"Remarks: {_model.Remarks}").Italic().FontSize(9);
                }
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.AlignBottom().Row(row =>
            {
                row.RelativeItem().AlignLeft().Text("Receiver Signature").SemiBold();
                row.RelativeItem().AlignRight().Text("Authorized Signatory").SemiBold();
            });
        }
    }
}
