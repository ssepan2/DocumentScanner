using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using DocumentScannerCommon;

namespace DocumentScannerServiceCommon
{
    [ServiceContract]
    public interface IPackageManifestService
    {

        [OperationContract]
        Boolean Ping(ref String errorMessage);

        [OperationContract]
        [ServiceKnownType(typeof(List<PackageManifest>))]
        List<PackageManifest> ManifestsConfirmed
        (
            ManifestContract contract, 
            ref String errorMessage
        );

        [OperationContract]
        [ServiceKnownType(typeof(List<ImageFile>))]
        List<ImageFile> DocumentsConfirmed
        (
            ManifestContract contract, 
            ref String errorMessage
        );

        [OperationContract]
        [ServiceKnownType(typeof(List<PackageManifest>))]
        List<PackageManifest> ManifestsAvailable
        (
            ManifestContract contract, 
            ref String errorMessage
        );

        [OperationContract]
        [ServiceKnownType(typeof(List<ImageFile>))]
        List<ImageFile> DocumentsAvailable
        (
            ManifestContract contract, 
            ref String errorMessage
        );

    }


    [DataContract]
    public class ManifestContract
    {
        private String _OperatorId = String.Empty;
        [DataMember]
        public String OperatorId
        {
            get { return _OperatorId; }
            set { _OperatorId = value; }
        }

        private String _TransactionId = String.Empty;
        [DataMember]
        public String TransactionId
        {
            get { return _TransactionId; }
            set { _TransactionId = value; }
        }

        private DateTime _Date = DateTime.Now;
        [DataMember]
        public DateTime Date
        {
            get { return _Date; }
            set { _Date = value; }
        }

        private String _ErrorMessage = default(String);
        [DataMember]
        public String ErrorMessage
        {
            get { return _ErrorMessage; }
            set { _ErrorMessage = value; }
        }
    }
}
