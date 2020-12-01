using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TransferServiceServer
{
    [ServiceContract]
    public interface IFileTransferService
    {

        [OperationContract]
        Boolean Ping(ref String errorMessage);

        [OperationContract]
        Boolean Push(TransactionContract transactionContract, ref String errorMessage);

        [OperationContract]
        Boolean Pull(ref TransactionContract transactionContract, ref String errorMessage);
    }


    [DataContract]
    public class TransactionContract
    {

        private String _ID = String.Empty;
        [DataMember]
        public String ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        private String _Operator = String.Empty;
        [DataMember]
        public String Operator
        {
            get { return _Operator; }
            set { _Operator = value; }
        }

        private String _Filename = default(String);
        [DataMember]
        public String Filename
        {
            get { return _Filename; }
            set { _Filename = value; }
        }

        private Byte[] _ByteArray = null;
        [DataMember]
        public Byte[] ByteArray
        {
            get { return _ByteArray; }
            set { _ByteArray = value; }
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
