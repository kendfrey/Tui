using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tui
{
    public struct CharData
    {
        static CharData()
        {
            Encoding = Encoding.GetEncoding(437);
        }

        public CharData(char character, TextColor foreground, TextColor background) : this()
        {
            Character = character;
            Foreground = foreground;
            Background = background;
        }

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

        public byte CharacterByte
        {
            get;
            set;
        }
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
        public TextColor Foreground
        {
            get;
            set;
        }
        public TextColor Background
        {
            get;
            set;
        }
    }
}
