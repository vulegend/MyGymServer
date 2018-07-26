using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HookappServer
{
    class Print
    {
        public static bool Enabled { get; set; } = true;

        public static void Debug(string str) { print(Type.Debug, str); }

        public static void Info(string str) { print(Type.Info, str); }

        public static void Success(string str) { print(Type.Success, str); }

        public static void Warn(string str) { print(Type.Warn, str); }

        public static void Error(string str) { print(Type.Error, str); }

        private enum Type { Debug, Info, Success, Warn, Error }

        private static void print(Type type, string str)
        {
            if (!Enabled)
                return;

            switch (type)
            {
                case Type.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case Type.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case Type.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case Type.Warn:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }

            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
