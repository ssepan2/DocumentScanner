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
    public class ConfirmListConfirmedManifestsCommand :
        ICommand
    {
        private ConfirmPackagesViewModel _ConfirmPackagesViewModel = default(ConfirmPackagesViewModel);

        public ConfirmListConfirmedManifestsCommand(ConfirmPackagesViewModel confirmPackagesViewModel)
        {
            _ConfirmPackagesViewModel = confirmPackagesViewModel;
        }

        public Boolean CanExecute(object parameter)
        {
            //TODO:test connection
            return (true);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            try
            {
                //call viewmodel here
                _ConfirmPackagesViewModel.StatusMessage = "";
                _ConfirmPackagesViewModel.ErrorMessage = "";

                if (CanExecute(null))
                {
                    _ConfirmPackagesViewModel.ListConfirmedManifests();

                    _ConfirmPackagesViewModel.StatusMessage = "";
                }
                else
                {
                    _ConfirmPackagesViewModel.StatusMessage = "Unable to List Confirmed Manifests.";
                }

            }
            catch (Exception ex)
            {
                _ConfirmPackagesViewModel.ErrorMessage = "List Confirmed Manifests failed.";
            }
        }
    }
}
