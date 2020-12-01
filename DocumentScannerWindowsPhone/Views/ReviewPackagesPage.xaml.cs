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
    public partial class ReviewPackagesPage : PhoneApplicationPage
    {
        // Constructor
        public ReviewPackagesPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(ReviewPackagesPage_Loaded);
        }

        private ApplicationBarIconButtonPaging _AppBarPaging = default(ApplicationBarIconButtonPaging);
        public ApplicationBarIconButtonPaging AppBarPaging
        {
            get { return _AppBarPaging; }
            set { _AppBarPaging = value; }
        }

        // Load data for the ViewModel Items
        private void ReviewPackagesPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the data context of the listbox control to the sample data
            if (App.ReviewPackagesVM == null)
            {
                //delayed creation
                App.ReviewPackagesVM = new ReviewPackagesViewModel();
            }
            DataContext = App.ReviewPackagesVM;

            //load viewmodel
            if (!App.ReviewPackagesVM.IsDataLoaded)
            {
                App.ReviewPackagesVM.LoadData();
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
                                "Unpackage",
                                new Uri("Images/appbar.folder.rest.png", UriKind.Relative),
                                AppBarUnpackage_Click
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


        private void AppBarUnpackage_Click(object sender, EventArgs e)
        {
            App.ReviewPackagesVM.UnpackageManifestCommand.Execute(null);
        }

        //private void AppBarNextButtonPage_Click(object sender, EventArgs e)
        //{
        //    App.ReviewPackagesVM.NextAppBarPageCommand.Execute(null);
        //}
    }
}