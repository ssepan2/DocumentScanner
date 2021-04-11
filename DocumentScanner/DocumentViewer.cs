#define USE_CONFIG_FILEPATH

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Ssepan.Application;
using Ssepan.Application.MVC;
using Ssepan.Collections;
using Ssepan.Graphics;
using Ssepan.Io;
using Ssepan.Utility;
using DocumentScannerCommon;
using DocumentScannerLibrary;
using DocumentScannerLibrary.Properties;
using DocumentScannerLibrary.MVC;

namespace DocumentScanner
{
    /// <summary>
    /// This is the View.
    /// </summary>
    public partial class DocumentViewer :
        Form,
        INotifyPropertyChanged
    {
        #region Declarations
        protected Boolean disposed;

        private Boolean _ValueChangedProgrammatically;

        //cancellation hook
        internal Action cancelDelegate = null;
        protected DSViewModel ViewModel = default(DSViewModel);
        #endregion Declarations

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public DocumentViewer(String[] args)
        {
            try
            {
                InitializeComponent();

                ////(re)define default output delegate
                //ConsoleApplication.defaultOutputDelegate = ConsoleApplication.messageBoxWrapperOutputDelegate;

                //subscribe to notifications
                this.PropertyChanged += PropertyChangedEventHandlerDelegate;

                InitViewModel();

                BindSizeAndLocation();
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }
        #endregion Constructors

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            try
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                throw;
            }
        }
        #endregion INotifyPropertyChanged

        #region PropertyChangedEventHandlerDelegate
        /// <summary>
        /// Note: model property changes update UI manually.
        /// Note: handle settings property changes manually.
        /// Note: because settings properties are a subset of the model 
        ///  (every settings property should be in the model, 
        ///  but not every model property is persisted to settings)
        ///  it is decided that for now the settigns handler will 
        ///  invoke the model handler as well.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void PropertyChangedEventHandlerDelegate
        (
            Object sender,
            PropertyChangedEventArgs e
        )
        {
            try
            {
                #region Model
                if (e.PropertyName == "IsChanged")
                {
                    //ConsoleApplication.defaultOutputDelegate(String.Format("{0}", e.PropertyName));
                    ApplySettings();
                }
                //Status Bar
                else if (e.PropertyName == "ActionIconIsVisible")
                {
                    StatusBarActionIcon.Visible = (ViewModel.ActionIconIsVisible);
                }
                else if (e.PropertyName == "ActionIconImage")
                {
                    StatusBarActionIcon.Image = (ViewModel != null ? ViewModel.ActionIconImage : (Image)null);
                }
                else if (e.PropertyName == "StatusMessage")
                {
                    //replace default action by setting control property
                    StatusBarStatusMessage.Text = (ViewModel != null ? ViewModel.StatusMessage : (String)null);
                    //e = new PropertyChangedEventArgs(e.PropertyName + ".handled");

                    //ConsoleApplication.defaultOutputDelegate(String.Format("{0}", StatusMessage));
                }
                else if (e.PropertyName == "ErrorMessage")
                {
                    //replace default action by setting control property
                    StatusBarErrorMessage.Text = (ViewModel != null ? ViewModel.ErrorMessage : (String)null);
                    //e = new PropertyChangedEventArgs(e.PropertyName + ".handled");

                    //ConsoleApplication.defaultOutputDelegate(String.Format("{0}", ErrorMessage));
                }
                else if (e.PropertyName == "ErrorMessageToolTipText")
                {
                    StatusBarErrorMessage.ToolTipText = ViewModel.ErrorMessageToolTipText;
                }
                else if (e.PropertyName == "ProgressBarValue")
                {
                    StatusBarProgressBar.Value = ViewModel.ProgressBarValue;
                }
                else if (e.PropertyName == "ProgressBarMaximum")
                {
                    StatusBarProgressBar.Maximum = ViewModel.ProgressBarMaximum;
                }
                else if (e.PropertyName == "ProgressBarMinimum")
                {
                    StatusBarProgressBar.Minimum = ViewModel.ProgressBarMinimum;
                }
                else if (e.PropertyName == "ProgressBarStep")
                {
                    StatusBarProgressBar.Step = ViewModel.ProgressBarStep;
                }
                else if (e.PropertyName == "ProgressBarIsMarquee")
                {
                    StatusBarProgressBar.Style = (ViewModel.ProgressBarIsMarquee ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks);
                }
                else if (e.PropertyName == "ProgressBarIsVisible")
                {
                    StatusBarProgressBar.Visible = (ViewModel.ProgressBarIsVisible);
                }
                else if (e.PropertyName == "DirtyIconIsVisible")
                {
                    StatusBarDirtyMessage.Visible = (ViewModel.DirtyIconIsVisible);
                }
                else if (e.PropertyName == "DirtyIconImage")
                {
                    StatusBarDirtyMessage.Image = ViewModel.DirtyIconImage;
                }
                //use if properties cannot be databound
                #endregion Model

                #region Settings
                if (e.PropertyName == "Dirty")
                {
                    //apply settings that don't have databindings
                    ViewModel.DirtyIconIsVisible = (DSClientSettingsController.Settings.Dirty);
                }
                #endregion Settings
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }
        #endregion PropertyChangedEventHandlerDelegate

        #region Properties
        private String _ViewName = Program.APP_NAME;
        public String ViewName
        {
            get { return _ViewName; }
            set
            {
                _ViewName = value;
                OnPropertyChanged("ViewName");
            }
        }
        #endregion Properties

        #region Events
        #region Form Events
        /// <summary>
        /// Process Form key presses.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override Boolean ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Boolean returnValue = default(Boolean);
            try
            {
                // Implement the Escape / Cancel keystroke
                if (keyData == Keys.Cancel || keyData == Keys.Escape)
                {
                    //if a long-running cancellable-action has registered 
                    //an escapable-event, trigger it
                    InvokeActionCancel();   
                    
                    // This keystroke was handled, 
                    //don't pass to the control with the focus
                    returnValue = true;     
                }
                else
                { 
                    returnValue = base.ProcessCmdKey(ref msg, keyData);
                }

            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        private void DocumentScannerViewer_Load(Object sender, EventArgs e)
        {
            try
            {
                ViewModel.StatusMessage = String.Format("{0} starting...", ViewName);

                ViewModel.StatusMessage = String.Format("{0} started.", ViewName);

                _Run();
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMessage = ex.Message;
                ViewModel.StatusMessage = String.Empty;

                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }

        private void DocumentScannerViewer_FormClosing(Object sender, FormClosingEventArgs e)
        {
            try
            {
                ViewModel.StatusMessage = String.Format("{0} completing...", ViewName);

                DisposeSettings();

                ViewModel.StatusMessage = String.Format("{0} completed.", ViewName);
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMessage = ex.Message.ToString();
                ViewModel.StatusMessage = "";

                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            finally
            {
                ViewModel = null;
            }
        }
        #endregion Form Events
        
        #region Menu Events
        private void menuFileNew_Click(Object sender, EventArgs e)
        {
            ViewModel.FileNew();
        }

        private void menuFileOpen_Click(Object sender, EventArgs e)
        {
            ViewModel.FileOpen();
        }

        private void menuFileSave_Click(Object sender, EventArgs e)
        {
            ViewModel.FileSave();
        }

        private void menuFileSaveAs_Click(Object sender, EventArgs e)
        {
            ViewModel.FileSaveAs();
        }

        private void menuFileExit_Click(Object sender, EventArgs e)
        {
            ViewModel.FileExit();
        }

        private void menuEditProperties_Click(Object sender, EventArgs e)
        {
            ViewModel.EditProperties();
        }

        private void menuEditCopyToClipboard_Click(Object sender, EventArgs e)
        {
            ViewModel.EditCopy();
        }

        private void menuHelpAbout_Click(Object sender, EventArgs e)
        {
            ViewModel.HelpAbout<AssemblyInfo>();
        }

        /// <summary>
        /// Confirmation menu Opening event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuConfirmation_Opening(Object sender, CancelEventArgs e)
        {
            ViewModel.Confirmation_Opening(sender as ContextMenuStrip);
        }

        /// <summary>
        /// Documents menu Opening event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuDocuments_Opening(Object sender, CancelEventArgs e)
        {
            ViewModel.Documents_Opening(sender as ContextMenuStrip);
        }

        /// <summary>
        /// Rotate document counter-clockwise one quarter turn.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuDocumentsRotateCCW90_Click(Object sender, EventArgs e)
        {
            ViewModel.Documents_RotateCCW90();
        }

        /// <summary>
        /// Rotate document clockwise one quarter turn.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuDocumentsRotateCW90_Click(Object sender, EventArgs e)
        {
            ViewModel.Documents_RotateCW90();
        }

        /// <summary>
        /// Move document item higher in the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuDocumentsPromoteDocument_Click(Object sender, EventArgs e)
        {
            ViewModel.Documents_PromoteDocument();
        }

        /// <summary>
        /// Move document item lower in the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuDocumentsDemoteDocument_Click(Object sender, EventArgs e)
        {
            ViewModel.Documents_DemoteDocument();
        }

        /// <summary>
        /// Print document.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuDocumentPrint_Click(Object sender, EventArgs e)
        {
            ViewModel.Documents_Print();
        }

        /// <summary>
        /// Initiate scan (acquire and transfer).
        /// Callback will process results.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuDocumentScan_Click(Object sender, EventArgs e)
        {
            ViewModel.File_Scan();
        }

        /// <summary>
        /// Open Twain UI to select scanner.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuFileSelect_Click(Object sender, EventArgs e)
        {
            ViewModel.File_Select();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuFilePackage_Click(Object sender, EventArgs e)
        {
            ViewModel.File_Package();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuFileSend_Click(Object sender, EventArgs e)
        {
            ViewModel.File_Send();
        }

        /// <summary>
        /// Given the Operator ID, a Transaction ID, and the specified date, 
        /// return a List(Of ImageFile) from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuConfirmListDocumentConfirmations_Click(Object sender, EventArgs e)
        {
            ViewModel.Confirmation_ListDocumentConfirmations();
        }
        
        /// <summary>
        /// Failed menu Opening event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuFailed_Opening(Object sender, CancelEventArgs e)
        {
            ViewModel.Failed_Opening(sender as ContextMenuStrip);
        }

        /// <summary>
        /// Given a (failed) package file path, move the package from the error folder to the data folder,
        /// and unpack it so that it can be edited.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuFailedUnpackage_Click(Object sender, EventArgs e)
        {
            ViewModel.Failed_Unpackage();
        }

        /// <summary>
        /// Given a saved package definition, split the definition above the selected document,
        /// and save the result in two separate package definitions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuDocumentsSplitPackageAbove_Click(Object sender, EventArgs e)
        {
            ViewModel.Documents_SplitPackageAbove();

        }

        /// <summary>
        /// Given a saved package definition, split the definition below the selected document,
        /// and save the result in two separate package definitions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuDocumentsSplitPackageBelow_Click(Object sender, EventArgs e)
        {
            ViewModel.Documents_SplitPackageBelow();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuReceiveListManifestDocuments_Click(Object sender, EventArgs e)
        {
            ViewModel.Receive_ListManifestDocuments();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuReceiveManifest_Click(Object sender, EventArgs e)
        {
            ViewModel.Receive_Manifest();
        }

        /// <summary>
        /// Receive menu Opening event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuReceive_Opening(Object sender, CancelEventArgs e)
        {
            ViewModel.Receive_Opening(sender as ContextMenuStrip);
        }
        #endregion Menu Events

        #region Control Events
        private void dgvDocuments_CurrentCellChanged(Object sender, EventArgs e)
        {
            try
            {
                DSClientModelController<DSClientModel>.Model.Refresh();
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        private void dgvDocuments_SelectionChanged(Object sender, EventArgs e)
        {
            try
            {
                DSClientModelController<DSClientModel>.Model.Refresh();
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        /// <summary>
        /// Show context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void failedDataGridView_CellMouseDown(Object sender, DataGridViewCellMouseEventArgs e)
        {
            ViewModel.Grid_CellMouseDown(sender as DataGridView, e.Button, e.RowIndex, e.X, e.Y);
        }

        /// <summary>
        /// Show context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void documents_CellMouseDown(Object sender, DataGridViewCellMouseEventArgs e)
        {
            ViewModel.Grid_CellMouseDown(sender as DataGridView, e.Button, e.RowIndex, e.X, e.Y);
        }

        /// <summary>
        /// Show context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void confirmedManifestsDataGridView_CellMouseDown(Object sender, DataGridViewCellMouseEventArgs e)
        {
            ViewModel.Grid_CellMouseDown(sender as DataGridView, e.Button, e.RowIndex, e.X, e.Y);
        }

        /// <summary>
        /// Bind grid on cell selection change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void confirmedManifestsDataGridView_CurrentCellChanged(Object sender, EventArgs e)
        {
            ViewModel.Grid_CurrentCellChanged(sender as DataGridView, BindConfirmedDocumentsUI);
        }

        /// <summary>
        /// Bind grid on row selection change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void confirmedManifestsDataGridView_SelectionChanged(Object sender, EventArgs e)
        {
            ViewModel.Grid_CurrentRowChanged(sender as DataGridView, BindConfirmedDocumentsUI);
        }

        /// <summary>
        /// Bind grid on cell selection change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void receivableManifestsDataGridView_CurrentCellChanged(Object sender, EventArgs e)
        {
            ViewModel.Grid_CurrentCellChanged(sender as DataGridView, BindAvailableDocumentsUI);
        }

        /// <summary>
        /// Bind grid on row selection change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void receivableManifestsDataGridView_SelectionChanged(Object sender, EventArgs e)
        {
            ViewModel.Grid_CurrentRowChanged(sender as DataGridView, BindAvailableDocumentsUI);
        }

        /// <summary>
        /// Show context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void receivableManifestsDataGridView_CellMouseDown(Object sender, DataGridViewCellMouseEventArgs e)
        {
            ViewModel.Grid_CellMouseDown(sender as DataGridView, e.Button, e.RowIndex, e.X, e.Y);
        }

        #region Background Workers
        #region ConfirmManifests
        /// <summary>
        /// Handle DoWork event for ConfirmManifests
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerConfirmManifests_DoWork(Object sender, DoWorkEventArgs e)
        {
            ViewModel.BackgroundWorker_DoWork
            (
                sender as BackgroundWorker,
                e,
                (
                    BackgroundWorker worker,
                    DoWorkEventArgs ea,
                    ref String errorMessage
                ) =>
                {
                    Boolean returnValue = default(Boolean);
                    List<PackageManifest> manifestList = default(List<PackageManifest>);

                    // Get Tuple object passed from RunWorkerAsync() method
                    Tuple<String /*operatorId*/, DateTime /*date*/> arguments =
                        e.Argument as Tuple<String /*operatorId*/, DateTime /*date*/>;

                    //run process
                    returnValue = 
                        DSClientModelController<DSClientModel>.ConfirmManifestsInBackground//Note:pass wrapper instead of this.--SJS
                        (
                            arguments.Item1,
                            arguments.Item2, 
                            worker,
                            ea,
                            ref manifestList,//List<ImageFile>
                            ref errorMessage
                        );
                    ea.Result = manifestList;
                    return true;
                }
            );
        }

        /// <summary>
        /// Handle ProgressChanged event for ConfirmManifests
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerConfirmManifests_ProgressChanged(Object sender, ProgressChangedEventArgs e)
        {
            ViewModel.BackgroundWorker_ProgressChanged("Listing confirmed manifests", e.UserState, e.ProgressPercentage);
        }

        /// <summary>
        /// Handle RunWorkerCompleted event for ConfirmManifests
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerConfirmManifests_RunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
        {
            ViewModel.BackgroundWorker_RunWorkerCompleted
            (
                "Confirmed manifests listed.",
                sender as BackgroundWorker,
                e,
                null,
                null,
                () =>
                {
                    //Confirmed manifests
                    confirmedManifestBindingSource.DataSource = (List<PackageManifest>)e.Result;

                    if (DSClientModelController<DSClientModel>.Model.AutoNavigateTabs)
                    {
                        //tabControl.SelectTab(tabPageConfirm);
                    }
                }
            );
        }
        #endregion ConfirmManifests

        #region AvailableManifests
        /// <summary>
        /// Handle DoWork event for AvailableManifests
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerAvailableManifests_DoWork(Object sender, DoWorkEventArgs e)
        {
            ViewModel.BackgroundWorker_DoWork
            (
                sender as BackgroundWorker,
                e,
                (
                    BackgroundWorker worker,
                    DoWorkEventArgs ea,
                    ref String errorMessage
                ) =>
                {
                    Boolean returnValue = default(Boolean);
                    List<PackageManifest> manifestList = default(List<PackageManifest>);

                    // Get Tuple object passed from RunWorkerAsync() method
                    Tuple<String /*operatorId*/> arguments =
                        e.Argument as Tuple<String /*operatorId*/>;

                    //run process
                    returnValue =
                        DSClientModelController<DSClientModel>.AvailableManifestsInBackground//Note:pass wrapper instead of this.--SJS
                        (
                            arguments.Item1,
                            worker,
                            ea,
                            ref manifestList,//List<ImageFile>
                            ref errorMessage
                        );
                    ea.Result = manifestList;
                    return true;
                }
            );
        }

        /// <summary>
        /// Handle ProgressChanged event for AvailableManifests
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerAvailableManifests_ProgressChanged(Object sender, ProgressChangedEventArgs e)
        {
            ViewModel.BackgroundWorker_ProgressChanged("Listing available manifests", e.UserState, e.ProgressPercentage);
        }

        /// <summary>
        /// Handle RunWorkerCompleted event for AvailableManifests
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerAvailableManifests_RunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
        {
            ViewModel.BackgroundWorker_RunWorkerCompleted
            (
                "Available manifests listed.",
                sender as BackgroundWorker,
                e,
                null,
                null,
                () =>
                {
                    //Available manifests
                    receivableManifestBindingSource.DataSource = (List<PackageManifest>)e.Result;

                    if (DSClientModelController<DSClientModel>.Model.AutoNavigateTabs)
                    {
                        tabControl.SelectTab(tabPageReceive);
                    }
                }
            );
        }
        #endregion AvailableManifests
        
        #region ConfirmDocuments
        //TODO:Background Workers ConfirmDocuments
        ///// <summary>
        ///// Handle DoWork event for ConfirmManifests
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void backgroundWorkerConfirmManifests_DoWork(Object sender, DoWorkEventArgs e)
        //{
        //    ViewModel.BackgroundWorker_DoWork
        //    (
        //        sender as BackgroundWorker,
        //        e,
        //        (
        //            BackgroundWorker worker,
        //            DoWorkEventArgs ea,
        //            ref String errorMessage
        //        ) =>
        //        {
        //            Boolean returnValue = default(Boolean);
        //            List<PackageManifest> manifestList = default(List<PackageManifest>);

        //            // Get Tuple object passed from RunWorkerAsync() method
        //            Tuple<String /*operatorId*/, DateTime /*date*/> arguments =
        //                e.Argument as Tuple<String /*operatorId*/, DateTime /*date*/>;

        //            //run process
        //            returnValue =
        //                DSClientModelController<DSClientModel>.ConfirmManifestsInBackground//Note:pass wrapper instead of this.--SJS
        //                (
        //                    arguments.Item1,
        //                    arguments.Item2,
        //                    worker,
        //                    ea,
        //                    ref manifestList,//List<ImageFile>
        //                    ref errorMessage
        //                );
        //            ea.Result = manifestList;
        //            return true;
        //        }
        //    );
        //}

        ///// <summary>
        ///// Handle ProgressChanged event for ConfirmManifests
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void backgroundWorkerConfirmManifests_ProgressChanged(Object sender, ProgressChangedEventArgs e)
        //{
        //    ViewModel.BackgroundWorker_ProgressChanged("Listing confirmed manifests", e.UserState, e.ProgressPercentage);
        //}

        ///// <summary>
        ///// Handle RunWorkerCompleted event for ConfirmManifests
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void backgroundWorkerConfirmManifests_RunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
        //{
        //    ViewModel.BackgroundWorker_RunWorkerCompleted
        //    (
        //        "Confirmed manifests listed.",
        //        sender as BackgroundWorker,
        //        e,
        //        null,
        //        null,
        //        () =>
        //        {
        //            //Confirmed manifests
        //            confirmedManifestBindingSource.DataSource = (List<PackageManifest>)e.Result;

        //            if (DSClientModelController<DSClientModel>.Model.AutoNavigateTabs)
        //            {
        //                //tabControl.SelectTab(tabPageConfirm);
        //            }
        //        }
        //    );
        //}
        #endregion ConfirmDocuments
        
        #region AvailableDocuments
        //TODO:Background Workers AvailableDocuments
        ///// <summary>
        ///// Handle DoWork event for AvailableManifests
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void backgroundWorkerAvailableManifests_DoWork(Object sender, DoWorkEventArgs e)
        //{
        //    ViewModel.BackgroundWorker_DoWork
        //    (
        //        sender as BackgroundWorker,
        //        e,
        //        (
        //            BackgroundWorker worker,
        //            DoWorkEventArgs ea,
        //            ref String errorMessage
        //        ) =>
        //        {
        //            Boolean returnValue = default(Boolean);
        //            List<PackageManifest> manifestList = default(List<PackageManifest>);

        //            // Get Tuple object passed from RunWorkerAsync() method
        //            Tuple<String /*operatorId*/> arguments =
        //                e.Argument as Tuple<String /*operatorId*/>;

        //            //run process
        //            returnValue =
        //                DSClientModelController<DSClientModel>.AvailableManifestsInBackground//Note:pass wrapper instead of this.--SJS
        //                (
        //                    arguments.Item1,
        //                    worker,
        //                    ea,
        //                    ref manifestList,//List<ImageFile>
        //                    ref errorMessage
        //                );
        //            ea.Result = manifestList;
        //            return true;
        //        }
        //    );
        //}

        ///// <summary>
        ///// Handle ProgressChanged event for AvailableManifests
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void backgroundWorkerAvailableManifests_ProgressChanged(Object sender, ProgressChangedEventArgs e)
        //{
        //    ViewModel.BackgroundWorker_ProgressChanged("Listing available manifests", e.UserState, e.ProgressPercentage);
        //}

        ///// <summary>
        ///// Handle RunWorkerCompleted event for AvailableManifests
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void backgroundWorkerAvailableManifests_RunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
        //{
        //    ViewModel.BackgroundWorker_RunWorkerCompleted
        //    (
        //        "Available manifests listed.",
        //        sender as BackgroundWorker,
        //        e,
        //        null,
        //        null,
        //        () =>
        //        {
        //            //Available manifests
        //            receivableManifestBindingSource.DataSource = (List<PackageManifest>)e.Result;

        //            if (DSClientModelController<DSClientModel>.Model.AutoNavigateTabs)
        //            {
        //                tabControl.SelectTab(tabPageReceive);
        //            }
        //        }
        //    );
        //}
        #endregion AvailableDocuments

        #region Send
        /// <summary>
        /// Handle DoWork event for Send
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerSend_DoWork(Object sender, DoWorkEventArgs e)
        {
            ViewModel.BackgroundWorker_DoWork
            (
                sender as BackgroundWorker,
                e,
                DSClientModelController<DSClientModel>.SendInBackground
            );
        }

        /// <summary>
        /// Handle ProgressChanged event for Send
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerSend_ProgressChanged(Object sender, ProgressChangedEventArgs e)
        {
            ViewModel.BackgroundWorker_ProgressChanged("Sending", e.UserState, e.ProgressPercentage);
        }

        /// <summary>
        /// Handle RunWorkerComplete event for Send
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerSend_RunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
        {
            ViewModel.BackgroundWorker_RunWorkerCompleted
            (
                "Sent.",
                sender as BackgroundWorker,
                e,
                (error) =>
                {
                    // Show the error message
                    ViewModel.StopProgressBar("Send Finished...", "One or more packages not sent.");

                    //go to Review tab to see failed packages too
                    if (DSClientModelController<DSClientModel>.Model.AutoNavigateTabs)
                    {
                        this.tabControl.SelectTab(this.tabPageReview);
                    }
                },
                null,
                () =>
                {

                    if (DSClientModelController<DSClientModel>.Model.AutoNavigateTabs)
                    {
                        tabControl.SelectTab(tabPageReview);
                    }
                }
            );
        }
        #endregion Send

        #region Receive
        /// <summary>
        /// Handle DoWork event for Receive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerReceive_DoWork(Object sender, DoWorkEventArgs e)
        {
            ViewModel.BackgroundWorker_DoWork
            (
                sender as BackgroundWorker,
                e,
                DSClientModelController<DSClientModel>.ReceiveInBackground
            );
        }

        /// <summary>
        /// Handle ProgressChanged event for Receive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerReceive_ProgressChanged(Object sender, ProgressChangedEventArgs e)
        {
            ViewModel.BackgroundWorker_ProgressChanged("Receiving", e.UserState, e.ProgressPercentage);
        }

        /// <summary>
        /// Handle RunWorkerComplete event for Receive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerReceive_RunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
        {
            ViewModel.BackgroundWorker_RunWorkerCompleted
            (
                "Received.", 
                sender as BackgroundWorker, 
                e, 
                (error) =>
                {
                    // Show the error message
                    ViewModel.StopProgressBar("Receive terminated...", "Package not received.");

                    //go to passed tab 
                    //if (DSClientModelController<DSClientModel>.Model.AutoNavigateTabs)
                    //{
                    //    this.tabControl.SelectTab(this.tabPageReceive);
                    //}
                },
                null,
                () =>
                {
                    //clear receivable manifests list
                    availableDocumentsDataGridView.ClearSelection();
                    receivableDocumentBindingSource.DataSource = new List<ImageFile>();

                    receivableManifestsDataGridView.ClearSelection();
                    receivableManifestBindingSource.DataSource = new List<PackageManifest>();

                    if (DSClientModelController<DSClientModel>.Model.AutoNavigateTabs)
                    {
                        tabControl.SelectTab(tabPageScan);
                    }
                }
            );
        }
        #endregion Receive

        #region Package
        /// <summary>
        /// Handle DoWork event for Package
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerPackage_DoWork(Object sender, DoWorkEventArgs e)
        {
            ViewModel.BackgroundWorker_DoWork
            (
                sender as BackgroundWorker,
                e,
                DSClientModelController<DSClientModel>.PackageInBackground
            );
        }

        /// <summary>
        /// Handle ProgressChanged event for Package
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerPackage_ProgressChanged(Object sender, ProgressChangedEventArgs e)
        {
            ViewModel.BackgroundWorker_ProgressChanged("Packaging", e.UserState, e.ProgressPercentage);
        }

        /// <summary>
        /// Handle RunWorkerCompleted event for Package
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerPackage_RunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
        {
            ViewModel.BackgroundWorker_RunWorkerCompleted
            (
                "Packaged.",
                sender as BackgroundWorker,
                e,
                null,
                null,
                () =>
                { 

                    if (DSClientModelController<DSClientModel>.Model.AutoNavigateTabs)
                    {
                        tabControl.SelectTab(tabPageSend);
                    }
                })
            ;
        }
        #endregion Package

        #region UnPackage
        /// <summary>
        /// Handle DoWork event for UnPackage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerUnPackage_DoWork(Object sender, DoWorkEventArgs e)
        {
            ViewModel.BackgroundWorker_DoWork
            (
                sender as BackgroundWorker,
                e,
                DSClientModelController<DSClientModel>.UnPackageInBackground
            );
        }

        /// <summary>
        /// Handle ProgressChanged event for UnPackage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerUnPackage_ProgressChanged(Object sender, ProgressChangedEventArgs e)
        {
            ViewModel.BackgroundWorker_ProgressChanged("UnPackaging", e.UserState, e.ProgressPercentage);
        }

        /// <summary>
        /// Handle RunWorkerCompleted event for UnPackage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerUnPackage_RunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
        {
            ViewModel.BackgroundWorker_RunWorkerCompleted
            (
                "UnPackaged.",
                sender as BackgroundWorker,
                e,
                null,
                null,
                () =>
                {

                    if (DSClientModelController<DSClientModel>.Model.AutoNavigateTabs)
                    {
                        tabControl.SelectTab(tabPageScan);
                    }
                }
            );
        }
        #endregion UnPackage

        #region Split Package Settings
        /// <summary>
        /// Handle DoWork event for Split Package Settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerSplitPackageSettings_DoWork(Object sender, DoWorkEventArgs e)
        {
            ViewModel.BackgroundWorker_DoWork
            (
                sender as BackgroundWorker, 
                e,
                DSClientModelController<DSClientModel>.SplitPackageSettingsInBackground
            );
        }

        /// <summary>
        /// Handle ProgressChanged event for Split Package Settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerSplitPackageSettings_ProgressChanged(Object sender, ProgressChangedEventArgs e)
        {
            ViewModel.BackgroundWorker_ProgressChanged("Splitting package settings", e.UserState, e.ProgressPercentage);
        }

        /// <summary>
        /// Handle RunWorkerCompleted event for Split Package Settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerSplitPackageSettings_RunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
        {
            ViewModel.BackgroundWorker_RunWorkerCompleted
            (
                "Package settings split.",
                sender as BackgroundWorker,
                e,
                null,
                null,
                () =>
                {
                    if (DSClientModelController<DSClientModel>.Model.AutoNavigateTabs)
                    {
                        tabControl.SelectTab(tabPageScan);
                    }
                }
            );
        }
        #endregion Split Package Settings

        #endregion Background Workers

        /// <summary>
        /// Handle DataError event; specifically, catch combobox cell data errors.
        /// This allows validation message to be displayed 
        ///  and still allows the user to fix and save the settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void documents_DataError(Object sender, DataGridViewDataErrorEventArgs e)
        {
            ViewModel.Grid_DataError(sender as DataGridView, e, true, false, false);
        }

        /// <summary>
        /// Handle DataError event; specifically, catch combobox cell data errors.
        /// This allows validation message to be displayed 
        ///  and still allows the user to fix and save the settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void receivableManifests_DataError(Object sender, DataGridViewDataErrorEventArgs e)
        {
            ViewModel.Grid_DataError(sender as DataGridView, e, true, false, false);
        }

        /// <summary>
        /// Handle DataError event; specifically, catch combobox cell data errors.
        /// This allows validation message to be displayed 
        ///  and still allows the user to fix and save the settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void availableDocuments_DataError(Object sender, DataGridViewDataErrorEventArgs e)
        {
            ViewModel.Grid_DataError(sender as DataGridView, e, true, false, false);
        }

        /// <summary>
        /// Given the Operator ID and the specified date, 
        /// return a List(Of PackageManifest) from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdListManifestConfirmations_Click(Object sender, EventArgs e)
        {
            // Declare Tuple object to pass multiple params to DoWork method.
            var arguments =
                Tuple.Create<String /*operatorId*/, DateTime /*date*/>
                (
                    DSClientSettingsController.Settings.Manifest.OperatorId,
                    confirmationDateTimePicker.Value
                );

            ViewModel.ListManifests("confirmed", backgroundWorkerConfirmManifests, arguments);
        }

        /// <summary>
        /// Given the Operator ID and the operator id, 
        /// return a List(Of PackageManifest) from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdListManifestsAvailable_Click(Object sender, EventArgs e)
        {
            // Declare Tuple object to pass multiple params to DoWork method.
            var arguments =
                Tuple.Create<String /*operatorId*/>
                (
                    DSClientSettingsController.Settings.Manifest.OperatorId
                );

            ViewModel.ListManifests("available", backgroundWorkerAvailableManifests, arguments);
        }
        #endregion Control Events
        #endregion Events

        #region Methods
        #region FormAppBase
        protected void InitViewModel()
        {
            try
            {
                //tell controller how model should notify view about non-persisted properties AND including model properties that may be part of settings
                ModelController<DSClientModel>.DefaultHandler = PropertyChangedEventHandlerDelegate;

                //tell controller how settings should notify view about persisted properties
                DSClientSettingsController.DefaultHandler = PropertyChangedEventHandlerDelegate;

                InitModelAndSettings();

                FileDialogInfo settingsFileDialogInfo =
                    new FileDialogInfo
                    (
                        DSClientSettingsController.FILE_NEW,
                        null,
                        null,
                        /*DSClientSettingsController.Settings*/SettingsBase.FileTypeExtension,
                        /*DSClientSettingsController.Settings*/SettingsBase.FileTypeDescription,
                        /*DSClientSettingsController.Settings*/SettingsBase.FileTypeName,
                        new String[] 
                        { 
                            "XML files (*.xml)|*.xml", 
                            "All files (*.*)|*.*" 
                        },
                        false,
                        default(Environment.SpecialFolder),
                        Environment.GetFolderPath(Environment.SpecialFolder.Personal).WithTrailingSeparator()
                    );

                //set dialog caption
                settingsFileDialogInfo.Title = this.Text;

                //class to handle standard behaviors
                ViewModelController<Bitmap, DSViewModel>.New
                (
                    ViewName,
                    new DSViewModel
                    (
                        this.PropertyChangedEventHandlerDelegate,
                        new Dictionary<String, Bitmap>() 
                        { 
                            { "Above", Resources.Above }, 
                            { "Below", Resources.Below }, 
                            { "Bottom", Resources.Bottom }, 
                            { "Copy", Resources.Copy },
                            { "Delete", Resources.Delete }, 
                            { "FastForward", Resources.FastForward }, 
                            { "FastRewind", Resources.FastRewind }, 
                            { "Forward", Resources.Forward }, 
                            { "New", Resources.New }, 
                            { "Open", Resources.Open },
                            { "Print", Resources.Print },
                            { "Properties", Resources.Properties },
                            { "Rewind", Resources.Rewind }, 
                            { "Save", Resources.Save },
                            { "Top", Resources.Top }, 
                            { "BoxEmpty", Resources.BoxEmpty }, 
                            { "BoxFull", Resources.BoxFull }, 
                            { "Download", Resources.Download }, 
                            { "Upload", Resources.Upload }, 
                            { "Package", Resources.Package }, 
                            { "List", Resources.List }, 
                            { "ListSplitAbove", Resources.ListSplitAbove }, 
                            { "ListSplitBelow", Resources.ListSplitBelow }, 
                            { "Network", Resources.Network }, 
                            { "Scan", Resources.Scan }, 
                            { "Search", Resources.Search }, 
                            { "RotateCCW", Resources.RotateCCW }, 
                            { "RotateCW", Resources.RotateCW } 
                        },
                        settingsFileDialogInfo,
                        this
                    )
                );
                ViewModel = ViewModelController<Bitmap, DSViewModel>.ViewModel[ViewName];

                BindFormUi();

                //Init config parameters
                if (!LoadParameters())
                {
                    throw new Exception(String.Format("Unable to load config file parameter(s)."));
                }

                //DEBUG:filename coming in is being converted/passed as DOS 8.3 format equivalent
                //Load
                if ((DSClientSettingsController.FilePath == null) || (DSClientSettingsController.Filename == DSClientSettingsController.FILE_NEW))
                {
                    //NEW
                    ViewModel.FileNew();
                }
                else
                {
                    //OPEN
                    ViewModel.FileOpen(false);
                }

    #if debug
                //debug view
                menuEditProperties_Click(sender, e);
    #endif

                //Display dirty state
                DSClientModelController<DSClientModel>.Model.Refresh();
            }
            catch (Exception ex)
            {
                if (ViewModel != null)
                {
                    ViewModel.ErrorMessage = ex.Message;
                }
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }

        protected void InitModelAndSettings()
        {
            //create Settings before first use by Model
            if (DSClientSettingsController.Settings == null)
            {
                DSClientSettingsController.New();
            }
            //Model properties rely on Settings, so don't call Refresh before this is run.
            if (DSClientModelController<DSClientModel>.Model == null)
            {
                DSClientModelController<DSClientModel>.New();
            }
        }

        protected void DisposeSettings()
        {
            //save user and application settings 
            Properties.Settings.Default.Save();

            if (DSClientSettingsController.Settings.Dirty)
            {
                //prompt before saving
                DialogResult dialogResult = MessageBox.Show("Save changes?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                switch (dialogResult)
                {
                    case DialogResult.Yes:
                        {
                            //SAVE
                            ViewModel.FileSave();

                            break;
                        }
                    case DialogResult.No:
                        {
                            break;
                        }
                    default:
                        {
                            throw new InvalidEnumArgumentException();
                        }
                }
            }

            //unsubscribe from model notifications
            DSClientModelController<DSClientModel>.Model.PropertyChanged -= PropertyChangedEventHandlerDelegate;
        }

        protected void _Run()
        {
        }
        #endregion FormAppBase

        #region Utility

        /// <summary>
        /// Bind Settings controls to SettingsController
        /// </summary>
        private void BindFormUi()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }
        
        /// <summary>
        /// Bind Settings controls to SettingsController
        /// </summary>
        private void BindModelUi()
        {
            String errorMessage = default(String);
            try
            {
                //Form
                //Manifest.DocumentFiles[n].DocumentType
                documentTypesBindingSource.DataSource = DocumentScannerCommon.DocumentType.GetDocumentTypes();

                //SEND
                //queued
                queuedBindingSource.DataSource =
                    (from path in DSClientModelController<DSClientModel>.Model.PackagesQueued
                     select new { Name = Path.GetFileNameWithoutExtension(path) }).ToList();

                //REVIEW
                //completed list
                completedBindingSource.DataSource =
                    (from path in DSClientModelController<DSClientModel>.Model.PackagesCompleted
                     select new { Name = Path.GetFileNameWithoutExtension(path) }).ToList();

                //failed list
                //Note: ToList overcomes a problem with binding IEnumerable<anonymoustype> to DataGridView,
                // where grid adds an empty row.
                failedBindingSource.DataSource =
                    (from path in DSClientModelController<DSClientModel>.Model.PackagesFailed
                     select new { Name = Path.GetFileNameWithoutExtension(path), Filename = Path.GetFileName(path), FilePath = path }).ToList();

                //SCAN
                //Manifest.Description
                manifestBindingSource.DataSource = DSClientSettingsController.Settings.Manifest;

                //Manifest.DocumentFiles
                documentBindingSource.DataSource = DSClientSettingsController.Settings.Manifest.DocumentFiles;

                //Document Image
                if (!ViewModel.RefreshImage(dgvDocuments, documentPictureBox, ref errorMessage))
                {
                    throw new Exception(String.Format("Unable to refresh image:\n{0}", errorMessage));
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        public void BindAvailableDocumentsUI
        (
            PackageManifest manifest
        )
        {
            try
            {
                receivableDocumentBindingSource.DataSource = manifest.DocumentFiles;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }

        public void BindConfirmedDocumentsUI
        (
            PackageManifest manifest
        )
        {
            try
            {
                confirmedDocumentBindingSource.DataSource = manifest.DocumentFiles;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }

        private void BindField<TControl, TModel>
        (
            TControl fieldControl,
            TModel model,
            String modelPropertyName,
            String controlPropertyName = "Text",
            Boolean formattingEnabled = false,
            DataSourceUpdateMode dataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged,
            Boolean reBind = true
        )
            where TControl : Control
        {
            try
            {
                fieldControl.DataBindings.Clear();
                if (reBind)
                {
                    fieldControl.DataBindings.Add(controlPropertyName, model, modelPropertyName, formattingEnabled, dataSourceUpdateMode);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Apply Settings to viewer.
        /// </summary>
        private void ApplySettings()
        {
            try
            {
                _ValueChangedProgrammatically = true;
                
                //apply document scanner model properties
                BindModelUi();

                this.StatusBarNetworkIndicator.Visible = NetworkInterface.GetIsNetworkAvailable();
                //TODO:encapsulate these so that they can be used by SetFunctionControlsEnable?
                this.menuFilePackage.Enabled = 
                    (
                        DSClientSettingsController.Settings.Valid()
                        &&
                        !DSClientSettingsController.Settings.Dirty
                    );
                if ((!this.menuFilePackage.Enabled) && (!DSClientSettingsController.Settings.Dirty))
                {
                    ViewModel.UpdateStatusBarMessages("", DSClientSettingsController.Settings.ErrorMessage);
                }
                this.buttonFilePackage.Enabled = this.menuFilePackage.Enabled;
                this.packageButton.Enabled = this.menuFilePackage.Enabled;

                this.menuFileSend.Enabled = 
                    (
                        NetworkInterface.GetIsNetworkAvailable() 
                        && 
                        DSClientModelController<DSClientModel>.Model.ArePackagesQueued
                    );
                this.buttonFileSend.Enabled = this.menuFileSend.Enabled;
                this.sendButton.Enabled = this.menuFileSend.Enabled;

                this.menuFileScan.Enabled = true;
                this.buttonFileScan.Enabled = this.menuFileScan.Enabled;
                this.scanButton.Enabled = this.menuFileScan.Enabled;

                //this.cancelButton.Enabled = false;

                //apply settings that shouldn't use databindings

                //apply settings that can't use databindings
                Text = Path.GetFileName(DSClientSettingsController.Filename) + " - " + ViewName;

                //apply settings that don't have databindings
                ViewModel.DirtyIconIsVisible = (DSClientSettingsController.Settings.Dirty);

                _ValueChangedProgrammatically = false;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        /// <summary>
        /// Set function button and menu to enable value, and cancel button to opposite.
        /// For now, do only disabling here and leave enabling based on biz logic 
        ///  to be triggered by refresh?
        /// </summary>
        /// <param name="functionButton"></param>
        /// <param name="functionMenu"></param>
        /// <param name="cancelButton"></param>
        /// <param name="enable"></param>
        internal void SetFunctionControlsEnable
        (
            Button functionButton,
            ToolStripButton functionToolbarButton,
            ToolStripMenuItem functionMenu,
            Button cancelButton,
            Boolean enable
        )
        {
            try
            {
                //stand-alone button
                if (functionButton != null)
                {
                    functionButton.Enabled = enable;
                }

                //toolbar button
                if (functionToolbarButton != null)
                {
                    functionToolbarButton.Enabled = enable;
                }

                //menu item
                if (functionMenu != null)
                {
                    functionMenu.Enabled = enable;
                }

                //stand-alone cancel button
                if (cancelButton != null)
                {
                    cancelButton.Enabled = !enable;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Invoke any delegate that has been registered 
        ///  to cancel a long-running background process.
        /// </summary>
        private void InvokeActionCancel()
        {
            try
            {
                //execute cancellation hook
                if (cancelDelegate != null)
                {
                    cancelDelegate();
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Load from app config; override with command line if present
        /// </summary>
        /// <returns></returns>
        private Boolean LoadParameters()
        {
            Boolean returnValue = default(Boolean);
            String _fileTransferServiceEndpointConfigurationName = default(String);
            String _packageManifestServiceEndpointConfigurationName = default(String);
            //Int32 _imageQualityPercent = default(Int32);
            Boolean _autoNavigateTabs = default(Boolean);
            Int32 _reNewWaitMilliseconds = default(Int32);
            String _dataPath = default(String);
            String _pushSendPath = default(String);
            String _pullReceivePath = default(String);
            Int32 _completedTransactionRetentionDays = default(Int32);
            Int32 _errorTransactionRetentionDays = default(Int32);

            try
            {
                if (!Configuration.ReadString("FileTransferServiceEndpointConfigurationName", out _fileTransferServiceEndpointConfigurationName))
                {
                    throw new Exception(String.Format("Unable to load FileTransferServiceEndpointConfigurationName: '{0}'", _fileTransferServiceEndpointConfigurationName));
                }
                DSClientModelController<DSClientModel>.Model.FileTransferServiceEndpointConfigurationName = _fileTransferServiceEndpointConfigurationName;

                if (!Configuration.ReadString("PackageManifestServiceEndpointConfigurationName", out _packageManifestServiceEndpointConfigurationName))
                {
                    throw new Exception(String.Format("Unable to load PackageManifestServiceEndpointConfigurationName: '{0}'", _packageManifestServiceEndpointConfigurationName));
                }
                DSClientModelController<DSClientModel>.Model.PackageManifestServiceEndpointConfigurationName = _packageManifestServiceEndpointConfigurationName;

                //if (!Configuration.ReadValue<Int32>("ImageQualityPercent", out _imageQualityPercent))
                //{
                //    throw new Exception(String.Format("Unable to load ImageQualityPercent: '{0}'", _imageQualityPercent));
                //}
                //DSClientModelController<DSClientModel>.Model.ImageQualityPercent = _imageQualityPercent;

                if (!Configuration.ReadValue<Boolean>("AutoNavigateTabs", out _autoNavigateTabs))
                {
                    throw new Exception(String.Format("Unable to load AutoNavigateTabs: '{0}'", _autoNavigateTabs));
                }
                DSClientModelController<DSClientModel>.Model.AutoNavigateTabs = _autoNavigateTabs;

                if (!Configuration.ReadValue<Int32>("ReNewWaitMilliseconds", out _reNewWaitMilliseconds))
                {
                    throw new Exception(String.Format("Unable to load ReNewWaitMilliseconds: '{0}'", _reNewWaitMilliseconds));
                }
                DSClientModelController<DSClientModel>.Model.ReNewWaitMilliseconds = _reNewWaitMilliseconds;

                if (!Configuration.ReadString("DataPath", out _dataPath))
                {
                    throw new Exception(String.Format("Unable to load DataPath: '{0}'", _dataPath));
                }
                DSClientModelController<DSClientModel>.Model.DataPath = _dataPath;

                if (!Configuration.ReadString("PushSendPath", out _pushSendPath))
                {
                    throw new Exception(String.Format("Unable to load PushSendPath: '{0}'", _pushSendPath));
                }
                DSClientModelController<DSClientModel>.Model.PushSendPath = _pushSendPath;

                if (!Configuration.ReadString("PullReceivePath", out _pullReceivePath))
                {
                    throw new Exception(String.Format("Unable to load PullReceivePath: '{0}'", _pullReceivePath));
                }
                DSClientModelController<DSClientModel>.Model.PullReceivePath = _pullReceivePath;

                if (!Configuration.ReadValue<Int32>("CompletedTransactionRetentionDays", out _completedTransactionRetentionDays))
                {
                    throw new Exception(String.Format("Unable to load CompletedTransactionRetentionDays: '{0}'", _completedTransactionRetentionDays));
                }
                DSClientModelController<DSClientModel>.Model.CompletedTransactionRetentionDays = _completedTransactionRetentionDays;

                if (!Configuration.ReadValue<Int32>("ErrorTransactionRetentionDays", out _errorTransactionRetentionDays))
                {
                    throw new Exception(String.Format("Unable to load ErrorTransactionRetentionDays: '{0}'", _errorTransactionRetentionDays));
                }
                DSClientModelController<DSClientModel>.Model.ErrorTransactionRetentionDays = _errorTransactionRetentionDays;

                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                //throw;
            }
            return returnValue;
        }

        private void BindSizeAndLocation()
        {
            //Note:Size must be done after InitializeComponent(); do Location this way as well.--SJS
            this.DataBindings.Add(new System.Windows.Forms.Binding("Location", global::DocumentScanner.Properties.Settings.Default, "Location", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.DataBindings.Add(new System.Windows.Forms.Binding("ClientSize", global::DocumentScanner.Properties.Settings.Default, "Size", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.ClientSize = global::DocumentScanner.Properties.Settings.Default.Size;
            this.Location = global::DocumentScanner.Properties.Settings.Default.Location;
        }
        #endregion Utility
        #endregion Methods
    }
}