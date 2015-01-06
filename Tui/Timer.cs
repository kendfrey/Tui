using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tui
{
    /// <summary>
    /// Allows firing events at specific times.
    /// </summary>
    public class Timer : IDisposable
    {
        System.Timers.Timer timer;
        Screen screen;

        /// <summary>
        /// Occurs when the interval since the last tick has elapsed.
        /// </summary>
        public event EventHandler Tick;

        /// <summary>
        /// Initializes and starts a new timer.
        /// </summary>
        /// <param name="interval">The interval between ticks.</param>
        /// <param name="screen">The screen containing the event loop used to process the ticks.</param>
        public Timer(TimeSpan interval, Screen screen)
        {
            this.screen = screen;
            timer = new System.Timers.Timer(interval.TotalMilliseconds);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            screen.PushEvent(() => OnTick(new EventArgs()));
        }

        /// <summary>
        /// Raises the Tick event.
        /// </summary>
        /// <param name="e">The event data to pass to the event.</param>
        protected virtual void OnTick(EventArgs e)
        {
            EventHandler tick = Tick;
            if (tick != null)
            {
                tick(this, e);
            }
        }

        /// <summary>
        /// Stops the timer and releases all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by the timer.
        /// </summary>
        /// <param name="disposing">Specifies whether to release managed resources in addition to unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Dispose();
            }
        }

        ~Timer()
        {
            Dispose(false);
        }
    }
}
