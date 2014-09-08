using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Tui
{
    public class Screen
    {
        ScreenWindow window;
        WriteableBitmap display;
        BitmapSource font;
        int fontWidth;
        int fontHeight;

        public int Width
        {
            get;
            private set;
        }

        public int Height
        {
            get;
            private set;
        }

        public Screen() : this(80, 25)
        {
        }

        public Screen(int width, int height)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            Thread windowThread = new Thread(() =>
                {
                    window = new ScreenWindow();
                    BitmapPalette palette = CreateDefaultPalette();
                    font = new FormatConvertedBitmap(new BitmapImage(new Uri("pack://application:,,,/Tui;component/Terminal.png")), PixelFormats.Indexed4, palette, 0);
                    if (font.PixelWidth % 512 != 0)
                    {
                        throw new ArgumentException("The font image must contain 256 glyphs and each glyph must be a multiple of 2 pixels wide.");
                    }
                    fontWidth = font.PixelWidth / 256;
                    fontHeight = font.PixelHeight;
                    int imageWidth = width * fontWidth;
                    int imageHeight = height * fontHeight;
                    display = new WriteableBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Indexed4, palette);
                    byte[] data = new byte[imageWidth / 2 * imageHeight];
                    display.WritePixels(new Int32Rect(0, 0, imageWidth, imageHeight), data, imageWidth / 2, 0, 0);
                    window.image.Source = display;
                    window.image.Width = width * fontWidth;
                    window.image.Height = height * fontHeight;
                    window.SizeToContent = SizeToContent.WidthAndHeight;
                    window.Show();
                    tcs.SetResult(null);
                    Dispatcher.Run();
                });
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.IsBackground = true;
            windowThread.Start();
            tcs.Task.Wait();
        }

        public void WriteCharData(CharData charData, int x, int y)
        {
            window.Dispatcher.Invoke(() =>
                {
                    byte[] data = new byte[fontWidth / 2 * fontHeight];
                    font.CopyPixels(new Int32Rect(fontWidth * charData.CharacterByte, 0, fontWidth, fontHeight), data, fontWidth / 2, 0);
                    // output = (font & !foreground) ^ (font | background)
                    byte a = (byte)(~(int)charData.Foreground & 0x0F);
                    a |= (byte)(a << 4);
                    byte b = (byte)((int)charData.Background & 0x0F);
                    b |= (byte)(b << 4);
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte)((data[i] & a) ^ (data[i] | b));
                    }
                    display.WritePixels(new Int32Rect(0, 0, fontWidth, fontHeight), data, fontWidth / 2, x * fontWidth, y * fontHeight);
                });
        }

        public void Close()
        {
            window.Dispatcher.Invoke(() =>
                {
                    window.Close();
                    Dispatcher.ExitAllFrames();
                });
        }

        static BitmapPalette CreateDefaultPalette()
        {
            List<Color> colors = new List<Color>()
                {
                    Colors.Black,
                    Colors.Maroon,
                    Colors.Green,
                    Colors.Olive,
                    Colors.Navy,
                    Colors.Purple,
                    Colors.Teal,
                    Colors.Silver,
                    Colors.Gray,
                    Colors.Red,
                    Colors.Lime,
                    Colors.Yellow,
                    Colors.Blue,
                    Colors.Fuchsia,
                    Colors.Aqua,
                    Colors.White
                };
            return new BitmapPalette(colors);
        }
    }
}
