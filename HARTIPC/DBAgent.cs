using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using Dapper;
using HARTIPC.Data;

namespace HARTIPC
{
    class DBAgent
    {
        private string connString{get;set;}
        public DBAgent(string connString) => this.connString = connString;
        public int OnNewGatewayEvent(object sender, GatewayDataArgs e)
        {
            var sql = "InsertOrUpdateGateway";
            using (IDbConnection connection = new SqlConnection(connString))
            {
                connection.Execute(sql, e, commandType: CommandType.StoredProcedure);
                var id = connection.Query("SELECT id FROM GATEWAYS WHERE GATEWAYADDRESS = @GatewayAddress", e);
                Console.WriteLine(id);
            }
            
            return 0;
            
        }
        public void OnNewDeviceEvent(object sender, DeviceDataArgs e)
        {
            Execute("InsertOrUpdateDevice", e, CommandType.StoredProcedure);
        }
        public void OnDataEntryReceivedEvent(object sender, EventArgs e)
        {
            string sql = "INSERT INTO dbo.Measurements (MeasurementTime, DeviceAddress, PVCurrent, PVUnit, PV) VALUES (@MeasurementTime, @DeviceAddress, @PVCurrent, @PVUnit, @PV);";
            Execute(sql, e, CommandType.Text);
        }
        private void Execute(string sql, EventArgs e, CommandType c)
        {
            using (IDbConnection connection = new SqlConnection(connString))
                connection.Execute(sql, e, commandType: c);
        }
    }
}
