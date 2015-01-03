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
    public class Timer
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
    }
}
