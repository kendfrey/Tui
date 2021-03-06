﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Tui
{
    /// <summary>
    /// Represents a text-mode screen or terminal.
    /// </summary>
    public class Screen
    {
        ScreenWindow window;
        WriteableBitmap display;
        byte[] font;
        int fontWidth;
        int fontHeight;
        BitmapPalette palette;
        CharData[,] buffer;
        BlockingCollection<Action> eventQueue;
        bool closing;
        KeyEventArgs keyArgs;
        TextCompositionEventArgs textArgs;
        Dictionary<Key, TextCompositionEventArgs> pressedKeys;
        string title;
        DisplayMode displayMode;

        /// <summary>
        /// Gets the width of the screen, in characters.
        /// </summary>
        public int Width
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the height of the screen, in characters.
        /// </summary>
        public int Height
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the title to display on the terminal window.
        /// </summary>
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                window.Dispatcher.Invoke(() => window.Title = title);
            }
        }

        /// <summary>
        /// Gets or sets the mode that the terminal window is displayed in.
        /// </summary>
        public DisplayMode DisplayMode
        {
            get
            {
                return displayMode;
            }
            set
            {
                if (displayMode == value)
                {
                    return;
                }
                displayMode = value;
                window.Dispatcher.Invoke(UpdateDisplayMode);
            }
        }

        /// <summary>
        /// Occurs when the user presses a key.
        /// </summary>
        public event EventHandler<KeyboardInputEventArgs> KeyboardInput;

        /// <summary>
        /// Occurs when the user releases a key.
        /// </summary>
        public event EventHandler<KeyboardInputEventArgs> KeyboardInputReleased;

        /// <summary>
        /// Occurs when the screen has been requested to close.
        /// </summary>
        public event EventHandler Closing;

        /// <summary>
        /// Occurs when the size of the screen has changed.
        /// </summary>
        public event EventHandler<ResizeEventArgs> Resized;

        /// <summary>
        /// Initializes a new instance of the Screen class, using the default size of 80x25.
        /// </summary>
        public Screen() : this(80, 25)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Screen class, using the specified size.
        /// </summary>
        /// <param name="width">The width of the screen, in characters.</param>
        /// <param name="height">The height of the screen, in characters.</param>
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
            Width = width;
            Height = height;
            ManualResetEvent windowInitialized = new ManualResetEvent(false);
            Thread windowThread = new Thread(() => InitializeWindow(windowInitialized));
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.IsBackground = true;
            windowThread.Start();
            eventQueue = new BlockingCollection<Action>();
            pressedKeys = new Dictionary<Key, TextCompositionEventArgs>();
            windowInitialized.WaitOne();
        }

        /// <summary>
        /// Clears all characters from the screen.
        /// </summary>
        public void Clear()
        {
            Clear(new Rectangle(0, 0, Width, Height));
        }

        /// <summary>
        /// Clears all characters from the specified area of the screen.
        /// </summary>
        /// <param name="rectangle">The area to clear.</param>
        public void Clear(Rectangle rectangle)
        {
            CharData charData = new CharData();
            charData.CharacterByte = 0;
            charData.Foreground = TextColor.LightGray;
            charData.Background = TextColor.Black;
            FillCharData(charData, rectangle);
        }

        /// <summary>
        /// Fills an area with the specified character, preserving color.
        /// </summary>
        /// <param name="character">The character to fill the area with.</param>
        /// <param name="rectangle">The area to fill.</param>
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

        /// <summary>
        /// Fills an area with the specified character, preserving color.
        /// </summary>
        /// <param name="character">The byte value of the character to fill the area with.</param>
        /// <param name="rectangle">The area to fill.</param>
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

        /// <summary>
        /// Fills an area with the specified colors, preserving characters.
        /// </summary>
        /// <param name="foreground">The color to fill the area's foreground with.</param>
        /// <param name="background">The color to fill the area's background with.</param>
        /// <param name="rectangle">The area to fill.</param>
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

        /// <summary>
        /// Fills an area with the specified foreground color, preserving characters and background color.
        /// </summary>
        /// <param name="foreground">The color to fill the area's foreground with.</param>
        /// <param name="rectangle">The area to fill.</param>
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

        /// <summary>
        /// Fills an area with the specified background color, preserving characters and foreground color.
        /// </summary>
        /// <param name="background">The color to fill the area's background with.</param>
        /// <param name="rectangle">The area to fill.</param>
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

        /// <summary>
        /// Fills an area with the specified character and color.
        /// </summary>
        /// <param name="charData">The character and color to fill the area with.</param>
        /// <param name="rectangle">The area to fill.</param>
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

        /// <summary>
        /// Writes a character to the specified location, preserving color.
        /// </summary>
        /// <param name="character">The character to write.</param>
        /// <param name="x">The X coordinate to write to.</param>
        /// <param name="y">The Y coordinate to write to.</param>
        public void WriteChar(char character, int x, int y)
        {
            CheckBounds(x, y);
            buffer[y, x].Character = character;
            Draw(x, y);
        }

        /// <summary>
        /// Writes a character to the specified location, preserving color.
        /// </summary>
        /// <param name="character">The byte value of the character to write.</param>
        /// <param name="x">The X coordinate to write to.</param>
        /// <param name="y">The Y coordinate to write to.</param>
        public void WriteCharByte(byte character, int x, int y)
        {
            CheckBounds(x, y);
            buffer[y, x].CharacterByte = character;
            Draw(x, y);
        }

        /// <summary>
        /// Writes the specified colors to the specified location, preserving characters.
        /// </summary>
        /// <param name="foreground">The foreground color to write.</param>
        /// <param name="background">The background color to write.</param>
        /// <param name="x">The X coordinate to write to.</param>
        /// <param name="y">The Y coordinate to write to.</param>
        public void WriteColors(TextColor foreground, TextColor background, int x, int y)
        {
            CheckBounds(x, y);
            buffer[y, x].Foreground = foreground;
            buffer[y, x].Background = background;
            Draw(x, y);
        }

        /// <summary>
        /// Writes the specified foreground color to the specified location, preserving characters and background color.
        /// </summary>
        /// <param name="foreground">The foreground color to write.</param>
        /// <param name="x">The X coordinate to write to.</param>
        /// <param name="y">The Y coordinate to write to.</param>
        public void WriteForeground(TextColor foreground, int x, int y)
        {
            CheckBounds(x, y);
            buffer[y, x].Foreground = foreground;
            Draw(x, y);
        }

        /// <summary>
        /// Writes the specified bacground color to the specified location, preserving characters and foreground color.
        /// </summary>
        /// <param name="background">The background color to write.</param>
        /// <param name="x">The X coordinate to write to.</param>
        /// <param name="y">The Y coordinate to write to.</param>
        public void WriteBackground(TextColor background, int x, int y)
        {
            CheckBounds(x, y);
            buffer[y, x].Background = background;
            Draw(x, y);
        }

        /// <summary>
        /// Writes the specified character and color to the specified location.
        /// </summary>
        /// <param name="charData">The character and color to write.</param>
        /// <param name="x">The X coordinate to write to.</param>
        /// <param name="y">The Y coordinate to write to.</param>
        public void WriteCharData(CharData charData, int x, int y)
        {
            CheckBounds(x, y);
            buffer[y, x] = charData;
            Draw(x, y);
        }

        /// <summary>
        /// Writes the specified string beginning at the specified location, preserving color.
        /// </summary>
        /// <param name="str">The string to write.</param>
        /// <param name="x">The X coordinate to write to.</param>
        /// <param name="y">The Y coordinate to write to.</param>
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

        /// <summary>
        /// Copies a character and color buffer directly to the screen.
        /// </summary>
        /// <param name="source">The buffer to copy from, represented in row-major order (Y coordinate first).</param>
        /// <param name="sourceX">The X coordinate of the location in the buffer to copy from.</param>
        /// <param name="sourceY">The Y coordinate of the location in the buffer to copy from.</param>
        /// <param name="rectangle">The area of the screen to copy to.</param>
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

        /// <summary>
        /// Gets the character and color currently displayed at the specified location.
        /// </summary>
        /// <param name="x">The X coordinate to get.</param>
        /// <param name="y">The Y coordinate to get.</param>
        /// <returns>The character and color from the specified location.</returns>
        public CharData GetCharData(int x, int y)
        {
            CheckBounds(x, y);
            return buffer[y, x];
        }

        /// <summary>
        /// Copies a character and color buffer directly from the screen.
        /// </summary>
        /// <param name="destination">The buffer to copy to, represented in row-major order (Y coordinate first).</param>
        /// <param name="destinationX">The X coordinate of the location in the buffer to copy to.</param>
        /// <param name="destinationY">The Y coordinate of the location in the buffer to copy to.</param>
        /// <param name="rectangle">The area of the screen to copy from.</param>
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

        /// <summary>
        /// Runs the event loop for the screen.
        /// </summary>
        public void Run()
        {
            while (!closing)
            {
                Action action = eventQueue.Take();
                action();
            }
            CloseWindow();
        }

        /// <summary>
        /// Changes the size of the screen.
        /// </summary>
        /// <param name="width">The width of the screen, in characters.</param>
        /// <param name="height">The height of the screen, in characters.</param>
        public void Resize(int width, int height)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException("width", width, "width must not be negative.");
            }
            if (height < 0)
            {
                throw new ArgumentOutOfRangeException("height", height, "height must not be negative.");
            }
            window.Dispatcher.Invoke(() => ResizeImage(width, height));
        }

        /// <summary>
        /// Changes the font used to display the characters on the screen.
        /// </summary>
        /// <param name="fontPath">The path to the font bitmap to use, or null to use the default font.</param>
        public void SetFont(string fontPath)
        {
            string uri;
            if (fontPath == null)
            {
                uri = "pack://application:,,,/Tui;component/Terminal.png";
            }
            else
            {
                uri = "file://" + Path.GetFullPath(fontPath);
            }
            BitmapSource fontBitmap = new FormatConvertedBitmap(new FormatConvertedBitmap(new BitmapImage(new Uri(uri)), PixelFormats.BlackWhite, null, 0), PixelFormats.Indexed4, CreateDefaultPalette(), 0);
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
            window.Dispatcher.Invoke(CreateImage);
        }

        /// <summary>
        /// Changes the color palette used to display the characters on the screen.
        /// </summary>
        /// <param name="colors">An array of 16 colors to use as the new palette.</param>
        public void SetPalette(ColorData[] colors)
        {
            if (colors == null || colors.Length != 16)
            {
                throw new ArgumentException("colors must be an array of exactly 16 items.");
            }
            palette = new BitmapPalette(colors.Select(c => Color.FromRgb(c.R, c.G, c.B)).ToList());
            window.Dispatcher.Invoke(CreateImage);
        }

        /// <summary>
        /// Closes the screen and terminates the event loop.
        /// </summary>
        public void Close()
        {
            closing = true;
        }

        internal void PushEvent(Action action)
        {
            eventQueue.Add(action);
        }

        /// <summary>
        /// Raises the Closing event.
        /// </summary>
        /// <param name="e">The event data to pass to the event.</param>
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

        /// <summary>
        /// Raises the KeyboardInput event.
        /// </summary>
        /// <param name="e">The event data to pass to the event.</param>
        protected virtual void OnKeyboardInput(KeyboardInputEventArgs e)
        {
            EventHandler<KeyboardInputEventArgs> keyboardInput = KeyboardInput;
            if (keyboardInput != null)
            {
                keyboardInput(this, e);
            }
        }

        /// <summary>
        /// Raises the KeyboardInputReleased event.
        /// </summary>
        /// <param name="e">The event data to pass to the event.</param>
        protected virtual void OnKeyboardInputReleased(KeyboardInputEventArgs e)
        {
            EventHandler<KeyboardInputEventArgs> keyboardInputReleased = KeyboardInputReleased;
            if (keyboardInputReleased != null)
            {
                keyboardInputReleased(this, e);
            }
        }

        /// <summary>
        /// Raises the Resized event.
        /// </summary>
        /// <param name="e">The event data to pass to the event.</param>
        protected virtual void OnResized(ResizeEventArgs e)
        {
            EventHandler<ResizeEventArgs> resized = Resized;
            if (resized != null)
            {
                resized(this, e);
            }
        }

        private void InitializeWindow(ManualResetEvent windowInitialized)
        {
            window = new ScreenWindow();
            buffer = new CharData[Height, Width];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    buffer[y, x].CharacterByte = 0;
                    buffer[y, x].Background = TextColor.Black;
                    buffer[y, x].Foreground = TextColor.LightGray;
                }
            }
            palette = CreateDefaultPalette();
            SetFont(null);
            Title = "Tui";
            displayMode = DisplayMode.FixedWindow;
            UpdateDisplayMode();
            window.TextInput += window_TextInput;
            window.KeyDown += window_KeyDown;
            window.KeyUp += window_KeyUp;
            window.SizeChanged += window_SizeChanged;
            window.Closing += window_Closing;
            window.Show();
            HwndSource hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
            hwndSource.AddHook(window_WndProc);
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

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (window.SizeToContent == SizeToContent.WidthAndHeight || DisplayMode == DisplayMode.FixedFullscreen)
            {
                // The image is already the correct size.
                return;
            }
            ResizeEventArgs args = new ResizeEventArgs();
            args.NewWidth = (int)window.grid.ActualWidth / fontWidth;
            args.NewHeight = (int)window.grid.ActualHeight / fontHeight;
            if (args.NewWidth != Width || args.NewHeight != Height)
            {
                ResizeImage(args.NewWidth, args.NewHeight);
                PushEvent(() => OnResized(args));
            }
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PushEvent(() => OnClosing(new EventArgs()));
        }

        private IntPtr window_WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_SIZEMOVE = 0x0232;
            switch (msg)
            {
                case WM_SIZEMOVE:
                    window.SizeToContent = SizeToContent.WidthAndHeight;
                    break;
            }
            return IntPtr.Zero;
        }

        private void UpdateDisplayMode()
        {
            window.image.Stretch = Stretch.None;
            window.WindowStyle = WindowStyle.SingleBorderWindow;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.WindowState = WindowState.Normal;
            window.ResizeMode = ResizeMode.CanMinimize;
            switch (displayMode)
            {
                case DisplayMode.FixedWindow:
                    break;
                case DisplayMode.ResizableWindow:
                    window.ResizeMode = ResizeMode.CanResize;
                    break;
                case DisplayMode.Fullscreen:
                    window.WindowStyle = WindowStyle.None;
                    window.SizeToContent = SizeToContent.Manual;
                    window.WindowState = WindowState.Maximized;
                    break;
                case DisplayMode.FixedFullscreen:
                    window.WindowStyle = WindowStyle.None;
                    window.SizeToContent = SizeToContent.Manual;
                    window.WindowState = WindowState.Maximized;
                    window.image.Stretch = Stretch.Fill;
                    window.image.Width = double.NaN;
                    window.image.Height = double.NaN;
                    break;
            }
        }

        private void ResizeImage(int width, int height)
        {
            int previousWidth = Width;
            int previousHeight = Height;
            Width = width;
            Height = height;
            CharData[,] newBuffer = new CharData[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < previousWidth && y < previousHeight)
                    {
                        newBuffer[y, x] = buffer[y, x];
                    }
                    else
                    {
                        newBuffer[y, x].CharacterByte = 0;
                        newBuffer[y, x].Background = TextColor.Black;
                        newBuffer[y, x].Foreground = TextColor.LightGray;
                    }
                }
            }
            buffer = newBuffer;
            CreateImage();
        }

        private void CreateImage()
        {
            int imageWidth = Width * fontWidth;
            int imageHeight = Height * fontHeight;
            display = new WriteableBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Indexed4, palette);
            window.image.Source = display;
            if (DisplayMode != DisplayMode.FixedFullscreen)
            {
                window.image.Width = imageWidth;
                window.image.Height = imageHeight;
            }
            Draw(new Rectangle(0, 0, Width, Height));
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
            if (!window.Dispatcher.CheckAccess())
            {
                window.Dispatcher.InvokeAsync(() => Draw(rectangle), DispatcherPriority.Send);
                return;
            }
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
