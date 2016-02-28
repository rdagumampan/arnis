using System;

namespace Arnis.Core
{
    public static class ConsoleEx
    {

        public static void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"INFO {message}");
        }

        public static void Ok(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"INFO {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }


        public static void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"WARNING {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

    }
}