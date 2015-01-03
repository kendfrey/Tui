using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tui
{
    /// <summary>
    /// Represents a character cell containing a character, foreground color, and background color.
    /// </summary>
    public struct CharData
    {
        static CharData()
        {
            Encoding = Encoding.GetEncoding(437);
        }

        /// <summary>
        /// Initializes a new instance of the CharData structure with the specified character and colors.
        /// </summary>
        /// <param name="character">The character to display.</param>
        /// <param name="foreground">The foreground color.</param>
        /// <param name="background">The background color.</param>
        public CharData(char character, TextColor foreground, TextColor background) : this()
        {
            Character = character;
            Foreground = foreground;
            Background = background;
        }

        /// <summary>
        /// Initializes a new instance of the CharData structure with the specified character and colors.
        /// </summary>
        /// <param name="characterByte">The byte value of the character to display.</param>
        /// <param name="foreground">The foreground color.</param>
        /// <param name="background">The background color.</param>
        public CharData(byte characterByte, TextColor foreground, TextColor background) : this()
        {
            CharacterByte = characterByte;
            Foreground = foreground;
            Background = background;
        }

        internal static Encoding Encoding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the byte value of the character to display, using code page 437.
        /// </summary>
        public byte CharacterByte
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the character to display.
        /// </summary>
        public char Character
        {
            get
            {
                return Encoding.GetChars(new byte[] { CharacterByte })[0];
            }
            set
            {
                CharacterByte = Encoding.GetBytes(new char[] { value })[0];
            }
        }

        /// <summary>
        /// Gets or sets the color to display the character in.
        /// </summary>
        public TextColor Foreground
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the color to display the background in.
        /// </summary>
        public TextColor Background
        {
            get;
            set;
        }
    }
}
