using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;


namespace DocumentScannerWindowsPhone
{
    public class MainMenuViewModel : 
        ViewModelBase
    {
        
        public MainMenuViewModel()
        {
            //this.ClickCommand = new ClickMeCommand(this);
            this.NavigateToManifestCommand = new MainNavigateToManifestCommand(this);
            this.NavigateToQueuedPackagesCommand = new MainNavigateToQueuedPackagesCommand(this);
            this.NavigateToReviewPackagesCommand = new MainNavigateToReviewPackagesCommand(this);
            this.NavigateToConfirmPackagesCommand = new MainNavigateToConfirmPackagesCommand(this);
            this.NavigateToReceivePackagesCommand = new MainNavigateToReceivePackagesCommand(this);
        }

        public MainMenuViewModel
        (
            INavigationHelper navigationHelper
        ) : this()
        {
            NavigationHelper = navigationHelper;
        }

        public INavigationHelper NavigationHelper = default(INavigationHelper);
        
        //public ICommand ClickCommand { get; private set; }
        public ICommand NavigateToManifestCommand { get; private set; }
        public ICommand NavigateToQueuedPackagesCommand { get; private set; }
        public ICommand NavigateToReviewPackagesCommand { get; private set; }
        public ICommand NavigateToConfirmPackagesCommand { get; private set; }
        public ICommand NavigateToReceivePackagesCommand { get; private set; }

        //private DateTime _ClickMeResult = default(DateTime);
        //public DateTime ClickMeResult
        //{
        //    get
        //    {
        //        return _ClickMeResult;
        //    }
        //    set
        //    {
        //        if (value != _ClickMeResult)
        //        {
        //            _ClickMeResult = value;
        //            NotifyPropertyChanged("ClickMeResult");
        //        }
        //    }
        //}


        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public override void LoadData()
        {
            //load data here...


            this.IsDataLoaded = true;
        }
    }
}