using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DocumentScannerWindowsPhone
{
    public class DocumentModel : INotifyPropertyChanged
    {
        private string _ID;
        /// <summary>
        /// unique id of image
        /// </summary>
        /// <returns></returns>
        public string ID
        {
            get
            {
                return _ID;
            }
            set
            {
                if (value != _ID)
                {
                    _ID = value;
                    NotifyPropertyChanged("ID");
                }
            }
        }

        private string _Description;
        /// <summary>
        /// description of document
        /// </summary>
        /// <returns></returns>
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                if (value != _Description)
                {
                    _Description = value;
                    NotifyPropertyChanged("Description");
                }
            }
        }

        private string _DocumentType;
        /// <summary>
        /// type of document represented
        /// </summary>
        /// <returns></returns>
        public string DocumentType
        {
            get
            {
                return _DocumentType;
            }
            set
            {
                if (value != _DocumentType)
                {
                    _DocumentType = value;
                    NotifyPropertyChanged("DocumentType");
                }
            }
        }

        private string _Filename;
        /// <summary>
        /// name of image file
        /// </summary>
        /// <returns></returns>
        public string Filename
        {
            get
            {
                return _Filename;
            }
            set
            {
                if (value != _Filename)
                {
                    _Filename = value;
                    NotifyPropertyChanged("Filename");
                }
            }
        }

        //private string _FilePath;
        /// <summary>
        /// name of image file
        /// </summary>
        /// <returns></returns>
        public string FilePath
        {
            get
            {
                return "/DocumentScannerWindowsPhone;component/SampleData/" + Filename;
            }
            set
            {
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}