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
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Controls;

namespace DocumentScannerWindowsPhone
{
    public partial class DocumentPage : PhoneApplicationPage
    {
        // Constructor
        public DocumentPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DocumentPage_Loaded);
        }

        private ApplicationBarIconButtonPaging _AppBarPaging = default(ApplicationBarIconButtonPaging);
        public ApplicationBarIconButtonPaging AppBarPaging
        {
            get { return _AppBarPaging; }
            set { _AppBarPaging = value; }
        }

        // Load data for the ViewModel Items
        private void DocumentPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the data context of the listbox control to the sample data
            if (App.DocumentVM == null)
            {
                //delayed creation
                App.DocumentVM = new DocumentViewModel(new NavigationHelper(NavigationService));
            }
            if (App.ManifestVM.DetailIndex != -1)
            {
                //loads data using detail index set by document list selectionchanged
                App.DocumentVM.LoadData();
            }
            DataContext = App.DocumentVM;

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
                                "Rotate CCW",
                                new Uri("Images/appbar.rotateCCW.rest.png", UriKind.Relative),
                                AppBarRotateCCW_Click
                            ),
                            new ApplicationBarAction
                            (
                                "Rotate CW",
                                new Uri("Images/appbar.rotateCW.rest.png", UriKind.Relative),
                                AppBarRotateCW_Click
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

        private void AppBarRotateCCW_Click(object sender, EventArgs e)
        {
            App.DocumentVM.DocumentRotateCCWCommand.Execute(null);
        }

        private void AppBarRotateCW_Click(object sender, EventArgs e)
        {
            App.DocumentVM.DocumentRotateCWCommand.Execute(null);
        }


        //private void AppBarNextButtonPage_Click(object sender, EventArgs e)
        //{
            //App.DocumentVM.NextAppBarPageCommand.Execute(null);
        //}
    }
}