using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentScannerWindowsPhone
{
    public interface INavigationHelper
    {
        //void Home();
        void Manifest();
        void QueuedPackages();
        void Review();
        void Confirm();
        void Receive();
        
        void Document(Int32 index);
        void AddImageMenu();

    }
}
