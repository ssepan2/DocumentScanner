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
using Ssepan.Application.MVC;
using Ssepan.Collections;
using Ssepan.Compression;
using Ssepan.Io;
using Ssepan.Transaction;
using Ssepan.Utility;
using DocumentScannerCommon;
using ManifestClientBusiness;
using TransferClientBusiness;
using TwainLib;

namespace DocumentScannerLibrary.MVC
{
    /// <summary>
    /// This is the MVC Controller
    /// </summary>
    public class DSClientModelController<TModel> : 
        ModelController<TModel>
        where TModel :
            class,
            IModel,
            new()
    {
        #region Declarations
        #endregion Declarations

        #region Constructors
        public DSClientModelController()
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
                DSClientModelController<DSClientModel>.Model.TwainSource.AcquireAndTransfer();

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
                if (DSClientModelController<DSClientModel>.Model.TwainSource.ScannedImages.Count > 0)
                {
                    //convert List<Image> to List<ImageFile>
                    foreach (Image image in DSClientModelController<DSClientModel>.Model.TwainSource.ScannedImages)
                    {
                        //save image to disk
                        imageFile = new ImageFile(String.Format("{0}.{1}", Guid.NewGuid().ToString(), ImageFile.IMAGE_FILE_TYPE));
                        if (imageFile.SaveDocumentItem(DSClientModelController<DSClientModel>.GetTransactionImagesPath(false), image))
                        {
                            //check for duplicates in collection; duplicate filenames should have been checked during image save
                            if (SettingsController<DSClientSettings>.Settings.Manifest.DocumentFiles.Any(i => i.Filename == imageFile.Filename))
                            {
                                Log.Write(String.Format("Unable to store duplicate image Filename in DSClientSettings Document Files: {0}", imageFile.Filename), EventLogEntryType.Error);
                                continue;
                            }

                            _ValueChanging = true;

                            SettingsController<DSClientSettings>.Settings.Manifest.DocumentFiles.Add(imageFile);

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
                DSClientModelController<DSClientModel>.Model.Refresh();
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
                DSClientModelController<DSClientModel>.Model.TwainSource.SelectSource();

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

                SettingsController<DSClientSettings>.Settings.Manifest.DocumentFiles.ShiftListItem<OrderedEquatableBindingList<ImageFile>, ImageFile>
                    (
                        shiftType, 
                        (item => item.Filename == imageFile.Filename), //match on Filename property
                        (item, swapItem) => true //return true for any item (match on swap item selected)
                    );

                _ValueChanging = false;

                //refresh
                DSClientModelController<DSClientModel>.Model.Refresh();

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
                DSClientModelController<DSClientModel>.Model.Refresh();

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
                if (SettingsController<DSClientSettings>.Filename == SettingsController<DSClientSettings>.FILE_NEW)
                {
                    //file is '(new)' and not saved; filename matches folder
                    returnValue = Path.Combine(DSClientModelController<DSClientModel>.Model.DataPath,SettingsController<DSClientSettings>.Filename);
                }
                else if (forceTransactionId)
                {
                    //force transaction id as folder name
                    returnValue = Path.Combine(DSClientModelController<DSClientModel>.Model.DataPath, SettingsController<DSClientSettings>.Settings.Manifest.TransactionId);
                }
                else
                {
                    //file is not '(new)' and was saved; transaction id matches folder
                    returnValue = Path.Combine(DSClientModelController<DSClientModel>.Model.DataPath, SettingsController<DSClientSettings>.Settings.Manifest.TransactionId);
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
        /// DSClientSettings (containing manifest data) are in a file named <transaction#>.documentscanner in DSClientModelController<DSClientModel>.Model.DataPath.
        /// Images are in a folder called <transaction#> in DSClientModelController<DSClientModel>.Model.DataPath.
        /// Manifest will be saved in a folder called <transaction#> in DSClientModelController<DSClientModel>.Model.DataPath.
        /// Copy Images folder and Manifest and create Package in a folder called <transaction#> in DSClientModelController<DSClientModel>.Model.DataPath.
        /// Move Package to DSClientModelController<DSClientModel>.Model.PushSendPath.
        /// Delete Images folder, Manifest, and DSClientSettings.
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
                        SettingsController<DSClientSettings>.Settings.Manifest.TransactionId,
                        DSClientModelController<DSClientModel>.Model.DataPath, //extract to subfolder in Data folder
                        DSClientModelController<DSClientModel>.Model.ReNewWaitMilliseconds,
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
                        SettingsController<DSClientSettings>.Settings.Manifest.TransactionId,
                        DSClientModelController<DSClientModel>.Model.PushSendPath,
                        DSClientModelController<DSClientModel>.Model.DataPath, //create package folder as subfolder in Data folder
                        DSClientModelController<DSClientModel>.Model.ReNewWaitMilliseconds,
                        reportProgressDelegate,
                        ref errorMessage
                    )
                )
                {
                    throw new Exception(String.Format("Package creation failure: '{0}'", errorMessage));
                }

                
                //clean up
                
                //delete images folder
                Folder.DeleteFolderWithWait(DSClientModelController<DSClientModel>.GetTransactionImagesPath(false), DSClientModelController<DSClientModel>.Model.ReNewWaitMilliseconds);
                
                //delete settings and manifest
                System.IO.File.Delete(SettingsController<DSClientSettings>.Filename);

                //perform New
                //6 of 6
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "new");
                //prevent change notifications that could update views; refresh views explicitly after this thread ends
                _NoUiOnThisThread = true;
                DSClientSettingsController.New();
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
                packageFilenames = DSClientModelController<DSClientModel>.Model.PackagesQueued;
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
                        DSClientModelController<DSClientModel>.Model.TransactionRootPath,
                        DSClientModelController<DSClientModel>.Model.TransactionWorkingPath,
                        DSClientModelController<DSClientModel>.Model.TransactionCompletedPath,
                        DSClientModelController<DSClientModel>.Model.TransactionErrorPath,
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
                            SettingsController<DSClientSettings>.Settings.Manifest.OperatorId, 
                            transactionFolders.WorkingFile, 
                            DSClientModelController<DSClientModel>.Model.FileTransferServiceEndpointConfigurationName, 
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
                transactionFolders.TransactionFilename = "*." + /*SettingsController<DSClientSettings>.Settings*/SettingsBase.FileTypeExtension; //if Use... flags are clear, a file extension is needed; this may not have been set if no files found to process.
                if 
                (
                    !transactionFolders.CleanUp
                    (
                        new TimeSpan(DSClientModelController<DSClientModel>.Model.CompletedTransactionRetentionDays, 0, 0, 0),
                        new TimeSpan(DSClientModelController<DSClientModel>.Model.ErrorTransactionRetentionDays, 0, 0, 0), 
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
                packageFilePath = Path.Combine(DSClientModelController<DSClientModel>.Model.PullReceivePath, packageFilename);


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
                        DSClientModelController<DSClientModel>.Model.FileTransferServiceEndpointConfigurationName, 
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
                        DSClientModelController<DSClientModel>.Model.PullReceivePath, //location of package file in receive
                        DSClientModelController<DSClientModel>.Model.DataPath, //extract to subfolder in Data folder
                        DSClientModelController<DSClientModel>.Model.ReNewWaitMilliseconds,
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
                        DSClientModelController<DSClientModel>.Model.DataPath, //extract to subfolder in Data folder
                        DSClientModelController<DSClientModel>.Model.ReNewWaitMilliseconds,
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

        #region Confirm/Available Manifest/Document Methods
        /// <summary>
        /// Confirm Manifests.
        /// State of settings is not a consideration.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="date"></param>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        /// <param name="manifestList"></param>
        /// <param name="errorMessage"></param>
        /// <returns>Boolean</returns>
        public static Boolean ConfirmManifestsInBackground
        (
            String operatorId,
            DateTime date,
            BackgroundWorker worker,
            DoWorkEventArgs e,
            ref List<PackageManifest> manifestList,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            manifestList = default(List<PackageManifest>);
            Single stepsCountComplete = default(Int32);
            Single stepsCountTotal = 1; //based on single call to service

            try
            {
                //report initial status
                worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), "querying...");
                
                returnValue =
                    Manifest.ManifestsConfirmed
                    (
                        operatorId,
                        date,
                        DSClientModelController<DSClientModel>.Model.PackageManifestServiceEndpointConfigurationName,
                        ref manifestList,
                        ref errorMessage
                    );

                if ((!returnValue) || (manifestList == null) || ((errorMessage != null) && (errorMessage != String.Empty)))
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
        /// <param name="manifestList"></param>
        /// <param name="errorMessage"></param>
        /// <returns>Boolean</returns>
        public static Boolean AvailableManifestsInBackground
        (
            String operatorId,
            BackgroundWorker worker,
            DoWorkEventArgs e,
            ref List<PackageManifest> manifestList,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            manifestList = default(List<PackageManifest>);
            Single stepsCountComplete = default(Int32);
            Single stepsCountTotal = 1; //based on single call to service

            try
            {
                //report initial status
                worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), "querying...");

                returnValue =
                    Manifest.ManifestsAvailable
                    (
                        operatorId,
                        DSClientModelController<DSClientModel>.Model.PackageManifestServiceEndpointConfigurationName,
                        ref manifestList,
                        ref errorMessage
                    );

                if ((!returnValue) || (manifestList == null) || ((errorMessage != null) && (errorMessage != String.Empty)))
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
        /// Confirm Documents.
        /// State of settings is not a consideration.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="transactionId"></param>
        /// <param name="date"></param>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        /// <param name="documentList"></param>
        /// <param name="errorMessage"></param>
        /// <returns>Boolean</returns>
        public static Boolean ConfirmDocumentsInBackground
        (
            String operatorId,
            String transactionId,
            DateTime date,
            BackgroundWorker worker,
            DoWorkEventArgs e,
            ref List<ImageFile> documentList,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            documentList = default(List<ImageFile>);
            Single stepsCountComplete = default(Int32);
            Single stepsCountTotal = 1; //based on single call to service

            try
            {
                //report initial status
                worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), "querying...");

                returnValue =
                    Manifest.DocumentsConfirmed
                    (
                        operatorId,
                        transactionId,
                        date,
                        DSClientModelController<DSClientModel>.Model.PackageManifestServiceEndpointConfigurationName,
                        ref documentList,
                        ref errorMessage
                    );

                if ((!returnValue) || (documentList == null) || ((errorMessage != null) && (errorMessage != String.Empty)))
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

        //TODO:AvailableDocumentsInBackground
        /// <summary>
        /// Available Documents.
        /// State of settings is not a consideration.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="transactionId"></param>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        /// <param name="documentList"></param>
        /// <param name="errorMessage"></param>
        /// <returns>Boolean</returns>
        public static Boolean AvailableDocumentsInBackground
        (
            String operatorId,
            String transactionId,
            BackgroundWorker worker,
            DoWorkEventArgs e,
            ref List<ImageFile> documentList,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            documentList = default(List<ImageFile>);
            Single stepsCountComplete = default(Int32);
            Single stepsCountTotal = 1; //based on single call to service

            try
            {
                //report initial status
                worker.ReportProgress((Int32)((stepsCountComplete / stepsCountTotal) * 100), "querying...");

                returnValue =
                    Manifest.DocumentsAvailable
                    (
                        operatorId,
                        transactionId,
                        DSClientModelController<DSClientModel>.Model.PackageManifestServiceEndpointConfigurationName,
                        ref documentList,
                        ref errorMessage
                    );

                if ((!returnValue) || (documentList == null) || ((errorMessage != null) && (errorMessage != String.Empty)))
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

        #endregion  Confirm/Available Manifest/Document Methods

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
                        DSClientModelController<DSClientModel>.Model.DataPath, //extract to subfolder in Data folder
                        DSClientModelController<DSClientModel>.Model.ReNewWaitMilliseconds,
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
                        DSClientModelController<DSClientModel>.Model.DataPath, //extract to subfolder in Data folder
                        DSClientModelController<DSClientModel>.Model.ReNewWaitMilliseconds,
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
                SettingsController<DSClientSettings>.Settings.SetDocumentFilesListChangedDelegate();
                manifestA.DocumentFiles = tempList.ToOrderedEquatableBindingList<ImageFile>();
                tempList = null;
                
                //save settings w/ manifest a
                //2 of 6
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "saving original");
                DSClientSettingsController.Save();
                sourceDocumentFolder = DSClientModelController<DSClientModel>.GetTransactionImagesPath(false);

                //new settings for b, will generate new GUID 
                //assign new GUID to manifest b transaction id
                //assign manifest b to manifest property, 
                //3 of 6
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "new settings for copy");
                DSClientSettingsController.New();
                manifestB.TransactionId = SettingsController<DSClientSettings>.Settings.Manifest.TransactionId;
                SettingsController<DSClientSettings>.Settings.Manifest = manifestB;

                //in manifest b, for each document item from indexAfter to count-1, delete item
                //4 of 6
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "removing duplicates from copy");
                tempList = manifestB.DocumentFiles.ToList<ImageFile>();
                tempList.RemoveRange(afterIndex, manifestB.DocumentFiles.Count - afterIndex);
                SettingsController<DSClientSettings>.Settings.SetDocumentFilesListChangedDelegate();
                manifestB.DocumentFiles = tempList.ToOrderedEquatableBindingList<ImageFile>();
                tempList = null;
                
                //save settings as <transactionid>.documentscanner;
                //this will create images folder for manifest b as new manifest b guid
                //5 of 6
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "saving copy");
                dataFilename = String.Format("{0}.{1}", manifestB.TransactionId, /*SettingsController<DSClientSettings>.Settings*/SettingsBase.FileTypeExtension);
                dataFilePath = Path.Combine(DSClientModelController<DSClientModel>.Model.DataPath, dataFilename);
                SettingsController<DSClientSettings>.Filename = dataFilePath;
                DSClientSettingsController.Save();

                //in manifest b, for each document item from 0 to indexBefore, 
                //move image file from manifest a images folder to manifest b images folder
                //6 of 6
                worker.ReportProgress((Int32)((++stepsCountComplete / stepsCountTotal) * 100), "moving copy's images");
                destinationDocumentFolder = DSClientModelController<DSClientModel>.GetTransactionImagesPath(false);
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
        ///// Custom override of DSClientSettingsController(Of TDSClientSettings).New(); manages data file and data folder.
        ///// </summary>
        //public static void New()
        //{//TODO: override SettingsController<DSClientSettings> and call that
        //    Action postNewDelegate = default(Action);

        //    try
        //    {
        //        //Optional delegate to be run after Save (in ModelControllerBase) but before Refresh.
        //        postNewDelegate =
        //            () =>
        //            {
        //                String folderPath = default(String);

        //                //check for folder and delete if present
        //                folderPath = Path.Combine(DSClientModelController<DSClientModel>.Model.DataPath, SettingsController<DSClientSettings>.FILE_NEW);

        //                //check for folder and delete if present
        //                Folder.DeleteFolderWithWait(folderPath, DSClientModelController<DSClientModel>.Model.ReNewWaitMilliseconds);

        //                //check for folder and create if missing
        //                if (!Directory.Exists(folderPath))
        //                {
        //                    Directory.CreateDirectory(folderPath);
        //                }
        //            };

        //        SettingsController<DSClientSettings>.New();

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
        //{//TODO: override SettingsController<DSClientSettings> and call that
        //    Action postOpenDelegate = default(Action);

        //    try
        //    {
        //        //Optional delegate to be run after Save (in ModelControllerBase) but before Refresh.
        //        postOpenDelegate =
        //            () =>
        //            {
        //                //force new Filename into OldFilename
        //               SettingsController<DSClientSettings>.Filename = SettingsController<DSClientSettings>.Filename;
        //            };

        //        //Open was not synchronizing OldFilename with Filename, and Save logic was seeing '(new)'
        //        //and performing wrong actions.
        //        //Note:this may be fixed in new DSClientSettings architecture (Ssepan.* v2.6) anyway...--SJS
        //        SettingsController<DSClientSettings>.Open();

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
        //{//TODO: override SettingsController<DSClientSettings> and call that
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

        //                if (SettingsController<DSClientSettings>.OldFilename.ToUpper() !=SettingsController<DSClientSettings>.Filename.ToUpper())
        //                {

        //                    if (SettingsController<DSClientSettings>.OldFilename == SettingsController<DSClientSettings>.FILE_NEW)
        //                    {
        //                        //source will be (new)
        //                        //if (new), then rename folder to transaction's id
        //                        oldFolderPath_ = Path.Combine(DSClientModelController<DSClientModel>.Model.DataPath, SettingsController<DSClientSettings>.OldFilename);
        //                        //force transaction id for destination
        //                        newFolderPath_ = DSClientModelController<DSClientModel>.GetTransactionImagesPath(true);
        //                        Directory.Move(oldFolderPath_, newFolderPath_);
        //                    }
        //                    else //(oldFilename != SettingsController<DSClientSettings>.FILE_NEW) 
        //                    {
        //                        //if not (new), then delete original data file
        //                        //filename is different; delete original because it has same transaction #
        //                        filePath = Path.Combine(DSClientModelController<DSClientModel>.Model.DataPath, SettingsController<DSClientSettings>.OldFilename);
        //                        System.IO.File.Delete(filePath);
        //                    }
        //                    //force new Filename into OldFilename
        //                   SettingsController<DSClientSettings>.Filename = SettingsController<DSClientSettings>.Filename;
        //                }
        //            };

        //        //Call to controller base Save() writes xml and triggers refresh...
        //        //...but refresh was seeing non-new transaction and getting wrong transaction path...
        //        // ... because folder was still 'new' until Move().
        //        SettingsController<DSClientSettings>.Save();

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
        //        DSClientModelController<DSClientModel>.Model.Refresh();//Value doesn't matter; a changed fire event;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
        //        throw;
        //    }
        //}
        #endregion Menu Methods

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
                DSClientModelController<DSClientModel>.Model.TwainSource = new TwainSource(handle, scanFinished);

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
                if (DSClientModelController<DSClientModel>.Model.TwainSource != null)
                {
                    DSClientModelController<DSClientModel>.Model.TwainSource.Dispose();
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
        //        DSClientModelController<DSClientModel>.Model.FileTransferServiceEndpointConfigurationName = _fileTransferServiceEndpointConfigurationName;

        //        if (!Configuration.ReadString("PackageManifestServiceEndpointConfigurationName", out _packageManifestServiceEndpointConfigurationName))
        //        {
        //            throw new Exception(String.Format("Unable to load PackageManifestServiceEndpointConfigurationName: '{0}'", _packageManifestServiceEndpointConfigurationName));
        //        }
        //        DSClientModelController<DSClientModel>.Model.PackageManifestServiceEndpointConfigurationName = _packageManifestServiceEndpointConfigurationName;

        //        //if (!Configuration.ReadValue<Int32>("ImageQualityPercent", out _imageQualityPercent))
        //        //{
        //        //    throw new Exception(String.Format("Unable to load ImageQualityPercent: '{0}'", _imageQualityPercent));
        //        //}
        //        //DSClientModelController<DSClientModel>.Model.ImageQualityPercent = _imageQualityPercent;

        //        if (!Configuration.ReadValue<Boolean>("AutoNavigateTabs", out _autoNavigateTabs))
        //        {
        //            throw new Exception(String.Format("Unable to load AutoNavigateTabs: '{0}'", _autoNavigateTabs));
        //        }
        //        DSClientModelController<DSClientModel>.Model.AutoNavigateTabs = _autoNavigateTabs;
                
        //        if (!Configuration.ReadValue<Int32>("ReNewWaitMilliseconds", out _reNewWaitMilliseconds))
        //        {
        //            throw new Exception(String.Format("Unable to load ReNewWaitMilliseconds: '{0}'", _reNewWaitMilliseconds));
        //        }
        //        DSClientModelController<DSClientModel>.Model.ReNewWaitMilliseconds = _reNewWaitMilliseconds;

        //        if (!Configuration.ReadString("DataPath", out _dataPath))
        //        {
        //            throw new Exception(String.Format("Unable to load DataPath: '{0}'", _dataPath));
        //        }
        //        DSClientModelController<DSClientModel>.Model.DataPath = _dataPath;
                
        //        if (!Configuration.ReadString("PushSendPath", out _pushSendPath))
        //        {
        //            throw new Exception(String.Format("Unable to load PushSendPath: '{0}'", _pushSendPath));
        //        }
        //        DSClientModelController<DSClientModel>.Model.PushSendPath = _pushSendPath;

        //        if (!Configuration.ReadString("PullReceivePath", out _pullReceivePath))
        //        {
        //            throw new Exception(String.Format("Unable to load PullReceivePath: '{0}'", _pullReceivePath));
        //        }
        //        DSClientModelController<DSClientModel>.Model.PullReceivePath = _pullReceivePath;

        //        if (!Configuration.ReadValue<Int32>("CompletedTransactionRetentionDays", out _completedTransactionRetentionDays))
        //        {
        //            throw new Exception(String.Format("Unable to load CompletedTransactionRetentionDays: '{0}'", _completedTransactionRetentionDays));
        //        }
        //        DSClientModelController<DSClientModel>.Model.CompletedTransactionRetentionDays = _completedTransactionRetentionDays;

        //        if (!Configuration.ReadValue<Int32>("ErrorTransactionRetentionDays", out _errorTransactionRetentionDays))
        //        {
        //            throw new Exception(String.Format("Unable to load ErrorTransactionRetentionDays: '{0}'", _errorTransactionRetentionDays));
        //        }
        //        DSClientModelController<DSClientModel>.Model.ErrorTransactionRetentionDays = _errorTransactionRetentionDays;

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
                DSClientSettingsController.New();
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
                SettingsController<DSClientSettings>.Settings.Manifest = PackageManifest.Load(manifestSourceFilePath);
                _NoUiOnThisThread = false;

                //copy images folder from package path to Data folder
                imagesSourcePath = Path.Combine(packageContentsPackageSubfolderPath, packageId);
                imagesDestinationPath = DSClientModelController<DSClientModel>.GetTransactionImagesPath(false); //this requires open manifest in new or named settings
                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory
                (
                    imagesSourcePath,
                    imagesDestinationPath
                );


                //report status
                progressDelegate(String.Format("saving settings..."));
                
                //save package and clean up
                //prevent change notifications that could update views; refresh views explicitly after this thread ends
                dataFilename = String.Format("{0}.{1}", packageId, /*SettingsController<DSClientSettings>.Settings*/SettingsBase.FileTypeExtension);
                dataFilePath = Path.Combine(packageContentsRootPath, dataFilename);
               SettingsController<DSClientSettings>.Filename = dataFilePath;
                _NoUiOnThisThread = true;
                DSClientSettingsController.Save();
                _NoUiOnThisThread = false;


                //report status
                progressDelegate(String.Format("cleaning up..."));

                //delete package directory used for unzipping
                Folder.DeleteFolderWithWait(packageContentsPackageSubfolderPath, deleteWaitMilliseconds);


                //report status
                progressDelegate(String.Format("validating..."));

                //Perform validation
                //re-validate manifest as part of settings
                if (!SettingsController<DSClientSettings>.Settings.Valid())
                {
                    throw new ApplicationException(String.Format("{0}", SettingsController<DSClientSettings>.Settings.ErrorMessage));
                }

                //validate settings determined at the time of the submit
                if (!SettingsController<DSClientSettings>.Settings.Complete())
                {
                    throw new ApplicationException(String.Format("{0}", SettingsController<DSClientSettings>.Settings.ErrorMessage));
                }

                //settings should not be Dirty
                if (SettingsController<DSClientSettings>.Settings.Dirty)
                {
                    throw new ApplicationException(String.Format("There are unsaved changes: '{0}'",SettingsController<DSClientSettings>.Filename));
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
                if (!SettingsController<DSClientSettings>.Settings.Valid())
                {
                    throw new ApplicationException(String.Format("{0}", SettingsController<DSClientSettings>.Settings.ErrorMessage));
                }

                //validate settings determined at the time of the submit
                if (!SettingsController<DSClientSettings>.Settings.Complete())
                {
                    throw new ApplicationException(String.Format("{0}", SettingsController<DSClientSettings>.Settings.ErrorMessage));
                }

                //settings should not be Dirty
                if (SettingsController<DSClientSettings>.Settings.Dirty)
                {
                    throw new ApplicationException(String.Format("There are unsaved changes: '{0}'",SettingsController<DSClientSettings>.Filename));
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
                PackageManifest.Save(SettingsController<DSClientSettings>.Settings.Manifest, manifestFilePath);

                //copy images folder to package path
                imagesSourcePath = DSClientModelController<DSClientModel>.GetTransactionImagesPath(false);
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
