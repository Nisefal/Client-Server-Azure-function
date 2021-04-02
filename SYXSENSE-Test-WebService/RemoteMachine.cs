using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.WebSockets;
using Newtonsoft.Json;

namespace SYXSENSE_Test_WebService
{
    class RemoteMachine
    {

        #region data
        
        private string _oSName = null;
        private string _machineName = null;
        private string _netVersion = null;
        private string _localZone = null;
        private string _lastTimeOnline = null;
        
        #endregion


        private HttpListenerContext _httpListenerContext = null;
        private WebSocket _webSocket = null;

        public RemoteMachine(HttpListenerContext listenerContext)
        {
            _httpListenerContext = listenerContext;
            ProcessRequest(_httpListenerContext);
        }

        public async void GetDataFromRemoteMachine()
        {
            //Code:1 - get data
            //Code:2 - ??
            byte[] data = Encoding.ASCII.GetBytes("Code:1");
            await _webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary,true,CancellationToken.None);
        }

        private async void ProcessRequest(HttpListenerContext listenerContext)
        {

            WebSocketContext webSocketContext = null;
            try
            {
                webSocketContext = await listenerContext.AcceptWebSocketAsync(subProtocol: null);

                //Console.WriteLine("Processed: {0}", this._machines.Count);
            }
            catch (Exception e)
            {
                listenerContext.Response.StatusCode = 500;
                listenerContext.Response.Close();
                Console.WriteLine("Exception: {0}", e);
                return;
            }

            _webSocket = webSocketContext.WebSocket;

            try
            {
                byte[] receiveBuffer = new byte[256];

                while (_webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult receiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);


                    _lastTimeOnline = DateTime.Now.ToString();

                    string message = Encoding.ASCII.GetString(receiveBuffer);

                    // process data
                    if(message.Contains("MachineName"))
                    {
                        dynamic data = JsonConvert.DeserializeObject(message);

                        _machineName = data.MachineName;
                        _oSName = data.OSName;
                        _localZone = data.Zone;
                        _netVersion = data.netVersion;
                    }

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    }
                    else if (receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Cannot accept text frame", CancellationToken.None);
                    }
                    else
                    {
                        await _webSocket.SendAsync(new ArraySegment<byte>(receiveBuffer, 0, receiveResult.Count), WebSocketMessageType.Binary, receiveResult.EndOfMessage, CancellationToken.None);
                    }
                }
            }
            catch (WebSocketException e)
            {
                Server.RemoveRemoteMachine(this);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
            finally
            {
                if (_webSocket != null)
                    _webSocket.Dispose();
            }
        }
    }
}
