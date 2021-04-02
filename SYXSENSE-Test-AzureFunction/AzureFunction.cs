using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using LinqToDB.Configuration;
using System.Linq;
using System;
using Newtonsoft.Json;
using System.IO;
using LinqToDB;
using System.Net;

namespace SYXSENSE_Test_AzureFunction
{
    public static class AzureFunction
    {
        [FunctionName("AzureFunction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            try
            {
                string requestBody = String.Empty;

                requestBody = req.Content.ReadAsStringAsync().Result;
                log.Info(requestBody);

                dynamic data = JsonConvert.DeserializeObject(requestBody);

                var builder = new LinqToDbConnectionOptionsBuilder();
                string connectionString = "Server=<mssql server>;Initial Catalog=<DB catalog>;Persist Security Info=False;User ID=<DB user>;Password=<user password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Database=<Database>";

                builder.UseConnectionString(LinqToDB.ProviderName.SqlServer2016, connectionString);

                using (var db = new DBTest(builder.Build()))
                {
                    foreach (var machine in data.machines)
                    {
                        log.Info("Processing: "+ machine.MachineName+"::"+machine.OSName);
                        MachineData mData = new MachineData() { MachineName = machine.MachineName, OSName = machine.OSName, Zone = machine.Zone, NetVersion = machine.netVersion };

                        var q =
                            from c in db.Machines
                            where c.MachineName == mData.MachineName && c.OSName == mData.MachineName && c.Zone == mData.Zone
                            select c;
                        
                        if (q.Count() == 0)
                        {
                            log.Info("There is no similar machine was found.");
                            Machine m = new Machine() { MachineName = mData.MachineName, OSName = mData.OSName, Zone = mData.Zone, NetVersion = mData.NetVersion, LastTimeOnline = DateTime.Now.ToString()};
                            db.Insert(m);
                        }
                        else
                        {
                            Machine m = q.First();
                            log.Info("Similar nachine has ID: "+m.MachineID);
                            m.LastTimeOnline = DateTime.Now.ToString();
                            db.Update(m);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Info(ex.ToString());
                return req.CreateResponse(HttpStatusCode.BadRequest, ex.ToString());
            }
            return req.CreateResponse(HttpStatusCode.OK, "Ok");
        }
    }
}
