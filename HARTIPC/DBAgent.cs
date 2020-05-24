using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using Dapper;
using HARTIPC.Data;

namespace HARTIPC
{
    /// <summary>
    /// DBAgent handles events from HARTIPClient and writes to the database
    /// </summary>
    class DBAgent
    {
        /// <summary>
        /// SQL-server connection string
        /// </summary>
        private string connString{get;set;}
        /// <summary>
        /// Constructor, takes connection string as argument
        /// </summary>
        /// <param name="connString">SQL connection string</param>
        public DBAgent(string connString) => this.connString = connString;
        /// <summary>
        /// Eventhandler for NewGatewayEvent.  Takes inn gateway-data and writes to DB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnNewGatewayEvent(object sender, GatewayDataArgs e)
        {
            var sql = "InsertOrUpdateGateway";
            using (IDbConnection connection = new SqlConnection(connString))
            {
                e.GatewayID = connection.Query<int>(sql, e, commandType: CommandType.StoredProcedure).First();
            }
            
            //return 0;
            
        }
        /// <summary>
        /// Eventhandler for NewDeviceEvent.  Takes inn device-data and writes to DB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnNewDeviceEvent(object sender, DeviceDataArgs e)
        {
            Execute("InsertOrUpdateDevice", e, CommandType.StoredProcedure);
        }
        /// <summary>
        /// Eventhandler for DataEntryReceivedEvent.  Takes inn data entry and writes to DB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnDataEntryReceivedEvent(object sender, EventArgs e)
        {
            Execute("DataEntry", e, CommandType.StoredProcedure);
        }
        /// <summary>
        /// Generic wrapper for IDbConnection.Execute.  Takes in SQL-statement,
        /// arguments and commandtype.
        /// </summary>
        /// <param name="sql">SQL-query</param>
        /// <param name="e">Arguments</param>
        /// <param name="c">Commandtype</param>
        private void Execute(string sql, EventArgs e, CommandType c)
        {
            using (IDbConnection connection = new SqlConnection(connString))
                connection.Execute(sql, e, commandType: c);
        }
    }
}
