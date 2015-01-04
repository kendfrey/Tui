using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tui
{
    /// <summary>
    /// Contains data for keyboard input events.
    /// </summary>
    public class KeyboardInputEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the event includes textual input.
        /// </summary>
        public bool HasText
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the text input from the event.
        /// </summary>
        public string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the event was triggered by a key.
        /// </summary>
        public bool HasKey
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the key that triggered the event.
        /// </summary>
        public Key Key
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the event is a repeated key.
        /// </summary>
        public bool IsRepeatKey
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Contains data for screen resize events.
    /// </summary>
    public class ResizeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the new width of the screen, in characters.
        /// </summary>
        public int NewWidth
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the new height of the screen, in characters.
        /// </summary>
        public int NewHeight
        {
            get;
            set;
        }
    }
}
