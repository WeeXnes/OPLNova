global using Console = OPLNova.Core.Console;
global using VanillaConsole = System.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace OPLNova.Core
{
    public static class Console
    {
        private static StreamWriter logWriter = null;
        private static readonly object logLock = new();

        public static void SetLogFile(string path)
        {
            if (logWriter != null)
            {
                logWriter.Dispose();
            }

            logWriter = new StreamWriter(path, append: true)
            {
                AutoFlush = true
            };
        }

        public static class Data
        {
            public static class Colors
            {
                public static bool colored_output = true;
                public static ConsoleColor int_color = ConsoleColor.Blue;
                public static ConsoleColor double_color = ConsoleColor.Cyan;
                public static ConsoleColor float_color = ConsoleColor.DarkCyan;
            }

            public static class Formatting
            {
                public static bool timestamp_prefix = false;
                public static string success_char = "✓";
                public static string warning_char = "⌬";
                public static string info_char = "◈";
                public static string error_char = "☓";
                public static string writeline_char = "•";
            }
        }

        private static void ConfiguredWriteline(
            string text, 
            ConsoleColor color,
            ConsoleColor foregroundColor = ConsoleColor.White)
        {
            try
            {
                VanillaConsole.OutputEncoding = Encoding.UTF8;
            }
            catch {}

            // Save current colors
            ConsoleColor prevColor = VanillaConsole.BackgroundColor;
            ConsoleColor prevForeColor = VanillaConsole.ForegroundColor;

            // Timestamp
            DateTime currentTime = DateTime.Now;
            string timestamp = currentTime.ToString("[HH:mm:ss] ");
            if (Data.Formatting.timestamp_prefix)
                text = timestamp + text;

            // Apply console colors
            if (Data.Colors.colored_output)
            {
                VanillaConsole.BackgroundColor = color;
                VanillaConsole.ForegroundColor = foregroundColor;
            }

            VanillaConsole.Write(text + " ");

            if (Data.Colors.colored_output)
            {
                VanillaConsole.BackgroundColor = prevColor;
                VanillaConsole.ForegroundColor = prevForeColor;
            }

            VanillaConsole.WriteLine("");

            // Log to file (if enabled)
            if (logWriter != null)
            {
                lock (logLock)
                {
                    try
                    {
                        logWriter.WriteLine(timestamp + text);
                    }
                    catch (Exception ex)
                    {
                        VanillaConsole.WriteLine("Log file write failed: " + ex.Message);
                    }
                }
            }
        }

        public static void WriteLine(string text,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            ConfiguredWriteline(" " + Data.Formatting.writeline_char + " (" + lineNumber + "|" + caller + ") " + text,
                VanillaConsole.BackgroundColor, ConsoleColor.White);
        }

        public static void WriteLine(float text,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            ConfiguredWriteline(" " + Data.Formatting.writeline_char + " (" + lineNumber + "|" + caller + ") " + text,
                Data.Colors.float_color, ConsoleColor.White);
        }

        public static void WriteLine(double text,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            ConfiguredWriteline(" " + Data.Formatting.writeline_char + " (" + lineNumber + "|" + caller + ") " + text,
                Data.Colors.double_color, ConsoleColor.White);
        }

        public static void WriteLine(int text,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            ConfiguredWriteline(" " + Data.Formatting.writeline_char + " (" + lineNumber + "|" + caller + ") " + text,
                Data.Colors.int_color, ConsoleColor.White);
        }

        public static void Success(string text,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            ConfiguredWriteline(" " + Data.Formatting.success_char + " (" + lineNumber + "|" + caller + ") " + text,
                ConsoleColor.Green, ConsoleColor.Black);
        }

        public static void Info(string text,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            ConfiguredWriteline(" " + Data.Formatting.info_char + " (" + lineNumber + "|" + caller + ") " + text,
                ConsoleColor.Blue, ConsoleColor.Black);
        }

        public static void Error(string text,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            ConfiguredWriteline(" " + Data.Formatting.error_char + " (" + lineNumber + "|" + caller + ") " + text,
                ConsoleColor.DarkRed, ConsoleColor.Black);
        }

        public static void Warning(string text,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null)
        {
            ConfiguredWriteline(" " + Data.Formatting.warning_char + " (" + lineNumber + "|" + caller + ") " + text,
                ConsoleColor.DarkYellow, ConsoleColor.Black);
        }

        public static void WriteLine<T>(List<T> List, bool verbose = true)
        {
            ConfiguredWriteline("List contains " + typeof(T) + "(" + List.Count + ")",
                ConsoleColor.DarkMagenta, ConsoleColor.Black);

            if (!verbose)
                return;

            for (int i = 0; i < List.Count; i++)
            {
                var color = (i % 2 == 0) ? ConsoleColor.DarkGray : ConsoleColor.Black;
                ConfiguredWriteline("(" + i + "): " + List[i], color);
            }
        }
    }
}
