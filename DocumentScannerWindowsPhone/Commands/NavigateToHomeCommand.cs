using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DocumentScannerWindowsPhone
{
    public class NavigateToHomeCommand :
        ICommand
    {
        private MainMenuViewModel _MainMenuViewModel = default(MainMenuViewModel);

        public NavigateToHomeCommand(MainMenuViewModel mainMenuViewModel)
        {
            _MainMenuViewModel = mainMenuViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            try
            {
                //call viewmodel here
                _MainMenuViewModel.StatusMessage = "";
                _MainMenuViewModel.ErrorMessage = "";

                if (CanExecute(null))
                {
                    _MainMenuViewModel.NavigationHelper.Home();

                    _MainMenuViewModel.StatusMessage = "";
                }
                else
                {
                    _MainMenuViewModel.ErrorMessage = "Unable to navigate to Home.";
                }

            }
            catch (Exception ex)
            {
                _MainMenuViewModel.ErrorMessage = "Navigate to Home failed.";
            }
        }
    }
}
