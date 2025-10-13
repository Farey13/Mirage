using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mirage.UI.Services
{
    public interface IPdfExportService
    {
        Task GenerateReportPdfAsync(string reportTitle, string[] columnHeaders, IEnumerable<object> items);
    }
}