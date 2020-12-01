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
    public partial class ManifestPage : PhoneApplicationPage
    {
        // Constructor
        public ManifestPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(ManifestPage_Loaded);
        }

        private ApplicationBarIconButtonPaging _AppBarPaging = default(ApplicationBarIconButtonPaging);
        public ApplicationBarIconButtonPaging AppBarPaging
        {
            get { return _AppBarPaging; }
            set { _AppBarPaging = value; }
        }

        // Load data for the ViewModel Items
        private void ManifestPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the data context of the listbox control to the sample data
            if (App.ManifestVM == null)
            {
                //this is the 1st instance of view
                //tell viewmodel about view, so that ApplicationBar is known
                App.ManifestVM = new ManifestViewModel(new NavigationHelper(NavigationService)/*, this*/);
            }
            //else
            //{
            //    //TODO:should app bar paging be called from viewmodel or view (note:appbar paging helper instance is in view, not viewmodel)
            //    //this is a new instance of view; tell viewmodel about it (and free old instance of view)
            //    App.ManifestVM.View = this;
            //}
            DataContext = App.ManifestVM;

            //load viewmodel
            if (!App.ManifestVM.IsDataLoaded)
            {
                App.ManifestVM.LoadData();
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
                                "Edit",
                                new Uri("Images/appbar.edit.rest.png", UriKind.Relative),
                                AppBarEdit_Click
                            ),
                            new ApplicationBarAction
                            (
                                "Add",
                                new Uri("Images/appbar.feature.camera.rest.png", UriKind.Relative),
                                AppBarAdd_Click
                            ),
                            new ApplicationBarAction
                            (
                                "Delete",
                                new Uri("Images/appbar.delete.rest.png", UriKind.Relative),
                                AppBarDelete_Click
                            ),
                            new ApplicationBarAction
                            (
                                "Promote",
                                new Uri("Images/appbar.promote.rest.png", UriKind.Relative),
                                AppBarPromote_Click
                            ),
                            new ApplicationBarAction
                            (
                                "Demote",
                                new Uri("Images/appbar.demote.rest.png", UriKind.Relative),
                                AppBarDemote_Click
                            ),
                            new ApplicationBarAction
                            (
                                "Package",
                                new Uri("Images/appbar.share.rest.png", UriKind.Relative),
                                AppBarPackage_Click
                            )
                        },
                        new ApplicationBarAction
                        (
                            "Next",
                            new Uri("Images/appbar.overflowdots2.png", UriKind.Relative),
                            AppBarNextButtonPage_Click
                        ),
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

        private void AppBarEdit_Click(object sender, EventArgs e)
        {
            App.ManifestVM.NavigateToDocumentCommand.Execute(null);
        }

        private void AppBarAdd_Click(object sender, EventArgs e)
        {
            App.ManifestVM.NavigateToAddImageMenuCommand.Execute(null);
        }

        private void AppBarDelete_Click(object sender, EventArgs e)
        {
            App.ManifestVM.DeleteImageCommand.Execute(null);
        }

        private void AppBarPromote_Click(object sender, EventArgs e)
        {
            App.ManifestVM.PromoteDocumentCommand.Execute(null);
        }

        private void AppBarDemote_Click(object sender, EventArgs e)
        {
            App.ManifestVM.DemoteDocumentCommand.Execute(null);
        }

        private void AppBarPackage_Click(object sender, EventArgs e)
        {
            App.ManifestVM.PackageManifestCommand.Execute(null);
        }

        private void AppBarNextButtonPage_Click(object sender, EventArgs e)
        {
            AppBarPaging.NextPage((ApplicationBar)this.ApplicationBar);
            //App.ManifestVM.NextAppBarPageCommand.Execute(null);
        }
    }
}