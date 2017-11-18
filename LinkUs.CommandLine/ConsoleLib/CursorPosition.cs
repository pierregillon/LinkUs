namespace LinkUs.CommandLine.ConsoleLib
{
    public class CursorPosition
    {
        public int Left { get; }
        public int Top { get; }

        public CursorPosition(int cursorLeft, int cursorTop)
        {
            Left = cursorLeft;
            Top = cursorTop;
        }
    }
}