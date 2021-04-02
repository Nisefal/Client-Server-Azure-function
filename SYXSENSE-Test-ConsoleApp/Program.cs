using System;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace SYXSENSE_Test_ConsoleApp
{
    class Program
    {
        private static object consoleLock = new object();
        private const int sendChunkSize = 256;
        private const int receiveChunkSize = 256;
        private const bool verbose = true;
        private static readonly TimeSpan delay = TimeSpan.FromMilliseconds(5000);
        private static ClientWebSocket webSocket = null;

        static void Main(string[] args)
        {
            Console.WriteLine("Connecting...");
            Connect("ws://127.0.0.1/socketserver").Wait();

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();

        }

        public static async Task Connect(string uri)
        {
            try
            {
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
                Console.WriteLine("WebSocket connection established.");
                await SendData(webSocket, false);
                Console.WriteLine("Data sent.");
                await Task.WhenAll(Receive(webSocket));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }
            finally
            {
                if (webSocket != null)
                    webSocket.Dispose();
                Console.WriteLine();

                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("WebSocket closed.");
                    Console.ResetColor();
                }
            }
        }

        private static async Task SendData(ClientWebSocket webSocket, bool flag)
        {
            var data = DesktopData.GetDesktopData();

            byte[] buffer = new byte[sendChunkSize];

            data.FormDataForSending(flag).CopyTo(buffer, 0);

            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, 256), WebSocketMessageType.Binary, false, CancellationToken.None);
            LogStatus(false, buffer, buffer.Length);
        }

        private static async Task Receive(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[receiveChunkSize];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    string message = Encoding.ASCII.GetString(buffer);
                    if(message.Contains("Code:1"))
                        await Task.WhenAll(SendData(webSocket, false));


                    LogStatus(true, buffer, result.Count);
                }
            }
        }

        private static void LogStatus(bool receiving, byte[] buffer, int length)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = receiving ? ConsoleColor.Green : ConsoleColor.Gray;
                Console.WriteLine("{0} {1} bytes... ", receiving ? "Received" : "Sent", length);

                if (verbose)
                    Console.WriteLine(BitConverter.ToString(buffer, 0, length));

                Console.ResetColor();
            }
        }

        ~Program()
        {
            SendData(webSocket, true).Wait();
            webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait();
        }
    }
}