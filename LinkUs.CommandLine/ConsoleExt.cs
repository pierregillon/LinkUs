using System;
using System.Collections.Generic;
using System.Linq;
using LinkUs.CommandLine.Handlers;

namespace LinkUs.CommandLine
{
    public static class ConsoleExt
    {
        private const ConsoleColor InfoColor = ConsoleColor.Gray;
        private const ConsoleColor ErrorColor = ConsoleColor.Red;
        private const ConsoleColor WarningColor = ConsoleColor.DarkYellow;

        public static void WriteInfo(string message, params object[] parameters)
        {
            WriteWithColor(string.Format(message, parameters), InfoColor);
        }
        public static void WriteError(string message, params object[] parameters)
        {
            WriteWithColor(string.Format(message, parameters), ErrorColor);
        }
        public static void WriteWarning(string message, params object[] parameters)
        {
            WriteWithColor(string.Format(message, parameters), WarningColor);
        }

        private static void WriteWithColor(string message, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }

        public static void WriteObjects<T>(IReadOnlyCollection<T> list, params string[] properties)
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
    }
}