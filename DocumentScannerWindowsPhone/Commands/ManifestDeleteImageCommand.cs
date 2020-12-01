﻿using System;
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
    public class ManifestDeleteImageCommand :
        ICommand
    {
        private ManifestViewModel _ManifestViewModel = default(ManifestViewModel);

        public ManifestDeleteImageCommand(ManifestViewModel manifestViewModel)
        {
            _ManifestViewModel = manifestViewModel;
        }

        public Boolean CanExecute(object parameter)
        {
            return (_ManifestViewModel.DetailIndex != -1);
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
                    _ManifestViewModel.DeleteImage();

                    _ManifestViewModel.StatusMessage = "";
                }
                else
                {
                    _ManifestViewModel.StatusMessage = "Please select a Document to Delete.";
                }

            }
            catch (Exception ex)
            {
                _ManifestViewModel.ErrorMessage = "Delete Image failed.";
            }
        }
    }
}
