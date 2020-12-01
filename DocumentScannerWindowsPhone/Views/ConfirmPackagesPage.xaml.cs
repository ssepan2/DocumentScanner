using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Controls;

namespace DocumentScannerWindowsPhone
{
    public partial class ConfirmPackagesPage : PhoneApplicationPage
    {
        // Constructor
        public ConfirmPackagesPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(ConfirmPackagesPage_Loaded);
        }

        private ApplicationBarIconButtonPaging _AppBarPaging = default(ApplicationBarIconButtonPaging);
        public ApplicationBarIconButtonPaging AppBarPaging
        {
            get { return _AppBarPaging; }
            set { _AppBarPaging = value; }
        }

        // Load data for the ViewModel Items
        private void ConfirmPackagesPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the data context of the listbox control to the sample data
            if (App.ConfirmPackagesVM == null)
            {
                //delayed creation
                App.ConfirmPackagesVM = new ConfirmPackagesViewModel();
            }
            DataContext = App.ConfirmPackagesVM;

            //load viewmodel
            if (!App.ConfirmPackagesVM.IsDataLoaded)
            {
                App.ConfirmPackagesVM.LoadData();
            }

            //load actions
            if (AppBarPaging == default(ApplicationBarIconButtonPaging))
            {
                AppBarPaging = 
                    new ApplicationBarIconButtonPaging
                    (
                        new List<ApplicationBarAction>
                        {
                            new ApplicationBarAction
                            (
                                "List",
                                new Uri("Images/appbar.list.rest.png", UriKind.Relative),
                                AppBarList_Click
                            )
                        },
                        null/*new ApplicationBarAction
                        (
                            "Next",
                            new Uri("Images/appbar.overflowdots2.png", UriKind.Relative),
                            AppBarNextButtonPage_Click
                        )*/,
                        ApplicationBarIconButtonPaging.ApplicationBarIconButtonPageEffectiveLimit
                    );
            }

            //render initial page of app bar buttons
            AppBarPaging.RenderPage
            (
                (ApplicationBar)this.ApplicationBar, 
                1, 
                1
            );

        }


        private void AppBarList_Click(object sender, EventArgs e)
        {
            App.ConfirmPackagesVM.ListConfirmedManifestsCommand.Execute(null);
        }

        //private void AppBarNextButtonPage_Click(object sender, EventArgs e)
        //{
        //    App.ReviewPackagesVM.NextAppBarPageCommand.Execute(null);
        //}
    }
}