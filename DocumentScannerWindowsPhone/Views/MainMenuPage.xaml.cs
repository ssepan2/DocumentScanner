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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
//using Microsoft.Phone.Reactive;

namespace DocumentScannerWindowsPhone
{
    public partial class MainMenuPage : PhoneApplicationPage
    {
        // Constructor
        public MainMenuPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(MainMenuPage_Loaded);

            //SystemTray.ProgressIndicator.IsVisible = true;
            //SystemTray.ProgressIndicator.IsIndeterminate = true;
            //SystemTray.ProgressIndicator.Text = "Click me...";
            //SystemTray.SetProgressIndicator(this, SystemTray.ProgressIndicator);
        }

        private ApplicationBarIconButtonPaging _AppBarPaging = default(ApplicationBarIconButtonPaging);
        public ApplicationBarIconButtonPaging AppBarPaging
        {
            get { return _AppBarPaging; }
            set { _AppBarPaging = value; }
        }

        // Load data for the ViewModel Items
        private void MainMenuPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the data context of the listbox control to the sample data
            if (App.MainVM == null)
            {
                //delayed creation
                App.MainVM = new MainMenuViewModel(new NavigationHelper(NavigationService));
            }
            DataContext = App.MainVM;

            ////load actions
            //if (AppBarPaging == default(ApplicationBarIconButtonPaging))
            //{
            //    AppBarPaging =
            //        new ApplicationBarIconButtonPaging
            //        (
            //            null/*new List<ApplicationBarAction>
            //            {
            //                new ApplicationBarAction
            //                (
            //                    "Click Me",
            //                    new Uri("Images/appbar.edit.rest.png", UriKind.Relative),
            //                    ApplicationBarIconButtonClickMe_Click
            //                )*/,
            //            null/*new ApplicationBarAction
            //            (
            //                "Next",
            //                new Uri("Images/appbar.overflowdots2.png", UriKind.Relative),
            //                AppBarNextButtonPage_Click
            //            )*/,
            //            ApplicationBarIconButtonPaging.ApplicationBarIconButtonPageEffectiveLimit
            //        );
            //}

            ////render initial page of app bar buttons
            //AppBarPaging.RenderPage
            //(
            //    (ApplicationBar)this.ApplicationBar,
            //    1,
            //    1
            //);

            if (!App.MainVM.IsDataLoaded)
            {
                App.MainVM.LoadData();
            }
        }

        //private void ApplicationBarIconButtonClickMe_Click(object sender, EventArgs e)
        //{
        //    if (App.MainVM.ClickCommand.CanExecute(null))
        //    {
        //        App.MainVM.ClickCommand.Execute(null);
        //    }
        //}

        //private void AppBarNextButtonPage_Click(object sender, EventArgs e)
        //{
        //    App.MainVM.NextAppBarPageCommand.Execute(null);
        //}
    }
}