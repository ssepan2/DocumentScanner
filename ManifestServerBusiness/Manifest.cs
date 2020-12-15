using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Ssepan.Utility;
using DocumentScannerCommon;
using DocumentScannerServerLibrary;

namespace ManifestServerBusiness
{
    public class Manifest //:
        //IManifest
    {
        //keep delegates here to act as bridge between ManifestServiceServer.PackageManifestService and DSServerController<DSServerModel>
        public delegate X ManifestsConfirmedDelegate<T, S, W, X>(T t, S s, ref W w);
        public static ManifestsConfirmedDelegate<String, DateTime, String, List<PackageManifest>> manifestsConfirmedDelegate = null; //DocumentScannerServerController.AsStatic.ManifestsConfirmed;

        public delegate E DocumentsConfirmedDelegate<A, F, B, D, E>(A a, F f, B b, ref D d);
        public static DocumentsConfirmedDelegate<String, String, DateTime, String, List<ImageFile>> documentsConfirmedDelegate = null; //DocumentScannerServerController.AsStatic.DocumentsConfirmed;

        public delegate XX ManifestsAvailableDelegate<TT, SS, WW, XX>(TT tt, SS ss, ref WW ww);
        public static ManifestsAvailableDelegate<String, DateTime, String, List<PackageManifest>> manifestsAvailableDelegate = null; //DocumentScannerServerController.AsStatic.ManifestsAvailable;

        public delegate EE DocumentsAvailableDelegate<AA, FF, BB, DD, EE>(AA aa, FF ff, BB bb, ref DD dd);
        public static DocumentsAvailableDelegate<String, String, DateTime, String, List<ImageFile>> documentsAvailableDelegate = null; //DocumentScannerServerController.AsStatic.DocumentsAvailable;

        /// <summary>
        /// Load delegates from DSServerController<DSServerModel>.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns>Boolean</returns>
        public static Boolean InitDelegates(ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);

            try
            {
                manifestsConfirmedDelegate = DSServerController<DSServerModel>.ManifestsConfirmed;
                documentsConfirmedDelegate = DSServerController<DSServerModel>.DocumentsConfirmed;
                manifestsAvailableDelegate = DSServerController<DSServerModel>.ManifestsAvailable;
                documentsAvailableDelegate = DSServerController<DSServerModel>.DocumentsAvailable;

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
