using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Ssepan.Application;
//using Ssepan.Application.MVC;
using Ssepan.Utility;
using System.Diagnostics;
using System.Reflection;
using DocumentScannerCommon;

namespace DocumentScannerServerLibrary.MVC
{
	/// <summary>
    /// persisted settings; run-time model depends on this
    /// </summary>
    //[DataContract]//was this
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    public class DSServerSettings :
        SettingsBase
    {
        #region Declarations
        private const String FILE_TYPE_EXTENSION = "DocumentScannerServer"; //"xml";
        private const String FILE_TYPE_NAME = "documentscannerserverfile";
        private const String FILE_TYPE_DESCRIPTION = "DocumentScannerServer Settings File";
        #endregion Declarations

        #region Constructors
        public DSServerSettings()
        {
            try
            {
                FileTypeExtension = FILE_TYPE_EXTENSION;
                FileTypeName = FILE_TYPE_NAME;
                FileTypeDescription = FILE_TYPE_DESCRIPTION;
                SerializeAs = SerializationFormat.Xml;//default
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }
        #endregion Constructors

        #region IDisposable support
        ~DSServerSettings()
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
        #endregion IDisposable support

        #region IEquatable<ISettings>
        /// <summary>
        /// Compare property values of two specified Settings objects.
        /// </summary>
        /// <param name="anotherSettings"></param>
        /// <returns></returns>
        public override Boolean Equals(ISettingsComponent other)
        {
            Boolean returnValue = default(Boolean);
            DSServerSettings otherSettings = default(DSServerSettings);

            try
            {
                otherSettings = other as DSServerSettings;

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
                    else if (_Version != __Version)
                    {
                        returnValue = true;
                    }
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
        [ReadOnly(true)]
        [DescriptionAttribute("Application major version"),
        CategoryAttribute("Misc"),
        DefaultValueAttribute(null)]
        [DataMember]
        public String Version
        {
            get { return _Version; }
            set
            {
                _Version = value;
                OnPropertyChanged("Version");
            }
        }
        #endregion Persisted Properties
        #endregion Properties

        #region Methods
        /// <summary>
        /// Copies property values from source working fields to detination working fields, then optionally syncs destination.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="sync"></param>
        public override void CopyTo(ISettingsComponent destination, Boolean sync)
        {
            DSServerSettings destinationSettings = default(DSServerSettings);

            try
            {
                destinationSettings = destination as DSServerSettings;

                destinationSettings.Version = this.Version;

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
        #endregion Methods
    }
}
