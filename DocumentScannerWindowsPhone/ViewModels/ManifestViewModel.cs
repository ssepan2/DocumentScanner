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
    public class ManifestViewModel : 
        ViewModelBase
    {
        public ManifestViewModel()
        {
            this.Manifest = new  ManifestModel();
            this.Manifest.Documents = new ObservableCollection<DocumentModel>();
            
            this.NavigateToDocumentCommand = new ManifestNavigateToDocumentCommand(this);
            //this.NextAppBarPageCommand = new ManifestNextAppBarPageCommand(this);
            this.NavigateToAddImageMenuCommand = new ManifestNavigateToAddImageMenuCommand(this);
            this.DeleteImageCommand = new ManifestDeleteImageCommand(this);
            this.PromoteDocumentCommand = new ManifestPromoteDocumentCommand(this);
            this.DemoteDocumentCommand = new ManifestDemoteDocumentCommand(this);
            this.PackageManifestCommand = new ManifestPackageManifestCommand(this);
            
            DetailIndex = -1;
        }

        public ManifestViewModel
        (
            INavigationHelper navigationHelper//,
            //ManifestPage view
        ) : this()
        {
            NavigationHelper = navigationHelper;
            //View = view;
        }

        public INavigationHelper NavigationHelper = default(NavigationHelper);
        //public ManifestPage View = default(ManifestPage);

        public ICommand NavigateToDocumentCommand { get; private set; }
        //public ICommand NextAppBarPageCommand { get; private set; }
        public ICommand NavigateToAddImageMenuCommand { get; private set; }
        public ICommand DeleteImageCommand { get; private set; }
        public ICommand PromoteDocumentCommand { get; private set; }
        public ICommand DemoteDocumentCommand { get; private set; }
        public ICommand PackageManifestCommand { get; private set; }

        public ManifestModel Manifest { get; private set; }

        private Int32 _DetailIndex = default(Int32);
        public Int32 DetailIndex
        {
            get
            {
                return _DetailIndex;
            }
            set
            {
                if (value != _DetailIndex)
                {
                    _DetailIndex = value;
                    NotifyPropertyChanged("DetailIndex");
                    NotifyPropertyChanged("DetailItem");
                }
            }
        }


        public DocumentModel DetailItem
        {
            get 
            {
                if (DetailIndex != -1)
                {
                    return this.Manifest.Documents[DetailIndex];
                }
                else
                {
                    return null;
                }
            }
            private set { }
        }

        /// <summary>
        /// Creates and adds a few DocumentViewModel objects into the Items collection.
        /// </summary>
        public override void LoadData()
        {
            // Sample data; replace with real data
            this.Manifest.ID = "06024141-4789-4330-8ae6-cf99ead9a5fc";
            this.Manifest.Description = "some Manifest";

            this.Manifest.Documents.Add(new DocumentModel() { ID = "4caed53a-bfd0-4ac5-a63e-2c175cb22071", Description = "document 1", DocumentType = "DocumentType1", Filename = "0.jpg" });
            this.Manifest.Documents.Add(new DocumentModel() { ID = "00fefd69-738c-4b63-826e-c8db48297a5f", Description = "document 2", DocumentType = "DocumentType2", Filename = "1.jpg" });
            this.Manifest.Documents.Add(new DocumentModel() { ID = "bd527137-d669-485d-bda2-7e7718f3abf8", Description = "document 3", DocumentType = "DocumentType3", Filename = "2.jpg" });
            this.Manifest.Documents.Add(new DocumentModel() { ID = "a2a08c5e-ab1f-4443-ade1-64577c92d94c", Description = "document 4", DocumentType = "DocumentType1", Filename = "3.jpg" });
            this.Manifest.Documents.Add(new DocumentModel() { ID = "d4c80864-28ec-4c91-b5c5-899c7c9133c2", Description = "document 5", DocumentType = "DocumentType2", Filename = "4.jpg" });
            this.Manifest.Documents.Add(new DocumentModel() { ID = "0c320e67-9e39-4ad6-8e1e-54eb895a94ca", Description = "document 6", DocumentType = "DocumentType3", Filename = "5.jpg" });

            this.IsDataLoaded = true;
        }

        internal void PackageManifest()
        {
            throw new NotImplementedException();
        }

        internal void PromoteDocument()
        {
            throw new NotImplementedException();
        }

        internal void DemoteDocument()
        {
            throw new NotImplementedException();
        }

        internal void DeleteImage()
        {
            throw new NotImplementedException();
        }
    }
}