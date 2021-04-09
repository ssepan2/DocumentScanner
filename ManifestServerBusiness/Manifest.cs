using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Ssepan.Utility;
using DocumentScannerCommon;
using DocumentScannerServerLibrary;
using DocumentScannerServerLibrary.MVC;

namespace ManifestServerBusiness
{
    public class Manifest //:
        //IManifest
    {
        //keep delegates here to act as bridge between ManifestServiceServer.PackageManifestService and DSServerModelController<DSServerModel>
        public delegate X ManifestsConfirmedDelegate<T, S, R, W, X>(T t, S s, ref R r, ref W w);
        public static ManifestsConfirmedDelegate<String, DateTime, List<PackageManifest>, String, Boolean> manifestsConfirmedDelegate = null; //DSServerModelController.AsStatic.ManifestsConfirmed;

        public delegate E DocumentsConfirmedDelegate<A, F, B, C, D, E>(A a, F f, B b, ref C c, ref D d);
        public static DocumentsConfirmedDelegate<String, String, DateTime, List<ImageFile>, String, Boolean> documentsConfirmedDelegate = null; //DSServerModelController.AsStatic.DocumentsConfirmed;

        public delegate XX ManifestsAvailableDelegate<TT, SS, RR, WW, XX>(TT tt, SS ss, ref RR rr, ref WW ww);
        public static ManifestsAvailableDelegate<String, DateTime, List<PackageManifest>, String, Boolean> manifestsAvailableDelegate = null; //DSServerModelController.AsStatic.ManifestsAvailable;

        public delegate EE DocumentsAvailableDelegate<AA, FF, BB, CC, DD, EE>(AA aa, FF ff, BB bb, ref CC cc, ref DD dd);
        public static DocumentsAvailableDelegate<String, String, DateTime, List<ImageFile>, String, Boolean> documentsAvailableDelegate = null; //DSServerModelController.AsStatic.DocumentsAvailable;

        /// <summary>
        /// Load delegates from DSServerModelController<DSServerModel>.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns>Boolean</returns>
        public static Boolean InitDelegates(ref String errorMessage)
        {
            Boolean returnValue = default(Boolean);

            try
            {
                manifestsConfirmedDelegate = DSServerModelController<DSServerModel>.ManifestsConfirmed;
                documentsConfirmedDelegate = DSServerModelController<DSServerModel>.DocumentsConfirmed;
                manifestsAvailableDelegate = DSServerModelController<DSServerModel>.ManifestsAvailable;
                documentsAvailableDelegate = DSServerModelController<DSServerModel>.DocumentsAvailable;

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
