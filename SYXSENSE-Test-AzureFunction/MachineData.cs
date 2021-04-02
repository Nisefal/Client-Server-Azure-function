using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYXSENSE_Test_AzureFunction
{
	class MachineData
	{
		public int MachineID { get; set; } // int
		public string MachineName { get; set; } // nvarchar(64)
		public string OSName { get; set; } // nvarchar(64)
		public string Zone { get; set; } // nvarchar(16)
		public string LastTimeOnline { get; set; } // nvarchar(32)
		public string NetVersion { get; set; } // nvarchar(16)
	}
}
