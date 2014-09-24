using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tui
{
    public class Timer
    {
        internal System.Timers.Timer InternalTimer
        {
            get;
            set;
        }

        public event EventHandler Tick;

        internal Timer()
        {
        }

        protected internal virtual void OnTick(EventArgs e)
        {
            EventHandler tick = Tick;
            if (tick != null)
            {
                tick(this, e);
            }
        }
    }
}
