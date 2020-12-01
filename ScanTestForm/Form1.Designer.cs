namespace ScanTestForm
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmdSelect = new System.Windows.Forms.Button();
            this.cmdAcquire = new System.Windows.Forms.Button();
            this.cmdShow = new System.Windows.Forms.Button();
            this.lblCount = new System.Windows.Forms.Label();
            this.cmdTransfer = new System.Windows.Forms.Button();
            this.cmdACquireAndXfer = new System.Windows.Forms.Button();
            this.cmdList = new System.Windows.Forms.Button();
            this.deviceList = new System.Windows.Forms.ListBox();
            this.imageList = new System.Windows.Forms.ListView();
            this.cmdSet = new System.Windows.Forms.Button();
            this.cmdReset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmdSelect
            // 
            this.cmdSelect.Location = new System.Drawing.Point(101, 12);
            this.cmdSelect.Name = "cmdSelect";
            this.cmdSelect.Size = new System.Drawing.Size(75, 23);
            this.cmdSelect.TabIndex = 0;
            this.cmdSelect.Text = "Select";
            this.cmdSelect.UseVisualStyleBackColor = true;
            this.cmdSelect.Click += new System.EventHandler(this.cmdSelect_Click);
            // 
            // cmdAcquire
            // 
            this.cmdAcquire.Location = new System.Drawing.Point(182, 12);
            this.cmdAcquire.Name = "cmdAcquire";
            this.cmdAcquire.Size = new System.Drawing.Size(75, 23);
            this.cmdAcquire.TabIndex = 0;
            this.cmdAcquire.Text = "Acquire";
            this.cmdAcquire.UseVisualStyleBackColor = true;
            this.cmdAcquire.Click += new System.EventHandler(this.cmdAcquire_Click);
            // 
            // cmdShow
            // 
            this.cmdShow.Location = new System.Drawing.Point(339, 12);
            this.cmdShow.Name = "cmdShow";
            this.cmdShow.Size = new System.Drawing.Size(75, 23);
            this.cmdShow.TabIndex = 1;
            this.cmdShow.Text = "Show";
            this.cmdShow.UseVisualStyleBackColor = true;
            this.cmdShow.Click += new System.EventHandler(this.cmdShow_Click);
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(171, 49);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(13, 13);
            this.lblCount.TabIndex = 2;
            this.lblCount.Text = "0";
            // 
            // cmdTransfer
            // 
            this.cmdTransfer.Location = new System.Drawing.Point(182, 39);
            this.cmdTransfer.Name = "cmdTransfer";
            this.cmdTransfer.Size = new System.Drawing.Size(75, 23);
            this.cmdTransfer.TabIndex = 0;
            this.cmdTransfer.Text = "Transfer";
            this.cmdTransfer.UseVisualStyleBackColor = true;
            this.cmdTransfer.Click += new System.EventHandler(this.cmdTransfer_Click);
            // 
            // cmdACquireAndXfer
            // 
            this.cmdACquireAndXfer.Location = new System.Drawing.Point(258, 12);
            this.cmdACquireAndXfer.Name = "cmdACquireAndXfer";
            this.cmdACquireAndXfer.Size = new System.Drawing.Size(75, 23);
            this.cmdACquireAndXfer.TabIndex = 0;
            this.cmdACquireAndXfer.Text = "Acq / Xfer";
            this.cmdACquireAndXfer.UseVisualStyleBackColor = true;
            this.cmdACquireAndXfer.Click += new System.EventHandler(this.cmdAcquireXfer_Click);
            // 
            // cmdList
            // 
            this.cmdList.Location = new System.Drawing.Point(20, 12);
            this.cmdList.Name = "cmdList";
            this.cmdList.Size = new System.Drawing.Size(75, 23);
            this.cmdList.TabIndex = 0;
            this.cmdList.Text = "List";
            this.cmdList.UseVisualStyleBackColor = true;
            this.cmdList.Click += new System.EventHandler(this.cmdList_Click);
            // 
            // deviceList
            // 
            this.deviceList.FormattingEnabled = true;
            this.deviceList.Location = new System.Drawing.Point(12, 68);
            this.deviceList.Name = "deviceList";
            this.deviceList.Size = new System.Drawing.Size(172, 95);
            this.deviceList.TabIndex = 3;
            // 
            // imageList
            // 
            this.imageList.Location = new System.Drawing.Point(200, 68);
            this.imageList.Name = "imageList";
            this.imageList.Size = new System.Drawing.Size(214, 193);
            this.imageList.TabIndex = 4;
            this.imageList.UseCompatibleStateImageBehavior = false;
            // 
            // cmdSet
            // 
            this.cmdSet.Enabled = false;
            this.cmdSet.Location = new System.Drawing.Point(20, 39);
            this.cmdSet.Name = "cmdSet";
            this.cmdSet.Size = new System.Drawing.Size(75, 23);
            this.cmdSet.TabIndex = 0;
            this.cmdSet.Text = "Set";
            this.cmdSet.UseVisualStyleBackColor = true;
            this.cmdSet.Visible = false;
            // 
            // cmdReset
            // 
            this.cmdReset.Location = new System.Drawing.Point(12, 238);
            this.cmdReset.Name = "cmdReset";
            this.cmdReset.Size = new System.Drawing.Size(75, 23);
            this.cmdReset.TabIndex = 5;
            this.cmdReset.Text = "Reset";
            this.cmdReset.UseVisualStyleBackColor = true;
            this.cmdReset.Click += new System.EventHandler(this.cmdReset_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 273);
            this.Controls.Add(this.cmdReset);
            this.Controls.Add(this.imageList);
            this.Controls.Add(this.deviceList);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.cmdShow);
            this.Controls.Add(this.cmdTransfer);
            this.Controls.Add(this.cmdACquireAndXfer);
            this.Controls.Add(this.cmdAcquire);
            this.Controls.Add(this.cmdSet);
            this.Controls.Add(this.cmdList);
            this.Controls.Add(this.cmdSelect);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdSelect;
        private System.Windows.Forms.Button cmdAcquire;
        private System.Windows.Forms.Button cmdShow;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.Button cmdTransfer;
        private System.Windows.Forms.Button cmdACquireAndXfer;
        private System.Windows.Forms.Button cmdList;
        private System.Windows.Forms.ListBox deviceList;
        private System.Windows.Forms.ListView imageList;
        private System.Windows.Forms.Button cmdSet;
        private System.Windows.Forms.Button cmdReset;
    }
}

