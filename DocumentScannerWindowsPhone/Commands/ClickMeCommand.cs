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
    public class ClickMeCommand :
        ICommand
    {
        private MainMenuViewModel _MainMenuViewModel = default(MainMenuViewModel);

        public ClickMeCommand(MainMenuViewModel mainMenuViewModel)
        {
            _MainMenuViewModel = mainMenuViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return _MainMenuViewModel.ClickMeResult == default(DateTime);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            try
            {
                //call viewmodel here
                _MainMenuViewModel.StatusMessage = "";
                _MainMenuViewModel.ErrorMessage = "";

                _MainMenuViewModel.ClickMeResult = DateTime.Now;
                
                _MainMenuViewModel.StatusMessage = "click completed";

            }
            catch (Exception ex)
            {
                _MainMenuViewModel.ErrorMessage = "click incomplete";
            }
        }
    }
}
