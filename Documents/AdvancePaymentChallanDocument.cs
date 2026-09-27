using System;
using System.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SSProjectSolution.Documents
{
    public class AdvanceChallanPdfData
    {
        public string CompanyName { get; set; } = "S.S. EMBROIDERY";
        public string Address { get; set; } = string.Empty;
        public string[] AddressLines { get; set; } = System.Array.Empty<string>();
        public string Phone { get; set; } = string.Empty;
        public string GstNumber { get; set; } = string.Empty;
        public byte[]? LogoBytes { get; set; }
        public string ChallanNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string AmountFormatted { get; set; } = string.Empty;
        public string AmountInWords { get; set; } = string.Empty;
        public string Remarks { get; set; } = "-";
        public string GeneratedBy { get; set; } = string.Empty;
    }

    public class AdvancePaymentChallanDocument : IDocument
    {
        private const string Navy = "#0B3A86";
        private const string TitleNavy = "#0A3478";
        private const string Text = "#1C2834";
        private const string Muted = "#5C6B7A";
        private const string LabelBg = "#F4F7FB";
        private const string AmountBg = "#E4F0FB";
        private const string BoxBg = "#E8F2FB";
        private const string Border = "#C5D2E4";
        private const string OuterBorder = "#8EA3C1";
        private const string Line = "#9AADC4";

        private readonly AdvanceChallanPdfData _data;
        private readonly string _scriptFont;
        private readonly bool _scriptAvailable;
        private readonly string _currencyFont;

        public AdvancePaymentChallanDocument(AdvanceChallanPdfData data)
        {
            _data = data;
            _scriptFont = FirstAvailableFont("Segoe Script", "Brush Script MT", "Lucida Handwriting");
            _scriptAvailable = _scriptFont.Length > 0;
            var currencyFont = FirstAvailableFont("Segoe UI", "Nirmala UI", "Arial");
            _currencyFont = currencyFont.Length == 0 ? "Arial" : currencyFont;
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"Advance Payment Challan {_data.ChallanNo}",
            Author = string.IsNullOrWhiteSpace(_data.GeneratedBy) ? "SS Management" : _data.GeneratedBy,
            Creator = "SS Management",
            Subject = "Advance Payment Challan"
        };

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5.Landscape());
                page.MarginHorizontal(7, Unit.Millimetre);
                page.MarginVertical(6, Unit.Millimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial").FontColor(Text));

                page.Content().Border(0.9f).BorderColor(OuterBorder).Padding(3, Unit.Millimetre).Column(col =>
                {
                    col.Item().Element(ComposeHeader);
                    col.Item().PaddingTop(2.2f, Unit.Millimetre).Element(ComposeTitle);
                    col.Item().PaddingTop(2.2f, Unit.Millimetre).Element(ComposeDetails);
                    col.Item().ExtendVertical().AlignBottom().Column(bottom =>
                    {
                        bottom.Item().Element(ComposeSignatures);
                        bottom.Item().PaddingTop(2, Unit.Millimetre).Element(ComposeThankYou);
                    });
                });
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.PaddingBottom(1.6f, Unit.Millimetre).Row(row =>
            {
                row.ConstantItem(36, Unit.Millimetre).AlignMiddle().Column(left =>
                {
                    if (_data.LogoBytes != null && _data.LogoBytes.Length > 0)
                    {
                        left.Item().AlignCenter().Height(15, Unit.Millimetre).Image(_data.LogoBytes).FitHeight();
                    }
                    else
                    {
                        left.Item().AlignCenter().Width(15, Unit.Millimetre).Height(15, Unit.Millimetre)
                            .Border(0.6f).BorderColor(Border).AlignCenter().AlignMiddle()
                            .Text("SS").Bold().FontSize(11).FontColor(Navy);
                    }

                    left.Item().PaddingTop(1).AlignCenter().Text(_data.CompanyName)
                        .Bold().FontSize(6.2f).FontColor(Navy);
                    left.Item().AlignCenter().Text("QUALITY IN EVERY STITCH")
                        .FontSize(4.6f).FontColor(Muted).LetterSpacing(0.25f);
                });

                row.ConstantItem(2.2f, Unit.Millimetre).AlignMiddle().PaddingVertical(1, Unit.Millimetre)
                    .LineVertical(0.6f).LineColor(Line);

                row.RelativeItem().AlignMiddle().PaddingHorizontal(2.4f, Unit.Millimetre).Column(center =>
                {
                    center.Item().Text(_data.CompanyName)
                        .Bold().FontSize(15).FontColor(Navy);

                    var addressLines = _data.AddressLines != null && _data.AddressLines.Length > 0
                        ? _data.AddressLines
                        : (string.IsNullOrWhiteSpace(_data.Address) ? System.Array.Empty<string>() : new[] { _data.Address });

                    if (addressLines.Length > 0)
                    {
                        center.Item().PaddingTop(1).Column(address =>
                        {
                            foreach (var line in addressLines)
                            {
                                address.Item().Text(line).FontSize(7.2f).FontColor(Text).LineHeight(1.05f);
                            }
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(_data.Phone))
                    {
                        center.Item().PaddingTop(0.6f).Text(text =>
                        {
                            text.Span("Phone: ").Bold().FontSize(7.2f).FontColor(Navy);
                            text.Span(_data.Phone).FontSize(7.2f).FontColor(Text);
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(_data.GstNumber))
                    {
                        center.Item().PaddingTop(0.4f).Text(text =>
                        {
                            text.Span("GST: ").Bold().FontSize(7.2f).FontColor(Navy);
                            text.Span(_data.GstNumber).FontSize(7.2f).FontColor(Text);
                        });
                    }
                });

                row.ConstantItem(2.2f, Unit.Millimetre).AlignMiddle().PaddingVertical(1, Unit.Millimetre)
                    .LineVertical(0.6f).LineColor(Line);

                row.ConstantItem(56, Unit.Millimetre).AlignMiddle()
                    .Border(0.7f).BorderColor(Border).Background(BoxBg)
                    .PaddingVertical(2, Unit.Millimetre).PaddingHorizontal(2.4f, Unit.Millimetre)
                    .Column(box =>
                    {
                        box.Item().AlignCenter().Text("ADVANCE CHALLAN")
                            .Bold().FontSize(8.5f).FontColor(Navy).LetterSpacing(0.4f);

                        box.Item().PaddingTop(1.4f, Unit.Millimetre).Row(info =>
                        {
                            info.ConstantItem(22, Unit.Millimetre).Text("Challan No").FontSize(7.4f).FontColor(Muted);
                            info.RelativeItem().Text(_data.ChallanNo).Bold().FontSize(7.6f).FontColor(Text);
                        });

                        box.Item().PaddingTop(0.8f, Unit.Millimetre).Row(info =>
                        {
                            info.ConstantItem(22, Unit.Millimetre).Text("Date").FontSize(7.4f).FontColor(Muted);
                            info.RelativeItem().Text(_data.Date.ToString("dd-MM-yyyy")).Bold().FontSize(7.6f).FontColor(Text);
                        });
                    });
            });
        }

        private void ComposeTitle(IContainer container)
        {
            container.Background(TitleNavy).PaddingVertical(3.4f, Unit.Millimetre).AlignCenter()
                .Text("ADVANCE PAYMENT CHALLAN")
                .Bold().FontSize(11).FontColor(Colors.White).LetterSpacing(0.7f);
        }

        private void ComposeDetails(IContainer container)
        {
            var remarks = string.IsNullOrWhiteSpace(_data.Remarks) ? "-" : _data.Remarks.Trim();
            if (remarks.Length > 280)
                remarks = remarks.Substring(0, 277) + "...";

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(34, Unit.Millimetre);
                    columns.RelativeColumn();
                });

                AddTextRow(table, "Name", _data.Name, false);
                AddTextRow(table, "Date", _data.Date.ToString("dd-MM-yyyy"), false);
                AddAmountRow(table);
                AddTextRow(table, "Remarks", remarks, false);
            });
        }

        private static void AddTextRow(TableDescriptor table, string label, string value, bool highlight)
        {
            var background = highlight ? AmountBg : LabelBg;

            table.Cell().Border(0.6f).BorderColor(Border).Background(background)
                .PaddingVertical(2.1f, Unit.Millimetre).PaddingHorizontal(2.2f, Unit.Millimetre)
                .AlignMiddle()
                .Row(row =>
                {
                    row.RelativeItem().Text(label).Bold().FontSize(9).FontColor(Text);
                    row.ConstantItem(8).AlignRight().Text(":").Bold().FontSize(9).FontColor(Muted);
                });

            table.Cell().Border(0.6f).BorderColor(Border).Background(highlight ? AmountBg : Colors.White)
                .PaddingVertical(2.1f, Unit.Millimetre).PaddingHorizontal(2.4f, Unit.Millimetre)
                .AlignMiddle()
                .Text(string.IsNullOrWhiteSpace(value) ? "-" : value).FontSize(9.5f).FontColor(Text);
        }

        private void AddAmountRow(TableDescriptor table)
        {
            table.Cell().Border(0.6f).BorderColor(Border).Background(AmountBg)
                .PaddingVertical(2.2f, Unit.Millimetre).PaddingHorizontal(2.2f, Unit.Millimetre)
                .AlignMiddle()
                .Row(row =>
                {
                    row.RelativeItem().Text("Amount").Bold().FontSize(9).FontColor(Navy);
                    row.ConstantItem(8).AlignRight().Text(":").Bold().FontSize(9).FontColor(Muted);
                });

            table.Cell().Border(0.6f).BorderColor(Border).Background(AmountBg)
                .PaddingVertical(1.8f, Unit.Millimetre).PaddingHorizontal(2.4f, Unit.Millimetre)
                .AlignMiddle()
                .Column(col =>
                {
                    col.Item().Text(text =>
                    {
                        text.Span("₹ ").FontFamily(_currencyFont).Bold().FontSize(13).FontColor(Navy);
                        text.Span(_data.AmountFormatted).FontFamily("Arial").Bold().FontSize(13).FontColor(Navy);
                    });
                    col.Item().PaddingTop(0.4f).Text($"({_data.AmountInWords})")
                        .FontSize(8).Italic().FontColor(Muted);
                });
        }

        private void ComposeSignatures(IContainer container)
        {
            container.PaddingTop(1, Unit.Millimetre).Row(row =>
            {
                row.RelativeItem().AlignCenter().Column(col =>
                {
                    col.Item().AlignCenter().Width(52, Unit.Millimetre).LineHorizontal(0.7f).LineColor(Line);
                    col.Item().PaddingTop(1.2f, Unit.Millimetre).AlignCenter()
                        .Text("Received By").Bold().FontSize(9).FontColor(Navy);
                    col.Item().AlignCenter().Text("Name & Signature").FontSize(7.5f).FontColor(Muted);
                });

                row.RelativeItem().AlignCenter().Column(col =>
                {
                    col.Item().AlignCenter().Width(56, Unit.Millimetre).LineHorizontal(0.7f).LineColor(Line);
                    col.Item().PaddingTop(1.2f, Unit.Millimetre).AlignCenter()
                        .Text("Authorized Signature").Bold().FontSize(9).FontColor(Navy);
                    col.Item().AlignCenter().Text($"For {_data.CompanyName}").FontSize(7.5f).FontColor(Muted);
                });
            });
        }

        private void ComposeThankYou(IContainer container)
        {
            container.AlignCenter().Column(col =>
            {
                if (_scriptAvailable)
                {
                    col.Item().AlignCenter().Text("Thank You")
                        .FontFamily(_scriptFont).FontSize(14).FontColor(Navy);
                }
                else
                {
                    col.Item().AlignCenter().Text("Thank You")
                        .Italic().FontSize(14).FontColor(Navy);
                }

                col.Item().PaddingTop(0.4f).AlignCenter()
                    .Text("YOUR TRUST DRIVES OUR GROWTH")
                    .FontSize(6.2f).FontColor("#8A94A3").LetterSpacing(0.55f);
            });
        }

        private static string FirstAvailableFont(params string[] families)
        {
            foreach (var family in families)
            {
                try
                {
                    using var font = new FontFamily(family);
                    return family;
                }
                catch
                {
                    // Font is not installed on this machine.
                }
            }

            return string.Empty;
        }
    }
}
