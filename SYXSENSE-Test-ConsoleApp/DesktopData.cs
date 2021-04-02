using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SYXSENSE_Test_ConsoleApp
{
    struct MachineData
    {
        public string MachineName;
        public string OSName;
        public string Zone;
        public string netVersion;
    }

    class DesktopData
    {
        private static DesktopData _thisMachine;

        private MachineData data; 

        private DesktopData()
        {
            RefreshData();
        }
           
        public void RefreshData()
        {
            if (data.MachineName == null)
                data = new MachineData();

            // get machine name 
            data.MachineName = Environment.MachineName;

            // get current TimeZone
            data.Zone = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString();

            // get OS's Name
            data.OSName = GetOSFriendlyName();

            // get .net version
            data.netVersion = Environment.Version.ToString();
        }

        public byte[] FormDataForSending(bool flag)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            string text = String.Empty;

            if (flag)
            {
                dynamic tmp_data = new object();
                tmp_data.MachineName = data.MachineName;
                tmp_data.OSName = data.OSName;
                tmp_data.Zone = data.Zone;
                tmp_data.netVersion = data.netVersion;
                tmp_data.LastMessage = true;

                text = JsonConvert.SerializeObject(tmp_data);
            }
            else
                text = JsonConvert.SerializeObject(data);

            byte[] bytes = Encoding.ASCII.GetBytes(text);
            return bytes;
        }

        private string GetOSFriendlyName()
        {
            string result = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get())
            {
                result = os["Caption"].ToString();
                break;
            }
            return result;
        }

        private string GetMotherBoardID()
        {
            String serial = "";
            ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            ManagementObjectCollection moc = mos.Get();

            foreach (ManagementObject mo in moc)
            {
                serial = mo["SerialNumber"].ToString();
            }
            return serial;
        }

        public static DesktopData GetDesktopData()
        {
            if (_thisMachine == null)
                _thisMachine = new DesktopData();
            else
                _thisMachine.RefreshData();
            return _thisMachine;
        }

    }
}
