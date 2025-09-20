using PortalMirage.Core.Models;

namespace PortalMirage.Business.Abstractions;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}