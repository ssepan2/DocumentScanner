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
    public partial class AddImageMenuPage : PhoneApplicationPage
    {
        // Constructor
        public AddImageMenuPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(AddImageMenuPage_Loaded);
        }

        private ApplicationBarIconButtonPaging _AppBarPaging = default(ApplicationBarIconButtonPaging);
        public ApplicationBarIconButtonPaging AppBarPaging
        {
            get { return _AppBarPaging; }
            set { _AppBarPaging = value; }
        }

        // Load data for the ViewModel Items
        private void AddImageMenuPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the data context of the listbox control to the sample data
            if (App.AddImageMenuVM == null)
            {
                //delayed creation
                App.AddImageMenuVM = new AddImageMenuViewModel(new NavigationHelper(NavigationService));
            }
            DataContext = App.AddImageMenuVM;

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

            if (!App.AddImageMenuVM.IsDataLoaded)
            {
                App.AddImageMenuVM.LoadData();
            }
        }

        //private void ApplicationBarIconButtonClickMe_Click(object sender, EventArgs e)
        //{
        //    if (App.AddImageMenuVM.ClickCommand.CanExecute(null))
        //    {
        //        App.AddImageMenuVM.ClickCommand.Execute(null);
        //    }
        //}
    }
}