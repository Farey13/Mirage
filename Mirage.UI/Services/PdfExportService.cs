using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.Services;

public class PdfExportService : IPdfExportService
{
    public async Task GenerateReportPdfAsync(string reportTitle, string[] columnHeaders, IEnumerable<object> items)
    {
        if (!items.Any())
        {
            MessageBox.Show("There is no data to export.", "Export Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        await Task.Run(() =>
        {
            try
            {
                // 1. Define the file path
                var today = DateTime.Today;
                var directoryPath = Path.Combine("C:", "MirageReports", today.ToString("yyyy"), today.ToString("MMMM"), today.ToString("dd"));
                Directory.CreateDirectory(directoryPath);

                var fileName = $"{reportTitle.Replace(' ', '_')}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(directoryPath, fileName);

                // 2. Create the PDF document definition
                var document = new ReportDocument(reportTitle, columnHeaders, items);

                // 3. Generate the PDF
                document.GeneratePdf(filePath);

                // 4. Notify user and open the file
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var result = MessageBox.Show($"Successfully exported report to:\n{filePath}\n\nWould you like to open the file?", "Export Successful", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        // This opens the file with the default PDF viewer
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"An error occurred while generating the PDF: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        });
    }
}

public class ReportDocument : IDocument
{
    private readonly string _title;
    private readonly string[] _headers;
    private readonly IEnumerable<object> _items;

    public ReportDocument(string title, string[] headers, IEnumerable<object> items)
    {
        _title = title;
        _headers = headers;
        _items = items;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                // 1. SET THE PAGE TO LANDSCAPE (HORIZONTAL) ORIENTATION
                page.Size(PageSizes.A4.Landscape());
                page.Margin(40);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
    }

    void ComposeHeader(IContainer container)
    {
        // Header logic remains the same...
        var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(_title).Style(titleStyle);
                column.Item().Text($"Date Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}");
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(20).Table(table =>
        {
            // Column width logic remains the same
            table.ColumnsDefinition(columns =>
            {
                switch (_title)
                {
                    case "Machine Breakdowns":
                        columns.RelativeColumn(1.5f); // Reported On
                        columns.RelativeColumn(2f);   // Machine
                        columns.RelativeColumn(3f);   // Reason
                        columns.RelativeColumn(1.2f); // Reported By
                        columns.RelativeColumn(0.8f); // Is Resolved
                        columns.RelativeColumn(1.5f); // Resolved On
                        columns.RelativeColumn(1.2f); // Resolved By
                        columns.RelativeColumn(3f);   // Resolution
                        columns.RelativeColumn(1f);   // Downtime (Mins)
                        columns.RelativeColumn(1f);   // Downtime
                        break;

                    default:
                        foreach (var header in _headers)
                        {
                            var width = header.Length > 15 ? 2f : 1f;
                            columns.RelativeColumn(width);
                        }
                        break;
                }
            });

            // UPDATED: Header styling
            table.Header(header =>
            {
                foreach (var text in _headers)
                {
                    // 1. Changed AlignCenter to AlignLeft and added FontSize(10)
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).AlignLeft().Text(text).FontColor(Colors.White).SemiBold().FontSize(10);
                }
            });

            // UPDATED: Data row styling
            if (_items.Any())
            {
                var properties = _items.First().GetType().GetProperties();
                foreach (var (item, index) in _items.Select((value, i) => (value, i)))
                {
                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(item);
                        var formattedValue = FormatValue(value);
                        var cell = table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);

                        if (index % 2 != 0)
                        {
                            cell.Background(Colors.Grey.Lighten4);
                        }

                        // 2. Added AlignLeft to ensure all content is left-aligned
                        cell.AlignLeft().AlignMiddle().Text(formattedValue);
                    }
                }
            }
        });
    }

    // FormatValue helper method remains the same...
    string FormatValue(object? value)
    {
        if (value is null) return "N/A";
        if (value is DateTime dt)
        {
            return dt == default ? "N/A" : dt.ToString("dd/MMM/yy HH:mm");
        }
        if (value is bool b) return b ? "Yes" : "No";
        return value.ToString() ?? string.Empty;
    }
}