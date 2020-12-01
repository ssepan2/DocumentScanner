using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TransferServerBusiness
{
    public class Transfer
    {
        public delegate X PushDelegate<T, S, U, V, W, X>(T t, S s, U u, V[] v, ref W w);
        public static  PushDelegate<String, String, String, Byte, String, Boolean> pushDelegate = null; //DocumentScannerServerController.AsStatic.PushFile;
        public delegate E PullDelegate<A, F, B, C, D, E>(A a, F f, B b, ref C[] c, ref D d);
        public static  PullDelegate<String, String, String, Byte, String, Boolean> pullDelegate = null; //DocumentScannerServerController.AsStatic.PullFile;
    }
}
