using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DocumentScannerWindowsPhone
{
    public abstract class ViewModelBase : 
        INotifyPropertyChanged
    {
        private String _StatusMessage = default(String);
        public String StatusMessage
        {
            get { return _StatusMessage; }
            set
            {
                if (value != _StatusMessage)
                {
                    _StatusMessage = value;
                    NotifyPropertyChanged("StatusMessage");
                }
            }
        }

        private String _ErrorMessage = default(String);
        public String ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                if (value != _ErrorMessage)
                {
                    _ErrorMessage = value;
                    NotifyPropertyChanged("ErrorMessage");
                }
            }
        }

        public Boolean IsDataLoaded
        {
            get;
            protected set;
        }

        public abstract void LoadData();


        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
