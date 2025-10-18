using System;
using System.Windows.Threading;

namespace Mirage.UI.Services
{
    public class InactivityService : IInactivityService
    {
        private readonly DispatcherTimer _inactivityTimer;
        public event Action? OnInactive;

        public InactivityService()
        {
            // Set the timeout period. We'll use 15 minutes for now.
            _inactivityTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(15)
            };

            // When the timer 'ticks' (i.e., the interval elapses), call our handler method.
            _inactivityTimer.Tick += InactivityTimer_Tick;
            _inactivityTimer.Start();
        }

        private void InactivityTimer_Tick(object? sender, EventArgs e)
        {
            // Stop the timer to prevent it from firing again.
            StopTimer();

            // Fire the event to notify the rest of the application.
            OnInactive?.Invoke();
        }

        public void ResetTimer()
        {
            // This method is called whenever the user is active.
            // It stops the timer and starts it again, resetting the countdown.
            _inactivityTimer.Stop();
            _inactivityTimer.Start();
        }

        public void StopTimer()
        {
            _inactivityTimer.Stop();
        }
    }
}