
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ssepan.Io;
using Ssepan.Utility;
using System.Diagnostics;
using System.Reflection;

namespace TransferClientBusiness
{
    public static class Transfer
    {
        #region Declarations
        public const String TEMP_FILE_TYPE = "tmp";
        #endregion Declarations

        /// <summary>
        /// Call Service Ping test
        /// </summary>
        /// <param name="endpointConfigurationName"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean Ping(String endpointConfigurationName, ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);
            try
            {
                TransferServiceClient.TransferService.EndpointConfigurationName = endpointConfigurationName;
                if (!TransferServiceClient.TransferService.Ping(ref errorMessage))
                { 
                    throw new Exception(String.Format("Transfer Client Business is unable to Ping Transfer Service Client: {0}", errorMessage));
                }

                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        /// <summary>
        /// Perform business logic for client side of pull; receive the bytes and store a file.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="operatorId"></param>
        /// <param name="filePath"></param>
        /// <param name="endpointConfigurationName"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean PullFile(String id, String operatorId, String filePath, String endpointConfigurationName, ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);
            Byte[] bytes = default(Byte[]);
            String filename = default(String);
            String receivePath = default(String);
            String tempFilePath = default(String);
            String packageFilePath = default(String);
            
            try
            {
                //Path must be stripped from filename for use in service.
                receivePath = Path.GetDirectoryName(filePath);
                filename = Path.GetFileName(filePath);

                //call service to receive bytes
                TransferServiceClient.TransferService.EndpointConfigurationName = endpointConfigurationName;
                if (!TransferServiceClient.TransferService.Pull(id, operatorId, filename, ref bytes, ref errorMessage))
                {
                    throw new Exception(String.Format("Transfer Client Business is unable to Pull file from Transfer Service Client: {0}\nID: {1}\nFilename: {2}", errorMessage, id, filename));
                }

                //get temp file path and write bytes to temp file
                tempFilePath = Path.Combine(receivePath, String.Format("{0}.{1}", Path.GetFileNameWithoutExtension(filename), TEMP_FILE_TYPE));
                if (!Ssepan.Io.Files.Write(tempFilePath, bytes))
                {
                    throw new Exception(String.Format("Transfer Client Business is unable to write file to client: {0}\nID: {1}\ntemp file path: {2}", errorMessage, id, tempFilePath));
                }

                //get package file path and rename temp file
                packageFilePath = Path.Combine(receivePath, filename);
                System.IO.File.Move(tempFilePath, packageFilePath);
                               
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
        /// Perform business logic for client side of push; load a file and send the bytes.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="operatorId"></param>
        /// <param name="filePath">Filename with path. Path will reflect configuration 'PushSendPath'. </param>
        /// <param name="endpointConfigurationName"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean PushFile(String id, String operatorId, String filePath, String endpointConfigurationName, ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);
            Byte[] bytes = default(Byte[]);

            try
            {
                //load file bytes from client
                if (!Ssepan.Io.Files.Read(filePath, ref bytes))
                {
                    throw new Exception(String.Format("Unable to read file from client: {0}\nID: {1}\nFilename: {2}", errorMessage, id, filePath));
                }

                //call service to send bytes
                //Path must be stripped from filename for use in service.
                TransferServiceClient.TransferService.EndpointConfigurationName = endpointConfigurationName;
                if (!TransferServiceClient.TransferService.Push(id, operatorId, Path.GetFileName(filePath), bytes, ref errorMessage))
                {
                    throw new Exception(String.Format("Transfer Client Business is unable to Push file to Transfer Service Client: {0}\nID: {1}\nFilename: {2}", errorMessage, id, filePath));
                }

                //delete package file after upload
                System.IO.File.Delete(filePath);

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
