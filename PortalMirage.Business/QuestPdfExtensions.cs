using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PortalMirage.Business
{
    public static class QuestPdfExtensions
    {
        // Defines the standard style for a data cell
        public static IContainer CellStyle(this IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(5);
        }

        // Defines the standard style for a header cell
        public static IContainer HeaderStyle(this IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Black)
                .PaddingVertical(5);
        }
    }
}