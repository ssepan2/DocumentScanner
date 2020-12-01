using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Microsoft.VisualBasic.FileIO;
using Ssepan.Collections;
using Ssepan.Io;
using Ssepan.Utility;
using System.Diagnostics;
using System.Reflection;

namespace DocumentScannerCommon
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PackageManifest :
        IEquatable<PackageManifest>,
        INotifyPropertyChanged
    {
        #region Declarations
        public const String DATA_FILE_TYPE = "xml";

        #region Delegates
        [XmlIgnore]
        [field:NonSerialized]
        private Action<Object, ListChangedEventArgs> documentFilesListChangedDelegate = null;
        #endregion Delegates
        #endregion Declarations

        #region IEquatable<T> Members
        public bool Equals(PackageManifest other)
        {
            Boolean returnValue = default(Boolean);

            try
            {
                if ((this == null) || (other == null))
                {
                    returnValue = false;
                }
                else if (!this.DocumentFiles.Equals(other.DocumentFiles))
                {
                    returnValue = false;
                }
                else if (this.Description != other.Description)
                {
                    returnValue = false;
                }
                else if (this.TransactionId != other.TransactionId)
                {
                    returnValue = false;
                }
                else if (this.OperatorId != other.OperatorId)
                {
                    returnValue = false;
                }
                //else if (this.FileImageFormat != other.FileImageFormat)
                //{
                //    returnValue = false;
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
        #endregion IEquatable<T> Members

        #region INotifyPropertyChanged support
        //If property of ISettings object changes, fire PropertyChanged, which notifies any subscribed observers.
        //Called by all 'set' statements in ISettigns object properties.
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            try
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
#if debug
                    Log.Write(MethodBase.GetCurrentMethod().DeclaringType.Module.Name, MethodBase.GetCurrentMethod() + Log.FormatEntry(String.Format("PropertyChanged: {0}", propertyName), MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name), EventLogEntryType.Information);
#endif
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                //throw;
            }
        }
        #endregion INotifyPropertyChanged support

        #region NonPersisted Properties
        private String _ErrorMessage = default(String);
        [XmlIgnore]
        public String ErrorMessage
        {
            get { return _ErrorMessage; }
            set { _ErrorMessage = value; }
        }
        #endregion NonPersisted Properties

        #region Persisted Properties
        //note:serializer won't serialize properties that contain the default value.
        private OrderedEquatableBindingList<ImageFile> _DocumentFiles = new OrderedEquatableBindingList<ImageFile>();
        /// <summary>
        /// Document file list.
        /// </summary>
        [DescriptionAttribute("Document file list."),
        CategoryAttribute("Manifest")]
        public OrderedEquatableBindingList<ImageFile> DocumentFiles
        {
            get { return _DocumentFiles; }
            set
            {
                if (DocumentFiles != null)
                {
                    DocumentFiles.ListChanged -= new ListChangedEventHandler(documentFilesListChangedDelegate);
                    //DocumentFiles.AddingNew -= new AddingNewEventHandler(DocumentFiles_AddingNew);
                }
                _DocumentFiles = value;
                if (DocumentFiles != null)
                {
                    //DocumentFiles.AddingNew += new AddingNewEventHandler(DocumentFiles_AddingNew);
                    DocumentFiles.ListChanged += new ListChangedEventHandler(documentFilesListChangedDelegate);
                }
                this.OnPropertyChanged("DocumentFiles");
            }
        }

        private String _Description = String.Empty;
        /// <summary>
        /// Describe Manifest.
        /// </summary>
        [DescriptionAttribute("Description."),
        CategoryAttribute("Manifest")]
        public String Description
        {
            get { return _Description; }
            set 
            { 
                _Description = value;
                this.OnPropertyChanged("Description");
            }
        }

        private String _TransactionId = String.Empty;
        /// <summary>
        /// Transaction-specific guid.
        /// </summary>
        [DescriptionAttribute("Transaction ID."),
        CategoryAttribute("Manifest")]
        public String TransactionId
        {
            get { return _TransactionId; }
            set 
            { 
                _TransactionId = value;
                this.OnPropertyChanged("TransactionId");
            }
        }

        private String _OperatorId = String.Empty;
        /// <summary>
        /// Username of operator
        /// </summary>
        [DescriptionAttribute("Operator ID."),
        CategoryAttribute("Manifest")]
        public String OperatorId
        {
            get { return _OperatorId; }
            set 
            { 
                _OperatorId = value;
                this.OnPropertyChanged("OperatorId");
            }
        }

        private Int32 _Count = default(Int32);
        /// <summary>
        /// A snapshot of the length of the Documents list.
        /// Note: the count should be be serialized from the list count, but not de-serialized from XML.
        /// </summary>
        [DescriptionAttribute("A snapshot of the length of the Documents list."),
        CategoryAttribute("Manifest"),
        ReadOnlyAttribute(true)]
        public Int32 Count
        {
            get
            {
                Int32 returnValue = default(Int32);
                try
                {
                    returnValue = DocumentFiles.Count;
                }
                catch (Exception ex)
                {
                    Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                    //throw;
                }
                return returnValue;
            }
            set
            {
                //throw-away to allow serialization; value comes from collection count
                _Count = value;
            }
        }

        //private ImageFormatTypes _FileImageFormat = ImageFormatTypes.Bitmap;
        // /// <summary>
        // /// Determines format used when graph is exported to a file.
        // /// </summary>
        // [DescriptionAttribute("Determines format used when graph is exported to a file."),
        // CategoryAttribute("Behavior"),
        //DefaultValueAttribute(Settings.ImageFormatTypes.Bitmap),
        //ReadOnlyAttribute(true)]
        // public ImageFormatTypes FileImageFormat
        // {
        //     get { return _FileImageFormat; }
        //     set 
        //     { 
        //         _FileImageFormat = value;
        //         this.OnPropertyChanged("FileImageFormat");
        //     }
        // }
        #endregion Persisted Properties

        #region non-static methods
        /// <summary>
        /// validate settings entered by user
        /// </summary>
        /// <param name="imageFolderPath"></param>
        /// <returns></returns>
        public Boolean Valid(String imageFolderPath)
        {
            Boolean returnValue = default(Boolean);

            try
            {
                ErrorMessage = String.Empty;

                if (this == null)
                {
                    returnValue = false;
                    ErrorMessage = String.Format("Manifest is null.");
                }
                else if (this.DocumentFiles == null)
                {
                    returnValue = false;
                    ErrorMessage = String.Format("DocumentFiles is null.");
                }
                else if (this.DocumentFiles.Count == 0)
                {
                    returnValue = false;
                    ErrorMessage = String.Format("DocumentFiles must contain one or more documents.");
                }
                else if ((this.Description == null) || (this.Description == String.Empty))
                {
                    returnValue = false;
                    ErrorMessage = String.Format("Manifest Description is not set.");
                }
                else if (this.DocumentFiles.Any(documentFile => !documentFile.Present(imageFolderPath)))
                {
                    returnValue = false;
                    ErrorMessage = String.Format("DocumentFiles contains one or more document filenames that do not exist.");
                }
                else if (this.DocumentFiles.Any(documentFile => (documentFile.DocumentType == null) || (documentFile.DocumentType == String.Empty)))
                {
                    returnValue = false;
                    ErrorMessage = String.Format("DocumentFiles contains one or more documents without a document type.");
                }
                ////check for any image file document types that do not match any known document types
                //else if (this.DocumentFiles.Any(documentFile => (!DocumentScannerCommon.DocumentType.GetDocumentTypes().Any(dt => dt.Name == documentFile.DocumentType))))
                //{
                //    returnValue = false;
                //    ErrorMessage = String.Format("DocumentFiles contains one or more documents without a known document type.");
                //}
                else if (this.DocumentFiles.Any(documentFile => (documentFile.Description == null) || (documentFile.Description == String.Empty)))
                {
                    returnValue = false;
                    ErrorMessage = String.Format("DocumentFiles contains one or more documents without a description.");
                }
                else if ((this.OperatorId == default(String)) || (this.OperatorId == String.Empty))
                {
                    returnValue = false;
                    ErrorMessage = String.Format("OperatorId is not set.");
                }
                else if ((this.TransactionId == default(String)) || (this.TransactionId == String.Empty))
                {
                    returnValue = false;
                    ErrorMessage = String.Format("TransactionId is not set.");
                }
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
        public void CopyTo(PackageManifest destination)
        {
            try
            {
                if (destination == null)
                {
                    throw new ArgumentNullException("Destination instance of Manifest is null.");
                }
                destination.DocumentFiles = this.DocumentFiles;
                destination.Description = this.Description;
                destination.TransactionId = this.TransactionId;
                destination.OperatorId = this.OperatorId;
                //destination.FileImageFormat = this.FileImageFormat;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        /// <summary>
        /// Sets delegate to be used for ListChanged events on DocumentFiles.
        /// </summary>
        /// <param name="listChangedDelegate"></param>
        public void SetDocumentFilesListChangedDelegate(Action<Object, ListChangedEventArgs> listChangedDelegate)
        {
            try
            {
                documentFilesListChangedDelegate = listChangedDelegate;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }
        #endregion non-static methods


        /// <summary>
        /// Loads the specified object with data from the specified file.
        /// </summary>
        /// <param name="filePath"></param>
        public static PackageManifest Load(String filePath)
        {
            PackageManifest returnValue = default(PackageManifest);

            try
            {
                returnValue = LoadXml(filePath);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
            return returnValue;
        }

        /// <summary>
        /// Saves the specified object's data to the specified file.
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="filePath"></param>
        public static void Save(PackageManifest manifest, String filePath)
        {
            try
            {
                PersistXml(manifest, filePath);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        /// <summary>
        /// Move a manifest, its images folder, and the images therein.
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="sourcePath"> . 
        /// <param name="destinationPath"></param>
        /// <param name="deleteWaitMilliseconds"></param>
        /// <param name="progressDelegate"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean MoveManifestAndData
        (
            PackageManifest manifest,
            String sourcePath,
            String destinationPath,
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
            String manifestDestinationFilename = default(String);
            String manifestDestinationFilePath = default(String);

            try
            {
                //report status
                progressDelegate(String.Format("calculating paths..."));

                //source paths
                manifestSourceFilename = String.Format("{0}.{1}", manifest.TransactionId, DocumentScannerCommon.PackageManifest.DATA_FILE_TYPE);
                manifestSourceFilePath = Path.Combine(sourcePath, manifestSourceFilename);
                imagesSourcePath = Path.Combine(sourcePath, manifest.TransactionId);


                //destination paths
                imagesDestinationPath = Path.Combine(destinationPath, manifest.TransactionId);
                manifestDestinationFilename = manifestSourceFilename;
                manifestDestinationFilePath = Path.Combine(destinationPath, manifestDestinationFilename);


                //report status
                progressDelegate(String.Format("moving manifest and data..."));

                //store images
                FileSystem.MoveDirectory//CopyDirectory
                (
                    imagesSourcePath,
                    imagesDestinationPath
                );

                //store manifest
                System.IO.File.Move(manifestSourceFilePath, manifestDestinationFilePath);


                //clean up

                //report status
                progressDelegate(String.Format("cleaning up source..."));

                //delete images folder
                Folder.DeleteFolderWithWait(imagesSourcePath, deleteWaitMilliseconds);
                
                //delete manifest
                System.IO.File.Delete(manifestSourceFilePath);


                //validate manifest, and return manifest object
                if
                (
                    !LoadAndValidateManifest
                    (
                        Path.GetFileNameWithoutExtension(manifestDestinationFilePath),
                        Path.GetDirectoryName(manifestDestinationFilePath),
                        progressDelegate,
                        ref errorMessage,
                        out manifest
                    )
                )
                {
                    throw new Exception(String.Format("Package folder preparation failure: '{0}'", errorMessage));
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
        /// Load and validate manifest
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="packageContentsPackageSubfolderPath"></param>
        /// <param name="progressDelegate"></param>
        /// <param name="errorMessage"></param>
        /// <param name="manifest"></param>
        /// <returns></returns>
        public static Boolean LoadAndValidateManifest
        (
            String packageId,
            String packageContentsPackageSubfolderPath,
            Action<String> progressDelegate,
            ref String errorMessage,
            out PackageManifest manifest
        )
        {
            Boolean returnValue = default(Boolean);
            String manifestFilename = default(String);
            String manifestFilePath = default(String);
            String imagesPath = default(String);

            manifest = default(PackageManifest);

            try
            {
                //report status
                progressDelegate(String.Format("validating manifest..."));

                //load manifest
                manifestFilename = String.Format("{0}.{1}", packageId, DATA_FILE_TYPE);
                manifestFilePath = Path.Combine(packageContentsPackageSubfolderPath, manifestFilename);
                manifest = PackageManifest.Load(manifestFilePath);

                //validate manifest
                imagesPath = Path.Combine(packageContentsPackageSubfolderPath, manifest.TransactionId);
                if (!manifest.Valid(imagesPath))
                {
                    throw new Exception(String.Format("Manifest '{0}' invalid: \n'{1}'", manifest.TransactionId, manifest.ErrorMessage));
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

        #region Protected Members
        /// <summary>
        /// Loads the specified object with data from the specified file, using XML Serializer.
        /// </summary>
        /// <param name="filePath"></param>
        protected static PackageManifest LoadXml(String filePath)
        {
            PackageManifest returnValue = default(PackageManifest);

            try
            {
                //XML Serializer of type Settings
                XmlSerializer xs = new XmlSerializer(typeof(PackageManifest));

                //Stream reader for file
                StreamReader sr = new StreamReader(filePath);

                //de-serialize into Settings
                returnValue = (PackageManifest)xs.Deserialize(sr);

                //close file
                sr.Close();
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
            return returnValue;
        }

        /// <summary>
        /// Saves the specified object's data to the specified file, using XML Serializer.
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="filePath"></param>
        protected static void PersistXml(PackageManifest manifest, String filePath)
        {
            try
            {
                //XML Serializer of type Settings
                XmlSerializer xs = new XmlSerializer(typeof(PackageManifest));

                //Stream writer for file
                StreamWriter sw = new StreamWriter(filePath);

                //serialize out of Settings
                xs.Serialize(sw, manifest);

                //close file
                sw.Close();
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }


        #endregion Protected Members

    }
}
