using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Ssepan.Utility;
using TransferClientBusiness;

namespace ServiceClientTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            String errorMessage = default(String);
            String id = default(String);
            String operatorId = default(String);
            String filename = default(String);
            String configuration = default(String);

            try
            {

                errorMessage = String.Empty;

                //just ping
                Console.WriteLine("pinging...");
                id = Guid.NewGuid().ToString();
                //filename = @"pullfile.txt";
                configuration = ConfigurationManager.AppSettings.GetValues("FileTransferServiceEndpointConfigurationName")[0];
                if (!Transfer.Ping(configuration, ref errorMessage))
                {
                    Console.WriteLine(String.Format("Unable to Ping Transfer Client Business: {0}", errorMessage));
                }
                else
                {
                    Console.WriteLine("pinged.");
                }

                
                //pull c:\temp\server\send\pullfile.txt to c:\temp\client\receive\pullfile.txt
                Console.WriteLine("Pulling file...");
                id = Guid.NewGuid().ToString();
                operatorId = Guid.NewGuid().ToString();
                filename =@"c:\temp\client\receive\pullfile.txt";
                if (!Transfer.PullFile(id, operatorId, filename, configuration, ref errorMessage))
                {
                    //throw new Exception(String.Format("Unable to Pull file from Transfer Client Business: {0}\nID: {1}\nFilename: {2}", errorMessage, id, filename));
                    Console.WriteLine(String.Format("Unable to Pull file from Transfer Client Business: {0}\nID: {1}\nFilename: {2}", errorMessage, id, filename));
                }
                else
                {
                    Console.WriteLine("File pulled.");
                }

                //push c:\temp\client\send\pushfile.txt to c:\temp\server\receive\pushfile.txt
                Console.WriteLine("Pushing File...");
                id = Guid.NewGuid().ToString();
                operatorId = Guid.NewGuid().ToString();
                filename = @"c:\temp\client\send\pushfile.txt";
                //filename = @"c:\temp\client\send\pushfile2.txt";
                //filename = @"C:\temp\client\send\e35618e1-5b90-430e-b872-3e3304fbe9eb.zip";
                if (!Transfer.PushFile(id, operatorId, filename, configuration, ref errorMessage))
                {
                    Console.WriteLine(String.Format("Unable to Push file to Transfer Client Business: {0}\nID: {1}\nFilename: {2}", errorMessage, id, filename));
                }
                else
                {
                    Console.WriteLine("File pushed.");
                }

                Console.Write("Press any key to exit:");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                //throw ex;
            }
        }
    }
}
