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
    public class ManifestNavigateToAddImageMenuCommand :
        ICommand
    {
        private ManifestViewModel _ManifestViewModel = default(ManifestViewModel);

        public ManifestNavigateToAddImageMenuCommand(ManifestViewModel manifestViewModel)
        {
            _ManifestViewModel = manifestViewModel;
        }

        public Boolean CanExecute(object parameter)
        {
            return (true);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            try
            {
                //call viewmodel here
                _ManifestViewModel.StatusMessage = "";
                _ManifestViewModel.ErrorMessage = "";

                if (CanExecute(null))
                {
                    _ManifestViewModel.NavigationHelper.AddImageMenu();

                    _ManifestViewModel.StatusMessage = "";
                }
                else
                { 
                    _ManifestViewModel.StatusMessage = "Unable to navigate to Add Image Menu page.";
                }

            }
            catch (Exception ex)
            {
                _ManifestViewModel.ErrorMessage = "Navigate to Add Image Menu page failed.";
            }
        }
    }
}
