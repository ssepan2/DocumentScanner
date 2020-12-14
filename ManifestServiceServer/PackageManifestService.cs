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
using DocumentScannerCommon;
using DocumentScannerServiceCommon;
using DocumentScannerServerLibrary;

namespace ManifestServiceServer
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class PackageManifestService : 
        IPackageManifestService
    {
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
        /// Given the Operator ID and the specified date, 
        /// return a List(Of PackageManifest) from the server.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public List<PackageManifest> ManifestsConfirmed
        (
            ManifestContract contract,
            ref String errorMessage
        )
        {
            List<PackageManifest> returnValue = default(List<PackageManifest>);

            try
            {
                returnValue = 
                    DSServerController<DSServerModel>.ManifestsConfirmed
                    (
                        contract.OperatorId, 
                        contract.Date, 
                        ref errorMessage
                    );
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
        /// Given the Operator ID, a Transaction ID, and the specified date, 
        /// return a List(Of ImageFile) from the server.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public List<ImageFile> DocumentsConfirmed
        (
            ManifestContract contract,
            ref String errorMessage
        )
        {
            List<ImageFile> returnValue = default(List<ImageFile>);

            try
            {
                returnValue =
                    DSServerController<DSServerModel>.DocumentsConfirmed
                    (
                        contract.OperatorId,
                        contract.TransactionId,
                        contract.Date,
                        ref errorMessage
                    );
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
        /// Given the Operator ID and the operator id, 
        /// return a List(Of PackageManifest) from the server.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public List<PackageManifest> ManifestsAvailable
        (
            ManifestContract contract,
            ref String errorMessage
        )
        {
            List<PackageManifest> returnValue = default(List<PackageManifest>);

            try
            {
                returnValue = 
                    DSServerController<DSServerModel>.ManifestsAvailable
                    (
                        contract.OperatorId, 
                        contract.Date, 
                        ref errorMessage
                    );
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
        /// Given the Operator ID, a Transaction ID, and the operator id, 
        /// return a List(Of ImageFile) from the server.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public List<ImageFile> DocumentsAvailable
        (
            ManifestContract contract,
            ref String errorMessage
        )
        {
            List<ImageFile> returnValue = default(List<ImageFile>);

            try
            {
                returnValue =
                    DSServerController<DSServerModel>.DocumentsAvailable
                    (
                        contract.OperatorId,
                        contract.TransactionId,
                        contract.Date,
                        ref errorMessage
                    );
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
