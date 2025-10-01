using PortalMirage.Core.Dtos;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}