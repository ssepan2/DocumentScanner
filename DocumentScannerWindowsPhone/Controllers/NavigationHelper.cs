using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;

namespace DocumentScannerWindowsPhone
{
    public class NavigationHelper : 
        INavigationHelper
    {
        private String UriRoot = "/DocumentScannerWindowsPhone;component/Views/";
        private NavigationService _navigationService;

        public NavigationHelper(NavigationService navSvc)
        {
            _navigationService = navSvc;
        }

        //public void Home()
        //{
        //    _navigationService.Navigate(new Uri(UriRoot + "MainMenuPage.xaml", UriKind.Relative));
        //}

        public void Manifest()
        {
            _navigationService.Navigate(new Uri(UriRoot + "ManifestPage.xaml", UriKind.Relative));
        }

        public void QueuedPackages()
        {
            _navigationService.Navigate(new Uri(UriRoot + "QueuedPackagesPage.xaml", UriKind.Relative));
        }

        public void Review()
        {
            _navigationService.Navigate(new Uri(UriRoot + "ReviewPackagesPage.xaml", UriKind.Relative));
        }

        public void Confirm()
        {
            _navigationService.Navigate(new Uri(UriRoot + "ConfirmPackagesPage.xaml", UriKind.Relative));
        }

        public void Receive()
        {
            _navigationService.Navigate(new Uri(UriRoot + "ReceivePackagesPage.xaml", UriKind.Relative));
        }


        public void Document(Int32 index)
        {
            _navigationService.Navigate(new Uri(UriRoot + "DocumentPage.xaml?selectedItem=" + index.ToString(), UriKind.Relative));
        }

        public void AddImageMenu()
        {
            _navigationService.Navigate(new Uri(UriRoot + "AddImageMenuPage.xaml", UriKind.Relative));
        }
    }
}
