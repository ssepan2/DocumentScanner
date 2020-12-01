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
    public class ReviewUnpackageManifestCommand :
        ICommand
    {
        private ReviewPackagesViewModel _ReviewPackagesViewModel = default(ReviewPackagesViewModel);

        public ReviewUnpackageManifestCommand(ReviewPackagesViewModel reviewPackagesViewModel)
        {
            _ReviewPackagesViewModel = reviewPackagesViewModel;
        }

        public Boolean CanExecute(object parameter)
        {
            return (_ReviewPackagesViewModel.FailedDetailIndex != -1);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            try
            {
                //call viewmodel here
                _ReviewPackagesViewModel.StatusMessage = "";
                _ReviewPackagesViewModel.ErrorMessage = "";

                if (CanExecute(null))
                {
                    _ReviewPackagesViewModel.UnpackageManifest();

                    _ReviewPackagesViewModel.StatusMessage = "";
                }
                else
                {
                    _ReviewPackagesViewModel.StatusMessage = "Please select a Failed Package to Unpackage.";
                }

            }
            catch (Exception ex)
            {
                _ReviewPackagesViewModel.ErrorMessage = "Unpackage Manifest failed.";
            }
        }
    }
}
