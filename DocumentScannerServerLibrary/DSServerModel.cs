using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Ssepan.Application;
using Ssepan.Utility;
using DocumentScannerCommon;

namespace DocumentScannerServerLibrary
{
    /// <summary>
    /// run-time model; relies on settings
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DSServerModel :
        ModelBase
    {
        #region Declarations
        #endregion Declarations

        #region Constructors
        public DSServerModel() 
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
        public override Boolean Equals(IModelComponent other)
        {
            Boolean returnValue = default(Boolean);
            DSServerModel otherModel = default(DSServerModel);

            try
            {
                otherModel = other as DSServerModel;

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
                    //else if (!this.SubObject.Equals(otherSettings.SubObject))
                    //{
                    //    returnValue = false;
                    //}
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
        public String Version
        {
            get { return SettingsController<Settings>.Settings.Version; }
            set
            {
                SettingsController<Settings>.Settings.Version = value;
                OnPropertyChanged("Version");
            }
        }
        #endregion Persisted Properties

        private String _UserSendFolder = default(String);
        public String UserSendFolder
        {
            get { return _UserSendFolder; }
            set 
            { 
                _UserSendFolder = value; 
                OnPropertyChanged("UserSendFolder");
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

        private String _PushReceivePath = default(String);
        public String PushReceivePath
        {
            get { return _PushReceivePath; }
            set 
            { 
                _PushReceivePath = value; 
                OnPropertyChanged("PushReceivePath");
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

        private Int32 _ReCreateWaitMilliseconds = default(Int32);
        public Int32 ReCreateWaitMilliseconds
        {
            get { return _ReCreateWaitMilliseconds; }
            set 
            { 
                _ReCreateWaitMilliseconds = value; 
                OnPropertyChanged("ReCreateWaitMilliseconds");
            }
        }

        /// <summary>
        /// Location of packages to be sent per transaction.
        /// </summary>
        public String TransactionRootPath
        {
            get { return PushReceivePath; }
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

         ///<summary>
         ///List of packages present in transmit-queue.
         ///</summary>
         ///<returns></returns>
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
