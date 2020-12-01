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
    public class QueuedPackagesViewModel : 
        ViewModelBase
    {
        public QueuedPackagesViewModel()
        {
            this.Packages = new ObservableCollection<PackageModel>();
            this.SendQueuedPackagesCommand = new QueuedSendQueuedPackagesCommand(this); 

            DetailIndex = -1;
        }

        public ICommand SendQueuedPackagesCommand { get; private set; }

        public ObservableCollection<PackageModel> Packages { get; private set; }

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

        public PackageModel DetailItem
        {
            get 
            {
                if (DetailIndex != -1)
                {
                    return Packages[DetailIndex];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Creates and adds a few QueuedPackagesViewModel objects into the Items collection.
        /// </summary>
        public override void LoadData()
        {
            // Sample data; replace with real data

            this.Packages.Add(new PackageModel() { Name = "36e22f9e-42f1-46a5-bac9-16de54ec352a" });
            this.Packages.Add(new PackageModel() { Name = "e4f8e525-3618-4607-ad0c-81f88e1b6106" });
            this.Packages.Add(new PackageModel() { Name = "461cd15c-6441-43f6-899a-3613eb3e9512" });

            this.IsDataLoaded = true;
        }

        internal void SendQueuedPackages()
        {
            throw new NotImplementedException();
        }
    }
}