using System;

namespace Mirage.UI.Services
{
    public interface IInactivityService
    {
        event Action OnInactive;
        void StartTimer(int minutes); // Changed from ResetTimer
        void ResetTimer();
        void StopTimer();
    }
}