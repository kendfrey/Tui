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
                    font = new FormatConvertedBitmap(new BitmapImage(new Uri("pack://application:,,,/Tui;component/Terminal.png")), PixelFormats.Indexed4, CreateDefaultPalette(), 0);
                    if (font.PixelWidth % 512 != 0)
                    {
                        throw new ArgumentException("The font image must contain 256 glyphs and each glyph must be a multiple of 2 pixels wide.");
                    }
                    fontWidth = font.PixelWidth / 256;
                    fontHeight = font.PixelHeight;
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
