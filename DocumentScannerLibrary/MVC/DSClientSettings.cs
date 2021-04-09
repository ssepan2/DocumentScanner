using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Ssepan.Application;
using Ssepan.Application.MVC;
using Ssepan.Collections;
using Ssepan.Utility;
using DocumentScannerCommon;

namespace DocumentScannerLibrary.MVC
{
	/// <summary>
    /// persisted settings; run-time model depends on this
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    public class DSClientSettings :
        SettingsBase
    {
        #region Declarations
        private const String FILE_TYPE_EXTENSION = "documentscanner";
        private const String FILE_TYPE_NAME = "documentscannerfile";
        private const String FILE_TYPE_DESCRIPTION = "DocumentScanner Settings File";

        #region Enums
        public enum ColorSchemeTypes
        {
            [XmlEnum(Name = "Mono")]
            Mono,
            [XmlEnum(Name = "Color")]
            Color            
        }

        public enum ImageFormatTypes
        {
            [XmlEnum(Name = "Metafile")]
            Metafile,
            [XmlEnum(Name = "Bitmap")]
            Bitmap
        }
        #endregion Enums
        #endregion Declarations

        #region Constructors
        public DSClientSettings()
        {
            try
            {
                FileTypeExtension = FILE_TYPE_EXTENSION;
                FileTypeName = FILE_TYPE_NAME;
                FileTypeDescription = FILE_TYPE_DESCRIPTION;
                SerializeAs = SerializationFormat.Xml;//default

                Manifest = new PackageManifest();
                Manifest.TransactionId = Guid.NewGuid().ToString();
                Manifest.OperatorId = Environment.UserName;
                Manifest.SetDocumentFilesListChangedDelegate(DocumentFiles_ListChanged);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                throw;
            }
        }
        #endregion Constructors

        #region IDisposable support
        ~DSClientSettings()
        {
            Dispose(false);
        }

        //inherited; override if additional cleanup needed
        protected override void Dispose(Boolean disposeManagedResources)
        {
            // process only if mananged and unmanaged resources have
            // not been disposed of.
            if (!disposed)
            {
                try
                {
                    //Resources not disposed
                    if (disposeManagedResources)
                    {
                        // dispose managed resources
                    }

                    disposed = true;
                }
                finally
                {
                    // dispose unmanaged resources
                    base.Dispose(disposeManagedResources);
                }
            }
            else
            {
                //Resources already disposed
            }
        }
        #endregion

        #region IEquatable<ISettings>
        /// <summary>
        /// Compare property values of two specified Settings objects.
        /// </summary>
        /// <param name="anotherSettings"></param>
        /// <returns></returns>
        public override Boolean Equals(ISettingsComponent other)
        {
            Boolean returnValue = default(Boolean);
            DSClientSettings otherSettings = default(DSClientSettings);

            try
            {
                otherSettings = other as DSClientSettings;

                if (this == otherSettings)
                {
                    returnValue = true;
                }
                else
                {
                    if (!base.Equals(other))
                    {
                        returnValue = false;
                    }
                    else if (this.Version != otherSettings.Version)
                    {
                        returnValue = false;
                    }
                    else if (!this.Manifest.Equals(otherSettings.Manifest))
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
        #endregion IEquatable<ISettings>

        #region ListChanged handlers
        void DocumentFiles_ListChanged(Object sender, ListChangedEventArgs e)
        {
            try
            {
                this.OnPropertyChanged(String.Format("Settings.Manifest.DocumentFiles[{0}].{1}", e.NewIndex, (e.PropertyDescriptor == null ? String.Empty : e.PropertyDescriptor.Name)));
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }
        #endregion ListChanged handlers

        #region AddingNew handlers
        //void DocumentFiles_AddingNew(Object sender, AddingNewEventArgs e)
        //{
        //    try
        //    {
        //        e.NewObject = new ImageFile(String.Format("{0}{1}", Guid.NewGuid().ToString(), ImageFile.FILE_EXTENSION));
        //    }
        //    catch (Exception ex)
        //    {
                //Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
        //        throw;
        //    }
        //}
        #endregion ListChanged handlers

        #region Properties
        [XmlIgnore]
        public override Boolean Dirty
        {
            get
            {
                Boolean returnValue = default(Boolean);

                try
                {
                    if (base.Dirty)
                    {
                        returnValue = true;
                    }
                    //else if ((this._Manifest == null) && (this.__Manifest == null))
                    //{
                    //}
                    else if ((this._Manifest == null) || (this.__Manifest == null))
                    {
                        //if either is true, we're not going to get a match, and it will show as dirty
                        returnValue = true;
                    }
                    else if (!this._Manifest.Equals(this.__Manifest))
                    {
                        returnValue = true;
                    }
                    else if (this._Version != this.__Version)
                    {
                        returnValue = true;
                    }
                    //else if (this._UserName != this.__UserName)
                    //{
                    //    returnValue = true;
                    //}
                    else
                    {
                        returnValue = false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                    throw;
                }

                return returnValue;
            }
        }

        private String _ErrorMessage = default(String);
        [XmlIgnore]
        public String ErrorMessage
        {
            get { return _ErrorMessage; }
            set { _ErrorMessage = value; }
        }

        //note:serializer won't serialize properties that contain the default value.
        #region Persisted Properties
        private PackageManifest __Manifest = default(PackageManifest);
        private PackageManifest _Manifest = default(PackageManifest);
        [DescriptionAttribute("Document file manifest."),
        CategoryAttribute("Manifest")]
        public PackageManifest Manifest
        {
            get { return _Manifest; }
            set 
            {
                if (Manifest != null)
                {
                    if (Manifest.DocumentFiles != null)
                    {
                        Manifest.DocumentFiles.ListChanged -= new ListChangedEventHandler(DocumentFiles_ListChanged);
                        //DocumentFiles.AddingNew -= new AddingNewEventHandler(DocumentFiles_AddingNew);
                    }
                }
                _Manifest = value;
                if (Manifest != null)
                {
                    if (Manifest.DocumentFiles != null)
                    {
                        //DocumentFiles.AddingNew += new AddingNewEventHandler(DocumentFiles_AddingNew);
                        Manifest.DocumentFiles.ListChanged += new ListChangedEventHandler(DocumentFiles_ListChanged);
                    }
                }
                OnPropertyChanged("Manifest");
            }
        }

        private String __Version = "0";
        private String _Version = "0";
        [DescriptionAttribute("Application major version"),
        CategoryAttribute("Misc"),
        DefaultValueAttribute(null)]
        public String Version
        {
            get { return _Version; }
            set
            {
                _Version = value;
                OnPropertyChanged("Version");
            }
        }

        //private String __UserName = FILE_NEW;
        //private String _UserName = FILE_NEW;
        ///// <summary>
        ///// Identifier used to store and retrieve the settings.
        ///// </summary>
        //[DescriptionAttribute("Identifier used to store and retrieve the settings."), 
        //CategoryAttribute("Misc"),
        //DefaultValueAttribute("(new)")]
        //public String UserName
        //{
        //    get { return _UserName; }
        //    set 
        //    { 
        //        _UserName = value;
        //        OnPropertyChanged("UserName");
        //    }
        //}

        #endregion Persisted Properties
        #endregion Properties

        #region Methods
        /// <summary>
        /// validate settings entered by user
        /// </summary>
        public Boolean Valid()
        {
            Boolean returnValue = default(Boolean);

            try
            {
                ErrorMessage = String.Empty;

                if (!_Manifest.Valid(DSClientModelController<DSClientModel>.GetTransactionImagesPath(false)))
                { 
                    returnValue = false;
                    ErrorMessage = _Manifest.ErrorMessage;
                }
                //else if (false)
                //{
                //    returnValue = false;
                //    ErrorMessage = String.Format("some other non-manifest, settings condition");
                //}
                else
                {
                    returnValue = true;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }

            return returnValue;
        }

        /// <summary>
        /// validate settings determined at the time of the collate
        /// </summary>
        public Boolean Complete()
        {
            Boolean returnValue = default(Boolean);

            try
            {
                ErrorMessage = String.Empty;

                if (!Valid())
                {
                    returnValue = false;
                    //ErrorMessage = String.Format("");
                }
                //else if (false)
                //{
                //    returnValue = false;
                //    ErrorMessage = String.Format("some other non-input, runtime criteria.");
                //}
                else
                {
                    returnValue = true;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }

            return returnValue;
        }

        /// <summary>
        /// Copies property values from source working fields to destination working fields, then optionally syncs destination.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="sync"></param>
        public override void CopyTo(ISettingsComponent destination, Boolean sync)
        {
            DSClientSettings destinationSettings = default(DSClientSettings);

            try
            {
                destinationSettings = destination as DSClientSettings;

                destinationSettings.Version = this.Version;
                //destinationSettings.UserName = this.UserName;
                destinationSettings.Manifest = this.Manifest;
                //this.PackageManifest.CopyTo(destinationSettings.PackageManifest);

                base.CopyTo(destination, sync);//also checks and optionally performs sync
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        /// <summary>
        /// Syncs property values from working fields to reference fields.
        /// </summary>
        public override void Sync()
        {
            try
            {
                __Version = _Version;
                //this.__UserName = this._UserName;
                if (_Manifest != null)
                {
                    __Manifest = ObjectHelper.Clone<PackageManifest>(_Manifest);
                }
                else
                { 
                    __Manifest = null;
                }

                base.Sync();

                if (Dirty)
                {
                    throw new ApplicationException("Sync failed.");
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        /// <summary>
        /// Set the delegate for instances that may not call new (clones).
        /// In order to de-couple certain objects with collections from settings,
        /// it is necessary to provide a mechanism to continue to wire up list changed events.
        /// </summary>
        public void SetDocumentFilesListChangedDelegate()
        {
            try
            {
                Manifest.SetDocumentFilesListChangedDelegate(DocumentFiles_ListChanged);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }
        #endregion Methods

    }
}
