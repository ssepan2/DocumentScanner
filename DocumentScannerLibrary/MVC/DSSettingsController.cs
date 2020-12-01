using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Ssepan.Application;
using Ssepan.Io;
using Ssepan.Utility;

namespace DocumentScannerLibrary
{
	/// <summary>
    /// Manager for the persisted Settings. Custom override.
    /// </summary>
    public class DSSettingsController :
        SettingsController<Settings>
    {
        #region constructors
        /// <summary>
        /// Init properties
        /// </summary>
        public DSSettingsController()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
                throw;
            }
        }
        #endregion constructors

        #region Properties
        #endregion Properties

        #region Methods

        /// <summary>
        /// Custom override of SettingsController(Of TSettings).New(); manages data file and data folder.
        /// </summary>
        public static new void New()
        {//TODO: override SettingsController<Settings> and call that
            Action postNewDelegate = default(Action);

            try
            {
                //Optional delegate to be run after Save (in SettingsControllerBase) but before Refresh.
                postNewDelegate =
                    () =>
                    {
                        String folderPath = default(String);

                        //check for folder and delete if present
                        folderPath = Path.Combine(DSController<DSModel>.Model.DataPath, SettingsController<Settings>.FILE_NEW);

                        //check for folder and delete if present
                        Folder.DeleteFolderWithWait(folderPath, DSController<DSModel>.Model.ReNewWaitMilliseconds);

                        //check for folder and create if missing
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }
                    };

                //Note:static method 'overridden' in base class
                SettingsController<Settings>.New();

                postNewDelegate();
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Custom override of SettingsController(Of TSettings).Open; provides for setting oldfilename to filename
        /// </summary>
        public static new void Open()
        {//TODO: override SettingsController<Settings> and call that
            Action postOpenDelegate = default(Action);

            try
            {
                //Optional delegate to be run after Save (in SettingsControllerBase) but before Refresh.
                postOpenDelegate =
                    () =>
                    {
                        //Open was not synchronizing OldFilename with Filename, and Save logic was seeing '(new)'
                        //and performing wrong actions.
                        //force new Filename into OldFilename
                        SettingsController<Settings>.Filename = SettingsController<Settings>.Filename;
                    };

                //Note:static method 'overridden' in base class
                SettingsController<Settings>.Open();

                postOpenDelegate();
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Custom override of SettingsController(Of TSettings).Save; provides delegate for managing
        /// data file and data folder.
        /// </summary>
        public static new void Save()
        {//TODO: override SettingsController<Settings> and call that
            Action postSaveDelegate = default(Action);

            try
            {
                //Optional delegate to be run after Save (in SettingsControllerBase) but before Refresh.
                postSaveDelegate =
                    () =>
                    {
                        String oldFolderPath_ = default(String);
                        String newFolderPath_ = default(String);
                        String filePath = default(String);

                        if (SettingsController<Settings>.OldFilename.ToUpper() != SettingsController<Settings>.Filename.ToUpper())
                        {

                            if (SettingsController<Settings>.OldFilename == SettingsController<Settings>.FILE_NEW)
                            {
                                //source will be (new)
                                //if (new), then rename folder to transaction's id
                                oldFolderPath_ = Path.Combine(DSController<DSModel>.Model.DataPath, SettingsController<Settings>.OldFilename);
                                //force transaction id for destination
                                newFolderPath_ = DSController<DSModel>.GetTransactionImagesPath(true);
                                Directory.Move(oldFolderPath_, newFolderPath_);
                            }
                            else //(oldFilename != SettingsController<Settings>.FILE_NEW) 
                            {
                                //if not (new), then delete original data file
                                //filename is different; delete original because it has same transaction #
                                filePath = Path.Combine(DSController<DSModel>.Model.DataPath, SettingsController<Settings>.OldFilename);
                                System.IO.File.Delete(filePath);
                            }

                            //force new Filename into OldFilename
                            SettingsController<Settings>.Filename = SettingsController<Settings>.Filename;
                        }
                    };

                //Call to controller base Save() writes xml and triggers refresh...
                //...but refresh was seeing non-new transaction and getting wrong transaction path...
                // ... because folder was still 'new' until Move().
                //Note:static method 'overridden' in base class
                SettingsController<Settings>.Save();

                postSaveDelegate();
            }
            catch (Exception ex)
            {
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
        }

        #endregion Methods

    }
}
