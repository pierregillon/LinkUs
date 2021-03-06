using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LinkUs.CommandLine.FileTransferts;
using LinkUs.CommandLine.Handlers;

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
            WriteWithColor(string.Format(message, parameters), InfoColor);
        }
        public void WriteLineError(string message, params object[] parameters)
        {
            WriteWithColor(string.Format(message, parameters), ErrorColor);
        }
        public void WriteLineWarning(string message, params object[] parameters)
        {
            WriteWithColor(string.Format(message, parameters), WarningColor);
        }
        public void WriteObjects<T>(IReadOnlyCollection<T> list, params string[] properties)
        {
            if (list.Any() == false) {
                return;
            }

            var propertiesToProcess = typeof(T)
                .GetProperties()
                .Where(x => x.CanRead);

            if (properties != null && properties.Any()) {
                propertiesToProcess = propertiesToProcess.Where(x => properties.Contains(x.Name));
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
        public void WriteProgress(Task task, IProgressable progressable)
        {
            try {
                var watch = new Stopwatch();
                watch.Start();
                while (task.Wait(500) == false) {
                    SetCursorLeft(0);
                    WriteProgress(progressable.Pourcentage, watch.Elapsed);
                }
                watch.Stop();
                SetCursorLeft(0);
                WriteProgress(progressable.Pourcentage, watch.Elapsed);
                NewLine();
            }
            catch (Exception) {
                SetCursorLeft(0);
                throw;
            }
        }

        // ----- Utils
        private void WriteWithColor(string message, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }
        private void WriteProgress(int pourcentage, TimeSpan ellapsed)
        {
            Write($"Progress: {pourcentage.ToString().PadLeft(3)}%\t{ellapsed:hh\\:mm\\:ss}");
        }
    }
}