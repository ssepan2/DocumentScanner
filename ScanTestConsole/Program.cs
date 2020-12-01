using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Ssepan.Graphics.Scan;
using Ssepan.Utility;

namespace ScanTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Image image = default(Image);
            String errorMessage = String.Empty;

            //test vistual scan
            ScanOptions scanOptions = new ScanOptions();
            scanOptions.ScannerName = Twain.VirtualScanner;
            scanOptions.VirtualScanPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "scan_simulation");
            if (!Twain.Capture(scanOptions, out image, ref errorMessage))
            {
                throw new Exception(String.Format("Unable to scan: {0}", errorMessage));
            }
        }
    }
}
