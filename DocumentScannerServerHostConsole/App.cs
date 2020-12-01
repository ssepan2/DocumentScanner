//#define USE_CONFIG_FILEPATH

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using Ssepan.Application;
using Ssepan.Application.WinConsole;
using Ssepan.Io;
using Ssepan.Utility;
using DocumentScannerServerLibrary;

namespace DocumentScannerServerHostConsole
{
    public class App :
        INotifyPropertyChanged
    {
        #region Declarations
        protected Boolean disposed;
        protected ConsoleViewModel<String, Settings, DSServerModel> ViewModel =
            default(ConsoleViewModel<String, Settings, DSServerModel>);
        private static Boolean _ValueChangedProgrammatically;
        #endregion Declarations

        #region Constructors
        public App()
        {
            try
            {
                ////(re)define default output delegate
                //ConsoleApplication.defaultOutputDelegate = ConsoleApplication.writeLineWrapperOutputDelegate;

                //subscribe to notifications
                this.PropertyChanged += PropertyChangedEventHandlerDelegate;

                InitViewModel();
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }
        #endregion Constructors

        #region IDisposable
        ~App()
        {
            Dispose(false);
        }

        public virtual void Dispose()
        {
            // dispose of the managed and unmanaged resources
            Dispose(true);

            // tell the GC that the Finalize process no longer needs
            // to be run for this object.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeManagedResources)
        {
            // process only if mananged and unmanaged resources have
            // not been disposed of.
            if (!this.disposed)
            {
                //Resources not disposed
                if (disposeManagedResources)
                {
                    // dispose managed resources
                    //unsubscribe from model notifications
                    this.PropertyChanged -= PropertyChangedEventHandlerDelegate;
                }
                // dispose unmanaged resources
                disposed = true;
            }
            else
            {
                //Resources already disposed
            }
        }
        #endregion IDisposable

        #region INotifyPropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(String propertyName)
            {
                try
                {
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
        protected void PropertyChangedEventHandlerDelegate
        (
            Object sender,
            PropertyChangedEventArgs e
        )
        {
            try
            {
                if (e.PropertyName == "IsChanged")
                {
                    //ConsoleApplication.defaultOutputDelegate(String.Format("{0}", e.PropertyName));
                    ApplySettings();
                }
                else if (e.PropertyName == "Progress")
                {
                    ConsoleApplication.defaultOutputDelegate(String.Format("{0}", Progress));
                }
                if (e.PropertyName == "StatusMessage")
                {
                    ConsoleApplication.defaultOutputDelegate(String.Format("{0}", ViewModel.StatusMessage));
                    e = new PropertyChangedEventArgs(e.PropertyName + ".handled");
                }
                else if (e.PropertyName == "ErrorMessage")
                {
                    ConsoleApplication.defaultOutputDelegate(String.Format("{0}", ViewModel.ErrorMessage));
                    e = new PropertyChangedEventArgs(e.PropertyName + ".handled");
                }
                //Note: not databound, so handle event
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }
        #endregion PropertyChangedEventHandlerDelegate

        #region Properties
        private String _ViewName = Program.APP_NAME;
        public String ViewName
        {
            get { return _ViewName; }
            set
            {
                _ViewName = value;
                OnPropertyChanged("ViewName");
            }
        }

        private Int32 _Progress = default(Int32);
        public Int32 Progress
        {
            get { return _Progress; }
            set
            {
                _Progress = value;
                OnPropertyChanged("Progress");
            }
        }
        #endregion Properties

        #region Methods
        #region ConsoleAppBase
        protected void InitViewModel()
        {
            try
            {
                //subscribe to notifications
                ModelController<DSServerModel>.Model.PropertyChanged += PropertyChangedEventHandlerDelegate;

                //class to handle standard behaviors
                ViewModelController<String, ConsoleViewModel<String, Settings, DSServerModel>>.New
                (
                    ViewName,
                    new ConsoleViewModel<String, Settings, DSServerModel>
                    (
                        this.PropertyChangedEventHandlerDelegate,
                        new Dictionary<String, String>() 
                        { 
                            { "New", "New" }, 
                            { "Save", "Save" },
                            { "Open", "Open" },
                            { "Print", "Print" },
                            { "Copy", "Copy" },
                            { "Properties", "Properties" }
                        }
                    )
                );
                ViewModel = ViewModelController<String, ConsoleViewModel<String, Settings, DSServerModel>>.ViewModel[ViewName];

                //Init config parameters
                if (!LoadParameters())
                {
                    throw new Exception(String.Format("Unable to load config file parameter(s)."));
                }

                //DEBUG:filename coming in is being converted/passed as DOS 8.3 format equivalent
                //Load
                if ((SettingsController<Settings>.FilePath == null) || (SettingsController<Settings>.Filename == SettingsController<Settings>.FILE_NEW))
                {
                    //NEW
                    ViewModel.FileNew();
                }
                else
                {
                    //OPEN
                    ViewModel.FileOpen();
                }
            }
            catch (Exception ex)
            {
                if (ViewModel != null)
                {
                    ViewModel.ErrorMessage = ex.Message;
                }
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }

        protected void DisposeSettings()
        {

            //save user and application settings 
            //Properties.Settings.Default.Save();

            if (SettingsController<Settings>.Settings.Dirty)
            {
                //SAVE
                ViewModel.FileSave();
            }

            //unsubscribe from model notifications
            ModelController<DSServerModel>.Model.PropertyChanged -= PropertyChangedEventHandlerDelegate;
        }

        public Int32 _Main()
        {
            Int32 returnValue = -1; //default to fail code

            try
            {
                ViewModel.StatusMessage = String.Format("{0} starting...", ViewName);

                ViewModel.StatusMessage = String.Format("{0} started.", ViewName);

                _Run();

                ViewModel.StatusMessage = String.Format("{0} completing...", ViewName);

                DisposeSettings();

                ViewModel.StatusMessage = String.Format("{0} completed.", ViewName);

                //return success code
                returnValue = 0;
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMessage = String.Format("{0} did NOT complete: '{1}'", ViewName, ViewModel.ErrorMessage);
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            finally
            {
                ViewModel = null;
            }
            return returnValue;
        }

        protected void _Run()
        {
        }
        #endregion ConsoleAppBase

        #region Utility

        /// <summary>
        /// Apply Settings to viewer.
        /// </summary>
        private void ApplySettings()
        {
            try
            {
                _ValueChangedProgrammatically = true;

                //apply settings that have databindings
                //BindModelUi();

                ////apply settings that shouldn't use databindings

                //apply settings that can't use databindings
                Console.Title = Path.GetFileName(SettingsController<Settings>.Filename) + " - " + ViewName;

                //apply settings that don't have databindings
                //ViewModel.StatusBarDirtyMessage.Visible = (SettingsController<Settings>.Settings.Dirty);

                _ValueChangedProgrammatically = false;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }

        /// <summary>
        /// Load from app config; override with command line if present
        /// </summary>
        /// <returns></returns>
        private Boolean LoadParameters()
        {
            Boolean returnValue = default(Boolean);
#if USE_CONFIG_FILEPATH
            String _settingsFilename = default(String);
#endif

            try
            {
                if ((Program.Filename != null) && (Program.Filename != SettingsController<Settings>.FILE_NEW))
                {
                    //got filename from command line
                    //DEBUG:filename coming in is being converted/passed as DOS 8.3 format equivalent
                    
                    if (!RegistryAccess.ValidateFileAssociation(Application.ExecutablePath, "." + /*SettingsController<Settings>.Settings.*/SettingsBase.FileTypeExtension))
                    {
                        throw new ApplicationException(String.Format("Settings filename not associated: '{0}'.\nCheck SettingsFilename in app config file.", Program.Filename));
                    }
                    //it passed; use value from command line
                }
                else
                {
#if USE_CONFIG_FILEPATH
                    //get filename from app.config
                    if (!Configuration.ReadString("SettingsFilename", out _settingsFilename))
                    {
                        throw new ApplicationException(String.Format("Unable to load SettingsFilename: {0}", "SettingsFilename"));
                    }
                    if ((_settingsFilename == null) || (_settingsFilename == SettingsController<Settings>.FILE_NEW))
                    {
                        throw new ApplicationException(String.Format("Settings filename not set: '{0}'.\nCheck SettingsFilename in app config file.", _settingsFilename));
                    }
                    //use with the supplied path
                    SettingsController<Settings>.Filename = _settingsFilename;

                    if (Path.GetDirectoryName(_settingsFilename) == String.Empty)
                    {
                        //supply default path if missing
                        SettingsController<Settings>.Pathname = Environment.GetFolderPath(Environment.SpecialFolder.Personal).WithTrailingSeparator();
                    }
#endif
                }

                returnValue = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                //throw;
            }
            return returnValue;
        }
        #endregion Utility
        #endregion Methods
    }
}
