using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SSProjectSolution.Models.DTOs;
using System;
using System.IO;

namespace SSProjectSolution.Documents
{
    public class RateQuotationDocument : IDocument
    {
        private readonly RateQuotationResponseDto _model;
        private readonly string _imagePath;

        public RateQuotationDocument(
            RateQuotationResponseDto model,
            string imagePath)
        {
            _model = model;
            _imagePath = imagePath;
        }

        public DocumentMetadata GetMetadata()
        {
            return DocumentMetadata.Default;
        }

        public DocumentSettings GetSettings()
        {
            return DocumentSettings.Default;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                // =========================================================
                // PAGE SETTINGS
                // =========================================================

                page.Size(PageSizes.A4);

                // A4 margins
                page.MarginHorizontal(14, Unit.Millimetre);
                page.MarginVertical(12, Unit.Millimetre);

                page.PageColor(Colors.White);

                page.DefaultTextStyle(x =>
                    x.FontFamily("Arial")
                     .FontSize(10.5f)
                     .FontColor(Colors.Black));

                // Main content
                page.Content()
                    .Element(ComposeContent);

                // Footer
                page.Footer()
                    .Element(ComposeFooter);
            });
        }

        private void ComposeContent(IContainer container)
        {
            const string blueColor = "#1E3A8A";
            const string greyColor = "#666666";
            const string borderColor = "#D9DDE3";

            // =============================================================
            // LOGO PATH
            // =============================================================

            var logoPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "logo.jpeg");

            container
                .Column(column =>
                {
                    // =====================================================
                    // HEADER
                    // =====================================================

                    column.Item()
                        .Row(row =>
                        {
                            // -------------------------------------------------
                            // LEFT SIDE - COMPANY INFORMATION
                            // -------------------------------------------------

                            row.RelativeItem()
                                .Column(left =>
                                {
                                    // Logo + Company Name
                                    left.Item()
                                        .Row(header =>
                                        {
                                            // Logo
                                            if (File.Exists(logoPath))
                                            {
                                                header.AutoItem()
                                                    .Width(62)
                                                    .Height(62)
                                                    .Image(logoPath);
                                            }
                                            else
                                            {
                                                header.AutoItem()
                                                    .Width(62)
                                                    .Height(62)
                                                    .Border(1)
                                                    .BorderColor(blueColor)
                                                    .AlignCenter()
                                                    .AlignMiddle()
                                                    .Text("SS")
                                                    .FontSize(22)
                                                    .Bold()
                                                    .FontColor(blueColor);
                                            }

                                            // Company name
                                            header.RelativeItem()
                                                .PaddingLeft(10)
                                                .AlignMiddle()
                                                .Column(title =>
                                                {
                                                    title.Item()
                                                        .Text("SS EMBROIDERY")
                                                        .FontSize(19)
                                                        .Bold()
                                                        .FontColor(blueColor);

                                                    title.Item()
                                                        .PaddingTop(2)
                                                        .Text("Quality Stitches, Timely Delivery")
                                                        .FontSize(9)
                                                        .FontColor(greyColor);
                                                });
                                        });

                                    // Address
                                    left.Item()
                                        .PaddingTop(8)
                                        .Text(
                                            "584/1-A, 12. Deiveega Nagar, " +
                                            "Ranganathapuram, Velampalayam, " +
                                            "Tirupur - 641 603, TamilNadu, India")
                                        .FontSize(8);

                                    // GST
                                    left.Item()
                                        .PaddingTop(3)
                                        .Text("GST: 33ABNJS9123JIZT")
                                        .FontSize(8);
                                });

                            // -------------------------------------------------
                            // RIGHT SIDE - QUOTATION INFORMATION
                            // -------------------------------------------------

                            row.ConstantItem(185)
                                .AlignRight()
                                .Column(right =>
                                {
                                    // Rate quotation title
                                    right.Item()
                                        .AlignRight()
                                        .Text("RATE QUOTATION")
                                        .FontSize(19)
                                        .Bold()
                                        .FontColor(blueColor);

                                    // Underline
                                    right.Item()
                                        .PaddingTop(4)
                                        .AlignRight()
                                        .Width(60)
                                        .LineHorizontal(1.5f)
                                        .LineColor(blueColor);

                                    // Quote details
                                    right.Item()
                                        .PaddingTop(14)
                                        .Table(table =>
                                        {
                                            table.ColumnsDefinition(columns =>
                                            {
                                                columns.ConstantColumn(58);
                                                columns.ConstantColumn(10);
                                                columns.RelativeColumn();
                                            });

                                            // Quote No
                                            table.Cell()
                                                .Text("Quote No")
                                                .Bold()
                                                .FontSize(9);

                                            table.Cell()
                                                .AlignCenter()
                                                .Text(":")
                                                .FontSize(9);

                                            table.Cell()
                                                .Text(
                                                    string.IsNullOrWhiteSpace(
                                                        _model.QuotationNo)
                                                        ? "QT-DRAFT"
                                                        : _model.QuotationNo)
                                                .FontSize(9);

                                            // Date
                                            table.Cell()
                                                .PaddingTop(6)
                                                .Text("Date")
                                                .Bold()
                                                .FontSize(9);

                                            table.Cell()
                                                .PaddingTop(6)
                                                .AlignCenter()
                                                .Text(":")
                                                .FontSize(9);

                                            table.Cell()
                                                .PaddingTop(6)
                                                .Text(
                                                    _model.QuotationDate
                                                        .ToString("dd-MMM-yyyy"))
                                                .FontSize(9);
                                        });
                                });
                        });

                    // =====================================================
                    // HEADER DIVIDER
                    // =====================================================

                    column.Item()
                        .PaddingTop(12)
                        .PaddingBottom(14)
                        .LineHorizontal(1)
                        .LineColor(blueColor);

                    // =====================================================
                    // QUOTATION DETAILS + DESIGN IMAGE
                    // =====================================================

                    column.Item()
                        .Row(row =>
                        {
                            // -------------------------------------------------
                            // LEFT SIDE - DETAILS TABLE
                            // -------------------------------------------------

                            row.RelativeItem()
                                .PaddingRight(25)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        // Label
                                        columns.RelativeColumn(4.2f);

                                        // :
                                        columns.ConstantColumn(18);

                                        // Value
                                        columns.RelativeColumn(5.8f);
                                    });

                                    // Helper method
                                    void AddRow(
                                        string label,
                                        string value)
                                    {
                                        // Label
                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(borderColor)
                                            .MinHeight(34)
                                            .PaddingVertical(8)
                                            .PaddingHorizontal(8)
                                            .AlignMiddle()
                                            .Text(label)
                                            .Bold()
                                            .FontSize(10);

                                        // Colon
                                        table.Cell()
                                            .BorderTop(1)
                                            .BorderBottom(1)
                                            .BorderColor(borderColor)
                                            .MinHeight(34)
                                            .PaddingVertical(8)
                                            .AlignCenter()
                                            .AlignMiddle()
                                            .Text(":")
                                            .FontSize(10);

                                        // Value
                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(borderColor)
                                            .MinHeight(34)
                                            .PaddingVertical(8)
                                            .PaddingHorizontal(8)
                                            .AlignMiddle()
                                            .Text(value)
                                            .FontSize(10);
                                    }

                                    // -------------------------------------------------
                                    // DATA
                                    // -------------------------------------------------

                                    AddRow(
                                        "Company Name",
                                        string.IsNullOrWhiteSpace(
                                            _model.CompanyName)
                                            ? "-"
                                            : _model.CompanyName);

                                    AddRow(
                                        "Style No",
                                        string.IsNullOrWhiteSpace(
                                            _model.StyleNo)
                                            ? "-"
                                            : _model.StyleNo);

                                    AddRow(
                                        "EMB Design",
                                        string.IsNullOrWhiteSpace(
                                            _model.DesignName)
                                            ? "-"
                                            : _model.DesignName);

                                    AddRow(
                                        "Chenille Colors",
                                        _model.ChenilleColors?
                                            .ToString()
                                            ?? "-");

                                    AddRow(
                                        "Normal EMB Colors",
                                        _model.NormalEmbColors?
                                            .ToString()
                                            ?? "-");

                                    AddRow(
                                        "No Of Stitches",
                                        _model.NoOfStitches?.ToString() ?? "-");

                                    AddRow(
                                        "Embroidery Cost / 1000 Sts",
                                        _model.RatePerMeter.HasValue
                                            ? $"Rs. {_model.RatePerMeter.Value:F2}"
                                            : "-");

                                    AddRow(
                                        "Payment Terms",
                                        string.IsNullOrWhiteSpace(
                                            _model.Remarks)
                                            ? "-"
                                            : _model.Remarks);
                                });

                            // -------------------------------------------------
                            // RIGHT SIDE - DESIGN IMAGE
                            // -------------------------------------------------

                            row.ConstantItem(145)
                                .AlignCenter()
                                .Column(imageColumn =>
                                {
                                    // Title
                                    imageColumn.Item()
                                        .PaddingBottom(8)
                                        .AlignCenter()
                                        .Text("DESIGN IMAGE")
                                        .FontSize(9)
                                        .Bold()
                                        .FontColor(blueColor);

                                    // Image box
                                    var imageBox =
                                        imageColumn.Item()
                                            .Width(125)
                                            .Height(125)
                                            .Border(1)
                                            .BorderColor(borderColor)
                                            .AlignCenter()
                                            .AlignMiddle();

                                    // Uploaded image
                                    if (!string.IsNullOrWhiteSpace(_imagePath)
                                        && File.Exists(_imagePath))
                                    {
                                        imageBox
                                            .Image(_imagePath)
                                            .FitArea();
                                    }

                                    // Reference text
                                    imageColumn.Item()
                                        .PaddingTop(7)
                                        .AlignCenter()
                                        .Text("(For Reference Only)")
                                        .FontSize(7)
                                        .FontColor(greyColor);
                                });
                        });

                    // =====================================================
                    // SPACE BEFORE TERMS
                    // =====================================================

                    column.Item()
                        .PaddingTop(22)
                        .PaddingBottom(12)
                        .LineHorizontal(1)
                        .LineColor(blueColor);

                    // =====================================================
                    // TERMS & CONDITIONS
                    // =====================================================

                    column.Item()
                        .Column(terms =>
                        {
                            // Heading
                            terms.Item()
                                .PaddingBottom(9)
                                .Text("TERMS & CONDITIONS")
                                .FontSize(12)
                                .Bold()
                                .FontColor(blueColor);

                            // Bullet helper
                            void AddBullet(string text)
                            {
                                terms.Item()
                                    .PaddingBottom(5)
                                    .Row(r =>
                                    {
                                        // Bullet
                                        r.ConstantItem(15)
                                            .Text("•")
                                            .FontSize(9);

                                        // Text
                                        r.RelativeItem()
                                            .Text(text)
                                            .FontSize(9);
                                    });
                            }

                            // Terms
                            AddBullet(
                                "Rate is based on the design and details provided by the customer.");

                            AddBullet(
                                "Any change in stitch count, colors or size may affect the quoted rate.");

                            AddBullet(
                                "GST will be charged extra as applicable.");

                            AddBullet(
                                "Quotation is valid for 30 days from the date of issue.");

                            AddBullet(
                                "5% - 10% Rejection must be allowed.");

                            AddBullet(
                                "Payment terms as mentioned above must be followed.");
                        });

                    // =====================================================
                    // SIGNATURE
                    // =====================================================

                    column.Item()
                        .PaddingTop(45)
                        .AlignRight()
                        .Column(signature =>
                        {
                            // Signature line
                            signature.Item()
                                .Width(140)
                                .LineHorizontal(1)
                                .LineColor(Colors.Black);

                            // Signature text
                            signature.Item()
                                .PaddingTop(6)
                                .Width(140)
                                .AlignCenter()
                                .Text("Authorized Signature")
                                .FontSize(8);
                        });
                });
        }

        // =============================================================
        // FOOTER
        // =============================================================

        private void ComposeFooter(IContainer container)
        {
            container
                .PaddingBottom(3)
                .Row(row =>
                {
                    // Left line
                    row.RelativeItem()
                        .AlignMiddle()
                        .LineHorizontal(0.8f)
                        .LineColor("#1E3A8A");

                    // Footer text
                    row.AutoItem()
                        .PaddingHorizontal(12)
                        .AlignCenter()
                        .Text("Thank you for your business!")
                        .FontSize(8)
                        .Italic()
                        .Bold()
                        .FontColor("#1E3A8A");

                    // Right line
                    row.RelativeItem()
                        .AlignMiddle()
                        .LineHorizontal(0.8f)
                        .LineColor("#1E3A8A");
                });
        }
    }
}