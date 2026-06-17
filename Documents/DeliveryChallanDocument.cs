using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SSProjectSolution.Request;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

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
                    // Custom Size 14x20 cm
                    page.Size(14, 20, Unit.Centimetre);
                    page.Margin(5, Unit.Millimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(7).FontFamily(Fonts.Arial));

                    var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.jpeg");
                    var transparentLogo = GetTransparentLogo(logoPath, 0.05f);
                    if (transparentLogo != null && transparentLogo.Length > 0)
                    {
                        page.Background().AlignCenter().AlignMiddle().Width(10, Unit.Centimetre).Image(transparentLogo);
                    }

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
                    row.RelativeItem().Padding(4).Row(companyRow =>
                    {
                        var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.jpeg");
                        companyRow.AutoItem().PaddingRight(4).Width(25).Image(logoPath);
                        companyRow.RelativeItem().Column(c =>
                        {
                            c.Item().Text("S.S.EMBROIDERY").FontSize(8).Bold().FontColor(Colors.Blue.Darken2);
                            c.Item().Text("No:12, Deeivega Nagar,").FontSize(5);
                            c.Item().Text("2nd Street, Ranganathapuram").FontSize(5);
                            c.Item().Text("Tiruppur-6416003, Tamilnadu, India").FontSize(5);
                        });
                    });

                    // Middle - Title
                    row.RelativeItem().Padding(2).AlignCenter().AlignMiddle().Column(c =>
                    {
                        c.Item().Text("DELIVERY CHALLAN (Not for sale)").FontSize(8).Bold().FontFamily(Fonts.TimesNewRoman);
                        
                        c.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Black);
                    });

                    // Right - Details
                    row.ConstantItem(90).BorderLeft(1).BorderColor(Colors.Black).Column(c =>
                    {
                        c.Item().Padding(3).BorderBottom(1).BorderColor(Colors.Black).Row(r =>
                        {
                            r.RelativeItem().Text("DC No.").FontSize(5);
                            r.ConstantItem(10).Text(":");
                            r.RelativeItem().Text(_model.DcNo).FontSize(5);
                        });
                        c.Item().Padding(3).BorderBottom(1).BorderColor(Colors.Black).Row(r =>
                        {
                            r.RelativeItem().Text("DC Date").FontSize(5);
                            r.ConstantItem(10).Text(":");
                            r.RelativeItem().Text(_model.Date).FontSize(5);
                        });
                    });
                });

                // 2. Info Panels
                    column.Item().BorderBottom(1).BorderColor(Colors.Black).Row(row =>
                    {
                        // COMPANY DETAILS
                        row.RelativeItem().BorderRight(1).BorderColor(Colors.Black).Padding(2).Column(c =>
                        {
                            c.Item().AlignCenter().Text("COMPANY DETAILS").Bold().FontSize(5);
                            c.Item().PaddingTop(5).PaddingBottom(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);

                            c.Item().PaddingTop(2).Row(r => { r.ConstantItem(55).Text("Company Name").FontSize(5); r.ConstantItem(10).Text(":").FontSize(5); r.RelativeItem().Text(_model.CompanyName).FontSize(5); });
                        });

                        // ITEM DETAILS
                        row.RelativeItem().BorderRight(1).BorderColor(Colors.Black).Padding(2).Column(c =>
                        {
                            c.Item().AlignCenter().Text("STYLE DETAILS").Bold().FontSize(5);
                            c.Item().PaddingTop(5).PaddingBottom(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);

                            c.Item().PaddingTop(2).Row(r => { r.ConstantItem(55).Text("Style Number").FontSize(5); r.ConstantItem(10).Text(":").FontSize(5); r.RelativeItem().Text(_model.Style).FontSize(5); });
                           
                        });

                        // CHALLAN DETAILS
                        row.RelativeItem().Padding(2).Column(c =>
                        {
                            c.Item().AlignCenter().Text("DESIGN DETAILS").Bold().FontSize(5);
                            c.Item().PaddingTop(5).PaddingBottom(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);

                             c.Item().PaddingTop(2).Row(r => { r.ConstantItem(55).Text("Design Reference").FontSize(5); r.ConstantItem(10).Text(":").FontSize(5); r.RelativeItem().Text(_model.DesignReference).FontSize(5); });

                        });
                    });

                    // 3. Remarks
                    column.Item().BorderBottom(1).BorderColor(Colors.Black).Padding(4).Row(row =>
                    {
                        row.ConstantItem(50).Text("Remarks").Bold();
                        row.ConstantItem(10).Text(":");
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Medium).Padding(5).Text(_model.Remarks);
                    });
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
                            columns.ConstantColumn(20);
                            columns.RelativeColumn();

                            foreach (var size in allSizes)
                                columns.ConstantColumn(20);

                            columns.ConstantColumn(30);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Border(1).PaddingVertical(3).PaddingHorizontal(2).AlignCenter().Text("S.No.").FontSize(5).Bold();
                            header.Cell().Border(1).PaddingVertical(3).PaddingHorizontal(2).Text("Colour").FontSize(5).Bold();

                            foreach (var size in allSizes)
                            {
                                header.Cell()
                                    .Border(1)
                                    .PaddingVertical(3).PaddingHorizontal(2)
                                    .AlignCenter()
                                    .Text(size)
                                    .FontSize(5)
                                    .Bold();
                            }

                            header.Cell()
                                .Border(1)
                                .PaddingVertical(3).PaddingHorizontal(2)
                                .AlignCenter()
                                .Text("TOTAL")
                                .FontSize(5)
                                .Bold();
                        });

                        int rowNo = 1;
                        int grandTotal = 0;

                        var sizeTotals = allSizes.ToDictionary(x => x, x => 0);

                        foreach (var colour in _model.ColourBreakdowns)
                        {
                            table.Cell().Border(1).PaddingVertical(4).PaddingHorizontal(3).AlignCenter().Text(rowNo.ToString()).FontSize(5);

                            table.Cell().Border(1).PaddingVertical(4).PaddingHorizontal(3).Text(colour.ColourName).FontSize(5);

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
                                    .FontSize(5);

                                rowTotal += qty;
                                sizeTotals[size] += qty;
                            }

                            table.Cell()
                                .Border(1)
                                .PaddingVertical(4).PaddingHorizontal(3)
                                .AlignCenter()
                                .Text(rowTotal.ToString())
                                .FontSize(5)
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
                            .FontSize(5)
                            .Bold();

                        foreach (var size in allSizes)
                        {
                            table.Cell()
                                .Border(1)
                                .PaddingVertical(4).PaddingHorizontal(3)
                                .AlignCenter()
                                .Text(sizeTotals[size].ToString())
                                .FontSize(5)
                                .Bold();
                        }

                        table.Cell()
                            .Border(1)
                            .PaddingVertical(4).PaddingHorizontal(3)
                            .AlignCenter()
                            .Text(grandTotal.ToString())
                            .FontSize(5)
                            .Bold();

                        sumTotal = grandTotal;
                    });

                    // 6. Grand Total Box
                    column.Item().BorderBottom(1).BorderColor(Colors.Black).Padding(2).AlignCenter().Row(r =>
                    {
                        r.AutoItem().Border(1).BorderColor(Colors.Grey.Medium).Padding(2).Row(inner =>
                        {
                            inner.AutoItem().AlignMiddle().Text("GRAND TOTAL QUANTITY : ").Bold().FontSize(6);
                            inner.AutoItem().AlignMiddle().Text(sumTotal.ToString()).FontColor(Colors.Green.Darken2).Bold().FontSize(6);
                            inner.AutoItem().AlignMiddle().PaddingLeft(5).Text(" PCS").FontSize(6);
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

                    
            });
        }

#pragma warning disable CA1416
        private byte[] GetTransparentLogo(string path, float opacity)
        {
            if (!File.Exists(path)) return new byte[0];
            using var original = new Bitmap(path);
            using var transparent = new Bitmap(original.Width, original.Height);
            using var graphics = Graphics.FromImage(transparent);
            
            var colorMatrix = new ColorMatrix();
            colorMatrix.Matrix33 = opacity;
            
            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            
            graphics.DrawImage(original, new Rectangle(0, 0, transparent.Width, transparent.Height), 
                               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, imageAttributes);
                               
            using var ms = new MemoryStream();
            transparent.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
#pragma warning restore CA1416
    }
}
