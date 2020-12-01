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
using System.Windows.Shapes;

namespace DocumentScannerWindowsPhone
{
    public class ApplicationBarAction : INotifyPropertyChanged
    {
        //private static Action<Object, EventArgs> applicationBarActionDelegate;

        public ApplicationBarAction
        (
            String key,
            Uri iconUri,
            EventHandler actionDelegate
        )
        {
            Key = key;
            IconUri = iconUri;
            ActionDelegate = actionDelegate;
        }

        private String _Key = default(String);
        public String Key
        {
            get { return _Key; }
            set 
            {
                if (value != _Key)
                {
                    _Key = value;
                    NotifyPropertyChanged("Key");
                }
            }
        }

        private Uri _IconUri = default(Uri);
        public Uri IconUri
        {
            get { return _IconUri; }
            set 
            {
                if (value != _IconUri)
                {
                    _IconUri = value;
                    NotifyPropertyChanged("IconUri");
                }
            }
        }

        private EventHandler _ActionDelegate = default(EventHandler);
        public EventHandler ActionDelegate
        {
            get { return _ActionDelegate; }
            set 
            {
                if (value != _ActionDelegate)
                {
                    _ActionDelegate = value;
                    NotifyPropertyChanged("ActionDelegate");
                }
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
