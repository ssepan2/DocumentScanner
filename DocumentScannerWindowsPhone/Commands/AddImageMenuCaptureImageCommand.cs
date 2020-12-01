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
    public class AddImageMenuCaptureImageCommand :
        ICommand
    {
        private AddImageMenuViewModel _AddImageMenuViewModel = default(AddImageMenuViewModel);

        public AddImageMenuCaptureImageCommand(AddImageMenuViewModel addImageMenuViewModel)
        {
            _AddImageMenuViewModel = addImageMenuViewModel;
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
                _AddImageMenuViewModel.StatusMessage = "";
                _AddImageMenuViewModel.ErrorMessage = "";

                if (CanExecute(null))
                {
                    _AddImageMenuViewModel.CaptureImage();

                    _AddImageMenuViewModel.StatusMessage = "";
                }
                else
                {
                    _AddImageMenuViewModel.StatusMessage = "Unable to Capture Image.";
                }

            }
            catch (Exception ex)
            {
                _AddImageMenuViewModel.ErrorMessage = "Capture Image failed.";
            }
        }
    }
}
