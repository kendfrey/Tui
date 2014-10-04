using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tui
{
    public class KeyboardInputEventArgs : EventArgs
    {
        public bool HasText
        {
            get;
            set;
        }

        public string Text
        {
            get;
            set;
        }

        public bool HasKey
        {
            get;
            set;
        }

        public Key Key
        {
            get;
            set;
        }

        public bool IsRepeatKey
        {
            get;
            set;
        }
    }
}
