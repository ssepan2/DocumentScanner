using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Reflection;
using System.Text;
using Ssepan.Utility;
using DocumentScannerServiceCommon;
using TransferServerBusiness;

namespace TransferServiceServer
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class FileTransferService : 
        IFileTransferService
    {

        public FileTransferService()
        {
            string errorMessage = default(String);

            //Was doing this:
            //TransferServerBusiness.Transfer.pushDelegate = DSServerModelController<DSServerModel>.PushFile;
            //TransferServerBusiness.Transfer.pullDelegate = DSServerModelController<DSServerModel>.PullFile;
            //Moved DSServerModelController assignment out of TransferServiceServer. 
            //Call TransferServerBusiness.Transfer.InitDelegates() to have it load delegates from another library.
            if (!TransferServerBusiness.Transfer.InitDelegates(ref errorMessage))
            {
                throw new Exception(String.Format("Transfer Service Server is unable to init delegates to Transfer Server Business: {0}", errorMessage));
            }

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
        public Boolean Push(TransferContract contract, ref String errorMessage)
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
        public Boolean Pull(ref TransferContract contract, ref String errorMessage)
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
