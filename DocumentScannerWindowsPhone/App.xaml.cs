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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DocumentScannerWindowsPhone
{
    public partial class App : Application
    {
        //TODO:inject model source into new newmodels
        //TODO:also inject views into respective viewmodels(?), then set datacontext in viewmodel constructor?
        //TODO:inject navigationhelper into viewmodel
        //TODO:these *must* be instantiated in view to work correctly
        private static MainMenuViewModel _MainVM = null;
        /// <summary>
        /// A static ViewModel used by the main views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static MainMenuViewModel MainVM
        {
            get { return _MainVM; }
            set { _MainVM = value; }
        }

        private static ManifestViewModel _ManifestVM = null;
        /// <summary>
        /// A static ViewModel used by the document views to bind against.
        /// </summary>
        /// <returns>The ManifestViewModel object.</returns>
        public static ManifestViewModel ManifestVM
        {
            get { return _ManifestVM; }
            set { _ManifestVM = value; }
        }

        private static DocumentViewModel _DocumentVM = null;
        /// <summary>
        /// A static ViewModel used by the document views to bind against.
        /// </summary>
        /// <returns>The DocumentViewModel object.</returns>
        public static DocumentViewModel DocumentVM
        {
            get { return _DocumentVM; }
            set { _DocumentVM = value; }
        }

        private static AddImageMenuViewModel _AddImageMenuVM = null;
        /// <summary>
        /// A static ViewModel used by the document views to bind against.
        /// </summary>
        /// <returns>The AddImagemenuViewModel object.</returns>
        public static AddImageMenuViewModel AddImageMenuVM
        {
            get { return _AddImageMenuVM; }
            set { _AddImageMenuVM = value; }
        }

        private static QueuedPackagesViewModel _QueuedPackagesVM = null;
        /// <summary>
        /// A static ViewModel used by the document views to bind against.
        /// </summary>
        /// <returns>The QueuedPackagesViewModel object.</returns>
        public static QueuedPackagesViewModel QueuedPackagesVM
        {
            get { return _QueuedPackagesVM; }
            set { _QueuedPackagesVM = value; }
        }

        private static ReviewPackagesViewModel _ReviewPackagesVM = null;
        /// <summary>
        /// A static ViewModel used by the document views to bind against.
        /// </summary>
        /// <returns>The ReviewPackagesViewModel object.</returns>
        public static ReviewPackagesViewModel ReviewPackagesVM
        {
            get { return _ReviewPackagesVM; }
            set { _ReviewPackagesVM = value; }
        }

        private static ConfirmPackagesViewModel _ConfirmPackagesVM = null;
        /// <summary>
        /// A static ViewModel used by the document views to bind against.
        /// </summary>
        /// <returns>The ConfirmPackagesViewModel object.</returns>
        public static ConfirmPackagesViewModel ConfirmPackagesVM
        {
            get { return _ConfirmPackagesVM; }
            set { _ConfirmPackagesVM = value; }
        }

        private static ReceivePackagesViewModel _ReceivePackagesVM = null;
        /// <summary>
        /// A static ViewModel used by the document views to bind against.
        /// </summary>
        /// <returns>The ReceivePackagesViewModel object.</returns>
        public static ReceivePackagesViewModel ReceivePackagesVM
        {
            get { return _ReceivePackagesVM; }
            set { _ReceivePackagesVM = value; }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                ////Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                ////Enable non-production analysis visualization mode, 
                ////which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // Ensure that application state is restored appropriately
            if (!App.MainVM.IsDataLoaded)
            {
                App.MainVM.LoadData();
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}