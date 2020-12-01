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
    public partial class QueuedPackagesPage : PhoneApplicationPage
    {
        // Constructor
        public QueuedPackagesPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(QueuedPackagesPage_Loaded);
        }

        private ApplicationBarIconButtonPaging _AppBarPaging = default(ApplicationBarIconButtonPaging);
        public ApplicationBarIconButtonPaging AppBarPaging
        {
            get { return _AppBarPaging; }
            set { _AppBarPaging = value; }
        }

        // Load data for the ViewModel Items
        private void QueuedPackagesPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the data context of the listbox control to the sample data
            if (App.QueuedPackagesVM == null)
            {
                //delayed creation
                App.QueuedPackagesVM = new QueuedPackagesViewModel();
            }
            DataContext = App.QueuedPackagesVM;

            //load viewmodel
            if (!App.QueuedPackagesVM.IsDataLoaded)
            {
                App.QueuedPackagesVM.LoadData();
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
                                "Send",
                                new Uri("Images/appbar.upload.rest.png", UriKind.Relative),
                                AppBarSend_Click
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


        private void AppBarSend_Click(object sender, EventArgs e)
        {
            App.QueuedPackagesVM.SendQueuedPackagesCommand.Execute(null);
        }

        //private void AppBarNextButtonPage_Click(object sender, EventArgs e)
        //{
        //    App.QueuedPackagesVM.NextAppBarPageCommand.Execute(null);
        //}
    }
}