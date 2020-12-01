
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TwainLib;

namespace ScanTestForm
{
    public partial class Form1 : Form
    {
        private TwainSource twainSource;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(Object sender, EventArgs e)
        {
            twainSource = new TwainSource(this.Handle, TwainSource_ScanFinished);
            //imageList.DataSource = twainSource.ScannedImages;
        }


        private void Form1_FormClosing(Object sender, FormClosingEventArgs e)
        {
            twainSource.Dispose();
        }

        /// <summary>
        /// Force a reset of twain library, to recover from errors 
        /// that have been corrected or to re-detect changes to devices.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdReset_Click(Object sender, EventArgs e)
        {
            //twainSource.Reset(this.Handle, TwainSource_ScanFinished);
        }

        private void TwainSource_ScanFinished(Object sender, EventArgs e)
        {
            Int32 i = default(Int32);

            this.Enabled = true;
            this.Activate();

            imageList.Clear();
            foreach (Image image in twainSource.ScannedImages)
            {
                i++;
                imageList.Items.Add(new ListViewItem(i.ToString()));
            }
        }

        /// <summary>
        /// get list of devices from Twain
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdList_Click(Object sender, EventArgs e)
        {
            deviceList.DataSource = twainSource.GetSources();
            deviceList.Refresh();
        }

        /// <summary>
        /// select device form list provided by twain UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdSelect_Click(Object sender, EventArgs e)
        {
            twainSource.SelectSource();
        }

        /// <summary>
        /// Direct call to Acquire without messaging will not 
        /// trigger or respond to Transfer Ready;
        /// requires separate call to TransferPictures
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdAcquire_Click(Object sender, EventArgs e)
        {
            twainSource.Acquire();
        }

        /// <summary>
        /// Direct call to TransferPictures;
        /// required if messaging not used with Acquire.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdTransfer_Click(Object sender, EventArgs e)
        {
            twainSource.Transfer();
            imageList.Refresh();
        }

        /// <summary>
        /// Call to Acquire using messaging; will trigger call to TransferPictures.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdAcquireXfer_Click(Object sender, EventArgs e)
        {

            //twainSource.ScanFinished += new EventHandler(TwainSource_ScanFinished);
            this.Enabled = false;
            twainSource.AcquireAndTransfer();
        }

        /// <summary>
        /// show image count
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdShow_Click(Object sender, EventArgs e)
        {
            lblCount.Text = twainSource.ScannedImages.Count.ToString();
        }
    }
}
