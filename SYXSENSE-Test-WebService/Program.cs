using System;

namespace SYXSENSE_Test_WebService
{       
    class Program
    {
        static void Main(string[] args)
        {
            Server server = Server.GetServerInstance();
            server.Start("http://127.0.0.1/socketserver/");
            do
            {
                Console.ReadKey();
                Console.WriteLine();
                Console.WriteLine("Stop server? [Y\\n]");
                char y = Console.ReadKey().KeyChar;
                Console.WriteLine();
                if (y == 'Y' || y == 'y')
                {
                    break;
                }
            }
            while (true);
        }
    }
}