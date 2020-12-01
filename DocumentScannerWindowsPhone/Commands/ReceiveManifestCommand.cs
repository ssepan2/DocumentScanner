using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Shell;
using System.Windows.Shapes;

namespace DocumentScannerWindowsPhone
{
    public class ReceiveManifestCommand :
        ICommand
    {
        private ReceivePackagesViewModel _ReceivePackagesViewModel = default(ReceivePackagesViewModel);

        public ReceiveManifestCommand(ReceivePackagesViewModel receivePackagesViewModel)
        {
            _ReceivePackagesViewModel = receivePackagesViewModel;
        }

        public Boolean CanExecute(object parameter)
        {
            //TODO:also test connection
            return (_ReceivePackagesViewModel.ManifestDetailIndex != -1);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            try
            {
                //call viewmodel here
                _ReceivePackagesViewModel.StatusMessage = "";
                _ReceivePackagesViewModel.ErrorMessage = "";

                if (CanExecute(null))
                {
                    _ReceivePackagesViewModel._ReceivePackages();

                    _ReceivePackagesViewModel.StatusMessage = "";
                }
                else
                {
                    _ReceivePackagesViewModel.StatusMessage = "Please select a Package to Receive.";
                }

            }
            catch (Exception ex)
            {
                _ReceivePackagesViewModel.ErrorMessage = "Receive Package failed.";
            }
        }
    }
}
