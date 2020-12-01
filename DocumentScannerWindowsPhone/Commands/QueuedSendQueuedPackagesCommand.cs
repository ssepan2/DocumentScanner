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
    public class QueuedSendQueuedPackagesCommand :
        ICommand
    {
        private QueuedPackagesViewModel _QueuedPackagesViewModel = default(QueuedPackagesViewModel);

        public QueuedSendQueuedPackagesCommand(QueuedPackagesViewModel queuedPackagesViewModel)
        {
            _QueuedPackagesViewModel = queuedPackagesViewModel;
        }

        public Boolean CanExecute(object parameter)
        {
            //TODO:move these into vm?
            //TODO:also test connection
            return (_QueuedPackagesViewModel.DetailIndex != -1);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            try
            {
                //call viewmodel here
                _QueuedPackagesViewModel.StatusMessage = "";
                _QueuedPackagesViewModel.ErrorMessage = "";

                if (CanExecute(null))
                {
                    _QueuedPackagesViewModel.SendQueuedPackages();

                    _QueuedPackagesViewModel.StatusMessage = "";
                }
                else
                {
                    _QueuedPackagesViewModel.StatusMessage = "Please select a Queued Package.";
                }

            }
            catch (Exception ex)
            {
                _QueuedPackagesViewModel.ErrorMessage = "Send Queued Packages failed.";
            }
        }
    }
}
