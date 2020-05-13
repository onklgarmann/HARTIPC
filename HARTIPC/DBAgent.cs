using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace HARTIPC
{
    class DBAgent
    {
        public DBAgent()
        {
            
        }
        public void OnDataEntryReceived(object sender, DataEntryEventArgs dataEntryEventArgs)
        {
            Console.WriteLine(BitConverter.ToString(dataEntryEventArgs.Address));
            Console.WriteLine(dataEntryEventArgs.datetime);
            //System.Threading.Thread.Sleep(10000);
            Console.WriteLine(dataEntryEventArgs.datetime);
        }
    }
}
