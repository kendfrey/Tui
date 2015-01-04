using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tui
{
    /// <summary>
    /// Specifies the way a screen will be displayed.
    /// </summary>
    public enum DisplayMode
    {
        /// <summary>
        /// The screen will be displayed in a window.
        /// </summary>
        FixedWindow,

        /// <summary>
        /// The screen will be displayed in a window with a draggable resize border.
        /// </summary>
        ResizableWindow,

        /// <summary>
        /// The screen will be displayed in fullscreen, with the character dimensions changed to fit the screen.
        /// </summary>
        Fullscreen,

        /// <summary>
        /// The screen will be displayed in fullscreen, with the characters scaled to fit the screen.
        /// </summary>
        FixedFullscreen
    }
}
