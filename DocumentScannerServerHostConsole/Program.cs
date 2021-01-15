using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using DocumentScannerServerLibrary;
using Ssepan.Application;
using Ssepan.Utility;

namespace DocumentScannerServerHostConsole
{
    //Note: wasn't static originally; changed to match winforms
    static class Program
    {
        #region Declarations
        public const String APP_NAME = "DocumentScannerServerHostConsole";
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
        public static void PropertyChangedEventHandlerDelegate
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
        [STAThread]//Note: wasn't STAThread originally; changed to match winforms--SJS
        static Int32 Main(String[] args)
        {
            //default to fail code
            Int32 returnValue = -1;

            try
            {
                //define default output delegate
                ConsoleApplication.defaultOutputDelegate = ConsoleApplication.writeLineWrapperOutputDelegate;

                //load, parse, run switches
                DoSwitches(args);

                returnValue = new App()._Main();
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                Console.WriteLine(String.Format("{0} did NOT complete: '{1}'", APP_NAME, ex.Message));
            }
            finally
            {
#if debug
                Console.Write("Press ENTER to continue> ");
                Console.ReadLine();
#endif
            }
            return returnValue;
        }

        #region ConsoleAppBase
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
                    new CommandLineSwitch("f", "f filename; overrides app.config", true, f)//,
                    //new CommandLineSwitch("H", "H invokes the Help command.", false, ConsoleApplication.Help)//may already be loaded
                }
            );
            //Note: switches are processed before Model or Settings are accessed.
        }
        #endregion ConsoleAppBase

        #region CommandLineSwitch Action Delegates
        /// <summary>
        /// Validate and set selected settings.
        /// Instance of an action conforming to delegate Action<T>, where T is String.
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
