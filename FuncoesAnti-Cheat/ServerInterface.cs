using System;

namespace FuncoesAC
{
    public class ServerInterface : MarshalByRefObject
    {
        public void IsInstalled(int clientPID)
        {
            Console.WriteLine("Anti-cheat injetado no processo: {0}.\r\n", clientPID);
        }

        public void ReportMessages(string[] messages)
        {
            for (int i = 0; i < messages.Length; i++)
            {
                Console.WriteLine(messages[i]);
            }
        }

        public void ReportMessage(string message, bool error)
        {
            if (error)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
            Console.WriteLine(message);
            Console.ResetColor();

        }

        public void ReportException(Exception e)
        {
            Console.WriteLine("O processo alvo retornou o erro:\r\n" + e.ToString());
        }

        int count = 0;
        public void Ping()
        {
            var oldTop = Console.CursorTop;
            var oldLeft = Console.CursorLeft;
            Console.CursorVisible = false;

            var chars = "\\|/-";
            Console.SetCursorPosition(Console.WindowWidth - 1, oldTop - 1);
            Console.Write(chars[count++ % chars.Length]);

            Console.SetCursorPosition(oldLeft, oldTop);
            Console.CursorVisible = true;
        }
    }
}
