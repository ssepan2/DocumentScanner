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
    public class ReviewPackagesViewModel : 
        ViewModelBase
    {
        public ReviewPackagesViewModel()
        {
            this.CompletedPackages = new ObservableCollection<PackageModel>();
            CompletedDetailIndex = -1;
            this.FailedPackages = new ObservableCollection<PackageModel>();
            FailedDetailIndex = -1;

            this.UnpackageManifestCommand = new ReviewUnpackageManifestCommand(this);
        }

        public ICommand UnpackageManifestCommand { get; private set; }

        public ObservableCollection<PackageModel> CompletedPackages { get; private set; }
        public ObservableCollection<PackageModel> FailedPackages { get; private set; }

        private Int32 _CompletedDetailIndex = default(Int32);
        public Int32 CompletedDetailIndex
        {
            get
            {
                return _CompletedDetailIndex;
            }
            set
            {
                if (value != _CompletedDetailIndex)
                {
                    _CompletedDetailIndex = value;
                    NotifyPropertyChanged("CompletedDetailIndex");
                    NotifyPropertyChanged("CompletedDetailItem");
                }
            }
        }

        public PackageModel CompletedDetailItem
        {
            get 
            {
                if (CompletedDetailIndex != -1)
                {
                    return CompletedPackages[CompletedDetailIndex];
                }
                else
                {
                    return null;
                }
            }
        }

        private Int32 _FailedDetailIndex = default(Int32);
        public Int32 FailedDetailIndex
        {
            get
            {
                return _FailedDetailIndex;
            }
            set
            {
                if (value != _FailedDetailIndex)
                {
                    _FailedDetailIndex = value;
                    NotifyPropertyChanged("FailedDetailIndex");
                    NotifyPropertyChanged("FailedDetailItem");
                }
            }
        }

        public PackageModel FailedDetailItem
        {
            get 
            {
                if (FailedDetailIndex != -1)
                {
                    return FailedPackages[FailedDetailIndex];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Creates and adds a few ReviewPackagesViewModel objects into the Items collection.
        /// </summary>
        public override void LoadData()
        {
            // Sample data; replace with real data

            this.CompletedPackages.Add(new PackageModel() { Name = "5f6a7637-f956-4ec8-b99d-34cedb4e7099" });
            this.CompletedPackages.Add(new PackageModel() { Name = "1c3dee6c-5975-490a-8738-cb4302d048df" });
            this.CompletedPackages.Add(new PackageModel() { Name = "0ea94cce-f9d7-467a-8ac7-a2c9b627bd58" });

            this.FailedPackages.Add(new PackageModel() { Name = "84c75ed4-fdc0-4327-84ca-aa87a9087d5f" });
            this.FailedPackages.Add(new PackageModel() { Name = "4f3d1b7b-8621-409d-b6f0-62d50e90ac8a" });
            this.FailedPackages.Add(new PackageModel() { Name = "b868f2c5-97fc-4c2a-86eb-0c4d7f451b46" });

            this.IsDataLoaded = true;
        }

        internal void UnpackageManifest()
        {
            throw new NotImplementedException();
        }
    }
}