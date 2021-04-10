using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Reflection;
using System.Text;
using Ssepan.Utility;
using DocumentScannerCommon;
using DocumentScannerServiceCommon;
using ManifestServiceClient.ManifestServiceClientReference;

namespace ManifestServiceClient
{
    public class ManifestService
    {
        /// <summary>
        /// Limited features, but seems to allow larger size of messages passed.
        /// </summary>
        public const String ENDPOINT_CONFIGURATION_BASIC = "BasicHttpBinding_IPackageManifestService";

        /// <summary>
        /// Supports more advanced features, but seems to be limited in the size of the messages passed.
        /// </summary>
        public const String ENDPOINT_CONFIGURATION_WS = "WSHttpBinding_IPackageManifestService";

        static ManifestService()
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
            PackageManifestServiceClient client = default(PackageManifestServiceClient);

            try
            {
                client = new PackageManifestServiceClient(EndpointConfigurationName);
                client.Open();

                //Ping service
                if (!client.Ping(ref errorMessage))
                {
                    throw new Exception(String.Format("Unable to Ping Manifest Service: {0}", errorMessage));
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
        /// Given the Operator ID and the specified date, 
        /// return a List(Of PackageManifest) from the server.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="date"></param>
        /// <param name="manifestList"></param>
        /// <param name="errorMessage"></param>
        /// <returns>Boolean</returns>
        public static Boolean ManifestsConfirmed
        (
            String operatorId,
            DateTime date, 
            ref List<PackageManifest> manifestList,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            manifestList = default(List<DocumentScannerCommon.PackageManifest>);
            DocumentScannerServiceCommon.ManifestContract contract = default(DocumentScannerServiceCommon.ManifestContract);
            PackageManifestServiceClient client = default(PackageManifestServiceClient);

            try
            {
                client = new PackageManifestServiceClient(EndpointConfigurationName);
                client.Open();

                //Ping service
                if (!client.Ping(ref errorMessage))
                {
                    throw new Exception(String.Format("Manifest Service Client is unable to Ping Manifest Service Server: {0}", errorMessage));
                }

                //perform package query
                contract = new DocumentScannerServiceCommon.ManifestContract();
                contract.OperatorId = operatorId;
                contract.Date = date;

                returnValue = client.ManifestsConfirmed(ref contract, ref errorMessage);//.ToList<PackageManifest>();
                if (contract.Manifests == null)
                {
                    throw new Exception(String.Format("Manifest Service Client is unable to query Manifest Service Server for package manifests: '{0}'\nUsername: '{1}'\nDate: '{2}'", errorMessage, contract.OperatorId, contract.Date));
                }
                manifestList = contract.Manifests;
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
        /// Given the Operator ID, a Transaction ID, and the specified date, 
        /// return a List(Of ImageFile) from the server.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="transactionId"></param>
        /// <param name="date"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static List<DocumentScannerCommon.ImageFile> DocumentsConfirmed
        (
            String operatorId,
            String transactionId,
            DateTime date, 
            ref String errorMessage
        )
        {
            List<DocumentScannerCommon.ImageFile> returnValue = default(List<DocumentScannerCommon.ImageFile>);
            DocumentScannerServiceCommon.ManifestContract contract = default(DocumentScannerServiceCommon.ManifestContract);
            PackageManifestServiceClient client = default(PackageManifestServiceClient);

            try
            {
                client = new PackageManifestServiceClient(EndpointConfigurationName);
                client.Open();

                //Ping service
                if (!client.Ping(ref errorMessage))
                {
                    throw new Exception(String.Format("Unable to Ping Manifest Service: {0}", errorMessage));
                }

                ///perform document query
                contract = new DocumentScannerServiceCommon.ManifestContract();
                contract.OperatorId = operatorId;
                contract.Date = date;
                contract.TransactionId = transactionId;

                returnValue = client.DocumentsConfirmed(contract, ref errorMessage);
                if (returnValue == null)
                {
                    throw new Exception(String.Format("Manifest Service Client is unable to query Manifest Service Server for documents: '{0}'\nUsername: '{1}'\nDate: '{2}'\nTransaction: '{3}'", errorMessage, contract.OperatorId, contract.Date, contract.TransactionId));
                }
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
        /// Given the Operator ID and the specified date, 
        /// return a List(Of PackageManifest) from the server.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="manifestList"></param>
        /// <param name="errorMessage"></param>
        /// <returns>Boolean</returns>
        public static Boolean ManifestsAvailable
        (
            String operatorId,
            ref List<PackageManifest> manifestList,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            manifestList = default(List<DocumentScannerCommon.PackageManifest>);
            DocumentScannerServiceCommon.ManifestContract contract = default(DocumentScannerServiceCommon.ManifestContract);
            //ManifestServiceClientReference.ManifestContract contract2 = default(ManifestServiceClientReference.ManifestContract);
            PackageManifestServiceClient client = default(PackageManifestServiceClient);

            try
            {
                client = new PackageManifestServiceClient(EndpointConfigurationName);
                client.Open();

                //Ping service
                if (!client.Ping(ref errorMessage))
                {
                    throw new Exception(String.Format("Manifest Service Client is unable to Ping Manifest Service Server: {0}", errorMessage));
                }

                //perform package query
                contract = new DocumentScannerServiceCommon.ManifestContract();
                contract.OperatorId = operatorId;

                returnValue = client.ManifestsAvailable(ref contract, ref errorMessage);
                if (contract.Manifests == null)
                {
                    throw new Exception(String.Format("Manifest Service Client is unable to query Manifest Service Server for package manifests: '{0}'\nUsername: '{1}'", errorMessage, contract.OperatorId));
                }
                manifestList = contract.Manifests;
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
        /// Given the Operator ID, a Transaction ID, and the specified date, 
        /// return a List(Of ImageFile) from the server.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="transactionId"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static List<DocumentScannerCommon.ImageFile> DocumentsAvailable
        (
            String operatorId,
            String transactionId,
            ref String errorMessage
        )
        {
            List<DocumentScannerCommon.ImageFile> returnValue = default(List<DocumentScannerCommon.ImageFile>);
            DocumentScannerServiceCommon.ManifestContract contract = default(DocumentScannerServiceCommon.ManifestContract);
            PackageManifestServiceClient client = default(PackageManifestServiceClient);

            try
            {
                client = new PackageManifestServiceClient(EndpointConfigurationName);
                client.Open();

                //Ping service
                if (!client.Ping(ref errorMessage))
                {
                    throw new Exception(String.Format("Unable to Ping Manifest Service: {0}", errorMessage));
                }

                ///perform document query
                contract = new DocumentScannerServiceCommon.ManifestContract();
                contract.OperatorId = operatorId;
                contract.TransactionId = transactionId;

                returnValue = client.DocumentsAvailable(contract, ref errorMessage);
                if (returnValue == null)
                {
                    throw new Exception(String.Format("Manifest Service Client is unable to query Manifest Service Server for documents: '{0}'\nUsername: '{1}'\nTransaction: '{2}'", errorMessage, contract.OperatorId, contract.TransactionId));
                }
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
