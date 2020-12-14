using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using Ssepan.Application;
using Ssepan.Collections;
using Ssepan.Compression;
using Ssepan.Io;
using Ssepan.Transaction;
using Ssepan.Utility;
using DocumentScannerCommon;
using TransferServiceClient;
using ManifestServiceClient;
using TwainLib;

namespace DocumentScannerLibrary
{
    /// <summary>
    /// This is the MVC Controller
    /// </summary>
    public class DSController<TModel> : 
        ModelController<TModel>
        where TModel :
            class,
            IModel,
            new()
    {
        #region Declarations
        #endregion Declarations

        #region Constructors
        public DSController()
        {
            ////Init config parameters
            //if (!LoadConfigParameters())
            //{
            //    throw new Exception(String.Format("Unable to load config file parameter(s)."));
            //}
        }
        #endregion Constructors

        #region Properties
        //Note:TModel Model exists in base
        #endregion Properties

        #region Scanner Methods
        /// <summary>
        /// The Controller method for acquiring captured images from Twain device.
        /// Trigger a Twain Transfer of images to a List(Of Image) in ScannedImages, 
        /// which will be handled by a callback attached to ScanFinished event.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="scanFinished"></param>
        /// <returns>Boolean</returns>
        public static Boolean ScanAcquire(IntPtr handle, EventHandler scanFinished)
        {
            Boolean returnValue = default(Boolean);

            try
            {
                if (!OpenTwainSource(handle, scanFinished))
                {
                    throw new Exception("Unable to open twain source for scanning.");
                }

                //scan image(s)
                DSController<DSModel>.Model.TwainSource.AcquireAndTransfer();

                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
            return returnValue;
        }

        /// <summary>
        /// The controller method for processing the scanned List(Of Image),
        /// saving each Image and creating a corresponding ImageFile in
        /// DocumentFiles.
        /// Called by handler for TwainSource_ScanFinished.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean ScanProcess
        (
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            ImageFile imageFile = default(ImageFile);

            try
            {
                if (DSController<DSModel>.Model.TwainSource.ScannedImages.Count > 0)
                {
                    //convert List<Image> to List<ImageFile>
                    foreach (Image image in DSController<DSModel>.Model.TwainSource.ScannedImages)
                    {
                        //save image to disk
                        imageFile = new ImageFile(String.Format("{0}.{1}", Guid.NewGuid().ToString(), ImageFile.IMAGE_FILE_TYPE));
                        if (imageFile.SaveDocumentItem(DSController<DSModel>.GetTransactionImagesPath(false), image))
                        {
                            //check for duplicates in collection; duplicate filenames should have been checked during image save
                            if (SettingsController<Settings>.Settings.Manifest.DocumentFiles.Any(i => i.Filename == imageFile.Filename))
                            {
                                Log.Write(String.Format("Unable to store duplicate image Filename in Settings Document Files: {0}", imageFile.Filename), EventLogEntryType.Error);
                                continue;
                            }

                            _ValueChanging = true;

                            SettingsController<Settings>.Settings.Manifest.DocumentFiles.Add(imageFile);

                            _ValueChanging = false;
                        }
                        else
                        {
                            Log.Write(String.Format("Unable to save image to file: {0}", imageFile.Filename), EventLogEntryType.Error);
                            continue;
                        }
                        imageFile = null;
                    }

                    returnValue = true;
                }
                else
                {
                    errorMessage = String.Format("No images were transferred.");
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            finally
            {
                CloseTwainSource();
                
                _ValueChanging = false;

                //update ui
                DSController<DSModel>.Model.Refresh();
            }
            return returnValue;
        }

        /// <summary>
        /// Controller method to inoke Twain Select UI.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="scanFinished"></param>
        /// <returns>Boolean</returns>
        public static Boolean SelectScanner(IntPtr handle, EventHandler scanFinished)
        {
            Boolean returnValue = default(Boolean);
            try
            {
                if (!OpenTwainSource(handle, scanFinished))
                {
                    throw new Exception("Unable to open twain source for scanning.");
                }

                //select scanner
                DSController<DSModel>.Model.TwainSource.SelectSource();

                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            finally 
            {
                CloseTwainSource();
            }
            return returnValue;
        }

        #endregion

        #region Methods

        #region Document Methods
        /// <summary>
        /// Search until end or until another item found; swap in latter case.
        /// </summary>
        /// <param name="imageFile"></param>
        /// <param name="shiftType"></param>
        public static Boolean ShiftDocumentItem(ImageFile imageFile, ListOfTExtension.ShiftTypes shiftType)
        {
            Boolean returnValue = default(Boolean);

            try
            {
                _ValueChanging = true;

                SettingsController<Settings>.Settings.Manifest.DocumentFiles.ShiftListItem<OrderedEquatableBindingList<ImageFile>, ImageFile>
                    (
                        shiftType, 
                        (item => item.Filename == imageFile.Filename), //match on Filename property
                        (item, swapItem) => true //return true for any item (match on swap item selected)
                    );

                _ValueChanging = false;

                //refresh
                DSController<DSModel>.Model.Refresh();

                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                _ValueChanging = false;
                throw;
            }
            return returnValue;
        }

        /// <summary>
        /// Rotate document image.
        /// </summary>
        /// <param name="imageFile"></param>
        /// <param name="rotateFlipType"></param>

        public static Boolean RotateDocumentItem(ImageFile imageFile, RotateFlipType rotateFlipType)
        {
            Boolean returnValue = default(Boolean);

            try
            {
                _ValueChanging = true;

                imageFile.RotateDocumentItem(GetTransactionImagesPath(false), rotateFlipType);

                _ValueChanging = false;

                //refresh
                DSController<DSModel>.Model.Refresh();

                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                _ValueChanging = false;
                throw;
            }
            return returnValue;
        }
        # endregion Document Methods

        #region Other Methods
        /// <summary>
        /// Return path of transaction image data folder under transaction data path.
        /// </summary>
        /// <param name="forceTransactionId"></param>
        /// <returns></returns>
        public static String GetTransactionImagesPath(Boolean forceTransactionId)
        {
            String returnValue = default(String);
            try
            {
                if (SettingsController<Settings>.Filename == SettingsController<Settings>.FILE_NEW)
                {
                    //file is '(new)' and not saved; filename matches folder
                    returnValue = Path.Combine(DSController<DSModel>.Model.DataPath,SettingsController<Settings>.Filename);
                }
                else if (forceTransactionId)
                {
                    //force transaction id as folder name
                    returnValue = Path.Combine(DSController<DSModel>.Model.DataPath, SettingsController<Settings>.Settings.Manifest.TransactionId);
                }
                else
                {
                    //file is not '(new)' and was saved; transaction id matches folder
                    returnValue = Path.Combine(DSController<DSModel>.Model.DataPath, SettingsController<Settings>.Settings.Manifest.TransactionId);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                //throw;
            }
            return returnValue;
        }

        /// <summary>
        /// Package documents and submit package for transfer.
        /// Should only be called for non-New settings that are not Dirty.
        /// 
        /// Settings (containing manifest data) are in a file named <transaction#>.documentscanner in DSController<DSModel>.Model.DataPath.
        /// Images are in a folder called <transaction#> in DSController<DSModel>.Model.DataPath.
        /// Manifest will be saved in a folder called <transaction#> in DSController<DSModel>.Model.DataPath.
        /// Copy Images folder and Manifest and create Package in a folder called <transaction#> in DSController<DSModel>.Model.DataPath.
        /// Move Package to DSController<DSModel>.Model.PushSendPath.
        /// Delete Images folder, Manifest, and Settings.
        /// Perform New.
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean PackageInBackground
        (
            BackgroundWorker worker,
            DoWorkEventArgs e,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            Single stepsCountComplete = 0;
            Single stepsCountTotal = 6;
            PackageManifest manifest;
            Action<String> reportProgressDelegate = default(Action<String>);

            try
            {
                //tell fill how to report progress
                reportProgressDelegate =
                    (s) =>
                    {
                        worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), s);
                    };


                //extract manifest and data from settings
                //1..2 of 6
                if
                (
                    !ExtractManifestSettings
                    (
                        SettingsController<Settings>.Settings.Manifest.TransactionId,
                        DSController<DSModel>.Model.DataPath, //extract to subfolder in Data folder
                        DSController<DSModel>.Model.ReNewWaitMilliseconds,
                        reportProgressDelegate,
                        ref errorMessage,
                        out manifest
                   )
                )
                {
                    throw new Exception(String.Format("Manifest extraction failure: '{0}'", errorMessage));
                }


                //one last chance to cancel, before point-of-no-return
                if (worker.CancellationPending)
                {
                    worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), "cancelling...");
                    //returnValue is still false for this call
                    e.Cancel = true;
                    return returnValue;
                }


                //zip manifest and images folder to package in transmit folder (as temp file)
                if 
                (
                    !DocumentScannerCommon.Package.FillManifestPackage
                    (
                        manifest,
                        SettingsController<Settings>.Settings.Manifest.TransactionId,
                        DSController<DSModel>.Model.PushSendPath,
                        DSController<DSModel>.Model.DataPath, //create package folder as subfolder in Data folder
                        DSController<DSModel>.Model.ReNewWaitMilliseconds,
                        reportProgressDelegate,
                        ref errorMessage
                    )
                )
                {
                    throw new Exception(String.Format("Package creation failure: '{0}'", errorMessage));
                }

                
                //clean up
                
                //delete images folder
                Folder.DeleteFolderWithWait(DSController<DSModel>.GetTransactionImagesPath(false), DSController<DSModel>.Model.ReNewWaitMilliseconds);
                
                //delete settings and manifest
                System.IO.File.Delete(SettingsController<Settings>.Filename);

                //perform New
                //6 of 6
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "new");
                //prevent change notifications that could update views; refresh views explicitly after this thread ends
                _NoUiOnThisThread = true;
                DSSettingsController.New();
                _NoUiOnThisThread = false;
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "complete");

                returnValue = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), ex.Message);
            }
            finally
            { 
                if (_NoUiOnThisThread)
                {
                    _NoUiOnThisThread = false;
                }
            }
            return returnValue;
        }
        #endregion Other Methods

        #region Send Receive Methods
        /// <summary>
        /// Transfer queued packages.
        /// Should only be called if PackagesQueued is true.
        /// State of settings is not a consideration.
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean SendInBackground
        (
            BackgroundWorker worker,
            DoWorkEventArgs e,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            Boolean accumulatedResult = true;
            Single stepsCountComplete = default(Int32);
            Single stepsCountTotal = 2; //based on package file count, plus 2 steps
            Int32 packageCount = default(Int32);
            Int32 packageCountTotal = default(Int32);
            List<String> packageFilenames = default(List<String>);
            Boolean pushResult = default(Boolean);
            TransactionFolders transactionFolders = default(TransactionFolders);

            try
            {
                //validate package list
                //0 of N + 2
                worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), "validating...");
                packageFilenames = DSController<DSModel>.Model.PackagesQueued;
                packageCountTotal = packageFilenames.Count;
                if (!(packageCountTotal > 0))
                {
                    throw new Exception("No packages queued.");
                }
                
                //steps equals total packages to send plus two additional processing steps
                stepsCountTotal += packageCountTotal;
                
                //1 of N + 2
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "initializing...");

                transactionFolders = 
                    new TransactionFolders
                    (
                        DSController<DSModel>.Model.TransactionRootPath,
                        DSController<DSModel>.Model.TransactionWorkingPath,
                        DSController<DSModel>.Model.TransactionCompletedPath,
                        DSController<DSModel>.Model.TransactionErrorPath,
                        true,
                        true,
                        false,
                        true,
                        true
                    );

                //send each package file in list
                foreach (String packageFilename in packageFilenames)
                {

                    pushResult = default(Boolean);
                    
                    //1 + N(i) of N + 2
                    packageCount++;
                    worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), String.Format("package File: {0} of {1}", packageCount, packageCountTotal));

                    //one chance to cancel per loop
                    //Note:cancellation must leave current transaction in original state
                    if (worker.CancellationPending)
                    {
                        worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), "cancelling...");
                        //pushResult is still false for this iteration
                        //returnValue is still false for this call
                        e.Cancel = true;
                        break; //out of loop and perform transaction cleanup
                    }

                    //start transaction
                    if (!transactionFolders.Begin(packageFilename, ref errorMessage))
                    {
                        throw new Exception(String.Format("Unable to begin package transaction for transfer: {0}\nFilename: {1}", errorMessage, packageFilename));
                    }

                    //push package
                    Log.Write(String.Format("Sending {0}", transactionFolders.WorkingFile), EventLogEntryType.Information);
                    pushResult = 
                        TransferClientBusiness.Transfer.PushFile
                        (
                            transactionFolders.ID, 
                            SettingsController<Settings>.Settings.Manifest.OperatorId, 
                            transactionFolders.WorkingFile, 
                            DSController<DSModel>.Model.FileTransferServiceEndpointConfigurationName, 
                            ref errorMessage
                        );
                    accumulatedResult = accumulatedResult && pushResult;
                    if (!pushResult)
                    {
                        Log.Write(String.Format("DocumentScanner is unable to push package file to Transfer Client Business: {0}\nID: {1}\nFilename: {2}", errorMessage, transactionFolders.ID, transactionFolders.WorkingFile), EventLogEntryType.Error);
                    }
                    else
                    {
                        Log.Write(String.Format("Sent {0}", transactionFolders.WorkingFile), EventLogEntryType.Information);
                    }

                    //finish transaction
                    if (!transactionFolders.End(pushResult, ref errorMessage))
                    {
                        throw new Exception(String.Format("Unable to end package transaction for transfer: {0}\nID: {1}\nFilename: {2}", errorMessage, transactionFolders.ID, transactionFolders.WorkingFile));
                    }
                }

                //report final status
                //N + 2 of N + 2
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "finalizing...");

                //clean up archives
                transactionFolders.TransactionFilename = "*." + /*SettingsController<Settings>.Settings*/SettingsBase.FileTypeExtension; //if Use... flags are clear, a file extension is needed; this may not have been set if no files found to process.
                if 
                (
                    !transactionFolders.CleanUp
                    (
                        new TimeSpan(DSController<DSModel>.Model.CompletedTransactionRetentionDays, 0, 0, 0),
                        new TimeSpan(DSController<DSModel>.Model.ErrorTransactionRetentionDays, 0, 0, 0), 
                        ref errorMessage
                    )
                )
                {
                    throw new Exception(String.Format("Unable to clean up package transaction object for transfer to Transfer Business: {0}\nFilename: {1}", errorMessage, transactionFolders.TransactionFilename));
                }

                transactionFolders = null;

                returnValue = accumulatedResult;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), ex.Message);
            }
            return returnValue;
        }

        /// <summary>
        /// Transfer Available manifest, by current Operator ID and 
        ///  selected Transaction ID.
        /// Triggers receive via web service. After service call returns, and package is present
        ///  in client-side Receive folder, logic proceeds much like UnPackage, except source is 
        ///  Receive instead of Error.
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean ReceiveInBackground
        (
            BackgroundWorker worker,
            DoWorkEventArgs e,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            String operatorId = default(String);
            String transactionId = default(String);
            Tuple<String /*operatorId*/, String /*transactionId*/> arguments = null;
            Single stepsCountComplete = 0;
            Single stepsCountTotal = 8;
            String packageFilename = String.Empty;
            String packageFilePath = String.Empty;
            PackageManifest manifest = default(PackageManifest);
            Action<String> reportProgressDelegate = default(Action<String>);

            try
            {
                worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), "preparing...");

                arguments = e.Argument as Tuple<String /*operatorId*/, String /*transactionId*/>;
                operatorId = arguments.Item1;
                transactionId = arguments.Item2;

                //calculate package file name
                packageFilename = String.Format("{0}.{1}", transactionId, DocumentScannerCommon.Package.PACKAGE_FILE_TYPE);
                packageFilePath = Path.Combine(DSController<DSModel>.Model.PullReceivePath, packageFilename);


                //tell extraction how to report progress
                reportProgressDelegate =
                    (s) =>
                    {
                        worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), s);
                    };


                //trigger receive request via web service
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "receiving package file...");
                Log.Write(String.Format("Receiving Transaction ID: '{0}'", transactionId), EventLogEntryType.Information);
                if 
                (
                    !TransferClientBusiness.Transfer.PullFile
                    (
                        transactionId, 
                        operatorId,
                        packageFilePath, 
                        DSController<DSModel>.Model.FileTransferServiceEndpointConfigurationName, 
                        ref errorMessage
                    )
                )
                {
                    throw new Exception(String.Format("Package receive failure: {0}\nID: {1}", errorMessage, transactionId));
                }
                else
                {
                    Log.Write(String.Format("Received'{0}'", transactionId), EventLogEntryType.Information);
                }


                //unpackage to a package subfolder of the Data path
                if
                (
                    !DocumentScannerCommon.Package.ExtractManifestPackage
                    (
                        transactionId,
                        DSController<DSModel>.Model.PullReceivePath, //location of package file in receive
                        DSController<DSModel>.Model.DataPath, //extract to subfolder in Data folder
                        DSController<DSModel>.Model.ReNewWaitMilliseconds,
                        reportProgressDelegate,
                        ref errorMessage,
                        out manifest
                    )
                )
                {
                    throw new Exception(String.Format("Package extraction failure: '{0}'", errorMessage));
                }


                //one last chance to cancel, before point-of-no-return
                if (worker.CancellationPending)
                {
                    worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), "cancelling...");
                    e.Cancel = true;
                    //returnValue is still false for this call
                    return returnValue;
                }


                //load settings with manifest from a package subfolder of the Data path
                if
                (
                    !FillManifestSettings
                    (
                        manifest,
                        transactionId,
                        DSController<DSModel>.Model.DataPath, //extract to subfolder in Data folder
                        DSController<DSModel>.Model.ReNewWaitMilliseconds,
                        reportProgressDelegate,
                        ref errorMessage
                    )
                )
                {
                    throw new Exception(String.Format("Manifest storage failure: '{0}'", errorMessage));
                }

                //delete package file in receive folder
                System.IO.File.Delete(packageFilePath);


                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "complete");

                returnValue = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }
        #endregion Send Receive Methods

        #region Manifest Methods
        /// <summary>
        /// Confirm Manifests.
        /// State of settings is not a consideration.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="date"></param>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static List<PackageManifest> ConfirmManifestsInBackground
        (
            String operatorId,
            DateTime date,
            BackgroundWorker worker,
            DoWorkEventArgs e,
            ref String errorMessage
        )
        {
            List<PackageManifest> returnValue = default(List<PackageManifest>);
            Single stepsCountComplete = default(Int32);
            Single stepsCountTotal = 1; //based on single call to service

            try
            {
                //report initial status
                worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), "querying...");
                
                returnValue =
                    ManifestsConfirmed
                    (
                        operatorId,
                        date,
                        ref errorMessage
                    );

                if ((returnValue == null) || ((errorMessage != null) && (errorMessage != String.Empty)))
                {
                    throw new Exception(errorMessage);
                }

                //report final status
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "done.");
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), ex.Message);
            }
            return returnValue;
        }

        /// <summary>
        /// Available Manifests.
        /// State of settings is not a consideration.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static List<PackageManifest> AvailableManifestsInBackground
        (
            String operatorId,
            BackgroundWorker worker,
            DoWorkEventArgs e,
            ref String errorMessage
        )
        {
            List<PackageManifest> returnValue = default(List<PackageManifest>);
            Single stepsCountComplete = default(Int32);
            Single stepsCountTotal = 1; //based on single call to service

            try
            {
                //report initial status
                worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), "querying...");

                returnValue =
                    ManifestsAvailable
                    (
                        operatorId,
                        ref errorMessage
                    );

                if ((returnValue == null) || ((errorMessage != null) && (errorMessage != String.Empty)))
                {
                    throw new Exception(errorMessage);
                }

                //report final status
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "done.");
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), ex.Message);
            }
            return returnValue;
        }
        #endregion Manifest Methods

        #region Package Methods
        /// <summary>
        /// UnPackage documents and manifest so that they can be edited.
        /// Should only be called for packages that failed to Send.
        /// 
        /// Note: if the manifest and images were internally inconsistent,
        /// then they never would have completed packaging, so all validation
        /// checks should pass here as well.
        /// 
        /// Create a new setings object.
        /// Prepare a directory in the data folder called 'package'.
        /// Unzip from the dated error folder to the package folder.
        /// Load the manifest from the package folder into the settings object.
        /// Copy the images subfolder from the package folder into the data folder.
        /// Perform a Save of the settings, which will rename the image fodler 
        ///  from new to the transaction id, and will write the settings file.
        /// Delete the package file from the dated error folder, and delete the package folder.
        /// Perform a final validation of the settings.
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean UnPackageInBackground
        (
            BackgroundWorker worker,
            DoWorkEventArgs e,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            String manifestFilename = String.Empty;
            String manifestFilePath = String.Empty;
            String dataFilename = String.Empty;
            String dataFilePath = String.Empty;
            String packageImagesFolderPath = String.Empty;
            String packagePath = String.Empty;
            String packageId = String.Empty;
            String packageFilePath = String.Empty;
            Single stepsCountComplete = 0;
            Single stepsCountTotal = 6;
            Tuple<String /*name*/, String /*filename*/, String /*filePath*/> arguments = null;
            PackageManifest manifest = default(PackageManifest);
            Action<String> reportProgressDelegate = default(Action<String>);

            try
            {
                arguments = (Tuple<String /*name*/, String /*filename*/, String /*filePath*/>)e.Argument;
                packageId = arguments.Item1;
                packageFilePath = arguments.Item3;

                //tell extraction how to report progress
                reportProgressDelegate =
                    (s) =>
                    {
                        worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), s);
                    };

                
                //unpackage to a package subfolder of the Data path
                //0..1 of 6
                if
                (
                    !DocumentScannerCommon.Package.ExtractManifestPackage
                    (
                        packageId,
                        Path.GetDirectoryName(packageFilePath), //location of package file
                        DSController<DSModel>.Model.DataPath, //extract to subfolder in Data folder
                        DSController<DSModel>.Model.ReNewWaitMilliseconds,
                        reportProgressDelegate,
                        ref errorMessage,
                        out manifest
                    )
                )
                {
                    throw new Exception(String.Format("Package extraction failure: '{0}'", errorMessage));
                }


                //one last chance to cancel, before point-of-no-return
                if (worker.CancellationPending)
                {
                    worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), "cancelling...");
                    e.Cancel = true;
                    //returnValue is still false for this call
                    return returnValue;
                }


                //load settings with manifest from a package subfolder of the Data path
                //2..6 of 6
                if
                (
                    !FillManifestSettings
                    (
                        manifest,
                        packageId,
                        DSController<DSModel>.Model.DataPath, //extract to subfolder in Data folder
                        DSController<DSModel>.Model.ReNewWaitMilliseconds,
                        reportProgressDelegate,
                        ref errorMessage
                    )
                )
                {
                    throw new Exception(String.Format("Manifest storage failure: '{0}'", errorMessage));
                }

                //delete package file in error folder
                System.IO.File.Delete(packageFilePath);


                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "complete");

                returnValue = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), ex.Message);
            }
            return returnValue;
        }

        /// <summary>
        /// Split package settings into two separate sets of settings file and images folder 
        /// Should only be called for saved (not dirty) settings.
        /// 
        /// Clone the manifest object from settings. The original will hold items below the split, 
        ///  and retain the settings filename and transaction id. The copy will hold items above the split, 
        ///  and will be given a new settings filename and transaction id.
        /// Adjust the descriptions of each.
        /// In the original, delete DocumentFiles items belonging to the copy.
        /// Save the original.
        /// Create a new settings instance for the copy. Store the new transaction if in the copy, 
        ///  and store the copy in the settings.
        /// In the copy, delete DocumentFiles items belonging to the original.
        /// Save the copy, and create the copy's images folder.
        /// Move the copy's images from the original's images folder to the copy's.
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean SplitPackageSettingsInBackground
        (
            BackgroundWorker worker,
            DoWorkEventArgs e,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            String dataFilename = String.Empty;
            String dataFilePath = String.Empty;
            String imagesFolderPath = String.Empty;
            Single stepsCountComplete = 0;
            Single stepsCountTotal = 6;
            PackageManifest manifestA = default(PackageManifest);
            Int32 beforeIndex = 0;
            Int32 afterIndex = 0;
            Tuple<PackageManifest /*manifest*/, Int32 /*beforeIndex*/, Int32 /*afterIndex*/> arguments = null;
            PackageManifest manifestB = default(PackageManifest);
            String sourceDocumentFolder = String.Empty;
            String sourceDocumentFilePath = String.Empty;
            String destinationDocumentFolder = String.Empty;
            String destinationDocumentFilePath = String.Empty;
            List<ImageFile> tempList = default(List<ImageFile>);

            try
            {
                arguments = (Tuple<PackageManifest /*manifest*/, Int32 /*beforeIndex*/, Int32 /*afterIndex*/>)e.Argument;
                manifestA = arguments.Item1;
                beforeIndex = arguments.Item2;
                afterIndex = arguments.Item3;

                //last chance to cancel before point of no return
                if (worker.CancellationPending)
                {
                    worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), "cancelling...");
                    //returnValue is still false for this call
                    e.Cancel = true;
                    return returnValue;
                }

                _NoUiOnThisThread = true;
                
                //clone original manifest (manifest a) to create manifest b; 
                //a will be items after split, b will be items before split.
                //0 of 6
                worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), "copying manifest");
                manifestB = ObjectHelper.Clone<PackageManifest>(manifestA);

                //adjust manifest descriptions in a
                manifestA.Description += " (below)";

                //adjust manifest descriptions in b
                manifestB.Description += " (above)";

                //in manifest a, for each document item from 0 to indexBefore, delete item
                //1 of 6
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "removing duplicates in original");
                tempList = manifestA.DocumentFiles.ToList<ImageFile>();
                tempList.RemoveRange(0, beforeIndex + 1);
                SettingsController<Settings>.Settings.SetDocumentFilesListChangedDelegate();
                manifestA.DocumentFiles = tempList.ToOrderedEquatableBindingList<ImageFile>();
                tempList = null;
                
                //save settings w/ manifest a
                //2 of 6
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "saving original");
                DSSettingsController.Save();
                sourceDocumentFolder = DSController<DSModel>.GetTransactionImagesPath(false);

                //new settings for b, will generate new GUID 
                //assign new GUID to manifest b transaction id
                //assign manifest b to manifest property, 
                //3 of 6
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "new settings for copy");
                DSSettingsController.New();
                manifestB.TransactionId = SettingsController<Settings>.Settings.Manifest.TransactionId;
                SettingsController<Settings>.Settings.Manifest = manifestB;

                //in manifest b, for each document item from indexAfter to count-1, delete item
                //4 of 6
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "removing duplicates from copy");
                tempList = manifestB.DocumentFiles.ToList<ImageFile>();
                tempList.RemoveRange(afterIndex, manifestB.DocumentFiles.Count - afterIndex);
                SettingsController<Settings>.Settings.SetDocumentFilesListChangedDelegate();
                manifestB.DocumentFiles = tempList.ToOrderedEquatableBindingList<ImageFile>();
                tempList = null;
                
                //save settings as <transactionid>.documentscanner;
                //this will create images folder for manifest b as new manifest b guid
                //5 of 6
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "saving copy");
                dataFilename = String.Format("{0}.{1}", manifestB.TransactionId, /*SettingsController<Settings>.Settings*/SettingsBase.FileTypeExtension);
                dataFilePath = Path.Combine(DSController<DSModel>.Model.DataPath, dataFilename);
                SettingsController<Settings>.Filename = dataFilePath;
                DSSettingsController.Save();

                //in manifest b, for each document item from 0 to indexBefore, 
                //move image file from manifest a images folder to manifest b images folder
                //6 of 6
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "moving copy's images");
                destinationDocumentFolder = DSController<DSModel>.GetTransactionImagesPath(false);
                foreach (ImageFile imageFile in manifestB.DocumentFiles)
                {
                    sourceDocumentFilePath = Path.Combine(sourceDocumentFolder, imageFile.Filename);
                    destinationDocumentFilePath = Path.Combine(destinationDocumentFolder, imageFile.Filename);

                    System.IO.File.Move(sourceDocumentFilePath, destinationDocumentFilePath);
                }

                _NoUiOnThisThread = false;

                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "complete");

                returnValue = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), ex.Message);
            }
            finally
            {
                if (_NoUiOnThisThread)
                {
                    _NoUiOnThisThread = false;
                }
            }
            return returnValue;
        }
        #endregion Package Methods

        #region Menu Methods
        ///// <summary>
        ///// Custom override of SettingsController(Of TSettings).New(); manages data file and data folder.
        ///// </summary>
        //public static void New()
        //{//TODO: override SettingsController<Settings> and call that
        //    Action postNewDelegate = default(Action);

        //    try
        //    {
        //        //Optional delegate to be run after Save (in ModelControllerBase) but before Refresh.
        //        postNewDelegate =
        //            () =>
        //            {
        //                String folderPath = default(String);

        //                //check for folder and delete if present
        //                folderPath = Path.Combine(DSController<DSModel>.Model.DataPath, SettingsController<Settings>.FILE_NEW);

        //                //check for folder and delete if present
        //                Folder.DeleteFolderWithWait(folderPath, DSController<DSModel>.Model.ReNewWaitMilliseconds);

        //                //check for folder and create if missing
        //                if (!Directory.Exists(folderPath))
        //                {
        //                    Directory.CreateDirectory(folderPath);
        //                }
        //            };

        //        SettingsController<Settings>.New();

        //        postNewDelegate();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
        //    }
        //}

        ///// <summary>
        ///// Custom override of Open; provides for setting oldfilename to filename
        ///// </summary>
        //public static void Open()
        //{//TODO: override SettingsController<Settings> and call that
        //    Action postOpenDelegate = default(Action);

        //    try
        //    {
        //        //Optional delegate to be run after Save (in ModelControllerBase) but before Refresh.
        //        postOpenDelegate =
        //            () =>
        //            {
        //                //force new Filename into OldFilename
        //               SettingsController<Settings>.Filename = SettingsController<Settings>.Filename;
        //            };

        //        //Open was not synchronizing OldFilename with Filename, and Save logic was seeing '(new)'
        //        //and performing wrong actions.
        //        //Note:this may be fixed in new Settings architecture (Ssepan.* v2.6) anyway...--SJS
        //        SettingsController<Settings>.Open();

        //        postOpenDelegate();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
        //    }
        //}

        ///// <summary>
        ///// Custom override of Save; provides delegate for managing
        ///// data file and data folder.
        ///// </summary>
        //public static void Save()
        //{//TODO: override SettingsController<Settings> and call that
        //    Action postSaveDelegate = default(Action);

        //    try
        //    {
        //        //Optional delegate to be run after Save (in ModelControllerBase) but before Refresh.
        //        postSaveDelegate =
        //            () =>
        //            {
        //                String oldFolderPath_ = default(String);
        //                String newFolderPath_ = default(String);
        //                String filePath = default(String);

        //                if (SettingsController<Settings>.OldFilename.ToUpper() !=SettingsController<Settings>.Filename.ToUpper())
        //                {

        //                    if (SettingsController<Settings>.OldFilename == SettingsController<Settings>.FILE_NEW)
        //                    {
        //                        //source will be (new)
        //                        //if (new), then rename folder to transaction's id
        //                        oldFolderPath_ = Path.Combine(DSController<DSModel>.Model.DataPath, SettingsController<Settings>.OldFilename);
        //                        //force transaction id for destination
        //                        newFolderPath_ = DSController<DSModel>.GetTransactionImagesPath(true);
        //                        Directory.Move(oldFolderPath_, newFolderPath_);
        //                    }
        //                    else //(oldFilename != SettingsController<Settings>.FILE_NEW) 
        //                    {
        //                        //if not (new), then delete original data file
        //                        //filename is different; delete original because it has same transaction #
        //                        filePath = Path.Combine(DSController<DSModel>.Model.DataPath, SettingsController<Settings>.OldFilename);
        //                        System.IO.File.Delete(filePath);
        //                    }
        //                    //force new Filename into OldFilename
        //                   SettingsController<Settings>.Filename = SettingsController<Settings>.Filename;
        //                }
        //            };

        //        //Call to controller base Save() writes xml and triggers refresh...
        //        //...but refresh was seeing non-new transaction and getting wrong transaction path...
        //        // ... because folder was still 'new' until Move().
        //        SettingsController<Settings>.Save();

        //        postSaveDelegate();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
        //    }
        //}

        /// <summary>
        /// The Controller method for copying a document to the clipboard.
        /// </summary>
        public static void CopyToClipboard()
        {
            try
            {
                throw new NotImplementedException("CopyToClipboard not implemented");
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        /// <summary>
        /// The Controller method for printing a document.
        /// </summary>
        public static void Print(PrintDocument printDocument, Image image)
        {
            PrintPageEventHandler printPageDelegate = default(PrintPageEventHandler);

            try
            {
                //delegate to be run by PrintDocument PrintPage event.
                printPageDelegate =
                    (o, e) =>
                    {
                        // Draw a picture.
                        e.Graphics.DrawImage(image, e.Graphics.VisibleClipBounds);

                        // Indicate that this is the last page to print.
                        e.HasMorePages = false;
                    };

                //assign event haldler delegate to PrintDocument
                printDocument.PrintPage += printPageDelegate;

                // Assumes the default printer.
                printDocument.Print();
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        ///// <summary>
        ///// The Controller method for triggering update notifications to the model.
        ///// </summary>
        //public static void Refresh()
        //{
        //    try
        //    {
        //        DSController<DSModel>.Model.Refresh();//Value doesn't matter; a changed fire event;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
        //        throw;
        //    }
        //}
        #endregion Menu Methods


        #region Manifest Service

        //TODO:move to ManifestClientBusiness; no callers to redirect
        /// <summary>
        /// Call Service Ping test
        /// </summary>
        /// <param name="endpointConfigurationName"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean PingManifestService(String endpointConfigurationName, ref String errorMessage)
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

            try
            {
                //call service to receive list
                ManifestService.EndpointConfigurationName = DSController<DSModel>.Model.PackageManifestServiceEndpointConfigurationName;
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

            try
            {
                //call service to receive list
                ManifestService.EndpointConfigurationName = DSController<DSModel>.Model.PackageManifestServiceEndpointConfigurationName;
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
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static List<PackageManifest> ManifestsAvailable
        (
            String operatorId,
            ref String errorMessage
        )
        {
            List<PackageManifest> returnValue = default(List<PackageManifest>);

            try
            {
                //call service to receive list
                ManifestService.EndpointConfigurationName = DSController<DSModel>.Model.PackageManifestServiceEndpointConfigurationName;
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
        //TODO:is this call-layer needed? can endpoint config be done by caller (also in ctrlr) (2009)
        //TODO:No, this layer belongs in as-yet unwritten ManifestClientBusiness (2020)
        //TODO:move to ManifestClientBusiness; ? caller to redirect
        /// <summary>
        /// Perform business logic for client side of document query; receive list as-is.
        ///Given the Operator ID, a Transaction ID, and the specified date, 
        ///return a List(Of ImageFile) from the server.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="transactionId"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static List<ImageFile> DocumentsAvailable
        (
            String operatorId,
            String transactionId,
            ref String errorMessage
        )
        {
            List<ImageFile> returnValue = default(List<ImageFile>);

            try
            {
                //call service to receive list
                ManifestService.EndpointConfigurationName = DSController<DSModel>.Model.PackageManifestServiceEndpointConfigurationName;
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
        #endregion Manifest Service

        #region private methods
        /// <summary>
        /// Before we try to select a source or scan a document,
        ///  the twain source must be initialized with a) the handle of the UI form, and
        ///  b) the call-back method to be used when a scan transfer has completed.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="scanFinished"></param>
        /// <returns></returns>
        private static Boolean OpenTwainSource(IntPtr handle, EventHandler scanFinished)
        {
            Boolean returnValue = default(Boolean);
            try
            {
                DSController<DSModel>.Model.TwainSource = new TwainSource(handle, scanFinished);

                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }
        /// <summary>
        /// After selecting a source or scan a document,
        ///  the twain source can be disposed, removing a) the handle of the UI form, and
        ///  b) the call-back method to be used when a scan transfer has completed.
        /// </summary>
        ///// <param name="handle"></param>
        ///// <param name="scanFinished"></param>
        /// <returns></returns>
        private static Boolean CloseTwainSource(/*IntPtr handle, EventHandler scanFinished*/)
        {
            Boolean returnValue = default(Boolean);
            try
            {
                if (DSController<DSModel>.Model.TwainSource != null)
                {
                    DSController<DSModel>.Model.TwainSource.Dispose();
                }

                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }


        ///// <summary>
        ///// Load from app config
        ///// </summary>
        ///// <returns></returns>
        //private Boolean LoadConfigParameters()
        //{
        //    Boolean returnValue = default(Boolean);
        //    String _fileTransferServiceEndpointConfigurationName = default(String);
        //    String _packageManifestServiceEndpointConfigurationName = default(String);
        //    //Int32 _imageQualityPercent = default(Int32);
        //    Boolean _autoNavigateTabs = default(Boolean);
        //    Int32 _reNewWaitMilliseconds = default(Int32);
        //    String _dataPath = default(String);
        //    String _pushSendPath = default(String);
        //    String _pullReceivePath = default(String);
        //    Int32 _completedTransactionRetentionDays = default(Int32);
        //    Int32 _errorTransactionRetentionDays = default(Int32);

        //    try
        //    {
        //        if (!Configuration.ReadString("FileTransferServiceEndpointConfigurationName", out _fileTransferServiceEndpointConfigurationName))
        //        {
        //            throw new Exception(String.Format("Unable to load FileTransferServiceEndpointConfigurationName: '{0}'", _fileTransferServiceEndpointConfigurationName));
        //        }
        //        DSController<DSModel>.Model.FileTransferServiceEndpointConfigurationName = _fileTransferServiceEndpointConfigurationName;

        //        if (!Configuration.ReadString("PackageManifestServiceEndpointConfigurationName", out _packageManifestServiceEndpointConfigurationName))
        //        {
        //            throw new Exception(String.Format("Unable to load PackageManifestServiceEndpointConfigurationName: '{0}'", _packageManifestServiceEndpointConfigurationName));
        //        }
        //        DSController<DSModel>.Model.PackageManifestServiceEndpointConfigurationName = _packageManifestServiceEndpointConfigurationName;

        //        //if (!Configuration.ReadValue<Int32>("ImageQualityPercent", out _imageQualityPercent))
        //        //{
        //        //    throw new Exception(String.Format("Unable to load ImageQualityPercent: '{0}'", _imageQualityPercent));
        //        //}
        //        //DSController<DSModel>.Model.ImageQualityPercent = _imageQualityPercent;

        //        if (!Configuration.ReadValue<Boolean>("AutoNavigateTabs", out _autoNavigateTabs))
        //        {
        //            throw new Exception(String.Format("Unable to load AutoNavigateTabs: '{0}'", _autoNavigateTabs));
        //        }
        //        DSController<DSModel>.Model.AutoNavigateTabs = _autoNavigateTabs;
                
        //        if (!Configuration.ReadValue<Int32>("ReNewWaitMilliseconds", out _reNewWaitMilliseconds))
        //        {
        //            throw new Exception(String.Format("Unable to load ReNewWaitMilliseconds: '{0}'", _reNewWaitMilliseconds));
        //        }
        //        DSController<DSModel>.Model.ReNewWaitMilliseconds = _reNewWaitMilliseconds;

        //        if (!Configuration.ReadString("DataPath", out _dataPath))
        //        {
        //            throw new Exception(String.Format("Unable to load DataPath: '{0}'", _dataPath));
        //        }
        //        DSController<DSModel>.Model.DataPath = _dataPath;
                
        //        if (!Configuration.ReadString("PushSendPath", out _pushSendPath))
        //        {
        //            throw new Exception(String.Format("Unable to load PushSendPath: '{0}'", _pushSendPath));
        //        }
        //        DSController<DSModel>.Model.PushSendPath = _pushSendPath;

        //        if (!Configuration.ReadString("PullReceivePath", out _pullReceivePath))
        //        {
        //            throw new Exception(String.Format("Unable to load PullReceivePath: '{0}'", _pullReceivePath));
        //        }
        //        DSController<DSModel>.Model.PullReceivePath = _pullReceivePath;

        //        if (!Configuration.ReadValue<Int32>("CompletedTransactionRetentionDays", out _completedTransactionRetentionDays))
        //        {
        //            throw new Exception(String.Format("Unable to load CompletedTransactionRetentionDays: '{0}'", _completedTransactionRetentionDays));
        //        }
        //        DSController<DSModel>.Model.CompletedTransactionRetentionDays = _completedTransactionRetentionDays;

        //        if (!Configuration.ReadValue<Int32>("ErrorTransactionRetentionDays", out _errorTransactionRetentionDays))
        //        {
        //            throw new Exception(String.Format("Unable to load ErrorTransactionRetentionDays: '{0}'", _errorTransactionRetentionDays));
        //        }
        //        DSController<DSModel>.Model.ErrorTransactionRetentionDays = _errorTransactionRetentionDays;

        //        returnValue = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
        //        //throw;
        //    }
        //    return returnValue;
        //}

        /// <summary>
        /// Embed manifest into a new settigns file.
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="packageId"></param>
        /// <param name="packageContentsRootPath"></param>
        /// <param name="deleteWaitMilliseconds"></param>
        /// <param name="progressDelegate"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private static Boolean FillManifestSettings
        (
            PackageManifest manifest,
            String packageId,
            String packageContentsRootPath,
            Int32 deleteWaitMilliseconds,
            Action<String> progressDelegate,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            String imagesSourcePath = default(String);
            String imagesDestinationPath = default(String);
            String manifestSourceFilename = default(String);
            String manifestSourceFilePath = default(String);
            String packageContentsPackageSubfolderPath = default(String);
            String dataFilename = String.Empty;
            String dataFilePath = String.Empty;

            try
            {
                //report status
                progressDelegate(String.Format("new settings..."));

                //perform New
                //prevent change notifications that could update views; refresh views explicitly after this thread ends
                _NoUiOnThisThread = true;
                DSSettingsController.New();
                _NoUiOnThisThread = false;


                //report status
                progressDelegate(String.Format("moving images..."));

                //package directory used for unzipping
                packageContentsPackageSubfolderPath = Path.Combine(packageContentsRootPath, DocumentScannerCommon.Package.PACKAGE_FOLDER);
                //open manifest from package path to construct settings (need to open manifest without validating, yet)
                manifestSourceFilename = String.Format("{0}.{1}", packageId, DocumentScannerCommon.PackageManifest.DATA_FILE_TYPE);
                manifestSourceFilePath = Path.Combine(packageContentsPackageSubfolderPath, manifestSourceFilename);
                
                //load manifest into settings, so transaction images path is available
                _NoUiOnThisThread = true;
                SettingsController<Settings>.Settings.Manifest = PackageManifest.Load(manifestSourceFilePath);
                _NoUiOnThisThread = false;

                //copy images folder from package path to Data folder
                imagesSourcePath = Path.Combine(packageContentsPackageSubfolderPath, packageId);
                imagesDestinationPath = DSController<DSModel>.GetTransactionImagesPath(false); //this requires open manifest in new or named settings
                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory
                (
                    imagesSourcePath,
                    imagesDestinationPath
                );


                //report status
                progressDelegate(String.Format("saving settings..."));
                
                //save package and clean up
                //prevent change notifications that could update views; refresh views explicitly after this thread ends
                dataFilename = String.Format("{0}.{1}", packageId, /*SettingsController<Settings>.Settings*/SettingsBase.FileTypeExtension);
                dataFilePath = Path.Combine(packageContentsRootPath, dataFilename);
               SettingsController<Settings>.Filename = dataFilePath;
                _NoUiOnThisThread = true;
                DSSettingsController.Save();
                _NoUiOnThisThread = false;


                //report status
                progressDelegate(String.Format("cleaning up..."));

                //delete package directory used for unzipping
                Folder.DeleteFolderWithWait(packageContentsPackageSubfolderPath, deleteWaitMilliseconds);


                //report status
                progressDelegate(String.Format("validating..."));

                //Perform validation
                //re-validate manifest as part of settings
                if (!SettingsController<Settings>.Settings.Valid())
                {
                    throw new ApplicationException(String.Format("{0}", SettingsController<Settings>.Settings.ErrorMessage));
                }

                //validate settings determined at the time of the submit
                if (!SettingsController<Settings>.Settings.Complete())
                {
                    throw new ApplicationException(String.Format("{0}", SettingsController<Settings>.Settings.ErrorMessage));
                }

                //settings should not be Dirty
                if (SettingsController<Settings>.Settings.Dirty)
                {
                    throw new ApplicationException(String.Format("There are unsaved changes: '{0}'",SettingsController<Settings>.Filename));
                }


                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            finally
            {
                if (_NoUiOnThisThread)
                {
                    _NoUiOnThisThread = false;
                }
            }
            return returnValue;
        }

        private static Boolean ExtractManifestSettings
        (
            String packageId,
            String packageContentsRootPath,
            Int32 deleteWaitMilliseconds,
            Action<String> progressDelegate,
            ref String errorMessage,
            out PackageManifest manifest
        )
        {
            Boolean returnValue = default(Boolean);
            String imagesSourcePath = default(String);
            String imagesDestinationPath = default(String);
            String manifestFilename = default(String);
            String manifestFilePath = default(String);
            String packageContentsPackageSubfolderPath = default(String);

            manifest = default(PackageManifest);

            try
            {
                //report status
                progressDelegate(String.Format("validating settings..."));

                //validate settings entered by user
                if (!SettingsController<Settings>.Settings.Valid())
                {
                    throw new ApplicationException(String.Format("{0}", SettingsController<Settings>.Settings.ErrorMessage));
                }

                //validate settings determined at the time of the submit
                if (!SettingsController<Settings>.Settings.Complete())
                {
                    throw new ApplicationException(String.Format("{0}", SettingsController<Settings>.Settings.ErrorMessage));
                }

                //settings should not be Dirty
                if (SettingsController<Settings>.Settings.Dirty)
                {
                    throw new ApplicationException(String.Format("There are unsaved changes: '{0}'",SettingsController<Settings>.Filename));
                }
                
                
                //report status
                progressDelegate(String.Format("preparing package folder..."));

                //prepare package subfolder
                packageContentsPackageSubfolderPath = Path.Combine(packageContentsRootPath, DocumentScannerCommon.Package.PACKAGE_FOLDER);
                if (Directory.Exists(packageContentsPackageSubfolderPath))
                {
                    Folder.DeleteFolderWithWait(packageContentsPackageSubfolderPath, deleteWaitMilliseconds);
                }
                if (!Directory.Exists(packageContentsPackageSubfolderPath))
                {
                    Directory.CreateDirectory(packageContentsPackageSubfolderPath);
                }
                
                
                //report status
                progressDelegate(String.Format("extracting manifest and images..."));

                //save only settings necessary to construct manifest to package path
                manifestFilename = String.Format("{0}.{1}", packageId, DocumentScannerCommon.PackageManifest.DATA_FILE_TYPE);
                manifestFilePath = Path.Combine(packageContentsPackageSubfolderPath, manifestFilename);
                PackageManifest.Save(SettingsController<Settings>.Settings.Manifest, manifestFilePath);

                //copy images folder to package path
                imagesSourcePath = DSController<DSModel>.GetTransactionImagesPath(false);
                imagesDestinationPath = Path.Combine(packageContentsPackageSubfolderPath, packageId);
                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory
                (
                    imagesSourcePath,
                    imagesDestinationPath
                );


                //report status
                progressDelegate(String.Format("validating manifest..."));

                //load manifest
                manifest = PackageManifest.Load(manifestFilePath);

                //validate manifest
                if (!manifest.Valid(imagesDestinationPath))
                {
                    throw new Exception(String.Format("Manifest '{0}' invalid: \n'{1}'", manifest.TransactionId, manifest.ErrorMessage));
                }

                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }
        #endregion private methods
        #endregion Methods
    }
}
