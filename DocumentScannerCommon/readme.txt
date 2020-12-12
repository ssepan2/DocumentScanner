DocumentScanner v0.12 (Alpha)


Purpose:
To let the user scan and package documents for transmission to server. 

Setup (Service):
Run the service setup on a web server. 
Create a folder to receive package files. Set PushReceivePath to this location. 
Create a folder to send package files. Set PullSendPath to this location. 
Set LogFilename and UseLogFile according to whether you want to use the Windows Event Logs or an application log file. To use The Application log in Windows Event Logs, set UseLogFile  to 'false' and LogFilename to '.'. See 'Known Issues' for more information. To use an application log set UseLogFile  to 'true' and LogFilename to 'Application.Log'.
Set ReCreateWaitMilliseconds to '1000' milliseconds to allow the file system to catch up after deleting a folder and before creating a new folder of the same name. See 'Fixes' for more information.
In the IIS management console, create IIS web application under 'Sites', DocumentScannerServer for the name, and a free port (i.e. 8080).
Under 'Application Pools', there should be an entry for 'DocumentScannerServer'. Make sure that it is set  for the version of the .Net Framework used in the projects (v4.0). Leave app pool 'Managed pipeline mode' as 'Integrated'. ('Classic' seems to run into problems with static filter.)
In the Web.config, set path/port in base address to 'http[s]://<host>:<port>/'. If you configured DocumentScannerServer to appear under a folder below the root, append that to the base path.
At the command line, in the appropriate .Net framework folder (i.e. - '<drive>:\<windows_folder>\microsoft.net\framework\v4.0.30319'), run "aspnet_regiis -i". In IIS, under App Pools you should see 'ASP.Net v<version>' 'ASP.Net v<version> Classic'.
Verify that you can see the service in your web browser at 'http[s]://<host>:<port>/[<subfolder>/]FileTransferService.svc'.

Setup (Server):
Run the server setup on an application server. This may be the same machine as the web server, but does not have to be.
In the app.config (compiled as DocumentScannerServer.exe.config), Set PushReceivePath to the folder used to receive package files (see 'Setup (Service)' section above).
Create a folder to store working data for package data (after it is un-packaged). Set DataPath to this location.
Set ReCreateWaitMilliseconds to '1000' milliseconds to allow the file system to catch up after deleting a folder and before creating a new folder of the same name. See 'Fixes' for more information.
Set CompletedTransactionRetentionDays and ErrorTransactionRetentionDays to a number of days to retain processed packages, which are stored in dated sub-folders under '<PushReceivePath>\Completed' and '<PushReceivePath>\Error' respectively. Use the value '-1' to retain packages indefinitely.
Set LogFilename and UseLogFile according to whether you want to use the Windows Event Logs or an application log file. To use The Application log in Windows Event Logs, set UseLogFile  to 'false' and LogFilename to '.'. See 'Known Issues' for more information. To use an application log set UseLogFile  to 'true' and LogFilename to 'Application.Log'.
Set SettingsFilename to 'Server.DocumentScannerServer'. Copy a template from settings folder in the installed folder, and place it into the MyDocuments folder. This is reserved for future use for settings.
Set UserSendFolder to the name of the send folder under each user's folder under the Data folder.
Customize the DocumentTypes.xml file with your document types.
Configure your operating system scheduler to point to the installed location of DocumentScannerServerHostConsole.exe, and set the schedule according to your preference.
If you want to make data available to the server to the Receive function, manually create and copy a manifest file and a matching-named images-folder in the user's folder (under Data), in a Send folder.

Setup (Client):
Run the client setup on a remote PC or laptop. 
In the app.config (compiled as DocumentScanner.exe.config), modify the 'endpoint address' entries to reflect the URL used in the server configuration (see 'Setup (Service)' section above).
Create a folder to store working data for package data (before it is packaged). Set DataPath to this location.
Create a folder to send package files. Set PushSendPath to this location. 
Create a folder to receive package files. Set PullReceivePath to this location. 
Choose one of the two bindings ('WsHttpBinding' or 'BasicHttpBinding') from the configuration file, by setting FileTransferEndpointConfigurationName to "BasicHttpBinding_IFileTransferService" or "WSHttpBinding_IFileTransferService". See the section 'Known Issues' for some relevant information.
Choose one of the two bindings ('WsHttpBinding' or 'BasicHttpBinding') from the configuration file, by setting PackageManifestEndpointConfigurationName to "BasicHttpBinding_IPackageManifestService" or "WSHttpBinding_IPackageManifestService". See the section 'Known Issues' for some relevant information.
Set AutoNavigateTabs to 'true' to enable navigation to specific tabs after performing specific actions. New, Open, or Scan will activate the Scan tab. Package will activate the Send tab. Send will activate the Review tab.
Set ReNewWaitMilliseconds to '1000' milliseconds to allow the file system to catch up after deleting an unsaved project folder '(new)' and before creating a new project folder of the same name. See 'Fixes' for more information.
Set CompletedTransactionRetentionDays and ErrorTransactionRetentionDays to a number of days to retain sent packages, which are stored in dated sub-folders under '<PushSendPath>\Completed' and '<PushSendPath>\Error' respectively. Use the value '-1' to retain packages indefinitely.
Set LogFilename and UseLogFile according to whether you want to use the Windows Event Logs or an application log file. To use The Application log in Windows Event Logs, set UseLogFile  to 'false' and LogFilename to '.'. See 'Known Issues' for more information. To use an application log set UseLogFile  to 'true' and LogFilename to 'Application.Log'.
Copy the customized DocumentTypes.xml file from the server, or edit the client copy to match.

Usage:
In the client, start a new project. This will create (or recreate) a folder called '(new)' under DataPath. In memory, a list of images and a new GUID transaction number are maintained.
Scan documents into the Documents list. This will store images (in .JPG format) under the '(new)' folder.
Save the list of documents the first time. The list is saved in XML format (with a .documentscanner extension) under DataPath. The '(new)' folder is renamed to the GUID transaction number.
Optionally scan more documents into the Documents list. This will store images (in .JPG format) under the GUID transaction number folder.
Save the list of documents after the first time. The list is re-saved in XML format (with a .documentscanner extension), under DataPath. 
Package the currently opened data (that is saved and has no unsaved changes). An archive (in .zip format) is created under PushSendPath with the name set to the GUID transaction number. The XML file created by serializing the manifest and is named from <transactionnumber>.documentscanner to data.xml, and is stored in the archive. The the folder of images is copied into the archive. Finally, a new project is started.
Send the package(s). Each packaged archive in the PushSendPath folder is transmitted from the client to the server via a Windows Communication Foundation (WCF) layer. An internal transaction is started, using the GUID transaction number. A sub-folder is created in '<PushSendPath>\Working' with the name set to the transaction number, and the package file is moved to '<PushSendPath>\Working\<TransactionNumber>\<TransactionNumber>.zip'. The file is read from this location into memory as an array of bytes, and transmitted via the Push function of the TransferService. The service responds by receiving the array of bytes and writing the transmitted data to PushReceivePath as '<PushReceivePath>\<TransactionNumber>.zip'. (Note: future releases may then perform additional server-side processing of the received file. See 'Possible Enhancements'.) 
The client application will then wrap up the transaction, based on whether the transmission succeeded or failed. On transaction success, the package file will be moved from '<PushSendPath>\Working\<TransactionNumber>\ to a dated sub-folder under '<PushSendPath>\Completed'. On transaction failure, the package file will be moved from '<PushSendPath>\Working\<TransactionNumber>\ to a dated sub-folder under '<PushSendPath>\Error'. Then, in either case, the transaction number sub-folder at '<PushSendPath>\Working\<TransactionNumber>\ will be deleted. 
Finally, after this process has been performed for each package file queued for transmission, the transaction logic will clean up by finding dated sub-folders under '<PushSendPath>\Completed' and '<PushSendPath>\Error' and deleting any older than the configured number of days.
On the server side, packages are processed as one or more are detected in a the PushReceivePath specified in App.config. Transactions are used to move packages to working folder, where they are unpacked, the manifest loaded, validated, and finally the images and manifest are copied to an operator-named sub-folder under the data folder (as specified in config).
Packages that have been processed may be viewed in the Confirm tab. Select a date to view and click 'List Manifests'. Manifest information will populate the grid to the left, and right-clicking each will pop up a menu with a menu choice 'List Documents', which will populate the grid to the right with document information. (Note: Images on the server are not displayed at this time.)
Packages that have failed to Send can now be un-packaged for editing and correction. Un-packaged data will produce a settings file of <transactionid>.documentscanner.
UnPackaged packages can be edited and can be split into two smaller package settings definitions to help with packages that fail due to their size. In the Scan tab, select a row, right click, and select 'Split Package Above' or 'Split Package Below' to split above or below the selected row, respectively. The portion below will retain the original name and transaction id. The portion above will be assigned a new name and transaction id, and will the one left open in the editor when the split is complete. Note: UnPackage is only available from the Failed list on the Review tab; is is not available from the Queued list on the Send tab.
Manifests can be queued on the server by an administrator (manual process for now) so that the client can pull the data as a package and open it. The unpackaged manifest and images-folder are placed in the server Data\<user>\<send>\ folder, where 'user' is a folder named with the operator id, and 'send' is a folder name defined by the UserSendFolder setting in the web config file. The user can view available manifests in the new Receive tab, and can right click a manifest and choose 'Receive'. This will trigger th eserver logic to package the manifest and images-folder, and transmit it back to the client. The package will then be unpackaged and opened in the client.

Enhancements:
0.Next: (PLANNED)
~TODO:research implementing package-receipt confirmation web service call, and delete package from server that way instead; implement a negative acknowledgement with a reset as well
~TODO:review use of ref errorMessage versus logging
~TODO:look to simplify new/open/save logic across UI/controller/model/settings.
~TODO:collapse extra layer of service calls in controller; pass endpoint directly in outer call?
~TODO:design a manifest merge function
~TODO:Implement printing of multiple selected documents.
~TODO:Implement receive of multiple selected manifests.
~TODO:implement live update of document types from server to clients; use auto notification and opt-in download.
~TODO:implement Cancel in Receive
~TODO:consolidate delete wait time settings name, location

0.12:
~Updated Ssepan.* to 2.7
~update to Framework 4.8
~convert to SDK style project
~Fixed issued in VS2015 where List<T> is returned as U[], by forcing return of generic list in WCF client proxy config (compile error remains, see next).
~Fixed compile error in VS2015 where List<T> is still returned as List<U>; moved service contract interfaces into new project DocumentScannerServiceCommon and Updated config files to reflect change.
~Fixed compile error in VS2015 where static SettingsBase properties had to be accessed directly instead of through property on SettingsController<T>.

0.11: (RELEASED)
~Refactored to use new MVVM / MVC hybrid.
~Updated Ssepan.* to 2.6

0.10: 
~Tested with DocketPORT 467.
~Fixed bug in initialization of twain in scanner select function.
~Using v2.5 of Ssepan.* libraries.

0.9:
~Fixed delay while recursively deleting temporary folder during Package.
~Client lists manifests available on server for Receive.
~Refactored portions of package, unpackage into DocumentScannerCommon.Package, DocumentScannerCommon.PackageManifest, and separate methods in DocumentScannerController.
~Implemented Receive (pull) of one manifest from server onto client; received manifest is opened in client.
~Updated Ssepan.* to reflect v2.4 and Folder class in Io.
~Confirm no longer needs to call server to list a manifest's documents' info. Receive designed similarly.

0.8:
~Fixed confirmedManifestBindingNavigator binding.
~Implemented Cancel functionality with ESC key in Send, Package, UnPackage, Split.

0.7:
~Implemented printing of a (single) selected document.
~Implemented disabling of function buttons / menus while function is running; Refresh will clear.
~Fixed crash when right-clicking empty Failed list; problem was with binding IEnumerable<anonymoustype> to DataGridView where grid adds an empty row; fixed with ToList().
~Replaced 'throw ex;' statements with 'throw;'.
~Researched first-scan bug, where occasionally the first image-selection will not transfer the selected image(s). See 'Known Issues'.
~Fixed readme2.txt not being installed in installation sets.

0.6:
~Created client UI and logic to unpack and re-edit failed packages that were not sent.
~Package now names manifest as '<transactionid>.xml' instead of 'data.xml'. Server processing was modified to expect this format. Client side UnPackage feature will expect this format.
~Created client UI and logic to split an open, saved package before/after a selected row, creating two smaller packages. This is to help with the problem of large packages not transmitting, and will follow an action to unpack and re-edit a failed package.
~Fixed error assignment in instances of BackgroundWorker.
~Added validation check for document type against document types list.
~Renumbered Ssepan.* to 2.3 for change to Ssepan.Utility.ObjectHelper to include Cast().
~Modified DocumentScannerCommon.DocumentType to contain GetDocumentTypes method, instead of DocumentTypes properties that were in DocumentScannerLibrary.DocumentScannerController and DocumentScannerServerLibrary.DocumentScannerServerController. DocumentTypes.xml content was moved into a 'Resources' subfolder and it's contents are accessible as a resource via DocumentScannerCommon.Properties.Resources.DocumentTypes.

0.5: 
~Modified Ssepan.Graphics to add static extension ImageExtensions to System.Drawing.Image; extensions provide access to ImageCodecInfo.
~Modified Ssepan.Transaction to add static GetDatedSubFolderNameFromDate method; updated all Ssepan.* libraries to 2.2.
~Modified server library to store packages in dated subfolders of user.
~Added ManifestService layers to solution, to provide features to interact with manifests on the server.
~Fixed missing property changed notifications on ImageFile Description.
~Renamed endpoint binding configuration setting for File Transfer in app config to be service-specific, and added new setting for Package Manifest.
~The following projects were deprecated and removed from the solution: ServiceClientTestConsole, TransferClientBusiness, ManifestClientBusiness. 
~TransferServerBusiness has been re-purposed to include only the interface between the service and the server library.
~Fixed Documents context menu association with other grids besides Documents grid on the Scan tab.
~Fixed image rotation of Jpeg images; was not compressing image when re-saved as Jpeg.

0.4: 
~Packages are processed on the server side as one or more are detected in an expected location. 
~Moved client and server side Data folders out of app Bin\Debug folder.
~Modified package creation to keep images in their transaction-numbered subfolder when placed into the package.
~Vaidating document type and document description.
~Fixed StopProgressBar overwriting UpdateStatusBarMessage. Stop does not clear error, start does.
~Changed DocumentTypes.xml from 'Copy if newer' to 'Copy always' because it was not appearing in the output folder.
~Renamed server setup to DocumentScannerService Setup, and added 3rd setup for server.
~Moved LICENSE.TXT and readme.TXT into DocumentScannerCommon; added common to web host.
~Removed scan_simulation folder from DocumentScanner\bin\Debug.

0.3:
~Added Description to manifest, added Description and DocumentType to image file. Added fields to UI.

0.2: 
~Corrected assembly info on TwainLib.
~Extracted Settings / Model / Controller into DocumentScannerLibrary.
~Extracted ImageFile into DocumentScannerCommon.
~(Breaking Change) Separated a PackageManifest class from the other Settings as a Manifest property. Added PackageManifest to DocumentScannerCommon. PackageManifest is now the serialized type. Note: version 0.2 settings and manifest files are not compatible with 0.1.
~Client / server side receives write as temp, then rename to zip, so that processing does not pick up file until writing is complete.
~Fixed Document Count not being serialized.
~Fixed ServiceClientTestConsole configuration file endpoints / bindings, and code.

0.1: 
~Initial release. Alpha Level (prototype).
~Scans multiple items at a time, saving JEPGs to disk and filenames to a List<Image>.
~Packages data (just filename for now) in XML format and images into a Zip.
~Sends package to server via WCF.
~Selecting New with unsaved new project would fail to fully delete (and thus re-create) '(new)' folder. When this happened, it somehow passed Directory.Exists() (because of a temporary file of the same name that appears briefly in explorer), so app thinks it was created. However, scan will fail on image save, displaying 'scan not completed' message. Status: Fixed. Version: 0.1. Work-around: Re-run File|New. Fix: Changed New to allow 1 second delay for file system to properly delete folder before re-creating.

Known Issues:
~Occasionally the scan does not transfer the selected image(s). This may occur either on the first scan, shortly after the list of available images in presented, or on subsequent scans if the available images are selected very quickly (by double-clicking.) It seams that by pausing for a second or two, something in the twain driver is able to respond as expected.
~Send w/ WsHttpBinding fails if zip archive larger than about 3.38MB (containing about 6 test doc images of ~1500KB each). Status: Researching. WorkAround: Limit size of documents per package. 
~Send w/ BasicHttpBinding fails if zip archive larger than about 4.12MB (containing about 7 test doc images of ~1500KB each).  Status: Researching. WorkAround: Limit size of documents per package.
~get_item() call from RefreshImage got 'Index 0 does not have a value.' at Line 1439. (not reproduced). Status: Monitor for additional incidents.
~Filename passed in command line argument is converted/passed in DOS 8.3 equivalent format. Cannot compare file extension directly. Status: research. 
~Running this app under Vista or Windows 7 requires that the library that writes to the event log (Ssepan.Utility.dll) have its name added to the list of allowed 'sources'. Rather than do it manually, the one way to be sure to get it right is to simply run the application the first time As Administrator, and the settings will be added. After that you may run it normally. To register additional DLLs for the event log, you can use this trick any time you get an error indicating that you cannot write to it. Or you can manually register DLLs by adding a key called '<filename>.dll' under HKLM\System\CurrentControlSet\services\eventlog\Application\, and adding the string value 'EventMessageFile' with the value like <C>:\<Windows>\Microsoft.NET\Framework\v2.0.50727\EventLogMessages.dll (where the drive letter and Windows folder match your system). Or if this solution includes the EventSourceRegistrationForm project, you can run that (as Administrator) and enter any .EXE or .DLL filenames that need access to the event log (start with ssepan.utility.dll and see if that doesn't clear up most of the issues). Status: work-around. 
~System.IO.File.Directry.GetFiles() has a known issue, dating back to at least 2005, and originating in the Win32 FileFind API, that takes an extension of of exactly 3 characters, and also returns any files with extensions of more than three characters where the first three match. This issue may affect the contents of the package grids. Impact: Low probability. Status: Hold.
~When Visual Studio generates the client proxy for a WCF service, it uses the service name (i.e. - 'XxxService') and appends 'Client' (i.e. - to produce the name 'XxxServiceClient'). If you have used this name anywhere else in the client project (such as in the name of the client project itself) you will run into compilation errors. Choose your service name and client project name accordingly.

Possible Enhancements:
~Move form size / location out of project settings and into an app or user level settings file.
~Develop authentication / authorization for web service. (See: http://www.codeproject.com/KB/WCF/WCFBasicHttpBinding.aspx).
~Implement a NetTcpBinding in addition to the BasicHttpBinding and WsHttpBinding now implemented. Research port sharing (http://msdn.microsoft.com/en-us/library/ms734772.aspx).


Steve Sepan
ssepanus@yahoo.com
3/14/2017