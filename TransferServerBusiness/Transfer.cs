using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Ssepan.Utility;
using DocumentScannerServerLibrary;

namespace TransferServerBusiness
{
    public class Transfer //:
        //ITransfer
    {
        //keep delegates here to act as bridge between FileTransferServer.FileTransferService and DSServerController<DSServerModel>
        public delegate X PushDelegate<T, S, U, V, W, X>(T t, S s, U u, V[] v, ref W w);
        public static  PushDelegate<String, String, String, Byte, String, Boolean> pushDelegate = null; //DocumentScannerServerController.AsStatic.PushFile;
        public delegate E PullDelegate<A, F, B, C, D, E>(A a, F f, B b, ref C[] c, ref D d);
        public static  PullDelegate<String, String, String, Byte, String, Boolean> pullDelegate = null; //DocumentScannerServerController.AsStatic.PullFile;

        /// <summary>
        /// Load pushDelegate, pullDelegate from DSServerController<DSServerModel>.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns>Boolean</returns>
        public static Boolean InitDelegates(ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);

            try
            {
                pushDelegate = DSServerController<DSServerModel>.PushFile;
                pullDelegate = DSServerController<DSServerModel>.PullFile;

                returnValue = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }
    }
}
