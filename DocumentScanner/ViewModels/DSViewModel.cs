using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Ssepan.Utility;
using Ssepan.Application;
using Ssepan.Application.WinForms;
using Ssepan.Collections;
using Ssepan.Graphics;
using Ssepan.Io;
using DocumentScannerCommon;
using DocumentScannerLibrary;
//using DocumentScannerLibrary.Properties;

namespace DocumentScanner
{
    /// <summary>
    /// Note: this class can subclass the base without type parameters.
    /// </summary>
    public class DSViewModel :
        FormsViewModel<Bitmap, Settings, DSModel, DocumentViewer>
    {
        #region Declarations
        //public delegate Boolean DoWork_WorkDelegate(BackgroundWorker worker, DoWorkEventArgs e, ref String errorMessage);

        #region Commands
        //public ICommand FileNewCommand { get; private set; }
        //public ICommand FileOpenCommand { get; private set; }
        //public ICommand FileSaveCommand { get; private set; }
        //public ICommand FileSaveAsCommand { get; private set; }
        //public ICommand FilePrintCommand { get; private set; }
        //public ICommand FileExitCommand { get; private set; }
        //public ICommand EditCopyToClipboardCommand { get; private set; }
        //public ICommand EditPropertiesCommand { get; private set; }
        //public ICommand ViewPreviousMonthCommand { get; private set; }
        //public ICommand ViewPreviousWeekCommand { get; private set; }
        //public ICommand ViewNextWeekCommand { get; private set; }
        //public ICommand ViewNextMonthCommand { get; private set; }
        //public ICommand HelpAboutCommand { get; private set; }
        #endregion Commands
        #endregion Declarations

        #region Constructors
        public DSViewModel() { }//Note: not called, but need to be present to compile--SJS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyChangedEventHandlerDelegate"></param>
        /// <param name="actionIconImages"></param>
        /// <param name="settingsFileDialogInfo"></param>
        public DSViewModel
        (
            PropertyChangedEventHandler propertyChangedEventHandlerDelegate,
            Dictionary<String, Bitmap> actionIconImages,
            FileDialogInfo settingsFileDialogInfo
        ) :
            base(propertyChangedEventHandlerDelegate, actionIconImages, settingsFileDialogInfo)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyChangedEventHandlerDelegate"></param>
        /// <param name="actionIconImages"></param>
        /// <param name="settingsFileDialogInfo"></param>
        /// <param name="view"></param>
        public DSViewModel
        (
            PropertyChangedEventHandler propertyChangedEventHandlerDelegate,
            Dictionary<String, Bitmap> actionIconImages,
            FileDialogInfo settingsFileDialogInfo,
            DocumentViewer view
        ) :
            base(propertyChangedEventHandlerDelegate, actionIconImages, settingsFileDialogInfo, view)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }
        #endregion Constructors

        #region Properties
        #endregion Properties

        #region Methods
        #region Menu
        /// <summary>
        /// Initiate scan (acquire and transfer).
        /// Callback will process results.
        /// </summary>
        internal void File_Scan()
        {
            try
            {
                StartProgressBar("Scanning...", String.Empty, View.menuDocumentsScan.Image as Bitmap, false, 0);

                //perform scan to obtain List<Image>
                if (!DSController<DSModel>.ScanAcquire(View.Handle, twainSourceScanFinishedEventhandler))
                {
                    throw new ApplicationException(String.Format("Scan not initiated."));
                }
                
                //enable cancel; disable relevant buttons / menus
                View.SetFunctionControlsEnable
                (
                    View.scanButton,
                    View.buttonFileScan,
                    View.menuFileScan,
                    null, //View.cancelButton
                    false
                );

                UpdateProgressBar("Scanning...Transferring...", 25);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                StopProgressBar(null, ex.Message);
            }
        }

        /// <summary>
        /// Open Twain UI to select scanner.
        /// Callback will process results.
        /// </summary>
        internal void File_Select()
        {
            try
            {
                StartProgressBar("Selecting...", null, View.menuDocumentsScan.Image as Bitmap, false, 0);

                //select scanner using twain ui
                if (!DSController<DSModel>.SelectScanner(View.Handle, twainSourceScanFinishedEventhandler))
                {
                    throw new ApplicationException(String.Format("Scanner not selected."));
                }

                UpdateProgressBar("Selected.", 100);
                //StopProgressBar("Selected.", null);//TODO:experiment with progress bar in test app--SJS
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                StopProgressBar(null, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal void File_Package()
        {

            try
            {
                StartProgressBar("Packaging...", null, View.menuFilePackage.Image as Bitmap, false, 0);

                //enable cancel; disable relevant buttons / menus
                View.SetFunctionControlsEnable
                (
                    View.packageButton,
                    View.buttonFilePackage,
                    View.menuFilePackage,
                    null, //View.cancelButton
                    false
                );

                //set cancellation hook
                View.cancelDelegate = View.backgroundWorkerPackage.CancelAsync;

                View.backgroundWorkerPackage.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                //clear cancellation hook
                View.cancelDelegate = null;

                StopProgressBar(null, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void File_Send()
        {
            try
            {
                StartProgressBar("Sending...", null, View.menuFileSave.Image as Bitmap, false, 0);

                //enable cancel; disable relevant buttons / menus
                View.SetFunctionControlsEnable
                (
                    View.sendButton,
                    View.buttonFileSend,
                    View.menuFileSend,
                    null, //View.cancelButton
                    false
                );

                //set cancellation hook
                View.cancelDelegate = View.backgroundWorkerSend.CancelAsync;

                View.backgroundWorkerSend.RunWorkerAsync(SettingsController<Settings>.Settings);//TODO:pass model insteadl, or neither?
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                //clear cancellation hook
                View.cancelDelegate = null;

                StopProgressBar(null, ex.Message);
            }
        }

        /// <summary>
        /// Documents menu Opening event.
        /// </summary>
        /// <param name="contextMenu"></param>
        internal void Documents_Opening(ContextMenuStrip contextMenu)
        {
            Int32 count = default(Int32);
            Int32 selectedDocumentRowIndex = -1;
            DataGridView source = default(DataGridView);
            ImageFile imageFile = default(ImageFile);

            try
            {
                source = contextMenu.SourceControl as DataGridView;
                count = source.SelectedRows.Count;

                if (count == 1)
                {
                    //one row/cell selected
                    imageFile = ((ImageFile)source.SelectedRows[0].DataBoundItem);
                    if ((imageFile.Filename == null) || (imageFile.Filename == String.Empty) /* || (imageFile.Filename == Document.NewItemName)*/)
                    {
                        UpdateStatusBarMessages("Document functions require document with filename.", null);

                        View.menuDocumentsPrint.Enabled = false;
                        View.menuDocumentsScan.Enabled = false;
                        View.menuDocumentsRotateCCW90.Enabled = false;
                        View.menuDocumentsRotateCW90.Enabled = false;
                        View.menuDocumentsPromoteDocument.Enabled = false;
                        View.menuDocumentsDemoteDocument.Enabled = false;
                        View.menuDocumentsSplitPackageAbove.Enabled = false;
                        View.menuDocumentsSplitPackageBelow.Enabled = false;
                    }
                    else
                    {
                        //cell has a value -- i.e. is not New.
                        selectedDocumentRowIndex = GetIndex(source);
                        if (selectedDocumentRowIndex != -1)
                        {
                            //a document is selected
                            //TODO:code to deal with printing multiple documents?
                            View.menuDocumentsPrint.Enabled = true;
                            View.menuDocumentsScan.Enabled = false/*true*/; //disable scan / re-scan for single  document
                            View.menuDocumentsRotateCCW90.Enabled = true;
                            View.menuDocumentsRotateCW90.Enabled = true;
                            View.menuDocumentsPromoteDocument.Enabled = true;
                            View.menuDocumentsDemoteDocument.Enabled = true;
                            //only split on saved settings
                            if (!SettingsController<Settings>.Settings.Dirty)
                            {
                                if (source.SelectedRows[0].Index == 0)
                                {
                                    //cannot split above if first row
                                    View.menuDocumentsSplitPackageAbove.Enabled = false;
                                }
                                else
                                {
                                    View.menuDocumentsSplitPackageAbove.Enabled = true;
                                }
                                if (source.SelectedRows[0].Index == source.RowCount - 1)
                                {
                                    //cannot split below if last row
                                    View.menuDocumentsSplitPackageBelow.Enabled = false;
                                }
                                else
                                {
                                    View.menuDocumentsSplitPackageBelow.Enabled = true;
                                }
                            }
                            else
                            {
                                View.menuDocumentsSplitPackageAbove.Enabled = false;
                                View.menuDocumentsSplitPackageBelow.Enabled = false;
                            }
                        }
                        else
                        {
                            View.menuDocumentsPrint.Enabled = false;
                            View.menuDocumentsScan.Enabled = false;
                            View.menuDocumentsRotateCCW90.Enabled = false;
                            View.menuDocumentsRotateCW90.Enabled = false;
                            View.menuDocumentsPromoteDocument.Enabled = false;
                            View.menuDocumentsDemoteDocument.Enabled = false;
                            View.menuDocumentsSplitPackageAbove.Enabled = false;
                            View.menuDocumentsSplitPackageBelow.Enabled = false;
                        }
                    }
                }
                else
                {
                    View.menuDocumentsPrint.Enabled = false;
                    View.menuDocumentsScan.Enabled = false;
                    View.menuDocumentsRotateCCW90.Enabled = false;
                    View.menuDocumentsRotateCW90.Enabled = false;
                    View.menuDocumentsPromoteDocument.Enabled = false;
                    View.menuDocumentsDemoteDocument.Enabled = false;
                    View.menuDocumentsSplitPackageAbove.Enabled = false;
                    View.menuDocumentsSplitPackageBelow.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        /// <summary>
        /// Print document.
        /// </summary>
        internal void Documents_Print()
        {
            //String statusMessage = String.Empty;
            String errorMessage = default(String);
            ImageFile imageFile = default(ImageFile);
            Image image = default(Image);

            try
            {
                UpdateProgressBar("Printing document...", 0);

                imageFile = View.dgvDocuments.SelectedRows[0].DataBoundItem as ImageFile;
                //TODO:code to deal with printing multiple documents?
                if (!RenderImage(ref image, imageFile, ref errorMessage))
                {
                    throw new Exception(String.Format("Unable to load image:\n{0}", errorMessage));
                }

                //pass the PrintDocument and the Image to be printed
                DSController<DSModel>.Print(View.printDocument, image);

                StopProgressBar("Printed.", null);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                StopProgressBar(null, String.Format("{0}", ex.Message));
            }
            finally
            {
            }
        }

        /// <summary>
        /// Rotate document counter-clockwise one quarter turn.
        /// </summary>
        internal void Documents_RotateCCW90()
        {
            ImageFile item = default(ImageFile);
            Int32 selectedDocumentRowIndex = -1;

            try
            {
                selectedDocumentRowIndex = GetIndex(View.dgvDocuments);
                if (selectedDocumentRowIndex != -1)
                {
                    if (View.dgvDocuments.SelectedRows.Count == 1)
                    {
                        item = (ImageFile)View.dgvDocuments.SelectedRows[0].DataBoundItem;

                        //Rotate counter-clockwise
                        if (!DSController<DSModel>.RotateDocumentItem(item, RotateFlipType.Rotate270FlipNone))
                        {
                            throw new ApplicationException(String.Format("Item '{0}' not roated counter-clockwise.", item.Filename));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                StopProgressBar(null, String.Format("{0}", ex.Message));
            }
        }

        /// <summary>
        /// Rotate document clockwise one quarter turn.
        /// </summary>
        internal void Documents_RotateCW90()
        {
            ImageFile item = default(ImageFile);
            Int32 selectedDocumentRowIndex = -1;

            try
            {
                selectedDocumentRowIndex = GetIndex(View.dgvDocuments);
                if (selectedDocumentRowIndex != -1)
                {
                    if (View.dgvDocuments.SelectedRows.Count == 1)
                    {
                        item = (ImageFile)View.dgvDocuments.SelectedRows[0].DataBoundItem;

                        //Rotate clockwise
                        if (!DSController<DSModel>.RotateDocumentItem(item, RotateFlipType.Rotate90FlipNone))
                        {
                            throw new ApplicationException(String.Format("Item '{0}' not rotated clockwise.", item.Filename));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                StopProgressBar(null, String.Format("{0}", ex.Message));
            }
        }

        /// <summary>
        /// Move document item higher in the list.
        /// </summary>
        internal void Documents_PromoteDocument()
        {
            ImageFile item = default(ImageFile);
            Int32 selectedDocumentRowIndex = -1;

            try
            {
                selectedDocumentRowIndex = GetIndex(View.dgvDocuments);
                if (selectedDocumentRowIndex != -1)
                {
                    if (View.dgvDocuments.SelectedRows.Count == 1)
                    {
                        item = (ImageFile)View.dgvDocuments.SelectedRows[0].DataBoundItem;

                        if (!DSController<DSModel>.ShiftDocumentItem(item, ListOfTExtension.ShiftTypes.Promote))
                        {
                            throw new ApplicationException(String.Format("Item '{0}' not promoted.", item.Filename));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                StopProgressBar(null, String.Format("{0}", ex.Message));
            }
        }

        /// <summary>
        /// Move document item lower in the list.
        /// </summary>
        internal void Documents_DemoteDocument()
        {
            ImageFile item = default(ImageFile);
            Int32 selectedDocumentRowIndex = -1;

            try
            {
                selectedDocumentRowIndex = GetIndex(View.dgvDocuments);
                if (selectedDocumentRowIndex != -1)
                {
                    if (View.dgvDocuments.SelectedRows.Count == 1)
                    {
                        item = (ImageFile)View.dgvDocuments.SelectedRows[0].DataBoundItem;

                        if (!DSController<DSModel>.ShiftDocumentItem(item, ListOfTExtension.ShiftTypes.Demote))
                        {
                            throw new ApplicationException(String.Format("Item '{0}' not demoted.", item.Filename));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                StopProgressBar(null, String.Format("{0}", ex.Message));
            }
        }

        /// <summary>
        /// Given a saved package definition, split the definition above the selected document,
        /// and save the result in two separate package definitions.
        /// </summary>
        internal void Documents_SplitPackageAbove()
        {
            try
            {
                StartProgressBar("Splitting package settings above...", null, View.menuFileSave.Image as Bitmap, false, 0);

                //Declare Tuple object to pass multiple params to DoWork method.
                var arguments =
                    Tuple.Create<PackageManifest /*manifest*/, Int32 /*beforeIndex*/, Int32 /*afterIndex*/>
                    (
                        SettingsController<Settings>.Settings.Manifest,
                        View.dgvDocuments.SelectedRows[0].Index - 1,
                        View.dgvDocuments.SelectedRows[0].Index
                    );

                //set cancellation hook
                View.cancelDelegate = View.backgroundWorkerSplitPackageSettings.CancelAsync;

                View.backgroundWorkerSplitPackageSettings.RunWorkerAsync(arguments);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                //clear cancellation hook
                View.cancelDelegate = null;

                StopProgressBar(null, ex.Message);
            }
        }

        /// <summary>
        /// Given a saved package definition, split the definition below the selected document,
        /// and save the result in two separate package definitions.
        /// </summary>
        internal void Documents_SplitPackageBelow()
        {
            try
            {
                StartProgressBar("Splitting package settings below...", null, View.menuFileSave.Image as Bitmap, false, 0);

                //Declare Tuple object to pass multiple params to DoWork method.
                var arguments =
                    Tuple.Create<PackageManifest /*manifest*/, Int32 /*beforeIndex*/, Int32 /*afterIndex*/>
                    (
                        SettingsController<Settings>.Settings.Manifest,
                        View.dgvDocuments.SelectedRows[0].Index,
                        View.dgvDocuments.SelectedRows[0].Index + 1
                    );

                //set cancellation hook
                View.cancelDelegate = View.backgroundWorkerSplitPackageSettings.CancelAsync;

                View.backgroundWorkerSplitPackageSettings.RunWorkerAsync(arguments);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                //clear cancellation hook
                View.cancelDelegate = null;

                StopProgressBar(null, ex.Message);
            }
        }

        /// <summary>
        /// Failed menu Opening event.
        /// </summary>
        /// <param name="contextMenu"></param>
        internal void Failed_Opening(ContextMenuStrip contextMenu)
        {
            Int32 count = default(Int32);
            Int32 selectedDocumentRowIndex = -1;
            DataGridView source = default(DataGridView);
            String packageFilePath = default(String);

            try
            {
                source = contextMenu.SourceControl as DataGridView;
                count = source.SelectedRows.Count;

                if (count == 1)
                {
                    //one row/cell selected
                    Object o = source.SelectedRows[0].DataBoundItem;
                    var typed = ObjectHelper.Cast(o, new { Name = String.Empty, Filename = String.Empty, FilePath = String.Empty });
                    packageFilePath = typed.FilePath;
                    if ((packageFilePath == null) || (packageFilePath == String.Empty)/* || (packageFilePath == Document.NewItemName)*/)
                    {
                        UpdateStatusBarMessages("Failed Package functions require file path.", null);

                        View.menuFailedUnpackage.Enabled = false;
                    }
                    else
                    {
                        //cell has a value -- i.e. is not New.
                        selectedDocumentRowIndex = GetIndex(source);
                        if (selectedDocumentRowIndex != -1)
                        {
                            //a document is selected
                            View.menuFailedUnpackage.Enabled = true;
                        }
                        else
                        {
                            View.menuFailedUnpackage.Enabled = false;
                        }
                    }
                }
                else
                {
                    View.menuFailedUnpackage.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        /// <summary>
        /// Given a (failed) package file path, move the package from the error folder to the data folder,
        /// and unpack it so that it can be edited.
        /// </summary>
        internal void Failed_Unpackage()
        {
            try
            {
                StartProgressBar("Unpacking package...", null, View.menuFileSave.Image as Bitmap, false, 0);

                //retrieve value from grid
                Object o = View.failedDataGridView.SelectedRows[0].DataBoundItem;
                var typed = ObjectHelper.Cast(o, new { Name = String.Empty, Filename = String.Empty, FilePath = String.Empty });

                //Declare Tuple Object to pass multiple params to DoWork method.
                var arguments =
                    Tuple.Create<String /*name*/, String /*filename*/, String /*filePath*/>
                    (
                        typed.Name,
                        typed.Filename,
                        typed.FilePath
                    );

                //set cancellation hook
                View.cancelDelegate = View.backgroundWorkerUnPackage.CancelAsync;

                View.backgroundWorkerUnPackage.RunWorkerAsync(arguments);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                //clear cancellation hook
                View.cancelDelegate = null;

                StopProgressBar(null, ex.Message);
            }
        }

        /// <summary>
        /// Confirmation menu Opening event.
        /// </summary>
        /// <param name="contextMenu"></param>
        internal void Confirmation_Opening(ContextMenuStrip contextMenu)
        {
            Int32 count = default(Int32);
            Int32 selectedDocumentRowIndex = -1;
            DataGridView source = default(DataGridView);
            PackageManifest manifest = default(PackageManifest);

            try
            {
                source = contextMenu.SourceControl as DataGridView;
                count = source.SelectedRows.Count;

                if (count == 1)
                {
                    //one row/cell selected
                    manifest = ((PackageManifest)source.SelectedRows[0].DataBoundItem);
                    if (manifest.Count == 0)
                    {
                        UpdateStatusBarMessages("Manifest confirmation functions require manifest with documents.", null);

                        //menuConfirmationListManifestDocumentConfirmations.Enabled = false;
                    }
                    else
                    {
                        //cell has a value -- i.e. is not New.
                        selectedDocumentRowIndex = GetIndex(source);
                        if (selectedDocumentRowIndex != -1)
                        {
                            //a manifest is selected
                            //menuConfirmationListManifestDocumentConfirmations.Enabled = false; // true; //feature redacted
                        }
                        else
                        {
                            //menuConfirmationListManifestDocumentConfirmations.Enabled = false;
                        }
                    }
                }
                else
                {
                    //menuConfirmationListManifestDocumentConfirmations.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        /// <summary>
        /// Given the Operator ID, a Transaction ID, and the specified date, 
        /// return a List(Of ImageFile) from the server.
        /// </summary>
        internal void Confirmation_ListDocumentConfirmations()
        {
            try
            {
                StartProgressBar("Listing confirmed manifest documents...", null, View.menuFileSave.Image as Bitmap, false, 0);

                // Declare Tuple object to pass multiple params to DoWork method.
                var arguments =
                    Tuple.Create<String /*operatorId*/, DateTime /*date*/, String /*transactionId*/>
                    (
                        SettingsController<Settings>.Settings.Manifest.OperatorId,
                        View.confirmationDateTimePicker.Value,
                        (View.confirmedManifestsDataGridView.CurrentRow.DataBoundItem as PackageManifest).TransactionId
                    );

                View.backgroundWorkerConfirmDocuments.RunWorkerAsync(arguments);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                StopProgressBar(null, ex.Message);
            }
        }

        /// <summary>
        /// Receive menu Opening event.
        /// </summary>
        /// <param name="contextMenu"></param>
        internal void Receive_Opening(ContextMenuStrip contextMenu)
        {
            Int32 count = default(Int32);
            Int32 selectedDocumentRowIndex = -1;
            DataGridView source = default(DataGridView);
            PackageManifest manifest = default(PackageManifest);

            try
            {
                source = contextMenu.SourceControl as DataGridView;
                count = source.SelectedRows.Count;

                if (count == 1)
                {
                    //one row/cell selected
                    manifest = ((PackageManifest)source.SelectedRows[0].DataBoundItem);
                    if (manifest.Count == 0)
                    {
                        UpdateStatusBarMessages("Manifest receive functions require manifest with documents.", null);

                        //menuReceiveListManifestDocuments.Enabled = false;
                        View.menuReceiveManifest.Enabled = false;
                    }
                    else
                    {
                        //cell has a value -- i.e. is not New.
                        selectedDocumentRowIndex = GetIndex(source);
                        if (selectedDocumentRowIndex != -1)
                        {
                            //a manifest is selected
                            //menuReceiveListManifestDocuments.Enabled = false; // true; //feature redacted
                            View.menuReceiveManifest.Enabled = true;
                        }
                        else
                        {
                            //menuReceiveListManifestDocuments.Enabled = false;
                            View.menuReceiveManifest.Enabled = false;
                        }
                    }
                }
                else
                {
                    //menuReceiveListManifestDocuments.Enabled = false;
                    View.menuReceiveManifest.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal void Receive_ListManifestDocuments()
        {
            try
            {
                StartProgressBar("Listing available manifest documents...", null, View.menuFileSave.Image as Bitmap, false, 0);

                // Declare Tuple object to pass multiple params to DoWork method.
                var arguments =
                    Tuple.Create<String /*operatorId*/, String /*transactionId*/>
                    (
                        SettingsController<Settings>.Settings.Manifest.OperatorId,
                        (View.receivableManifestsDataGridView.CurrentRow.DataBoundItem as PackageManifest).TransactionId
                    );

                View.backgroundWorkerAvailableDocuments.RunWorkerAsync(arguments);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                StopProgressBar(null, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal void Receive_Manifest()
        {
            try
            {
                StartProgressBar("Receiving selected manifest...", null, View.menuFileSave.Image as Bitmap, false, 0);

                // Declare Tuple object to pass multiple params to DoWork method.
                var arguments =
                    Tuple.Create<String /*operatorId*/, String /*transactionId*/>
                    (
                        SettingsController<Settings>.Settings.Manifest.OperatorId,
                        (View.receivableManifestsDataGridView.CurrentRow.DataBoundItem as PackageManifest).TransactionId
                    );


                //enable cancel; disable relevant buttons / menus
                View.SetFunctionControlsEnable
                (
                    null,
                    null,
                    View.menuReceiveManifest,
                    null, //View.cancelButton
                    false
                );

                //set cancellation hook
                View.cancelDelegate = View.backgroundWorkerReceive.CancelAsync;

                View.backgroundWorkerReceive.RunWorkerAsync(arguments);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                StopProgressBar(null, ex.Message);
            }
        }
        #endregion Menu

        #region Control
        /// <summary>
        /// Show context menu
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="mouseButtons"></param>
        /// <param name="rowIndex"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        internal void Grid_CellMouseDown(DataGridView grid, MouseButtons mouseButtons, Int32 rowIndex, Int32 x, Int32 y)
        {
            try
            {
                if (mouseButtons == MouseButtons.Right)
                {
                    if (rowIndex != -1)
                    {
                        grid[0, rowIndex].Selected = true;
                        grid.Rows[rowIndex].Selected = true;
                        grid.ContextMenuStrip.Show(grid, new Point(x, y));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                StopProgressBar(null, String.Format("{0}", ex.Message));
            }
        }

        /// <summary>
        /// Bind grid on cell selection change.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="bindUiDelegate"></param>
        internal void Grid_CurrentCellChanged(DataGridView grid, Action<PackageManifest> bindUiDelegate)
        {
            PackageManifest manifest = default(PackageManifest);
            Int32 index = default(Int32);

            try
            {
                index = GetIndex(grid);
                if (index != -1)
                {
                    manifest = (grid.Rows[index].DataBoundItem as PackageManifest);
                    if (manifest.DocumentFiles != null)
                    {
                        //bind documents grid to current manifest's documents
                        bindUiDelegate(manifest);
                    }
                }

                StopProgressBar(null, null);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                StopProgressBar(null, String.Format("{0}", ex.Message));

                throw;
            }
        }

        /// <summary>
        /// Bind grid on row selection change.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="bindUiDelegate"></param>
        internal void Grid_CurrentRowChanged(DataGridView grid, Action<PackageManifest> bindUiDelegate)
        {
            PackageManifest manifest = default(PackageManifest);
            Int32 index = default(Int32);

            try
            {
                index = GetIndex(grid);
                if (index != -1)
                {
                    manifest = (grid.Rows[index].DataBoundItem as PackageManifest);
                    if (manifest.DocumentFiles != null)
                    {
                        //bind documents grid to current manifest's documents
                        bindUiDelegate
                        (
                            manifest
                        );
                    }
                }

                StopProgressBar(null, null);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                StopProgressBar(null, String.Format("{0}", ex.Message));

                throw;
            }
        }

        /// <summary>
        /// Given the Operator ID and the specified date, 
        /// return a List(Of PackageManifest) from the server.
        /// Declare Tuple object to pass multiple params to DoWork method.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="worker"></param>
        /// <param name="arguments">Object, specifically a Tuple; passed on as-is.</param>
        internal void ListManifests(String description, BackgroundWorker worker, /*Tuple*/Object arguments)
        {
            try
            {
                StartProgressBar(String.Format("Listing {0} manifests...", description), null, View.menuFileSave.Image as Bitmap, false, 0);

                worker.RunWorkerAsync(arguments);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                StopProgressBar(null, String.Format("{0}", ex.Message));
            }
        }
        #endregion Control

        #region Private

        /// <summary>
        /// Checks for current document and performs an image render.
        /// </summary>
        /// <param name="documentListGrid"></param>
        /// <param name="pictureBox"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        internal Boolean RefreshImage(DataGridView documentListGrid, PictureBox pictureBox, ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);
            Int32 selectedRowIndex = -1;
            ImageFile imageFile = default(ImageFile);
            Image documentImage = default(Image);

            try
            {
                selectedRowIndex = GetIndex(documentListGrid);
                if (selectedRowIndex != -1)
                {
                    //Get bound imageFile directly; if null, it will behave like 'else' case.
                    imageFile = documentListGrid.Rows[selectedRowIndex].DataBoundItem as ImageFile;

                    // Render image for control.
                    if (!RenderImage(ref documentImage, imageFile, ref errorMessage))
                    {
                        throw new Exception(String.Format("Unable to load image:\n{0}", errorMessage));
                    }
                }
                else
                {
                    // Render no image.
                    if (!RenderImage(ref documentImage, (ImageFile)null, ref errorMessage))
                    {
                        throw new Exception(String.Format("Unable to clear image:\n{0}", errorMessage));
                    }
                }
                pictureBox.Image = documentImage;

                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        /// <summary>
        /// Clears Image and renders a new one. 
        /// Pass image explicitly.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="file"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private Boolean RenderImage(ref Image image, ImageFile file, ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);
            String filePath = default(String);

            try
            {
                if (file == null)
                {
                    image = Properties.Resources.no_document_selected;
                }
                else
                {
                    if ((file.Filename == null) || (file.Filename == String.Empty))
                    {
                        image = Properties.Resources.no_image_present;
                    }
                    else
                    {
                        //get image from file
                        //use path defined by config and by current transaction id for the sub-folder.
                        filePath = Path.Combine(Path.Combine(Application.StartupPath, DSController<DSModel>.GetTransactionImagesPath(false)), file.Filename);
                        if (!Transform.LoadImageUnlocked(ref image, filePath, ref errorMessage))
                        {
                            image = Properties.Resources.no_image_loaded;
                        }
                    }
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

        /// <summary>
        /// Respond to TwainSource's ScanFinished by saving each Image
        /// in ScannedImages and loading List of ImageFile in DocumentFiles
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void twainSourceScanFinishedEventhandler(Object sender, EventArgs e)
        {
            String errorMessage = default(String);
            try
            {
                UpdateProgressBar("Scanning...Processing...", 75);

                //perform processing of scanned List<Image>
                if (!DSController<DSModel>.ScanProcess(ref errorMessage))
                {
                    throw new ApplicationException(String.Format("Scan not completed: {0}", errorMessage));
                }

                UpdateProgressBar("Scanned.", 100);

                if (DSController<DSModel>.Model.AutoNavigateTabs)
                {
                    View.tabControl.SelectTab(View.tabPageScan);
                }

                //refresh
                DSController<DSModel>.Model.Refresh();

                View.Activate();
                //StopProgressBar("Scanned.", null);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                StopProgressBar(null, ex.Message);
            }
        }

        /// <summary>
        /// Look up a selected row number in the DataGridView, first by checking for selected rows, then by checking row of selected cells
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        private Int32 GetIndex(DataGridView grid)
        {
            Int32 returnValue = -1;
            try
            {
                //look for selected row first
                if (grid.SelectedRows.Count == 1)
                {
                    returnValue = grid.SelectedRows[0].Index;
                }
                if (returnValue == -1)
                {
                    //look for selected cell next
                    if (grid.SelectedCells.Count == 1)
                    {
                        returnValue = grid.SelectedCells[0].OwningRow.Index;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
            return returnValue;
        }
        #endregion Private
        #endregion Methods

    }
}
