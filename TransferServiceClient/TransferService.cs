using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Reflection;
using System.Text;
using Ssepan.Utility;
using DocumentScannerServiceCommon;
using TransferServiceClient.TransferServiceClientReference;

namespace TransferServiceClient
{
    public static class TransferService
    {
      
        /// <summary>
        /// Limited features, but seems to allow larger size of messages passed.
        /// </summary>
        public const String ENDPOINT_CONFIGURATION_BASIC = "BasicHttpBinding_IFileTransferService";
        
        /// <summary>
        /// Supports more advanced features, but seems to be limited in the size of the messages passed.
        /// </summary>
        public const String ENDPOINT_CONFIGURATION_WS = "WSHttpBinding_IFileTransferService";

        static TransferService()
        {
            EndpointConfigurationName = ENDPOINT_CONFIGURATION_BASIC;
        }

        private static String _EndpointConfigurationName = default(String);
        public static String EndpointConfigurationName
        {
            get { return _EndpointConfigurationName; }
            set { _EndpointConfigurationName = value; }
        }

        /// <summary>
        /// Just Ping service
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean Ping(ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);
            FileTransferServiceClient client = default(FileTransferServiceClient);

            try
            {
                client = new FileTransferServiceClient(EndpointConfigurationName);
                client.Open();

                //Ping service
                if (!client.Ping(ref errorMessage))
                {
                    throw new Exception(String.Format("Unable to Ping Transfer Service: {0}", errorMessage));
                }


                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            finally
            {
                if ((client.State != CommunicationState.Closed))
                {
                    client.Close();
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Push file data to service
        /// </summary>
        /// <param name="id"></param>
        /// <param name="operatorId"></param>
        /// <param name="filename"></param>
        /// <param name="bytes"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean Push(String id, String operatorId, String filename, Byte[] bytes, ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);
            DocumentScannerServiceCommon.TransactionContract contract = default(DocumentScannerServiceCommon.TransactionContract);
            FileTransferServiceClient client = default(FileTransferServiceClient);

            try
            {
                client = new FileTransferServiceClient(EndpointConfigurationName);
                client.Open();

                //Ping service
                if (!client.Ping(ref errorMessage))
                {
                    throw new Exception(String.Format("Transfer Service Client is unable to Ping Transfer Service Server: {0}", errorMessage));
                }

                //perform Push
                contract = new DocumentScannerServiceCommon.TransactionContract();
                contract.ID = id;
                contract.Operator = operatorId;
                contract.Filename = filename;
                contract.ByteArray = bytes;
                if (!client.Push(contract, ref errorMessage)) 
                {
                    throw new Exception(String.Format("Transfer Service Client is unable to Push to Transfer Service Server: {0}\nID: {1}\nFilename: {2}", errorMessage, contract.ID, contract.Filename));
                }

                returnValue = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            finally
            { 
                if ((client.State != CommunicationState.Closed))
                {
                    client.Close();
                }
            }
            return returnValue;
        }
        
        /// <summary>
        /// Pull file data from service.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="operatorId"></param>
        /// <param name="filename"></param>
        /// <param name="bytes"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean Pull(String id, String operatorId, String filename, ref Byte[] bytes, ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);
            DocumentScannerServiceCommon.TransactionContract contract = default(DocumentScannerServiceCommon.TransactionContract);
            FileTransferServiceClient client = default(FileTransferServiceClient);

            try
            {
                client = new FileTransferServiceClient(EndpointConfigurationName);
                client.Open();

                //Ping service
                if (!client.Ping(ref errorMessage))
                {
                    throw new Exception(String.Format("Unable to Ping Transfer Service: {0}", errorMessage));
                }
                
                ///perform Pull
                contract = new DocumentScannerServiceCommon.TransactionContract();
                contract.ID = id;
                contract.Operator = operatorId;
                contract.Filename = filename;
                if (!client.Pull(ref contract, ref errorMessage))
                {
                    throw new Exception(String.Format("Transfer Service Client is unable to Pull from Transfer Service Server: {0}\nID: {1}\nFilename: {2}", errorMessage, contract.ID, contract.Filename));
                }
                bytes = contract.ByteArray; 

                returnValue = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            finally
            { 
                if ((client.State != CommunicationState.Closed))
                {
                    client.Close();
                }
            }
            return returnValue;
        }
    }
}
