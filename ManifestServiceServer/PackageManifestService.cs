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
using ManifestServerBusiness;
using DocumentScannerCommon;
using DocumentScannerServiceCommon;

namespace ManifestServiceServer
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class PackageManifestService : 
        IPackageManifestService
    {

        public PackageManifestService()
        {
            string errorMessage = default(String);

            //Moved DSServerModelController assignment out of ManifestServiceServer. 
            //Call TransferServerBusiness.Transfer.InitDelegates() to have it load delegates from another library.
            if (!ManifestServerBusiness.Manifest.InitDelegates(ref errorMessage))
            {
                throw new Exception(String.Format("Manifest Service Server is unable to init delegates to Manifest Server Business: {0}", errorMessage));
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
        /// Given a contract with the Operator ID and the specified date, 
        /// return a contract with a List(Of PackageManifest) from the server.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns>Boolean</returns>
        public Boolean ManifestsConfirmed
        (
            ref ManifestContract contract,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            List<PackageManifest> manifestList = default(List<PackageManifest>);

            try
            {
                returnValue = 
                    ManifestServerBusiness.Manifest.manifestsConfirmedDelegate
                    (
                        contract.OperatorId, 
                        contract.Date, 
                        ref manifestList,//cannot pass property by ref
                        ref errorMessage
                    );
                contract.Manifests = manifestList;
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
        /// Given a contract with the Operator ID, a Transaction ID, and the specified date, 
        /// return a contract with a List(Of ImageFile) from the server.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns>Boolean</returns>
        public Boolean DocumentsConfirmed
        (
            ref DocumentContract contract,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            List<ImageFile> documentList = default(List<ImageFile>);

            try
            {
                returnValue = 
                    ManifestServerBusiness.Manifest.documentsConfirmedDelegate
                    (
                        contract.OperatorId,
                        contract.TransactionId,
                        contract.Date,
                        ref documentList,//cannot pass property by ref
                        ref errorMessage
                    );
                contract.Documents = documentList;
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
        /// Given a contract with the Operator ID and the operator id, 
        /// return a contract with a List(Of PackageManifest) from the server.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns>Boolean</returns>
        public Boolean ManifestsAvailable
        (
            ref ManifestContract contract,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            List<PackageManifest> manifestList = default(List<PackageManifest>);

            try
            {
                returnValue = 
                    ManifestServerBusiness.Manifest.manifestsAvailableDelegate
                    (
                        contract.OperatorId, 
                        contract.Date,
                        ref manifestList,//cannot pass property by ref
                        ref errorMessage
                    );
                contract.Manifests = manifestList;
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
        /// Given a contract with the Operator ID, a Transaction ID, and the operator id, 
        /// return a contract with a List(Of ImageFile) from the server.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns>Boolean</returns>
        public Boolean DocumentsAvailable
        (
            ref DocumentContract contract,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            List<ImageFile> documentList = default(List<ImageFile>);

            try
            {
                returnValue = 
                    ManifestServerBusiness.Manifest.documentsAvailableDelegate
                    (
                        contract.OperatorId,
                        contract.TransactionId,
                        contract.Date,
                        ref documentList,//cannot pass property by ref
                        ref errorMessage
                    );
                contract.Documents = documentList;
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
