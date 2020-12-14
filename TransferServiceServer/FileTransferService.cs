using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Reflection;
using System.Text;
//using Ssepan.Application;
using Ssepan.Utility;
using TransferServerBusiness;
using DocumentScannerServerLibrary;
//using DocumentScannerServiceCommon;

namespace TransferServiceServer
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class FileTransferService : 
        IFileTransferService
    {

        public FileTransferService()
        { 
            //TODO:find way to move DSServerController assignment out of TransferServiceServer, so that these delegates can be injected somehow
            TransferServerBusiness.Transfer.pushDelegate = DSServerController<DSServerModel>.PushFile;
            TransferServerBusiness.Transfer.pullDelegate = DSServerController<DSServerModel>.PullFile;
        }

        /// <summary>
        /// Responds to client Ping.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public Boolean Ping(ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);

            try
            {
                returnValue = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        /// <summary>
        /// Client pushes transaction file.
        /// </summary>
        /// <param name="transactionContract"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public Boolean Push(TransactionContract contract, ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);
            Byte[] bytes = default(Byte[]);

            try
            {
                bytes = contract.ByteArray;
                if (!TransferServerBusiness.Transfer.pushDelegate(contract.ID, contract.Operator, contract.Filename, bytes, ref errorMessage))
                {
                    throw new Exception(String.Format("Transfer Service Server is unable to Push to DocumentScannerServer: {0}\nID: {1}\nFilename: {2}", errorMessage, contract.ID, contract.Filename));
                }

                returnValue = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                contract.ErrorMessage = errorMessage;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        /// <summary>
        /// Client pulls transaction file.
        /// </summary>
        /// <param name="transactionContract"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public Boolean Pull(ref TransactionContract contract, ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);
            Byte[] bytes = default(Byte[]);

            try
            {
                if (!TransferServerBusiness.Transfer.pullDelegate(contract.ID, contract.Operator, contract.Filename, ref bytes, ref errorMessage))
                {
                    throw new Exception(String.Format("Transfer Service Server is unable to Pull from DocumentScannerServer: {0}\nID: {1}\nFilename: {2}", errorMessage, contract.ID, contract.Filename));
                }
                contract.ByteArray = bytes;

                returnValue = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                contract.ErrorMessage = errorMessage;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }
    }
}
