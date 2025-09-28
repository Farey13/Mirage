using System;
namespace PortalMirage.Business;

public interface ITimeProvider { DateTime Now { get; } DateTime Today { get; } }
public class SystemTimeProvider : ITimeProvider { public DateTime Now => DateTime.Now; public DateTime Today => DateTime.Today; }