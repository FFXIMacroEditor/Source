namespace Yekyaa.FFXIEncoding
{
    partial class SplashScreen
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param char_Name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashScreen));
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.programVersion = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.encodingVersion = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AccessibleRole = System.Windows.Forms.AccessibleRole.ToolTip;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // pictureBox1
            // 
            this.pictureBox1.AccessibleRole = System.Windows.Forms.AccessibleRole.Graphic;
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label2.Name = "label2";
            // 
            // programVersion
            // 
            this.programVersion.AccessibleRole = System.Windows.Forms.AccessibleRole.TitleBar;
            resources.ApplyResources(this.programVersion, "programVersion");
            this.programVersion.BackColor = System.Drawing.Color.Transparent;
            this.programVersion.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.programVersion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.programVersion.ForeColor = System.Drawing.Color.ForestGreen;
            this.programVersion.Name = "programVersion";
            this.programVersion.UseMnemonic = false;
            // 
            // progressBar
            // 
            this.progressBar.AccessibleRole = System.Windows.Forms.AccessibleRole.ProgressBar;
            this.progressBar.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            resources.ApplyResources(this.progressBar, "progressBar");
            this.progressBar.Maximum = 100000;
            this.progressBar.Name = "progressBar";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // encodingVersion
            // 
            this.encodingVersion.AccessibleRole = System.Windows.Forms.AccessibleRole.Alert;
            resources.ApplyResources(this.encodingVersion, "encodingVersion");
            this.encodingVersion.BackColor = System.Drawing.Color.Transparent;
            this.encodingVersion.Cursor = System.Windows.Forms.Cursors.Hand;
            this.encodingVersion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.encodingVersion.ForeColor = System.Drawing.Color.Black;
            this.encodingVersion.Name = "encodingVersion";
            this.encodingVersion.Click += new System.EventHandler(this.SplashScreenEncoding_Click);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            this.label3.Click += new System.EventHandler(this.FFXITag_Click);
            // 
            // SplashScreen
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = global::Yekyaa.FFXIEncoding.FFXIEncodingResources.SplashScreen2;
            this.ControlBox = false;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.encodingVersion);
            this.Controls.Add(this.programVersion);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplashScreen";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Tag = "";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.SplashScreen_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label programVersion;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label encodingVersion;
        private System.Windows.Forms.Label label3;


    }
}