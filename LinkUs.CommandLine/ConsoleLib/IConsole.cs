using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LinkUs.CommandLine.ModuleIntegration.Default.FileTransferts;

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
        void MoveCursorLeftBack(int length);
        void SetCursorLeft(int left);
        Task<T> WriteProgress<T>(string text, IProgressable progressable, Task<T> task);
        Task WriteProgress(string text, IProgressable progressable, Task task);
        Task WriteTaskStatus(string text, Task task);
        void CleanLine();
        string ReadLine();
        CursorPosition GetCursorPosition();
        void SetCursorPosition(CursorPosition cursorPosition);
        int Read(char[] buffer, int offset, int length);
    }
}