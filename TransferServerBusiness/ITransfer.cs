using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TransferServerBusiness
{
    public interface ITransfer
    {
        Boolean PullFile(String id, String operatorId, String filename, ref Byte[] bytes, ref String errorMessage);
        Boolean PushFile(String id, String operatorId, String filename, Byte[] bytes, ref String errorMessage);
    }
}
