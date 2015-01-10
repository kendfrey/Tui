# Tui

#### What is it?

Tui is a text rendering engine designed to look like a terminal or text-mode screen.

#### How do I use it?

What better way to find out than by saying hello to this wonderful world of ours?

```csharp
using Tui;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Screen screen = new Screen();
            screen.WriteString("Hello, World!", 0, 0);
            screen.Run();
        }
    }
}
```

![Hello, World!](img/hello1.png)

#### What else can I do?

Colorful text!

```csharp
Screen screen = new Screen();
screen.WriteString("Hello, World!", 0, 0);
screen.FillForeground(TextColor.Red, new Rectangle(0, 0, 5, 1));
screen.FillForeground(TextColor.Yellow, new Rectangle(7, 0, 5, 1));
screen.Run();
```

![*Hello, World!*](img/hello2.png)

Fancy art!

```csharp
Screen screen = new Screen();
screen.WriteString("╒═══════════════════╕", 0, 0);
screen.WriteString("│ ♥ Hello, World! ♥ │", 0, 1);
screen.WriteString("╘═══════════════════╛", 0, 2);
screen.FillColors(TextColor.White, TextColor.DarkBlue, new Rectangle(0, 0, 21, 3));
screen.FillForeground(TextColor.Magenta, new Rectangle(2, 1, 17, 1));
screen.FillForeground(TextColor.Yellow, new Rectangle(4, 1, 13, 1));
screen.Run();
```

![**Hello, World!**](img/hello3.png)

Eye-catching animations!

```csharp
Screen screen = new Screen();
screen.WriteString("╒═══════════════════╕", 0, 0);
screen.WriteString("│ ♥ Hello, World! ♥ │", 0, 1);
screen.WriteString("╘═══════════════════╛", 0, 2);
screen.FillColors(TextColor.White, TextColor.DarkBlue, new Rectangle(0, 0, 21, 3));
screen.FillForeground(TextColor.Magenta, new Rectangle(2, 1, 17, 1));
screen.FillForeground(TextColor.Yellow, new Rectangle(4, 1, 13, 1));
Timer timer = new Timer(TimeSpan.FromSeconds(0.5), screen);
bool blink = false;
timer.Tick += (s, e) =>
    {
        screen.FillForeground(blink ? TextColor.Yellow : TextColor.Red, new Rectangle(4, 1, 13, 1));
        blink = !blink;
    };
screen.Run();
timer.Dispose();
```

![***Hello, World!***](img/hello4.gif)

Using these tools, and more, you can create text-mode applications that do all kinds of crazy things!

#### But how do I reference it in a project?

The first thing to do is clone or download the project to disk. You can do this from the main page of the repository.

Next, create a new console application in Visual Studio, and in the project properties, change the output type to "Windows Application".

Then, add the Tui project into the solution with Add > Existing Project... and browsing to the location of Tui.csproj.

Finally, open the Add Reference dialog for your new project and select Tui in the Projects section.

You are now ready to use Tui!
