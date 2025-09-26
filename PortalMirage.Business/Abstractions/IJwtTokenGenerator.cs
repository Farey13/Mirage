using PortalMirage.Core.Models;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions;

public interface IJwtTokenGenerator
{
    Task<string> GenerateTokenAsync(User user);
}