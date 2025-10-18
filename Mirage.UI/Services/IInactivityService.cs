using System;

namespace Mirage.UI.Services
{
    public interface IInactivityService
    {
        event Action OnInactive;
        void ResetTimer();
        void StopTimer();
    }
}