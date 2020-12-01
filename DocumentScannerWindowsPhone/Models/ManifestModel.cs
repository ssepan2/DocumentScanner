using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class ManifestModel : INotifyPropertyChanged
    {
        private string _ID;
        /// <summary>
        /// unique id of manifest
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
        /// description of manifest
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

        private ObservableCollection<DocumentModel> _Documents = default(ObservableCollection<DocumentModel>);
        public ObservableCollection<DocumentModel> Documents
        {
            get { return _Documents; }
            set { _Documents = value; }
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