using System;
using System.Windows.Threading;

namespace Mirage.UI.Services
{
    public class InactivityService : IInactivityService
    {
        private DispatcherTimer? _inactivityTimer;
        public event Action? OnInactive;

        public void StartTimer(int minutes)
        {
            // Stop any existing timer first
            StopTimer();

            _inactivityTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(minutes)
            };

            _inactivityTimer.Tick += InactivityTimer_Tick;
            _inactivityTimer.Start();
        }

        private void InactivityTimer_Tick(object? sender, EventArgs e)
        {
            StopTimer();
            OnInactive?.Invoke();
        }

        public void ResetTimer()
        {
            if (_inactivityTimer != null)
            {
                _inactivityTimer.Stop();
                _inactivityTimer.Start();
            }
        }

        public void StopTimer()
        {
            _inactivityTimer?.Stop();
            _inactivityTimer = null;
        }
    }
}