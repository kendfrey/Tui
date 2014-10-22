using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Tui
{
    public class Screen
    {
        ScreenWindow window;
        WriteableBitmap display;
        byte[] font;
        int fontWidth;
        int fontHeight;
        CharData[,] buffer;
        BlockingCollection<Action> eventQueue;
        bool closing;
        KeyEventArgs keyArgs;
        TextCompositionEventArgs textArgs;
        Dictionary<Key, TextCompositionEventArgs> pressedKeys;

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

        public event EventHandler<KeyboardInputEventArgs> KeyboardInput;

        public event EventHandler<KeyboardInputEventArgs> KeyboardInputReleased;

        public event EventHandler Closing;

        public Screen() : this(80, 25)
        {
        }

        public Screen(int width, int height)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException("width", width, "width must not be negative.");
            }
            if (height < 0)
            {
                throw new ArgumentOutOfRangeException("height", height, "height must not be negative.");
            }
            ManualResetEvent windowInitialized = new ManualResetEvent(false);
            Thread windowThread = new Thread(() => InitializeWindow(width, height, windowInitialized));
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.IsBackground = true;
            windowThread.Start();
            Width = width;
            Height = height;
            buffer = new CharData[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    buffer[y, x].CharacterByte = 0;
                    buffer[y, x].Background = TextColor.Black;
                    buffer[y, x].Foreground = TextColor.LightGray;
                }
            }
            eventQueue = new BlockingCollection<Action>();
            pressedKeys = new Dictionary<Key, TextCompositionEventArgs>();
            windowInitialized.WaitOne();
        }

        public void Clear()
        {
            Clear(new Rectangle(0, 0, Width, Height));
        }

        public void Clear(Rectangle rectangle)
        {
            CharData charData = new CharData();
            charData.CharacterByte = 0;
            charData.Foreground = TextColor.LightGray;
            charData.Background = TextColor.Black;
            FillCharData(charData, rectangle);
        }

        public void FillChar(char character, Rectangle rectangle)
        {
            for (int y = rectangle.Y; y < rectangle.Y + rectangle.Height; y++)
            {
                for (int x = rectangle.X; x < rectangle.X + rectangle.Width; x++)
                {
                    CheckBounds(x, y);
                    buffer[y, x].Character = character;
                }
            }
            Draw(rectangle);
        }

        public void FillCharByte(byte character, Rectangle rectangle)
        {
            for (int y = rectangle.Y; y < rectangle.Y + rectangle.Height; y++)
            {
                for (int x = rectangle.X; x < rectangle.X + rectangle.Width; x++)
                {
                    CheckBounds(x, y);
                    buffer[y, x].CharacterByte = character;
                }
            }
            Draw(rectangle);
        }

        public void FillColors(TextColor foreground, TextColor background, Rectangle rectangle)
        {
            for (int y = rectangle.Y; y < rectangle.Y + rectangle.Height; y++)
            {
                for (int x = rectangle.X; x < rectangle.X + rectangle.Width; x++)
                {
                    CheckBounds(x, y);
                    buffer[y, x].Foreground = foreground;
                    buffer[y, x].Background = background;
                }
            }
            Draw(rectangle);
        }

        public void FillForeground(TextColor foreground, Rectangle rectangle)
        {
            for (int y = rectangle.Y; y < rectangle.Y + rectangle.Height; y++)
            {
                for (int x = rectangle.X; x < rectangle.X + rectangle.Width; x++)
                {
                    CheckBounds(x, y);
                    buffer[y, x].Foreground = foreground;
                }
            }
            Draw(rectangle);
        }

        public void FillBackground(TextColor background, Rectangle rectangle)
        {
            for (int y = rectangle.Y; y < rectangle.Y + rectangle.Height; y++)
            {
                for (int x = rectangle.X; x < rectangle.X + rectangle.Width; x++)
                {
                    CheckBounds(x, y);
                    buffer[y, x].Background = background;
                }
            }
            Draw(rectangle);
        }

        public void FillCharData(CharData charData, Rectangle rectangle)
        {
            for (int y = rectangle.Y; y < rectangle.Y + rectangle.Height; y++)
            {
                for (int x = rectangle.X; x < rectangle.X + rectangle.Width; x++)
                {
                    CheckBounds(x, y);
                    buffer[y, x] = charData;
                }
            }
            Draw(rectangle);
        }

        public void WriteChar(char character, int x, int y)
        {
            CheckBounds(x, y);
            buffer[y, x].Character = character;
            Draw(x, y);
        }

        public void WriteCharByte(byte character, int x, int y)
        {
            CheckBounds(x, y);
            buffer[y, x].CharacterByte = character;
            Draw(x, y);
        }

        public void WriteColors(TextColor foreground, TextColor background, int x, int y)
        {
            CheckBounds(x, y);
            buffer[y, x].Foreground = foreground;
            buffer[y, x].Background = background;
            Draw(x, y);
        }

        public void WriteForeground(TextColor foreground, int x, int y)
        {
            CheckBounds(x, y);
            buffer[y, x].Foreground = foreground;
            Draw(x, y);
        }

        public void WriteBackground(TextColor background, int x, int y)
        {
            CheckBounds(x, y);
            buffer[y, x].Background = background;
            Draw(x, y);
        }

        public void WriteCharData(CharData charData, int x, int y)
        {
            CheckBounds(x, y);
            buffer[y, x] = charData;
            Draw(x, y);
        }

        public void WriteString(string str, int x, int y)
        {
            for (int i = 0; i < str.Length; i++)
            {
                WriteChar(str[i], x, y);
                x++;
                if (x >= Width)
                {
                    x = 0;
                    y++;
                    if (y >= Height)
                    {
                        y = 0;
                    }
                }
            }
        }

        public void WriteBuffer(CharData[,] source, int sourceX, int sourceY, Rectangle rectangle)
        {
            for (int y = 0; y < rectangle.Height; y++)
            {
                for (int x = 0; x < rectangle.Width; x++)
                {
                    CheckBounds(x + rectangle.X, y + rectangle.Y);
                    buffer[y + rectangle.Y, x + rectangle.X] = source[y + sourceY, x + sourceX];
                }
            }
            Draw(rectangle);
        }

        public CharData GetCharData(int x, int y)
        {
            CheckBounds(x, y);
            return buffer[y, x];
        }

        public void GetBuffer(CharData[,] destination, int destinationX, int destinationY, Rectangle rectangle)
        {
            for (int y = 0; y < rectangle.Height; y++)
            {
                for (int x = 0; x < rectangle.Width; x++)
                {
                    CheckBounds(x + rectangle.X, y + rectangle.Y);
                    destination[y + destinationY, x + destinationX] = buffer[y + rectangle.Y, x + rectangle.X];
                }
            }
        }

        public void Run()
        {
            while (!closing)
            {
                Action action = eventQueue.Take();
                action();
            }
            CloseWindow();
        }

        public void Close()
        {
            closing = true;
        }

        internal void PushEvent(Action action)
        {
            eventQueue.Add(action);
        }

        protected virtual void OnClosing(EventArgs e)
        {
            EventHandler closing = Closing;
            if (closing != null)
            {
                closing(this, e);
            }
            else
            {
                Close();
            }
        }

        protected virtual void OnKeyboardInput(KeyboardInputEventArgs e)
        {
            EventHandler<KeyboardInputEventArgs> keyboardInput = KeyboardInput;
            if (keyboardInput != null)
            {
                keyboardInput(this, e);
            }
        }

        protected virtual void OnKeyboardInputReleased(KeyboardInputEventArgs e)
        {
            EventHandler<KeyboardInputEventArgs> keyboardInputReleased = KeyboardInputReleased;
            if (keyboardInputReleased != null)
            {
                keyboardInputReleased(this, e);
            }
        }

        private void InitializeWindow(int width, int height, ManualResetEvent windowInitialized)
        {
            window = new ScreenWindow();
            BitmapPalette palette = CreateDefaultPalette();
            BitmapSource fontBitmap = new FormatConvertedBitmap(new FormatConvertedBitmap(new BitmapImage(new Uri("pack://application:,,,/Tui;component/Terminal.png")), PixelFormats.BlackWhite, null, 0), PixelFormats.Indexed4, palette, 0);
            if (fontBitmap.PixelWidth % 512 != 0)
            {
                throw new ArgumentException("The font image must contain 256 glyphs and each glyph must be a multiple of 2 pixels wide.");
            }
            fontWidth = fontBitmap.PixelWidth / 256;
            fontHeight = fontBitmap.PixelHeight;
            font = new byte[fontWidth / 2 * fontHeight * 256];
            for (int i = 0; i < 256; i++)
            {
                fontBitmap.CopyPixels(new Int32Rect(fontWidth * i, 0, fontWidth, fontHeight), font, fontWidth / 2, fontWidth / 2 * fontHeight * i);
            }
            int imageWidth = width * fontWidth;
            int imageHeight = height * fontHeight;
            display = new WriteableBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Indexed4, palette);
            byte[] data = new byte[imageWidth / 2 * imageHeight];
            display.WritePixels(new Int32Rect(0, 0, imageWidth, imageHeight), data, imageWidth / 2, 0, 0);
            window.image.Source = display;
            window.image.Width = width * fontWidth;
            window.image.Height = height * fontHeight;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.TextInput += window_TextInput;
            window.KeyDown += window_KeyDown;
            window.KeyUp += window_KeyUp;
            window.Closing += window_Closing;
            window.Show();
            windowInitialized.Set();
            Dispatcher.Run();
        }

        private void window_TextInput(object sender, TextCompositionEventArgs e)
        {
            if (textArgs != null)
            {
                ProcessKeyboardInput();
            }
            textArgs = e;
            window.Dispatcher.InvokeAsync(ProcessKeyboardInput, DispatcherPriority.Input);
        }

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            if (keyArgs != null)
            {
                ProcessKeyboardInput();
            }
            keyArgs = e;
            window.Dispatcher.InvokeAsync(ProcessKeyboardInput, DispatcherPriority.Input);
        }

        private void window_KeyUp(object sender, KeyEventArgs e)
        {
            if (pressedKeys.ContainsKey((Key)e.Key))
            {
                TextCompositionEventArgs textArgs = pressedKeys[(Key)e.Key];
                pressedKeys.Remove((Key)e.Key);
                ProcessKeyBoardInputReleased(textArgs, e);
            }
        }

        private void ProcessKeyboardInput()
        {
            KeyboardInputEventArgs e = new KeyboardInputEventArgs();
            if (textArgs != null)
            {
                e.HasText = true;
                e.Text = textArgs.Text;
            }
            if (keyArgs != null)
            {
                e.HasKey = true;
                e.Key = (Key)keyArgs.Key;
                if (!pressedKeys.ContainsKey(e.Key))
                {
                    pressedKeys.Add(e.Key, textArgs);
                }
                else
                {
                    e.IsRepeatKey = true;
                }
            }
            PushEvent(() => OnKeyboardInput(e));
            if (keyArgs == null)
            {
                ProcessKeyBoardInputReleased(textArgs, null);
            }
            textArgs = null;
            keyArgs = null;
        }

        private void ProcessKeyBoardInputReleased(TextCompositionEventArgs textArgs, KeyEventArgs keyArgs)
        {
            KeyboardInputEventArgs args = new KeyboardInputEventArgs();
            if (textArgs != null)
            {
                args.HasText = true;
                args.Text = textArgs.Text;
            }
            if (keyArgs != null)
            {
                args.HasKey = true;
                args.Key = (Key)keyArgs.Key;
                args.IsRepeatKey = keyArgs.IsRepeat;
            }
            PushEvent(() => OnKeyboardInputReleased(args));
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PushEvent(() => OnClosing(new EventArgs()));
        }

        private void CloseWindow()
        {
            window.Dispatcher.Invoke(() =>
                {
                    window.Close();
                    Dispatcher.ExitAllFrames();
                });
        }

        private void CheckBounds(int x, int y)
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException("x", x, "x must be between 0 and Width - 1.");
            }
            if (y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException("y", y, "y must be between 0 and Height - 1.");
            }
        }

        private void Draw(int x, int y)
        {
            Draw(new Rectangle(x, y, 1, 1));
        }

        private void Draw(Rectangle rectangle)
        {
            window.Dispatcher.InvokeAsync(() =>
            {
                byte[] data = new byte[fontWidth / 2 * fontHeight];
                unsafe
                {
                    display.Lock();
                    byte* pBuffer = (byte*)display.BackBuffer;
                    int bufferStride = display.BackBufferStride;
                    for (int y = rectangle.Y; y < rectangle.Y + rectangle.Height; y++)
                    {
                        for (int x = rectangle.X; x < rectangle.X + rectangle.Width; x++)
                        {
                            Array.Copy(font, fontWidth / 2 * fontHeight * buffer[y, x].CharacterByte, data, 0, fontWidth / 2 * fontHeight);
                            // output = (font & !foreground) ^ (font | background)
                            byte a = (byte)(~(int)buffer[y, x].Foreground & 0x0F);
                            a |= (byte)(a << 4);
                            byte b = (byte)((int)buffer[y, x].Background & 0x0F);
                            b |= (byte)(b << 4);
                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = (byte)((data[i] & a) ^ (data[i] | b));
                            }
                            for (int py = 0; py < fontHeight; py++)
                            {
                                for (int px = 0; px < fontWidth / 2; px++)
                                {
                                    pBuffer[(py + y * fontHeight) * bufferStride + (px + x * fontWidth / 2)] = data[py * fontWidth / 2 + px];
                                }
                            }
                        }
                    }
                    display.AddDirtyRect(new Int32Rect(rectangle.X * fontWidth, rectangle.Y * fontHeight, rectangle.Width * fontWidth, rectangle.Height * fontHeight));
                    display.Unlock();
                }
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
