using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LinkUs.CommandLine.Handlers;
using LinkUs.CommandLine.ModuleIntegration.Default.FileTransferts;
using LinkUs.CommandLine.ModuleIntegration.RemoteShell;

namespace LinkUs.CommandLine.ConsoleLib
{
    public class WindowsConsole : IConsole
    {
        private const ConsoleColor InfoColor = ConsoleColor.DarkBlue;
        private const ConsoleColor ErrorColor = ConsoleColor.Red;
        private const ConsoleColor WarningColor = ConsoleColor.DarkYellow;

        // ----- Public methods
        public void WriteLineInfo(string message, params object[] parameters)
        {
            WriteLineWithColor(string.Format(message, parameters), InfoColor);
        }
        public void WriteLineError(string message, params object[] parameters)
        {
            WriteLineWithColor(string.Format(message, parameters), ErrorColor);
        }
        public void WriteLineWarning(string message, params object[] parameters)
        {
            WriteLineWithColor(string.Format(message, parameters), WarningColor);
        }
        public void WriteObjects<T>(IReadOnlyCollection<T> list, params string[] properties)
        {
            if (list.Any() == false) {
                return;
            }

            var propertiesToProcess = typeof(T)
                .GetProperties()
                .Where(x => x.CanRead)
                .ToArray();

            if (propertiesToProcess.Any() == false) {
                throw new Exception("No properties can be read to generate data.");
            }

            if (properties?.Any() == true) {
                propertiesToProcess = propertiesToProcess.Where(x => properties.Contains(x.Name)).ToArray();
            }

            var dico = propertiesToProcess
                .ToDictionary(
                    propertyInfo => propertyInfo.Name,
                    propertyInfo => list.Max(x => propertyInfo.GetValue(x).ToNormalizedString()).Length);

            foreach (var obj in list) {
                foreach (var propertyInfo in propertiesToProcess) {
                    Console.Write(propertyInfo.GetValue(obj).ToNormalizedString().PadRight(dico[propertyInfo.Name] + 2));
                }
                Console.WriteLine();
            }
        }
        public void WriteLine(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }
        public void Write(string message, params object[] args)
        {
            Console.Write(message, args);
        }
        public void NewLine()
        {
            Console.WriteLine();
        }
        public void MoveCursorLeftBack(int length)
        {
            Console.CursorLeft -= length;
        }
        public void SetCursorLeft(int left)
        {
            Console.CursorLeft = left;
        }
        public Task<T> WriteProgress<T>(string text, IProgressable progressable, Task<T> task)
        {
            return WriteProgressInternal(text, progressable, task)
                .ContinueWith(x => {
                    if (x.IsFaulted) {
                        throw task.Exception;
                    }
                    return task.Result;
                });
        }
        public Task WriteProgress(string text, IProgressable progressable, Task task)
        {
            return WriteProgressInternal(text, progressable, task);
        }
        public Task WriteTaskStatus(string text, Task task)
        {
            Write($"{text}\t=> ");
            return task.ContinueWith(WriteTaskStatus);
        }
        public void CleanLine()
        {
            SetCursorLeft(0);
            for (int i = 0; i < Console.WindowWidth - 1; i++) {
                Console.Write(" ");
            }
            SetCursorLeft(0);
        }
        public string ReadLine()
        {
            return Console.ReadLine();
        }
        public CursorPosition GetCursorPosition()
        {
            return new CursorPosition(Console.CursorLeft, Console.CursorTop);
        }
        public void SetCursorPosition(CursorPosition cursorPosition)
        {
            Console.CursorLeft = cursorPosition.Left;
            Console.CursorTop = cursorPosition.Top;
        }
        public int Read(char[] buffer, int offset, int length)
        {
            return Console.In.Read(buffer, offset, length);
        }

        // ----- Internal logics
        private Task WriteProgressInternal(string text, IProgressable progressable, Task task)
        {
            return Task.Factory.StartNew(() => {
                           var watch = new Stopwatch();
                           watch.Start();
                           while (task.Wait(500) == false) {
                               CleanLine();
                               Write($"{text}\t=> {progressable.Pourcentage}%  (from {watch.Elapsed:hh\\:mm\\:ss})");
                           }
                           watch.Stop();
                       })
                       .ContinueWith(x => {
                           CleanLine();
                           Write($"{text}\t=> ");
                           WriteTaskStatus(task);
                       });
        }
        private void WriteTaskStatus(Task task)
        {
            if (task.IsCanceled) {
                WriteLineWarning("[CANCELED]");
            }
            else if (task.IsCompleted) {
                WriteLineSuccess("[DONE]");
            }
            else if (task.IsFaulted) {
                WriteLineError("[FAILED]");
                throw task.Exception;
            }
        }

        // ----- Utils
        private void WriteLineWithColor(string message, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }
        private void WriteLineSuccess(string message)
        {
            WriteLineWithColor(message, ConsoleColor.Green);
        }
    }
}