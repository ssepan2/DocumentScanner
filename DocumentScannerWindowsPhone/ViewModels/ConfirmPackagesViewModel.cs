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
    public class ConfirmPackagesViewModel : 
        ViewModelBase
    {
        public ConfirmPackagesViewModel()
        {
            this.Manifests = new ObservableCollection<ManifestModel>();
            ManifestDetailIndex = -1;
            this.Documents = new ObservableCollection<DocumentModel>();
            DocumentDetailIndex = -1;

            this.ListConfirmedManifestsCommand = new ConfirmListConfirmedManifestsCommand(this);
        }

        public ICommand ListConfirmedManifestsCommand { get; private set; }

        public ObservableCollection<ManifestModel> Manifests { get; private set; }
        public ObservableCollection<DocumentModel> Documents { get; private set; }

        private DateTime _SelectedDate = default(DateTime);
        public DateTime SelectedDate 
        {
            get
            {
                return _SelectedDate;
            }
            set
            {
                if (value != _SelectedDate)
                {
                    _SelectedDate = value;
                    NotifyPropertyChanged("SelectedDate");
                }
            }
        }

        private Int32 _ManifestDetailIndex = default(Int32);
        public Int32 ManifestDetailIndex
        {
            get 
            {
                return _ManifestDetailIndex;
            }
            set 
            {
                if (value != _ManifestDetailIndex)
                {
                    _ManifestDetailIndex = value;
                    NotifyPropertyChanged("ManifestDetailIndex");
                    NotifyPropertyChanged("ManifestDetailItem");
                }
            }
        }

        public ManifestModel ManifestDetailItem
        {
            get 
            {
                if (ManifestDetailIndex != -1)
                {
                    return Manifests[ManifestDetailIndex];
                }
                else
                {
                    return null;
                }
            }
            private set {}
        }

        private Int32 _DocumentDetailIndex = default(Int32);
        public Int32 DocumentDetailIndex
        {
            get 
            {
                return _ManifestDetailIndex;
            }
            set
            {
                if (value != _DocumentDetailIndex)
                {
                    _DocumentDetailIndex = value;
                    NotifyPropertyChanged("DocumentDetailIndex");
                    NotifyPropertyChanged("DocumentDetailItem");
                }
            }
        }

        public DocumentModel DocumentDetailItem
        {
            get 
            {
                if (DocumentDetailIndex != -1)
                {
                    return Documents[DocumentDetailIndex];
                }
                else
                {
                    return null;
                }
            }
            private set { }
        }

        /// <summary>
        /// Creates and adds a few ConfirmPackagesViewModel objects into the Items collection.
        /// </summary>
        public override void LoadData()
        {
            // Sample data; replace with real data
            this.SelectedDate = DateTime.Now.Date;

            this.Manifests.Add
                (
                    new ManifestModel
                    {
                        ID = "6d989a1c-f7b8-4fe4-b6ef-56f4a89e4021",
                        Description = "some Manifest",
                        Documents = new ObservableCollection<DocumentModel>
                        { 
                            new DocumentModel 
                            { 
                                ID="e57d805a-cae2-41b3-b4f1-03bc62810610", 
                                Description="document 1", 
                                DocumentType="DocumentType1", 
                                Filename="0.jpg" 
                            },
                            new DocumentModel
                            { 
                                ID="7a580f8b-817c-4874-8d1f-191bbb4cf2ac", 
                                Description="document 2", 
                                DocumentType="DocumentType2", 
                                Filename="1.jpg" 
                            },
                            new DocumentModel
                            { 
                                ID="28a36b5c-ed39-40d1-b0cc-e1d9214cfc73", 
                                Description="document 3", 
                                DocumentType="DocumentType3", 
                                Filename="2.jpg" 
                            }
                        }
                    }
                );

            this.Manifests.Add
                (
                    new ManifestModel
                    {
                        ID = "34e4da17-492d-49a0-80aa-f801015682c8",
                        Description = "another Manifest",
                        Documents = new ObservableCollection<DocumentModel>
                        { 
                            new DocumentModel
                            { 
                                ID="4d41a26d-d573-4850-a7d6-9e97647bc0ec", 
                                Description="document 4", 
                                DocumentType="DocumentType1", 
                                Filename="0.jpg" 
                            },
                            new DocumentModel
                            { 
                                ID="94362fdc-f668-4068-a6e9-6d6b51ad4ecb", 
                                Description="document 5", 
                                DocumentType="DocumentType2", 
                                Filename="1.jpg" 
                            },
                            new DocumentModel
                            { 
                                ID="1ddd660d-fda9-4201-84c8-23740140e84f", 
                                Description="document 6", 
                                DocumentType="DocumentType3", 
                                Filename="2.jpg" 
                            }
                        }
                    }
                );

            this.Manifests.Add
                (
                    new ManifestModel
                    {
                        ID = "0ed9157b-2b33-4206-a715-0d76c9c719b5",
                        Description = "a different Manifest",
                        Documents = new ObservableCollection<DocumentModel>
                        { 
                            new DocumentModel
                            { 
                                ID="e84208ba-aa65-4963-8f2d-477bbc7a23fe", 
                                Description="document 7", 
                                DocumentType="DocumentType1", 
                                Filename="0.jpg" 
                            },
                            new DocumentModel
                            { 
                                ID="4395f92e-da28-41b0-a96a-cd11ffc7e07d", 
                                Description="document 8", 
                                DocumentType="DocumentType2", 
                                Filename="1.jpg" 
                            },
                            new DocumentModel 
                            { 
                                ID="a5e109bc-b54e-454e-8898-8e24483b262e", 
                                Description="document 9", 
                                DocumentType="DocumentType3", 
                                Filename="2.jpg" 
                            }
                        }
                    }
                );

            this.IsDataLoaded = true;
        }

        internal void ListConfirmedManifests()
        {
            throw new NotImplementedException();
        }
    }
}