using System;

using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Mapping;

namespace SYXSENSE_Test_AzureFunction
{
	/// <summary>
	/// Database       : syxsense_test
	/// Data Source    : DESKTOP-BFP6A9S\MSSQL_TEST
	/// Server Version : 15.00.2000
	/// </summary>

	[Table(Schema = "dbo", Name = "Machines")]
	public partial class Machine
	{
		[Column(), Identity] public int MachineID { get; set; } // int
		[Column(), Nullable] public string MachineName { get; set; } // nvarchar(64)
		[Column(), Nullable] public string OSName { get; set; } // nvarchar(64)
		[Column(), Nullable] public string Zone { get; set; } // nvarchar(16)
		[Column(), Nullable] public string LastTimeOnline { get; set; } // nvarchar(32)
		[Column("netVersion"), Nullable] public string NetVersion { get; set; } // nvarchar(16)
	}
}