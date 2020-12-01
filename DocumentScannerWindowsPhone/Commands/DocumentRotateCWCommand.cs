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
    public class DocumentRotateCWCommand :
        ICommand
    {
        private DocumentViewModel _DocumentViewModel = default(DocumentViewModel);

        public DocumentRotateCWCommand(DocumentViewModel documentViewModel)
        {
            _DocumentViewModel = documentViewModel;
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
                _DocumentViewModel.StatusMessage = "";
                _DocumentViewModel.ErrorMessage = "";

                if (CanExecute(null))
                {
                    _DocumentViewModel.DocumentRotateCW();

                    _DocumentViewModel.StatusMessage = "";
                }
                else
                {
                    _DocumentViewModel.StatusMessage = "Unable to Rotate Document CW.";
                }

            }
            catch (Exception ex)
            {
                _DocumentViewModel.ErrorMessage = "Rotate Document CW failed.";
            }
        }
    }
}
