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
    public class ManifestNextAppBarPageCommand :
        ICommand
    {
        private ManifestViewModel _ManifestViewModel = default(ManifestViewModel);

        public ManifestNextAppBarPageCommand(ManifestViewModel manifestViewModel)
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
                    _ManifestViewModel.View.AppBarPaging.NextPage((ApplicationBar)_ManifestViewModel.View.ApplicationBar);

                    _ManifestViewModel.StatusMessage = "";
                }
                else
                { 
                    _ManifestViewModel.StatusMessage = "Unable to navigate to next App Bar page.";
                }

            }
            catch (Exception ex)
            {
                _ManifestViewModel.ErrorMessage = "Navigate to next App Bar page failed.";
            }
        }
    }
}
