using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Ssepan.Collections;
using Ssepan.Graphics;
using Ssepan.Utility;
using System.Diagnostics;
using System.Reflection;

namespace DocumentScannerCommon
{
    [Serializable()]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ImageFile :
        IEquatable<ImageFile>,
        INotifyPropertyChanged
    {
        #region Declarations
        public const String IMAGE_FILE_TYPE = "jpg";
        public static readonly ImageFormat IMAGE_FORMAT = ImageFormat.Jpeg; 
        #endregion Declarations

        #region Constructors
        public ImageFile()
        { 
        }

        public ImageFile(String filename)
        {
            Filename = filename;
        }
        #endregion Constructors

        #region INotifyPropertyChanged support
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(String propertyName)
        {
            try
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
#if debug
                    Log.Write(
                        MethodBase.GetCurrentMethod().DeclaringType.Module.Name,
                        Log.FormatEntry(String.Format("PropertyChanged: {0}", propertyName), MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name),
                        EventLogEntryType.Information);
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

        #region IEquatable<T> Members
        public bool Equals(ImageFile other)
        {
            Boolean returnValue = default(Boolean);

            try
            {
                if (!this.Filename.Equals(other.Filename))
                {
                    returnValue = false;
                }
                else if (this.Description != other.Description)
                {
                    returnValue = false;
                }
                else if (this.DocumentType != other.DocumentType)
                {
                    returnValue = false;
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
        #endregion IEquatable<T> Members

        #region Properties
        #region Non-Persisted Properties
        #endregion Non-Persisted Properties

        #region Persisted Properties
        private String _Filename = String.Empty; 
        public String Filename
        {
            get { return _Filename; }
            set 
            { 
                _Filename = value;
                this.OnPropertyChanged("Filename");
            }
        }

        private String _Description = String.Empty;
        /// <summary>
        /// Describe ImageFile.
        /// </summary>
        public String Description
        {
            get { return _Description; }
            set 
            { 
                _Description = value;
                this.OnPropertyChanged("Description");
            }
        }

        private String _DocumentType = String.Empty;
        public String DocumentType
        {
            get { return _DocumentType; }
            set 
            { 
                _DocumentType = value;
                this.OnPropertyChanged("DocumentType");
            }
        }
        #endregion Persisted Properties
        #endregion Properties

        #region Public Method
        /// <summary>
        /// Determine whether this file exists at the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Boolean Present(String path)
        {
            Boolean returnValue = default(Boolean);

            try
            {
                if (System.IO.File.Exists(Path.Combine(path, this.Filename)))
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

        /// <summary>
        /// Rotate document item's image.
        /// </summary>
        /// <param name="dataPath"></param>
        /// <param name="rotateFlipType"></param>
        public void RotateDocumentItem(String dataPath, RotateFlipType rotateFlipType)
        {
            try
            {
                Transform.RotateImageFile(Path.Combine(dataPath, this.Filename), rotateFlipType);
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        /// <summary>
        /// Save item's scanned image.
        /// </summary>
        /// <param name="transactionPath"></param>
        /// <param name="image"></param>
        /// <param name="overwrite">Optional</param>
        /// <returns></returns>
        public Boolean SaveDocumentItem(String transactionPath, Image image, Boolean overwrite = false)
        {
            Boolean returnValue = default(Boolean);
            String path = String.Empty;

            try
            {
                path = Path.Combine(transactionPath, this.Filename);
                if (System.IO.File.Exists(path))
                {
                    if (!overwrite)
                    {
                        throw new Exception(String.Format("Image filename already exists: {0}", path));
                    }
                }
                image.Save(path, IMAGE_FORMAT);
                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
            return returnValue;
        }
        #endregion Public Method
    }
}
