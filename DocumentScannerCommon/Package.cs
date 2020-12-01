using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ssepan.Compression;
using Ssepan.Io;
using Ssepan.Utility;
using System.Diagnostics;
using System.Reflection;

namespace DocumentScannerCommon
{
    public class Package
    {
        #region Declarations
        public const String PACKAGE_FILE_TYPE = "zip";
        public const String PACKAGE_FOLDER = "package";
        public const String TEMP_FILE_TYPE = "tmp";
        #endregion Declarations

        /// <summary>
        /// Store (package) manifest and images in package file.
        /// </summary>
        /// <param name="manifest">Loaded instance of manifest to be packaged.</param>
        /// <param name="packageId"></param>
        /// <param name="packagePath"></param>
        /// <param name="packageContentsRootPath"></param>
        /// <param name="progressDelegate"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Boolean FillManifestPackage
        (
            PackageManifest manifest,
            String packageId,
            String packagePath,
            String packageContentsRootPath,
            Int32 deleteWaitMilliseconds,
            Action<String> progressDelegate,
            ref String errorMessage 
        )
        {
            Boolean returnValue = default(Boolean);
            String packageTempFilename = String.Empty;
            String packageTempFilePath = String.Empty;
            String packageContentsPackageSubfolderPath = default(String);
            String packageFinishedFilename = String.Empty;
            String packageFinishedFilePath = String.Empty;

            try
            {
                //identify package contents
                packageContentsPackageSubfolderPath = Path.Combine(packageContentsRootPath, PACKAGE_FOLDER);


                if
                (
                    !DocumentScannerCommon.PackageManifest.LoadAndValidateManifest
                    (
                        packageId,
                        packageContentsPackageSubfolderPath,
                        progressDelegate,
                        ref errorMessage,
                        out manifest
                    )
                )
                {
                    throw new Exception(String.Format("Package folder preparation failure: '{0}'", errorMessage));
                }

                
                //report status
                progressDelegate(String.Format("packaging from folder..."));
                
                //zip manifest and images folder to package in transmit folder (as temp file)
                packageTempFilename = String.Format("{0}.{1}", packageId, TEMP_FILE_TYPE);
                packageTempFilePath = Path.Combine(packagePath, packageTempFilename);
                //zip folder of images and  manifest file in one shot
                if (!Zip.Compress(packageTempFilePath, packageContentsPackageSubfolderPath, true, "", "", ref errorMessage))
                {
                    throw new ApplicationException(String.Format("Unable to compress '{0}' in '{1}' to '{2}': {3}", packageId, packageContentsPackageSubfolderPath, packageTempFilePath, errorMessage));
                }


                //report status
                progressDelegate(String.Format("submitting (renaming) package..."));

                //Submit package; move (rename) zip file within transmit folder
                packageFinishedFilename = String.Format("{0}.{1}", packageId, PACKAGE_FILE_TYPE);
                packageFinishedFilePath = Path.Combine(packagePath, packageFinishedFilename);
                System.IO.File.Move(packageTempFilePath, packageFinishedFilePath);

                
                //report status
                progressDelegate(String.Format("cleaning up..."));
                
                //clean up files and folders

                //delete package directory used for zipping
                if (Directory.Exists(packageContentsPackageSubfolderPath))
                {
                    Folder.DeleteFolderWithWait(packageContentsPackageSubfolderPath, deleteWaitMilliseconds);
                }


                returnValue = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        /// <summary>
        /// Extract (unpackage) manifest and images from package file.
        /// Assumes destination package subfolder does not exist, or is empty. 
        ///  Will delete and recreate it if necessary. Will leave it after filling it.
        /// </summary>
        /// <param name="packageId">Transaction ID</param>
        /// <param name="packagePath">Path containing package file.</param>
        /// <param name="packageContentsRootPath">Path where package subfolder will be created.</param>
        /// <param name="progressDelegate"></param>
        /// <param name="errorMessage"></param>
        /// <param name="manifest">Loaded instance of unpackaged manifest.</param>
        /// <returns></returns>
        public static Boolean ExtractManifestPackage
        (
            String packageId,
            String packagePath,
            String packageContentsRootPath,
            Int32 deleteWaitMilliseconds,
            Action<String> progressDelegate,
            ref String errorMessage,
            out PackageManifest manifest
        )
        {
            Boolean returnValue = default(Boolean);
            String packageFilename = default(String);
            String packageFilePath = default(String);
            String packageContentsPackageSubfolderPath = default(String);

            manifest = default(PackageManifest);

            try
            {
                //prepare package subfolder
                packageContentsPackageSubfolderPath = Path.Combine(packageContentsRootPath, PACKAGE_FOLDER);
                if
                (
                    !DocumentScannerCommon.Package.PreparePackageSubFolder
                    (
                        packageContentsPackageSubfolderPath,
                        deleteWaitMilliseconds,
                        progressDelegate,
                        ref errorMessage
                    )
                )
                {
                    throw new Exception(String.Format("Package folder preparation failure: '{0}'", errorMessage));
                }


                //report status
                progressDelegate(String.Format("un-packaging to folder..."));

                //unzip package, as loose contents in specified folder
                packageFilename = String.Format("{0}.{1}", packageId, DocumentScannerCommon.Package.PACKAGE_FILE_TYPE);
                packageFilePath = Path.Combine(packagePath, packageFilename);
                if (!Zip.Decompress(packageFilePath, packageContentsPackageSubfolderPath, "", ref errorMessage))
                {
                    throw new Exception(String.Format("Package unzip failure: '{0}'", errorMessage));
                }


                //validate manifest, and return manifest object
                if
                (
                    !DocumentScannerCommon.PackageManifest.LoadAndValidateManifest
                    (
                        packageId,
                        packageContentsPackageSubfolderPath,
                        progressDelegate,
                        ref errorMessage,
                        out manifest
                    )
                )
                {
                    throw new Exception(String.Format("Package folder preparation failure: '{0}'", errorMessage));
                }

                returnValue = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }

        /// <summary>
        /// Ensure Package subfolder is present and clean.
        /// </summary>
        /// <param name="packageContentsPackageSubfolderPath"></param>
        /// <param name="deleteWaitMilliseconds"></param>
        /// <param name="progressDelegate"></param>
        /// <param name="errorMessage"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public static Boolean PreparePackageSubFolder
        (
            String packageContentsPackageSubfolderPath,
            Int32 deleteWaitMilliseconds,
            Action<String> progressDelegate,
            ref String errorMessage
        )
        {
            Boolean returnValue = default(Boolean);
            try
            {
                //report status
                progressDelegate(String.Format("preparing package folder..."));

                //delete and recreate as necessary
                if (Directory.Exists(packageContentsPackageSubfolderPath))
                {
                    Folder.DeleteFolderWithWait(packageContentsPackageSubfolderPath, deleteWaitMilliseconds);
                }
                if (!Directory.Exists(packageContentsPackageSubfolderPath))
                {
                    Directory.CreateDirectory(packageContentsPackageSubfolderPath);
                }

                returnValue = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.Write(ex, MethodBase.GetCurrentMethod(), EventLogEntryType.Error);
            }
            return returnValue;
        }
    }
}
