using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using Ssepan.Application;
using System.IO;
using System.Linq;
using System.Reflection;
using Ssepan.Utility;
using TwainLib;
using DocumentScannerCommon;
//using TransferServiceClient;
using ManifestServiceClient;

namespace DocumentScannerLibrary
{
    /// <summary>
    /// run-time model; relies on settings
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DSModel :
        ModelBase
    {
        #region Declarations
        #endregion Declarations

        #region Constructors
        public DSModel() 
        {
            if (SettingsController<Settings>.Settings == null)
            {
                SettingsController<Settings>.New();
            }
            Debug.Assert(SettingsController<Settings>.Settings != null, "SettingsController<Settings>.Settings != null");
        }
        #endregion Constructors

        #region IEquatable<IModel>
        /// <summary>
        /// Compare property values of two specified Model objects.
        /// </summary>
        /// <param name="anotherSettings"></param>
        /// <returns></returns>
        public override Boolean Equals(IModel other)
        {
            Boolean returnValue = default(Boolean);
            DSModel otherModel = default(DSModel);

            try
            {
                otherModel = other as DSModel;

                if (this == otherModel)
                {
                    returnValue = true;
                }
                else
                {
                    if (!base.Equals(other))
                    {
                        returnValue = false;
                    }
                    else if (this.Version != otherModel.Version)
                    {
                        returnValue = false;
                    }
                    else if (!this.Manifest.Equals(otherModel.Manifest))
                    {
                        returnValue = false;
                    }
                    else
                    {
                        returnValue = true;
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
        #endregion IEquatable<IModel>

        #region Properties
        #region Persisted Properties
        public PackageManifest Manifest
        {
            get { return SettingsController<Settings>.Settings.Manifest; }
            set
            {
                SettingsController<Settings>.Settings.Manifest = value;
                OnPropertyChanged("Manifest");
            }
        }

        public String Version
        {
            get { return SettingsController<Settings>.Settings.Version; }
            set
            {
                SettingsController<Settings>.Settings.Version = value;
                OnPropertyChanged("Version");
            }
        }

        //public String UserName
        //{
        //    get { return SettingsController<Settings>.Settings.UserName; }
        //    set 
        //    { 
        //        SettingsController<Settings>.Settings.UserName = value;
        //        OnPropertyChanged("UserName");
        //    }
        //}
        #endregion Persisted Properties

        private TwainSource _TwainSource = default(TwainSource);
        public TwainSource TwainSource
        {
            get { return _TwainSource; }
            set 
            { 
                _TwainSource = value; 
                OnPropertyChanged("TwainSource");
            }
        }

        private String _FileTransferServiceEndpointConfigurationName = default(String);
        public String FileTransferServiceEndpointConfigurationName
        {
            get { return _FileTransferServiceEndpointConfigurationName; }
            set 
            { 
                _FileTransferServiceEndpointConfigurationName = value; 
                OnPropertyChanged("FileTransferServiceEndpointConfigurationName");
            }
        }

        private String _PackageManifestServiceEndpointConfigurationName = default(String);
        public String PackageManifestServiceEndpointConfigurationName
        {
            get { return _PackageManifestServiceEndpointConfigurationName; }
            set 
            { 
                _PackageManifestServiceEndpointConfigurationName = value; 
                OnPropertyChanged("PackageManifestServiceEndpointConfigurationName");
            }
        }

        //private Int32 _ImageQualityPercent = default(Int32);
        //public Int32 ImageQualityPercent
        //{
        //    get { return _ImageQualityPercent; }
        //    set 
            //{ 
            //    _ImageQualityPercent = value; 
                //OnPropertyChanged("ImageQualityPercent");
            //}
        //}

        private Boolean _AutoNavigateTabs = default(Boolean);
        public Boolean AutoNavigateTabs
        {
            get { return _AutoNavigateTabs; }
            set 
            { 
                _AutoNavigateTabs = value; 
                OnPropertyChanged("AutoNavigateTabs");
            }
        }

        private Int32 _ReNewWaitMilliseconds = default(Int32);
        public Int32 ReNewWaitMilliseconds
        {
            get { return _ReNewWaitMilliseconds; }
            set 
            { 
                _ReNewWaitMilliseconds = value; 
                OnPropertyChanged("ReNewWaitMilliseconds");
            }
        }

        private String _DataPath = default(String);
        public String DataPath
        {
            get { return _DataPath; }
            set 
            { 
                _DataPath = value; 
                OnPropertyChanged("DataPath");
            }
        }

        private String _PushSendPath = default(String);
        public String PushSendPath
        {
            get { return _PushSendPath; }
            set 
            { 
                _PushSendPath = value; 
                OnPropertyChanged("PushSendPath");
            }
        }

        private String _PullReceivePath = default(String);
        public String PullReceivePath
        {
            get { return _PullReceivePath; }
            set 
            { 
                _PullReceivePath = value; 
                OnPropertyChanged("PullReceivePath");
            }
        }
        
        private Int32 _CompletedTransactionRetentionDays = default(Int32);
        public Int32 CompletedTransactionRetentionDays
        {
            get { return _CompletedTransactionRetentionDays; }
            set 
            { 
                _CompletedTransactionRetentionDays = value; 
                OnPropertyChanged("CompletedTransactionRetentionDays");
            }
        }

        private Int32 _ErrorTransactionRetentionDays = default(Int32);
        public Int32 ErrorTransactionRetentionDays
        {
            get { return _ErrorTransactionRetentionDays; }
            set 
            { 
                _ErrorTransactionRetentionDays = value; 
                OnPropertyChanged("ErrorTransactionRetentionDays");
            }
        }

        /// <summary>
        /// Location of packages to be sent per transaction.
        /// </summary>
        public String TransactionRootPath
        {
            get { return PushSendPath; }
        }

        /// <summary>
        /// Location of 
        /// </summary>
        public String TransactionWorkingPath
        {
            get { return Path.Combine(TransactionRootPath, DocumentScannerCommon.Common.TRANSACTION_FOLDER_WORKING); }
        }

        public String TransactionCompletedPath
        {
            get { return Path.Combine(TransactionRootPath, DocumentScannerCommon.Common.TRANSACTION_FOLDER_COMPLETED); }
        }

        public String TransactionErrorPath
        {
            get { return Path.Combine(TransactionRootPath, DocumentScannerCommon.Common.TRANSACTION_FOLDER_ERROR); }
        }

        /// <summary>
        /// Indicate whether packages are present in transmit-queue.
        /// </summary>
        /// <returns></returns>
        public Boolean ArePackagesQueued
        {
            get 
            {
                return (PackagesQueuedCount > 0); 
            }
        }

        /// <summary>
        /// Indicate how many packages present in transmit-queue.
        /// </summary>
        /// <returns></returns>
        public Int32 PackagesQueuedCount
        {
            get 
            {
                return PackagesQueued.Count(); 
            }
        }
        
        /// <summary>
        /// List of packages present in transmit-queue.
        /// </summary>
        /// <returns></returns>
        public List<String> PackagesQueued
        {
            get 
            {
                //get file list in path, by type of package file 
                return Directory.GetFiles(TransactionRootPath, String.Format("*.{0}", DocumentScannerCommon.Package.PACKAGE_FILE_TYPE)).ToList();
            }
        }
        
        /// <summary>
        /// List of packages present in completed folder.
        /// </summary>
        /// <returns></returns>
        public List<String> PackagesCompleted
        {
            get 
            {
                //get file list in path, by type of package file 
                return Directory.GetFiles(TransactionCompletedPath, String.Format("*.{0}", DocumentScannerCommon.Package.PACKAGE_FILE_TYPE), SearchOption.AllDirectories).ToList();
            }
        }
        
        /// <summary>
        /// List of packages present in error folder.
        /// </summary>
        /// <returns></returns>
        public List<String> PackagesFailed
        {
            get 
            {
                //get file list in path, by type of package file 
                return Directory.GetFiles(TransactionErrorPath, String.Format("*.{0}", DocumentScannerCommon.Package.PACKAGE_FILE_TYPE), SearchOption.AllDirectories).ToList();
            }
        }
        #endregion Properties

        #region Methods
        #endregion Methods
    }
}
