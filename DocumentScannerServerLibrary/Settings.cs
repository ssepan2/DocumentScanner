using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Ssepan.Application;
using Ssepan.Utility;
using System.Diagnostics;
using System.Reflection;

namespace DocumentScannerServerLibrary
{
	/// <summary>
    /// persisted settings; run-time model depends on this
    /// </summary>
    //[DataContract]//was this
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Serializable]
    public class Settings :
        SettingsBase
    {
        #region Declarations
        private const String FILE_TYPE_EXTENSION = "DocumentScannerServer"; //"xml";
        private const String FILE_TYPE_NAME = "documentscannerserver";
        private const String FILE_TYPE_DESCRIPTION = "DocumentScannerServer Settings File";
        #endregion Declarations

        #region Constructors
        public Settings()
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
        ~Settings()
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
            Settings otherSettings = default(Settings);

            try
            {
                otherSettings = other as Settings;

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

        #region Persisted Properties
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
            Settings destinationSettings = default(Settings);

            try
            {
                destinationSettings = destination as Settings;

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
