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
    public class MainMenuItemModel : INotifyPropertyChanged
    {
        private string _Name;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string _Description;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
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

        private string _Details;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string Details
        {
            get
            {
                return _Details;
            }
            set
            {
                if (value != _Details)
                {
                    _Details = value;
                    NotifyPropertyChanged("Details");
                }
            }
        }

        private string _PageFilename;
        /// <summary>
        /// name of image file
        /// </summary>
        /// <returns></returns>
        public string PageFilename
        {
            get
            {
                return _PageFilename;
            }
            set
            {
                if (value != _PageFilename)
                {
                    _PageFilename = value;
                    NotifyPropertyChanged("Filename");
                }
            }
        }

        //private string _FilePath;
        /// <summary>
        /// name of image file
        /// </summary>
        /// <returns></returns>
        public string PageFilePath
        {
            get
            {
                //absolute
                //return "/DocumentScannerWindowsPhone;component/Views/" + Filename;
                //relative
                return "/DocumentScannerWindowsPhone;component/Views/" + PageFilename;
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