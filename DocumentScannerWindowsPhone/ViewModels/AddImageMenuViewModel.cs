#define BypassEmulatorCameraAndSimulate

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Microsoft.Phone;
using Microsoft.Phone.Tasks;

namespace DocumentScannerWindowsPhone
{
    public class AddImageMenuViewModel : 
        ViewModelBase
    {

        //The camera chooser used to capture a picture.
        public CameraCaptureTask cameraCaptureTask;

        //Global variables for the WriteableBitmap objects used throughout the application.
        public WriteableBitmap CapturedImage { get; set; }
        //public static WriteableBitmap CroppedImage;

        public AddImageMenuViewModel()
        {
            //this.NavigateToManifestCommand = new AddImageMenuNavigateToManifestCommand(this);
            this.CaptureImageCommand = new AddImageMenuCaptureImageCommand(this);
            this.SelectImageCommand = new AddImageMenuSelectImageCommand(this);
            
            //Create new instance of CameraCaptureClass
            cameraCaptureTask = new CameraCaptureTask();// use PhotoChooserTask() for choosing

            //Create new event handler for capturing a photo
            cameraCaptureTask.Completed += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);
        }

        public AddImageMenuViewModel
        (
            INavigationHelper navigationHelper
        ) : this()
        {
            NavigationHelper = navigationHelper;
        }

        public INavigationHelper NavigationHelper = default(INavigationHelper);
        
        //public ICommand NavigateToManifestCommand { get; private set; }
        public ICommand CaptureImageCommand { get; private set; }
        public ICommand SelectImageCommand { get; private set; }


        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public override void LoadData()
        {
            //load data here...
            //// Open a Uri and decode a BMP image
            //Uri myUri = new Uri("DocumentScannerWindowsPhone,componebt;/SampleData/tulipfarm.bmp", UriKind.RelativeOrAbsolute);
            //System.Windows.Media.Imaging..BmpBitmapDecoder decoder2 = new BmpBitmapDecoder(myUri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            //BitmapSource bitmapSource2 = decoder2.Frames[0];


            this.IsDataLoaded = true;
        }

        //http://msdn.microsoft.com/en-us/library/ff431744%28v=VS.92%29.aspx#BKMK_Features

        /// <summary>
        /// Capture image from phone camera, using chooser.
        ///Microsoft.Phone.Tasks.CameraCaptureTask
        ///http://msdn.microsoft.com/en-us/library/microsoft.phone.tasks.cameracapturetask_members%28v=VS.92%29.aspx
        /// </summary>
        internal void CaptureImage()
        {
          
            StatusMessage = "Capturing...";

#if BypassEmulatorCameraAndSimulate
            //TODO:simulate camera capture here
            
            //get image
            //?
            
            //convert to System.IO.Stream
            //System.Windows.Media.Imaging.BitmapImage image;
            //image = BitmapImage.FromFile(@"c:\image.bmp");
            //using (MemoryStream stream = new MemoryStream())
            //{
            //    // Save image to stream.
            //    image.Save(stream, ImageFormat.Bmp);
            //}
            
            //// Open a Uri and decode a BMP image
            //Uri myUri = new Uri("tulipfarm.bmp", UriKind.RelativeOrAbsolute);
            //BmpBitmapDecoder decoder2 = new BmpBitmapDecoder(myUri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            //BitmapSource bitmapSource2 = decoder2.Frames[0];
            
            //fire event
            //?
#else
            //Show the camera.
            cameraCaptureTask.Show();
#endif
        }

        internal void SelectImage()
        {
            throw new NotImplementedException();
            //Microsoft.Phone.Tasks.PhotoChooserTask
            //http://msdn.microsoft.com/en-us/library/microsoft.phone.tasks.photochoosertask.photochoosertask%28v=VS.92%29.aspx
        }


        /// <summary>
        /// Event handler for retrieving the JPEG photo stream.
        /// Also to for decoding JPEG stream into a writeable bitmap and displaying.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cameraCaptureTask_Completed(object sender, PhotoResult e)
        {

            if ((e.TaskResult == TaskResult.OK) && (e.ChosenPhoto != null))
            {

                //Take JPEG stream and decode into a WriteableBitmap object
                CapturedImage = PictureDecoder.DecodeJpeg(e.ChosenPhoto);

                //TODO:save image to storage
                //save()

                //TODO:generate document linked to saved images
                //?


                StatusMessage = "Capture completed.";
            }
            else
            {
                StatusMessage = "Capture cancelled.";
            }
        }
        

        //internal void save()
        //{
        //    //Make progress bar visible for the event handler as there may be posible latency.
        //    progressBar2.Visibility = Visibility.Visible;

        //    //Create filename for JPEG in isolated storage
        //    String tempJPEG = "TempJPEG.jpg";

        //    //Create virtual store and file stream. Check for duplicate tempJPEG files.
        //    var myStore = IsolatedStorageFile.GetUserStoreForApplication();
        //    if (myStore.FileExists(tempJPEG))
        //    {
        //        myStore.DeleteFile(tempJPEG);
        //    }
        //    IsolatedStorageFileStream myFileStream = myStore.CreateFile(tempJPEG);



        //    //Encode the WriteableBitmap into JPEG stream and place into isolated storage.
        //    Extensions.SaveJpeg(App.CapturedImage, myFileStream, App.CapturedImage.PixelWidth, App.CapturedImage.PixelHeight, 0, 85);
        //    myFileStream.Close();

        //    //Create a new file stream.
        //    myFileStream = myStore.OpenFile(tempJPEG, FileMode.Open, FileAccess.Read);

        //    //Add the JPEG file to the photos library on the device.
        //    MediaLibrary library = new MediaLibrary();
        //    Picture pic = library.SavePicture("SavedPicture.jpg", myFileStream);
        //    myFileStream.Close();

        //    progressBar2.Visibility = Visibility.Collapsed;

        //    textStatus.Text = "Picture saved to photos library on the device.";


        //}

        //public static void save2()
        //{
        //    int Width = (int)LayoutRoot.RenderSize.Width;
        //    int Height = (int)LayoutRoot.RenderSize.Height;

        //    // Write the map control to a WwriteableBitmap
        //    WriteableBitmap screenshot = new WriteableBitmap(LayoutRoot, new TranslateTransform());

        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        // Save it to a memory stream
        //        screenshot.SaveJpeg(ms, Width, Height, 0, 100);

        //        // Take saved memory stream and put it back into an BitmapImage
        //        BitmapImage img = new BitmapImage();
        //        img.SetSource(ms);

        //        // Assign to our image control
        //        ImageFromMap.Width = img.PixelWidth;
        //        ImageFromMap.Height = img.PixelHeight;
        //        ImageFromMap.Source = img;

        //        // Cleanup
        //        ms.Close();
        //    }
        //}
    }
}