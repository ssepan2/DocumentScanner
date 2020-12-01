using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;


namespace DocumentScannerWindowsPhone
{
    public class DocumentViewModel :
        ViewModelBase
    {
        public DocumentViewModel()
        {
            this.Document = new DocumentModel();

            this.DocumentRotateCCWCommand = new DocumentRotateCCWCommand(this);
            this.DocumentRotateCWCommand = new DocumentRotateCWCommand(this);
        }

        public DocumentViewModel
        (
            INavigationHelper navigationHelper
        ) : this()
        {
            NavigationHelper = navigationHelper;
        }

        public INavigationHelper NavigationHelper = default(NavigationHelper);

        public ICommand DocumentRotateCCWCommand { get; private set; }
        public ICommand DocumentRotateCWCommand { get; private set; }

        public DocumentModel Document { get; private set; }

        /// <summary>
        /// gets a  DocumentViewModel object .
        /// </summary>
        public override void LoadData()
        {
            // Sample data; replace with real data
            this.Document = App.ManifestVM.DetailItem;

            this.IsDataLoaded = true;
        }

        internal void DocumentRotateCCW()
        {
            throw new NotImplementedException();
        }

        internal void DocumentRotateCW()
        {
            throw new NotImplementedException();
        }
    }
}