
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Ssepan.Io;
using Ssepan.Utility;
using DocumentScannerCommon;
using ManifestServiceClient;

namespace ManifestClientBusiness
{
    public static class Manifest
    {
        #region Declarations
        public const String TEMP_FILE_TYPE = "tmp";
        #endregion Declarations

        ///// <summary>
        ///// Call Service Ping test
        ///// </summary>
        ///// <param name="endpointConfigurationName"></param>
        ///// <param name="errorMessage"></param>
        ///// <returns></returns>
        //public static Boolean Ping
        //(
        //    String endpointConfigurationName, 
        //    ref String errorMessage
        //)
        //{
        //    Boolean returnValue = default(Boolean);
        //    try
        //    {
        //        ManifestService.EndpointConfigurationName = endpointConfigurationName;
        //        if (!ManifestService.Ping(ref errorMessage))
        //        { 
        //            throw new Exception(String.Format("Manifest Client Business is unable to Ping Manifest Service Client: {0}", errorMessage));
        //        }

        //        returnValue = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
        //    }
        //    return returnValue;
        //}

        ///// <summary>
        ///// Perform business logic for client side of pull; receive the bytes and store a file.
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="operatorId"></param>
        ///// <param name="filePath"></param>
        ///// <param name="endpointConfigurationName"></param>
        ///// <param name="errorMessage"></param>
        ///// <returns></returns>
        //public static Boolean PullFile
        //(
        //    String id, 
        //    String operatorId, 
        //    String filePath, 
        //    String endpointConfigurationName, 
        //    ref String errorMessage
        //)
        //{
        //    Boolean returnValue = default(Boolean);
        //    Byte[] bytes = default(Byte[]);
        //    String filename = default(String);
        //    String receivePath = default(String);
        //    String tempFilePath = default(String);
        //    String packageFilePath = default(String);

        //    try
        //    {
        //        //Path must be stripped from filename for use in service.
        //        receivePath = Path.GetDirectoryName(filePath);
        //        filename = Path.GetFileName(filePath);

        //        //call service to receive bytes
        //        ManifestService.EndpointConfigurationName = endpointConfigurationName;
        //        if (!ManifestService.Pull(id, operatorId, filename, ref bytes, ref errorMessage))
        //        {
        //            throw new Exception(String.Format("Manifest Client Business is unable to Pull file from Manifest Service Client: {0}\nID: {1}\nFilename: {2}", errorMessage, id, filename));
        //        }

        //        //get temp file path and write bytes to temp file
        //        tempFilePath = Path.Combine(receivePath, String.Format("{0}.{1}", Path.GetFileNameWithoutExtension(filename), TEMP_FILE_TYPE));
        //        if (!Ssepan.Io.Files.Write(tempFilePath, bytes))
        //        {
        //            throw new Exception(String.Format("Manifest Client Business is unable to write file to client: {0}\nID: {1}\ntemp file path: {2}", errorMessage, id, tempFilePath));
        //        }

        //        //get package file path and rename temp file
        //        packageFilePath = Path.Combine(receivePath, filename);
        //        System.IO.File.Move(tempFilePath, packageFilePath);

        //        returnValue = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        errorMessage = ex.Message;
        //        Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
        //    }
        //    return returnValue;
        //}

        ///// <summary>
        ///// Perform business logic for client side of push; load a file and send the bytes.
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="operatorId"></param>
        ///// <param name="filePath">Filename with path. Path will reflect configuration 'PushSendPath'. </param>
        ///// <param name="endpointConfigurationName"></param>
        ///// <param name="errorMessage"></param>
        ///// <returns></returns>
        //public static Boolean PushFile
        //(
        //    String id, 
        //    String operatorId, 
        //    String filePath, 
        //    String endpointConfigurationName, 
        //    ref String errorMessage
        //)
        //{
        //    Boolean returnValue = default(Boolean);
        //    Byte[] bytes = default(Byte[]);

        //    try
        //    {
        //        //load file bytes from client
        //        if (!Ssepan.Io.Files.Read(filePath, ref bytes))
        //        {
        //            throw new Exception(String.Format("Unable to read file from client: {0}\nID: {1}\nFilename: {2}", errorMessage, id, filePath));
        //        }

        //        //call service to send bytes
        //        //Path must be stripped from filename for use in service.
        //        ManifestService.EndpointConfigurationName = endpointConfigurationName;
        //        if (!ManifestService.Push(id, operatorId, Path.GetFileName(filePath), bytes, ref errorMessage))
        //        {
        //            throw new Exception(String.Format("Manifest Client Business is unable to Push file to Manifest Service Client: {0}\nID: {1}\nFilename: {2}", errorMessage, id, filePath));
        //        }

        //        //TODO:make configuration-driven
        //        if (false)
        //        { 
        //            //delete package file after upload
        //            System.IO.File.Delete(filePath);
        //        }

        //        returnValue = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        errorMessage = ex.Message;
        //        Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
        //    }
        //    return returnValue;
        //}

        #region Manifest Client Business

        /// <summary>
        /// Call Service Ping test
        /// </summary>
        /// <param name="endpointConfigurationName"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean Ping
        (
            String endpointConfigurationName,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            try
            {
                ManifestService.EndpointConfigurationName = endpointConfigurationName;
                if (!ManifestService.Ping(ref errorMessage))
                {
                    throw new Exception(String.Format("Manifest Client Business is unable to Ping Manifest Service Client: {0}", errorMessage));
                }

                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        //TODO:move to ManifestClientBusiness; ConfirmManifestsInBackground caller to redirect
        /// <summary>
        /// Perform business logic for client side of manifest query; receive the list and convert.
        /// Given the Operator ID and the specified date, 
        /// return a List(Of PackageManifest) from the server.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="date"></param>
        /// <param name="endpointConfigurationName"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static List<PackageManifest> ManifestsConfirmed
        (
            String operatorId,
            DateTime date,
            String endpointConfigurationName,
            ref String errorMessage
        )
        {
            List<PackageManifest> returnValue = default(List<PackageManifest>);

            try
            {
                //call service to receive list
                ManifestService.EndpointConfigurationName = endpointConfigurationName;
                returnValue = ManifestService.ManifestsConfirmed(operatorId, date, ref errorMessage);
                if (returnValue == null)
                {
                    throw new Exception(String.Format("DocumentScanner Controller is unable to query Manifest Service Client for package manifests: '{0}'\nUsername: '{1}'\nDate: '{2}'", errorMessage, operatorId, date));
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        //TODO:move to ManifestClientBusiness; no caller to redirect
        /// <summary>
        /// Perform business logic for client side of document query; receive list as-is.
        ///Given the Operator ID, a Transaction ID, and the specified date, 
        ///return a List(Of ImageFile) from the server.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="transactionId"></param>
        /// <param name="date"></param>
        /// <param name="endpointConfigurationName"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static List<ImageFile> DocumentsConfirmed
        (
            String operatorId,
            String transactionId,
            DateTime date,
            String endpointConfigurationName,
            ref String errorMessage
        )
        {
            List<ImageFile> returnValue = default(List<ImageFile>);

            try
            {
                //call service to receive list
                ManifestService.EndpointConfigurationName = endpointConfigurationName;
                returnValue = ManifestService.DocumentsConfirmed(operatorId, transactionId, date, ref errorMessage);
                if (returnValue == null)
                {
                    throw new Exception(String.Format("DocumentScanner Controller is unable to query Manifest Service Client for documents: '{0}'\nUsername: '{1}'\nDate: '{2}'\nTransaction: '{3}'", errorMessage, operatorId, date, transactionId));
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        //TODO:move to ManifestClientBusiness; AvailableManifestsInBackground caller to redirect
        /// <summary>
        /// Perform business logic for client side of manifest query; receive the list and convert.
        /// Given the Operator ID and the specified date, 
        /// return a List(Of PackageManifest) from the server.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="endpointConfigurationName"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static List<PackageManifest> ManifestsAvailable
        (
            String operatorId,
            String endpointConfigurationName,
            ref String errorMessage
        )
        {
            List<PackageManifest> returnValue = default(List<PackageManifest>);

            try
            {
                //call service to receive list
                ManifestService.EndpointConfigurationName = endpointConfigurationName;
                returnValue = ManifestService.ManifestsAvailable(operatorId, ref errorMessage);
                if (returnValue == null)
                {
                    throw new Exception(String.Format("DocumentScanner Controller is unable to query Manifest Service Client for package manifests: '{0}'\nUsername: '{1}'", errorMessage, operatorId));
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        //TODO:move to ManifestClientBusiness; no caller to redirect
        /// <summary>
        /// Perform business logic for client side of document query; receive list as-is.
        ///Given the Operator ID, a Transaction ID, and the specified date, 
        ///return a List(Of ImageFile) from the server.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="transactionId"></param>
        /// <param name="endpointConfigurationName"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static List<ImageFile> DocumentsAvailable
        (
            String operatorId,
            String transactionId,
            String endpointConfigurationName,
            ref String errorMessage
        )
        {
            List<ImageFile> returnValue = default(List<ImageFile>);

            try
            {
                //call service to receive list
                ManifestService.EndpointConfigurationName = endpointConfigurationName;
                returnValue = ManifestService.DocumentsAvailable(operatorId, transactionId, ref errorMessage);
                if (returnValue == null)
                {
                    throw new Exception(String.Format("DocumentScanner Controller is unable to query Manifest Service Client for documents: '{0}'\nUsername: '{1}'\nTransaction: '{3}'", errorMessage, operatorId, transactionId));
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }
        #endregion Manifest Client Business

    }
}
