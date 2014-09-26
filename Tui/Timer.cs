using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tui
{
    public class Timer
    {
        System.Timers.Timer timer;
        Screen screen;

        public event EventHandler Tick;

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
