using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Ssepan.Application;
using Ssepan.Io;
using Ssepan.Transaction;
using Ssepan.Utility;
using DocumentScannerCommon;
//using TransferServerBusiness;

namespace DocumentScannerServerLibrary
{
    /// <summary>
    /// This is the MVC Controller
    /// </summary>
    public class DSServerController<TModel> : 
        ModelController<TModel>
        where TModel :
            class,
            IModel,
            new()
    {
        #region Declarations
        #endregion Declarations

        #region Constructors
        public DSServerController()
        {
                //Init config parameters
                if (!LoadConfigParameters())
                {
                    throw new Exception(String.Format("Unable to load config file parameter(s)."));
                }
        }
        #endregion Constructors

        #region Properties
        //Note:TModel Model exists in base
        #endregion Properties

        /// <summary>
        /// Load from app config
        /// </summary>
        public static Boolean LoadConfigParameters()
        {
            Boolean returnValue = default(Boolean);
            Int32 _reCreateWaitMilliseconds = default(Int32);
            String _userSendFolder = default(String);
            String _dataPath = default(String);
            String _pushReceivePath = default(String);
            Int32 _completedTransactionRetentionDays = default(Int32);
            Int32 _errorTransactionRetentionDays = default(Int32);

            try
            {

                if (!Configuration.ReadValue<Int32>("ReCreateWaitMilliseconds", out _reCreateWaitMilliseconds))
                {
                    throw new Exception(String.Format("Unable to load ReCreateWaitMilliseconds: '{0}'", _reCreateWaitMilliseconds));
                }
                DSServerController<DSServerModel>.Model.ReCreateWaitMilliseconds = _reCreateWaitMilliseconds;

                if (!Configuration.ReadString("UserSendFolder", out _userSendFolder))
                {
                    throw new Exception(String.Format("Unable to load UserSendFolder: '{0}'", _userSendFolder));
                }
                DSServerController<DSServerModel>.Model.UserSendFolder = _userSendFolder;

                if (!Configuration.ReadString("DataPath", out _dataPath))
                {
                    throw new Exception(String.Format("Unable to load DataPath: '{0}'", _dataPath));
                }
                DSServerController<DSServerModel>.Model.DataPath = _dataPath;

                if (!Configuration.ReadString("PushReceivePath", out _pushReceivePath))
                {
                    throw new Exception(String.Format("Unable to load PushReceivePath: '{0}'", _pushReceivePath));
                }
                DSServerController<DSServerModel>.Model.PushReceivePath = _pushReceivePath;

                if (!Configuration.ReadValue<Int32>("CompletedTransactionRetentionDays", out _completedTransactionRetentionDays))
                {
                    throw new Exception(String.Format("Unable to load CompletedTransactionRetentionDays: '{0}'", _completedTransactionRetentionDays));
                }
                DSServerController<DSServerModel>.Model.CompletedTransactionRetentionDays = _completedTransactionRetentionDays;

                if (!Configuration.ReadValue<Int32>("ErrorTransactionRetentionDays", out _errorTransactionRetentionDays))
                {
                    throw new Exception(String.Format("Unable to load ErrorTransactionRetentionDays: '{0}'", _errorTransactionRetentionDays));
                }
                DSServerController<DSServerModel>.Model.ErrorTransactionRetentionDays = _errorTransactionRetentionDays;
                
                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                //throw;
            }
            return returnValue;
        }

        #region TransferServerBusiness.ITransfer
        /// <summary>
        /// Perform business logic for server side of pull; takes a filename, loads a file, and returns the bytes.
        /// Used as delegate method in TransferServerBusiness.Transfer for use by TransferServerServer.FileTransferService.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="operatorId"></param>
        /// <param name="filename"></param>
        /// <param name="bytes"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean PullFile(String id, String operatorId, String filename, ref Byte[] bytes, ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);
            String sendPath = default(String);
            String packageFilePath = default(String);
            String userPath = default(String);
            String userSendPath = default(String);
            String packageContentsPackageSubfolderPath = default(String);
            PackageManifest manifest;
            Action<String> reportProgressDelegate = default(Action<String>);

            try
            {
                //tell methods how to report progress
                reportProgressDelegate =
                    (s) =>
                    {
                        Log.Write(s, EventLogEntryType.Information);
                    };


                //get send path
                if (!Ssepan.Utility.Configuration.ReadString("PullSendPath", out sendPath))
                {
                    throw new Exception(String.Format("Unable to read configuration setting: {0}", "PullSendPath"));
                }
                
                //get user path
                userPath = Path.Combine(DSServerController<DSServerModel>.Model.DataPath, operatorId);

                //get user send path
                userSendPath = Path.Combine(userPath, DSServerController<DSServerModel>.Model.UserSendFolder);
               
                
                //validate manifest, and return manifest object
                if
                (
                    !DocumentScannerCommon.PackageManifest.LoadAndValidateManifest
                    (
                        id,
                        userSendPath,
                        reportProgressDelegate,
                        ref errorMessage,
                        out manifest
                    )
                )
                {
                    throw new Exception(String.Format("Manifest data validation failure: '{0}'", errorMessage));
                }
                

                //get package contents path
                packageContentsPackageSubfolderPath = Path.Combine(userSendPath, DocumentScannerCommon.Package.PACKAGE_FOLDER);
                
                //prepare package subfolder
                if
                (
                    !DocumentScannerCommon.Package.PreparePackageSubFolder
                    (
                        packageContentsPackageSubfolderPath,
                        DSServerController<DSServerModel>.Model.ReCreateWaitMilliseconds,
                        reportProgressDelegate,
                        ref errorMessage
                    )
                )
                {
                    throw new Exception(String.Format("Package folder preparation failure: '{0}'", errorMessage));
                }
                
                
                //move manifest and data
                if
                (
                    !DocumentScannerCommon.PackageManifest.MoveManifestAndData
                    (
                        manifest,
                        userSendPath,
                        packageContentsPackageSubfolderPath,
                        DSServerController<DSServerModel>.Model.ReCreateWaitMilliseconds,
                        reportProgressDelegate,
                        ref errorMessage
                    )
                )
                {
                    throw new Exception(String.Format("Manifest move failure: '{0}'", errorMessage));
                }
                

                //perform package of file with id, from user folder  (operator passed from client)
                //zip manifest and images folder to package in transmit folder
                if
                (
                    !DocumentScannerCommon.Package.FillManifestPackage
                    (
                        manifest,
                        id,
                        sendPath,
                        userSendPath, //creates package folder as subfolder of user folder in Data folder
                        DSServerController<DSServerModel>.Model.ReCreateWaitMilliseconds,
                        reportProgressDelegate,
                        ref errorMessage
                    )
                )
                {
                    throw new Exception(String.Format("Package creation failure: '{0}'", errorMessage));
                }

                //get package file path and load file bytes from package file
                packageFilePath = Path.Combine(sendPath, filename);
                if (!Files.Read(packageFilePath, ref bytes))
                {
                    throw new Exception(String.Format("Transfer Server Business is unable to read file from server: {0}\nID: {1}\nFilename: {2}", errorMessage, id, filename));
                }

                //delete package file after upload
                System.IO.File.Delete(packageFilePath);

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
        /// Perform business logic for server side of push; takes the bytes and store a file.
        /// Used as delegate method in TransferServerBusiness.Transfer for use by TransferServerServer.FileTransferService.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="operatorId"></param>
        /// <param name="filename">Filename only; no path. Path will be obtained here from configuration 'PushReceivePath'.</param>
        /// <param name="bytes"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean PushFile(String id, String operatorId, String filename, Byte[] bytes, ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);
            String receivePath = default(String);
            String tempFilePath = default(String);
            String packageFilePath = default(String);

            try
            {
                //get receive path
                if (!Ssepan.Utility.Configuration.ReadString("PushReceivePath", out receivePath))
                {
                    throw new Exception(String.Format("Transfer Server Business is unable to read configuration setting: {0}", "PushReceivePath"));
                }

                //get temp file path and write bytes to temp file
                tempFilePath = Path.Combine(receivePath, String.Format("{0}.{1}", Path.GetFileNameWithoutExtension(filename), Package.TEMP_FILE_TYPE));
                if (!Files.Write(tempFilePath, bytes))
                {
                    throw new Exception(String.Format("Transfer Server Business is unable to write file to server: {0}\nID: {1}\ntemp file path: {2}", errorMessage, id, tempFilePath));
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
        #endregion TransferServerBusiness.ITransfer

        #region IManifestBusiness.IManifest
        //TODO call from delegate method in ManifestServerBusiness.Manifest
        /// <summary>
        /// Given the Operator ID and the specified date, 
        /// return a List(Of PackageManifest) from the server.
        /// Used as delegate method in ManifestServerBusiness.Manifest for use by ManifestServiceServer.PackageManifestService.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="date"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static List<PackageManifest> ManifestsConfirmed
        (
            String operatorId,
            DateTime date,
            ref String errorMessage
        )
        {
            List<PackageManifest> returnValue = default(List<PackageManifest>);
            List<String> manifestFilePaths = default(List<String>);
            String userDataPath = default(String);
            String datedUserDataPath = default(String);
            PackageManifest manifest = default(PackageManifest);
            String manifestImagesPath = default(String);

            try
            {
                returnValue = new List<PackageManifest>();

                //user folder in data folder
                userDataPath = Path.Combine(DSServerController<DSServerModel>.Model.DataPath, operatorId);

                //dated folder in user folder
                datedUserDataPath = Path.Combine(userDataPath, TransactionFolders.GetDatedSubFolderNameFromDate(date));
                //do not throw error if dated folder does not exist -- just return empty list
                if (!Directory.Exists(datedUserDataPath))
                {
                    Log.Write(new ApplicationException(String.Format("Path '{0}' not present for date '{1}'.", datedUserDataPath, date)),
                        MethodBase.GetCurrentMethod()/*.DeclaringType.ToString() + "." + MethodBase.GetCurrentMethod().Name*/,
                        EventLogEntryType.Warning);

                    return returnValue;
                }

                //get file list in dated user data path, by type of manifest file 
                manifestFilePaths = Directory.GetFiles(datedUserDataPath, String.Format("*.{0}", DocumentScannerCommon.PackageManifest.DATA_FILE_TYPE)).ToList();

                //load each manifest into list
                foreach (String manifestFilePath in manifestFilePaths)
                {
                    manifest = PackageManifest.Load(manifestFilePath);

                    //validate manifest
                    manifestImagesPath = Path.Combine(datedUserDataPath, manifest.TransactionId);
                    if (!manifest.Valid(manifestImagesPath))
                    {
                        throw new Exception(String.Format("Manifest '{0}' invalid: \n'{1}'", manifest.TransactionId, manifest.ErrorMessage));
                    }

                    returnValue.Add(manifest);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        //TODO call from delegate method in ManifestServerBusiness.Manifest
        /// <summary>
        /// Given the Operator ID, a Transaction ID, and the specified date, 
        /// return a List(Of ImageFile) from the server.
        /// Used as delegate method in ManifestServerBusiness.Manifest for use by ManifestServiceServer.PackageManifestService.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="transactionId"></param>
        /// <param name="date"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static List<ImageFile> DocumentsConfirmed
        (
            String operatorId,
            String transactionId,
            DateTime date,
            ref String errorMessage
        )
        {
            List<ImageFile> returnValue = default(List<ImageFile>);
            PackageManifest manifest = default(PackageManifest);
            String manifestFilePath = default(String);
            String userDataPath = default(String);
            String datedUserDataPath = default(String);
            String manifestImagesPath = default(String);

            try
            {
                returnValue = new List<ImageFile>();

                //user folder in data folder
                userDataPath = Path.Combine(DSServerController<DSServerModel>.Model.DataPath, operatorId);

                //dated folder in user folder
                datedUserDataPath = Path.Combine(userDataPath, TransactionFolders.GetDatedSubFolderNameFromDate(date));

                //load manifest 
                manifestFilePath = Path.Combine(datedUserDataPath, String.Format("{0}.{1}", transactionId, DocumentScannerCommon.PackageManifest.DATA_FILE_TYPE));
                manifest = PackageManifest.Load(manifestFilePath);

                //validate manifest
                manifestImagesPath = Path.Combine(datedUserDataPath, manifest.TransactionId);
                if (!manifest.Valid(manifestImagesPath))
                {
                    throw new Exception(String.Format("Manifest '{0}' invalid: \n'{1}'", manifest.TransactionId, manifest.ErrorMessage));
                }

                //load document list into return list
                returnValue = manifest.DocumentFiles.ToList<ImageFile>();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        //TODO call from delegate method in ManifestServerBusiness.Manifest
        /// <summary>
        /// Given the Operator ID and the operator id, 
        /// return a List(Of PackageManifest) from the server.
        /// Used as delegate method in ManifestServerBusiness.Manifest for use by ManifestServiceServer.PackageManifestService.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="date"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static List<PackageManifest> ManifestsAvailable
        (
            String operatorId,
            DateTime date,
            ref String errorMessage
        )
        {
            List<PackageManifest> returnValue = default(List<PackageManifest>);
            List<String> manifestFilePaths = default(List<String>);
            String userDataPath = default(String);
            String sendUserDataPath = default(String);
            PackageManifest manifest = default(PackageManifest);
            String manifestImagesPath = default(String);

            try
            {
                returnValue = new List<PackageManifest>();

                //user folder in data folder
                userDataPath = Path.Combine(DSServerController<DSServerModel>.Model.DataPath, operatorId);

                //send folder in user folder
                sendUserDataPath = Path.Combine(userDataPath, DSServerController<DSServerModel>.Model.UserSendFolder);
                //do not throw error if dated folder does not exist -- just return empty list
                if (!Directory.Exists(sendUserDataPath))
                {
                    Log.Write(
                        new ApplicationException(String.Format("Path '{0}' not present.", sendUserDataPath)),
                        MethodBase.GetCurrentMethod()/*.DeclaringType.ToString() + "." + MethodBase.GetCurrentMethod().Name*/,
                        EventLogEntryType.Warning);

                    return returnValue;
                }

                //get file list in dated user data path, by type of manifest file 
                manifestFilePaths = Directory.GetFiles(sendUserDataPath, String.Format("*.{0}", DocumentScannerCommon.PackageManifest.DATA_FILE_TYPE)).ToList();

                //load each manifest into list
                foreach (String manifestFilePath in manifestFilePaths)
                {
                    manifest = PackageManifest.Load(manifestFilePath);

                    //validate manifest
                    manifestImagesPath = Path.Combine(sendUserDataPath, manifest.TransactionId);
                    if (!manifest.Valid(manifestImagesPath))
                    {
                        throw new Exception(String.Format("Manifest '{0}' invalid: \n'{1}'", manifest.TransactionId, manifest.ErrorMessage));
                    }

                    returnValue.Add(manifest);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        //TODO call from delegate method in ManifestServerBusiness.Manifest
        /// <summary>
        /// Given the Operator ID, a Transaction ID, and the operator id, 
        /// return a List(Of ImageFile) from the server.
        /// Used as delegate method in ManifestServerBusiness.Manifest for use by ManifestServiceServer.PackageManifestService.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="transactionId"></param>
        /// <param name="date"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static List<ImageFile> DocumentsAvailable
        (
            String operatorId,
            String transactionId,
            DateTime date,
            ref String errorMessage
        )
        {
            List<ImageFile> returnValue = default(List<ImageFile>);
            PackageManifest manifest = default(PackageManifest);
            String manifestFilePath = default(String);
            String userDataPath = default(String);
            String sendUserDataPath = default(String);
            String manifestImagesPath = default(String);

            try
            {
                returnValue = new List<ImageFile>();

                //user folder in data folder
                userDataPath = Path.Combine(DSServerController<DSServerModel>.Model.DataPath, operatorId);

                //send folder in user folder
                sendUserDataPath = Path.Combine(userDataPath, DSServerController<DSServerModel>.Model.UserSendFolder);

                //load manifest 
                manifestFilePath = Path.Combine(sendUserDataPath, String.Format("{0}.{1}", transactionId, DocumentScannerCommon.PackageManifest.DATA_FILE_TYPE));
                manifest = PackageManifest.Load(manifestFilePath);

                //validate manifest
                manifestImagesPath = Path.Combine(sendUserDataPath, manifest.TransactionId);
                if (!manifest.Valid(manifestImagesPath))
                {
                    throw new Exception(String.Format("Manifest '{0}' invalid: \n'{1}'", manifest.TransactionId, manifest.ErrorMessage));
                }

                //load document list into return list
                returnValue = manifest.DocumentFiles.ToList<ImageFile>();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }
        #endregion IManifestBusiness.IManifest

        #region Methods
        /// <summary>
        /// The Controller method for triggering update notifications to the model.
        /// </summary>
        public static void Refresh()
        {
            try
            {
                DSServerController<DSServerModel>.Model.IsChanged = true;//Value doesn't matter; a changed fire event;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        /// <summary>
        /// Process transferred queued packages.
        /// Should only be called if PackagesQueued is true.
        /// State of settings is not a consideration.
        /// </summary>
        /// <param name="reportProgressDelegate">Action(Of String) that reports progress in a manner defined by caller.</param>
        /// <returns></returns>
        public static Boolean Poll
        (
            Action<String> reportProgressDelegate
        )
        {
            Boolean returnValue = default(Boolean);
            Boolean accumulatedResult = true;
            String errorMessage = String.Empty;
            String statusMessage = String.Empty;
            String packageFilename = default(String);
            Int32 packageCount = default(Int32);
            List<String> packageFilenames = default(List<String>);
            Boolean processResult = default(Boolean);
            TransactionFolders transactionFolders = default(TransactionFolders);

            try
            {
                reportProgressDelegate(String.Format("validating...{0}", packageCount));

                packageFilenames = DSServerController<DSServerModel>.Model.PackagesQueued;
                if (!(packageFilenames.Count > 0))
                {
                    reportProgressDelegate(String.Format("No packages queued....{0}", packageFilenames.Count));
                    returnValue = true;
                    return returnValue;
                }
                else
                {
                    //init transaction folders
                    transactionFolders =
                        new TransactionFolders
                        (
                            DSServerController<DSServerModel>.Model.TransactionRootPath,
                            DSServerController<DSServerModel>.Model.TransactionWorkingPath,
                            DSServerController<DSServerModel>.Model.TransactionCompletedPath,
                            DSServerController<DSServerModel>.Model.TransactionErrorPath,
                            true,
                            true,
                            false,
                            true,
                            true
                        );

                    while (packageFilenames.Count > 0)
                    {
                        processResult = default(Boolean);

                        //process only first package from package list; list may change (increase) during processing.
                        packageCount++;
                        reportProgressDelegate(String.Format("package...{0}", packageCount));
                        packageFilename = packageFilenames.First<String>();

                        ////one chance to cancel per loop
                        ////TODO:implement cancellation 
                        ////Note:cancellation must leave current transaction in original state
                        //if (worker.CancellationPending)
                        //{
                        //    e.Cancel = true;
                        //    errorMessage = "Cancelled.";
                        //    return returnValue;
                        //}

                        //start transaction
                        if (!transactionFolders.Begin(packageFilename, ref errorMessage))
                        {
                            throw new Exception(String.Format("Unable to begin package transaction for processing: {0}\nFilename: {1}", errorMessage, packageFilename));
                        }

                        //process package
                        reportProgressDelegate(String.Format("Processing {0}", transactionFolders.WorkingFile));
                        Log.Write(String.Format("Processing {0}", transactionFolders.WorkingFile), EventLogEntryType.Information);
                        processResult = Process(transactionFolders, reportProgressDelegate, ref errorMessage);
                        accumulatedResult = accumulatedResult && processResult;
                        if (!processResult)
                        {
                            statusMessage = String.Format("DocumentScannerServer is unable to process package file to Transfer Client Business: {0}\nID: {1}\nFilename: {2}", errorMessage, transactionFolders.ID, transactionFolders.WorkingFile);
                            Log.Write(statusMessage, EventLogEntryType.Error);
                            reportProgressDelegate(statusMessage);
                        }
                        else
                        {
                            statusMessage = String.Format("Processed package {0}", transactionFolders.WorkingFile);
                            Log.Write(statusMessage, EventLogEntryType.Information);
                            reportProgressDelegate(statusMessage);
                        }

                        //finish transaction
                        if (!transactionFolders.End(processResult, ref errorMessage))
                        {
                            throw new Exception(String.Format("Unable to end package transaction for processing: {0}\nID: {1}\nFilename: {2}", errorMessage, transactionFolders.ID, transactionFolders.WorkingFile));
                        }
                                            
                        //reload package list
                        packageFilenames = DSServerController<DSServerModel>.Model.PackagesQueued;
                    }

                    //report final status
                    reportProgressDelegate(String.Format("packages processed...finalizing...{0}", packageCount));

                    //clean up transaction archives
                    transactionFolders.TransactionFilename = "*." + /*SettingsController<Settings>.Settings.*/SettingsBase.FileTypeExtension; //if Use... flags are clear, a file extension is needed; this may not have been set if no files found to process.
                    if
                    (
                        !transactionFolders.CleanUp
                        (
                            new TimeSpan(DSServerController<DSServerModel>.Model.CompletedTransactionRetentionDays, 0, 0, 0),
                            new TimeSpan(DSServerController<DSServerModel>.Model.ErrorTransactionRetentionDays, 0, 0, 0),
                            ref errorMessage
                        )
                    )
                    {
                        throw new Exception(String.Format("Unable to clean up package transaction object for transfer to Transfer Business: {0}\nFilename: {1}", errorMessage, transactionFolders.TransactionFilename));
                    }
                    transactionFolders = null;
                }

                returnValue = accumulatedResult;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        /// <summary>
        /// Process the package file in the working folder.
        /// </summary>
        /// <param name="transactionFolders"></param>
        /// <param name="reportProgressDelegate">Action(Of String) that reports progress in a manner defined by caller.</param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private static Boolean Process
        (
            TransactionFolders transactionFolders,
            Action<String> reportProgressDelegate,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            PackageManifest manifest = default(PackageManifest);
            String userPath = default(String);
            String datedFolder = default(String);
            String datedPath = default(String);
            String packageContentsPackageSubfolderPath = default(String);

            try
            {
                //report status
                reportProgressDelegate(String.Format("extracting manifest..."));

                //unpackage to a package subfolder of the transaction working folder path
                if 
                (   
                    !DocumentScannerCommon.Package.ExtractManifestPackage
                    (
                        transactionFolders.ID,
                        transactionFolders.WorkingFilePath, //location of package file
                        transactionFolders.WorkingFilePath, //extract to subfolder in same location as package file
                        DSServerController<DSServerModel>.Model.ReCreateWaitMilliseconds,
                        reportProgressDelegate,
                        ref errorMessage,
                        out manifest
                    )
                )
                { 
                    throw new Exception(String.Format("Package extraction failure: '{0}'", errorMessage));
                }


                //report status
                reportProgressDelegate(String.Format("creating destination..."));

                //prepare destination
                userPath = Path.Combine(DSServerController<DSServerModel>.Model.DataPath, manifest.OperatorId); 
                if (!Directory.Exists(userPath))
                {
                    //if user path does not exist, create it
                    Directory.CreateDirectory(userPath);
                    
                    //log notification that new user path was created
                    Log.Write(String.Format("User folder had to be created for operator '{0}' for transaction id '{1}'.",  manifest.OperatorId, manifest.TransactionId), EventLogEntryType.Warning);
                }

                datedFolder = TransactionFolders.GetDatedSubFolderNameFromDate(DateTime.Now);
                datedPath = Path.Combine(userPath, datedFolder);
                if (!Directory.Exists(datedPath))
                {
                    //if dated path does not exist, create it
                    Directory.CreateDirectory(datedPath);
                    
                    //log notification that new dated path was created
                    Log.Write(String.Format("Dated folder '{0}' created for operator '{1}' for transaction id '{2}'.", datedFolder, manifest.OperatorId, manifest.TransactionId), EventLogEntryType.Information);
                }


                //report status
                reportProgressDelegate(String.Format("moving manifest..."));

                //move manifest and data
                packageContentsPackageSubfolderPath = Path.Combine(transactionFolders.WorkingFilePath, DocumentScannerCommon.Package.PACKAGE_FOLDER);
                if
                (
                    !DocumentScannerCommon.PackageManifest.MoveManifestAndData
                    (
                        manifest,
                        packageContentsPackageSubfolderPath,
                        datedPath,
                        DSServerController<DSServerModel>.Model.ReCreateWaitMilliseconds,
                        reportProgressDelegate,
                        ref errorMessage
                    )
                )
                {
                    throw new Exception(String.Format("Manifest move failure: '{0}'", errorMessage));
                }


                returnValue = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }
        #endregion Methods

    }
}
