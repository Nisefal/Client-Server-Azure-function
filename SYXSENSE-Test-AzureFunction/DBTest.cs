using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.Data;
using System.Linq;
using LinqToDB.Mapping;

namespace SYXSENSE_Test_AzureFunction
{
	public partial class DBTest : LinqToDB.Data.DataConnection
	{
		public ITable<Machine> Machines { get { return this.GetTable<Machine>(); } }

		public DBTest()
		{
			InitDataContext();
			InitMappingSchema();
		}

		public DBTest(string configuration)
			: base(configuration)
		{
			InitDataContext();
			InitMappingSchema();
		}

		public DBTest(LinqToDbConnectionOptions options)
			: base(options)
		{
			InitDataContext();
			InitMappingSchema();
		}

		partial void InitDataContext();
		partial void InitMappingSchema();
	}
}
