using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using DocumentScannerLibrary;
using Ssepan.Application;
using Ssepan.Utility;

namespace DocumentScanner
{
    static class Program
    {
        #region Declarations
        public const String APP_NAME = "DocumentViewer";
        #endregion Declarations

        #region INotifyPropertyChanged
        public static event PropertyChangedEventHandler PropertyChanged;
        public static void OnPropertyChanged(String propertyName)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(null, new PropertyChangedEventArgs(propertyName));
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);

                throw;
            }
        }
        #endregion INotifyPropertyChanged

        #region PropertyChangedEventHandlerDelegate
        /// <summary>
        /// Note: property changes update UI manually.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void ModelPropertyChangedEventHandlerDelegate
        (
            Object sender,
            PropertyChangedEventArgs e
        )
        {
            try
            {
                if (e.PropertyName == "Filename")
                {
                    ConsoleApplication.defaultOutputDelegate(String.Format("{0}", Filename));
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }
        #endregion PropertyChangedEventHandlerDelegate

        #region Properties
        private static String _Filename = default(String);
        public static String Filename
        {
            get { return _Filename; }
            set
            {
                _Filename = value;
                OnPropertyChanged("Filename");
            }
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Int32</returns>
        [STAThread]
        static Int32 Main(String[] args)
        {
            //default to fail code
            Int32 returnValue = -1;

            try
            {
                //define default output delegate
                ConsoleApplication.defaultOutputDelegate = ConsoleApplication.messageBoxWrapperOutputDelegate;

                //subscribe to notifications
                PropertyChanged += ModelPropertyChangedEventHandlerDelegate;

                //load, parse, run switches
                DoSwitches(args);

                InitModelAndSettings();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new DocumentViewer(args));

                returnValue = 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        #region FormAppBase
        /// <summary>
        /// Note: switches are processed before Model or Settings are accessed.
        /// </summary>
        /// <param name="args"></param>
        static void DoSwitches(String[] args)
        {
            //define supported switches
            // -t -f:"filename" -h
            ConsoleApplication.DoCommandLineSwitches
            (
                args,
                new CommandLineSwitch[]  
                { 
                    new CommandLineSwitch("f", "f filename; overrides app.config", true, f),
                    //new CommandLineSwitch("H", "H invokes the Help command.", false, ConsoleApplication.Help)//may already be loaded
                }
            );
        }

        static void InitModelAndSettings()
        {
            //create Settings before first use by Model
            if (SettingsController<Settings>.Settings == null)
            {
                SettingsController<Settings>.New();
            }
            //Model properties rely on Settings, so don't call Refresh before this is run.
            if (DSController<DSModel>.Model == null)
            {
                DSController<DSModel>.New();
            }
        }
        #endregion FormAppBase

        #region CommandLineSwitch Action Delegates
        /// <summary>
        /// Instance of an action conforming to delegate Action(Of T), where T is String.
        /// Command 'f' passes filename.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="outputDelegate"></param>
        static void f(String value, Action<String> outputDelegate)
        {
            try
            {
#if debug
                outputDelegate(String.Format("s{0}\t{1}", ConsoleApplication.CommandLineSwitchValueSeparator, value));
#endif

                //validate settings file path
                if (!System.IO.File.Exists(value))
                {
                    throw new ArgumentException(String.Format("Invalid settings file path: '{0}'", value));
                }
                Filename = value;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }
        #endregion CommandLineSwitch Action Delegates
        #endregion Methods
    }
}