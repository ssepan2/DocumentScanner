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
        //[ServiceKnownType(typeof(List<PackageManifest>))]
        Boolean ManifestsConfirmed
        (
            ref ManifestContract contract, 
            ref String errorMessage
        );

        [OperationContract]
        //[ServiceKnownType(typeof(List<ImageFile>))]
        Boolean DocumentsConfirmed
        (
            ref DocumentContract contract, 
            ref String errorMessage
        );

        [OperationContract]
        //[ServiceKnownType(typeof(List<PackageManifest>))]
        Boolean ManifestsAvailable
        (
            ref ManifestContract contract, 
            ref String errorMessage
        );

        [OperationContract]
        //[ServiceKnownType(typeof(List<ImageFile>))]
        Boolean DocumentsAvailable
        (
            ref DocumentContract contract, 
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

        private List<PackageManifest> _Manifests = null;
        [DataMember]
        public List<PackageManifest> Manifests
        {
            get { return _Manifests; }
            set { _Manifests = value; }
        }

        private String _ErrorMessage = default(String);
        [DataMember]
        public String ErrorMessage
        {
            get { return _ErrorMessage; }
            set { _ErrorMessage = value; }
        }
    }


    [DataContract]
    public class DocumentContract
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

        private List<ImageFile> _Documents = null;
        [DataMember]
        public List<ImageFile> Documents
        {
            get { return _Documents; }
            set { _Documents = value; }
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
