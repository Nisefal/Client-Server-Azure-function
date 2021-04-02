using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using System.Threading;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;

namespace SYXSENSE_Test_WebService
{     
    class Server
    {
        private static Server _server = null;

        private List<RemoteMachine> _machines = null;

        private void SendCollectedData(Object source, ElapsedEventArgs e)
        {
            CollectData();
            Thread.Sleep(2000); // wait for all responses

            // start azure function
            SendDataToAzureFunction();

            CleanUpSuspendedMachines();
        }

        private void SendDataToAzureFunction()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("<azure function http-address>");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(_machines, Formatting.Indented); ;

                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Console.WriteLine(result);
            }
        }

        private void CleanUpSuspendedMachines()
        {

        }

        private void CollectData()
        {
            foreach (var m in _machines)
            {
                m.GetDataFromRemoteMachine();
            }
        }

        private Server() 
        {
            _machines = new List<RemoteMachine>();
        }

        private void AddRemoteMachine(HttpListenerContext context)
        {
            _machines.Add(new RemoteMachine(context));
        }

        public static void RemoveRemoteMachine(RemoteMachine machine)
        {
            GetServerInstance()._machines.Remove(machine);
        }

        public static Server GetServerInstance()
        {
            if (_server == null)
                _server = new Server();
            return _server;
        }

        public async void Start(string listenerPrefix)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(listenerPrefix);

            System.Timers.Timer t = new System.Timers.Timer(300000); // 5 minutes
            t.AutoReset = true;
            t.Elapsed += new ElapsedEventHandler(SendCollectedData);
            t.Start();

            Console.WriteLine("Starting scheduled task...");

            //EXCEPTION FIX: netsh http add urlacl url=http://127.0.0.1:80/socketserver user=USERNAME (in cmd terminal with admin rights)
            listener.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                HttpListenerContext listenerContext = await listener.GetContextAsync();
                if (listenerContext.Request.IsWebSocketRequest)
                {

                    AddRemoteMachine(listenerContext);
                }
                else
                {
                    listenerContext.Response.StatusCode = 400;
                    listenerContext.Response.Close();
                }
            }
        }
    }
}