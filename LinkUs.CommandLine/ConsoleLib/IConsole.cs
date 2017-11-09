using System.Collections.Generic;

namespace LinkUs.CommandLine.ConsoleLib
{
    public interface IConsole
    {
        void WriteLineError(string message, params object[] parameters);
        void WriteLineInfo(string message, params object[] parameters);
        void WriteLineWarning(string message, params object[] parameters);
        void WriteObjects<T>(IReadOnlyCollection<T> collection, params string[] properties);
        void WriteLine(string message, params object[] args);
        void Write(string message, params object[] args);
        void NewLine();
        void MoveCursorLeft(int length);
    }
}