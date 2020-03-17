namespace SwaMe.Desktop
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.fileNameLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ChooseSpectralLibraryButton = new System.Windows.Forms.Button();
            this.SpectralLibraryLabel = new System.Windows.Forms.Label();
            this.CacheToDiskCheckBox = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.irtToleranceUpDown = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.irtPeptidesUpDown = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.IntensityFilterUpDown = new System.Windows.Forms.NumericUpDown();
            this.MaxThreadsUpDown = new System.Windows.Forms.NumericUpDown();
            this.MaxQueueUpDown = new System.Windows.Forms.NumericUpDown();
            this.MinIrtIntensityUpDown = new System.Windows.Forms.NumericUpDown();
            this.rtDivisionUpDown = new System.Windows.Forms.NumericUpDown();
            this.BasePeakMassToleranceUpDown = new System.Windows.Forms.NumericUpDown();
            this.BasePeakRtToleranceUpDown = new System.Windows.Forms.NumericUpDown();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.irtToleranceUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.irtPeptidesUpDown)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IntensityFilterUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxThreadsUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxQueueUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MinIrtIntensityUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rtDivisionUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BasePeakMassToleranceUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BasePeakRtToleranceUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(160, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Choose input file";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.choose_file_click);
            // 
            // fileNameLabel
            // 
            this.fileNameLabel.AutoSize = true;
            this.fileNameLabel.Location = new System.Drawing.Point(185, 16);
            this.fileNameLabel.Name = "fileNameLabel";
            this.fileNameLabel.Size = new System.Drawing.Size(119, 15);
            this.fileNameLabel.TabIndex = 1;
            this.fileNameLabel.Text = "No input file selected";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "RT divisions";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(141, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Base peak mass tolerance";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "Cache to disk";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // ChooseSpectralLibraryButton
            // 
            this.ChooseSpectralLibraryButton.Location = new System.Drawing.Point(6, 22);
            this.ChooseSpectralLibraryButton.Name = "ChooseSpectralLibraryButton";
            this.ChooseSpectralLibraryButton.Size = new System.Drawing.Size(265, 23);
            this.ChooseSpectralLibraryButton.TabIndex = 6;
            this.ChooseSpectralLibraryButton.Text = "Choose spectral library";
            this.ChooseSpectralLibraryButton.UseVisualStyleBackColor = true;
            // 
            // SpectralLibraryLabel
            // 
            this.SpectralLibraryLabel.AutoSize = true;
            this.SpectralLibraryLabel.Location = new System.Drawing.Point(6, 48);
            this.SpectralLibraryLabel.Name = "SpectralLibraryLabel";
            this.SpectralLibraryLabel.Size = new System.Drawing.Size(105, 15);
            this.SpectralLibraryLabel.TabIndex = 9;
            this.SpectralLibraryLabel.Text = "No library selected";
            // 
            // CacheToDiskCheckBox
            // 
            this.CacheToDiskCheckBox.AutoSize = true;
            this.CacheToDiskCheckBox.Location = new System.Drawing.Point(214, 32);
            this.CacheToDiskCheckBox.Name = "CacheToDiskCheckBox";
            this.CacheToDiskCheckBox.Size = new System.Drawing.Size(15, 14);
            this.CacheToDiskCheckBox.TabIndex = 10;
            this.CacheToDiskCheckBox.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 91);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(104, 15);
            this.label6.TabIndex = 11;
            this.label6.Text = "iRT mass tolerance";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(261, 342);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(0, 15);
            this.label7.TabIndex = 12;
            // 
            // irtToleranceUpDown
            // 
            this.irtToleranceUpDown.DecimalPlaces = 3;
            this.irtToleranceUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.irtToleranceUpDown.Location = new System.Drawing.Point(214, 89);
            this.irtToleranceUpDown.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.irtToleranceUpDown.Name = "irtToleranceUpDown";
            this.irtToleranceUpDown.Size = new System.Drawing.Size(57, 23);
            this.irtToleranceUpDown.TabIndex = 13;
            this.irtToleranceUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            196608});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 122);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(126, 15);
            this.label8.TabIndex = 14;
            this.label8.Text = "Minimum iRT peptides";
            // 
            // irtPeptidesUpDown
            // 
            this.irtPeptidesUpDown.Location = new System.Drawing.Point(213, 120);
            this.irtPeptidesUpDown.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.irtPeptidesUpDown.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.irtPeptidesUpDown.Name = "irtPeptidesUpDown";
            this.irtPeptidesUpDown.Size = new System.Drawing.Size(58, 23);
            this.irtPeptidesUpDown.TabIndex = 13;
            this.irtPeptidesUpDown.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.MinIrtIntensityUpDown);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.ChooseSpectralLibraryButton);
            this.groupBox1.Controls.Add(this.irtPeptidesUpDown);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.irtToleranceUpDown);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.SpectralLibraryLabel);
            this.groupBox1.Location = new System.Drawing.Point(12, 236);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(292, 202);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "iRT peptides";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(157, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Ignore peaks below intensity";
            this.label1.Click += new System.EventHandler(this.label2_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox2.Controls.Add(this.MaxQueueUpDown);
            this.groupBox2.Controls.Add(this.MaxThreadsUpDown);
            this.groupBox2.Controls.Add(this.IntensityFilterUpDown);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.CacheToDiskCheckBox);
            this.groupBox2.Location = new System.Drawing.Point(12, 56);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(292, 174);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "File read settings";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 155);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(126, 15);
            this.label9.TabIndex = 3;
            this.label9.Text = "Minimum iRT intensity";
            this.label9.Click += new System.EventHandler(this.label2_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 97);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(104, 15);
            this.label10.TabIndex = 11;
            this.label10.Text = "Maximum threads";
            this.label10.Click += new System.EventHandler(this.label10_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 90);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(126, 15);
            this.label11.TabIndex = 4;
            this.label11.Text = "Base peak RT tolerance";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.BasePeakRtToleranceUpDown);
            this.groupBox3.Controls.Add(this.BasePeakMassToleranceUpDown);
            this.groupBox3.Controls.Add(this.rtDivisionUpDown);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(12, 444);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(292, 132);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Swame.Core settings";
            this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 129);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(120, 15);
            this.label12.TabIndex = 11;
            this.label12.Text = "Maximum queue size";
            this.label12.Click += new System.EventHandler(this.label10_Click);
            // 
            // IntensityFilterUpDown
            // 
            this.IntensityFilterUpDown.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.IntensityFilterUpDown.Location = new System.Drawing.Point(213, 63);
            this.IntensityFilterUpDown.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.IntensityFilterUpDown.Name = "IntensityFilterUpDown";
            this.IntensityFilterUpDown.Size = new System.Drawing.Size(57, 23);
            this.IntensityFilterUpDown.TabIndex = 12;
            this.IntensityFilterUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // MaxThreadsUpDown
            // 
            this.MaxThreadsUpDown.Location = new System.Drawing.Point(213, 95);
            this.MaxThreadsUpDown.Name = "MaxThreadsUpDown";
            this.MaxThreadsUpDown.Size = new System.Drawing.Size(58, 23);
            this.MaxThreadsUpDown.TabIndex = 13;
            this.MaxThreadsUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // MaxQueueUpDown
            // 
            this.MaxQueueUpDown.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.MaxQueueUpDown.Location = new System.Drawing.Point(213, 127);
            this.MaxQueueUpDown.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.MaxQueueUpDown.Name = "MaxQueueUpDown";
            this.MaxQueueUpDown.Size = new System.Drawing.Size(57, 23);
            this.MaxQueueUpDown.TabIndex = 14;
            this.MaxQueueUpDown.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            // 
            // MinIrtIntensityUpDown
            // 
            this.MinIrtIntensityUpDown.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.MinIrtIntensityUpDown.Location = new System.Drawing.Point(213, 153);
            this.MinIrtIntensityUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.MinIrtIntensityUpDown.Name = "MinIrtIntensityUpDown";
            this.MinIrtIntensityUpDown.Size = new System.Drawing.Size(57, 23);
            this.MinIrtIntensityUpDown.TabIndex = 15;
            this.MinIrtIntensityUpDown.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // rtDivisionUpDown
            // 
            this.rtDivisionUpDown.Location = new System.Drawing.Point(214, 28);
            this.rtDivisionUpDown.Name = "rtDivisionUpDown";
            this.rtDivisionUpDown.Size = new System.Drawing.Size(57, 23);
            this.rtDivisionUpDown.TabIndex = 15;
            this.rtDivisionUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // BasePeakMassToleranceUpDown
            // 
            this.BasePeakMassToleranceUpDown.DecimalPlaces = 3;
            this.BasePeakMassToleranceUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.BasePeakMassToleranceUpDown.Location = new System.Drawing.Point(214, 57);
            this.BasePeakMassToleranceUpDown.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.BasePeakMassToleranceUpDown.Name = "BasePeakMassToleranceUpDown";
            this.BasePeakMassToleranceUpDown.Size = new System.Drawing.Size(57, 23);
            this.BasePeakMassToleranceUpDown.TabIndex = 15;
            this.BasePeakMassToleranceUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            196608});
            // 
            // BasePeakRtToleranceUpDown
            // 
            this.BasePeakRtToleranceUpDown.Location = new System.Drawing.Point(214, 86);
            this.BasePeakRtToleranceUpDown.Name = "BasePeakRtToleranceUpDown";
            this.BasePeakRtToleranceUpDown.Size = new System.Drawing.Size(57, 23);
            this.BasePeakRtToleranceUpDown.TabIndex = 15;
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(413, -66);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(108, 472);
            this.panel1.TabIndex = 18;
            // 
            // Form1
            // 
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1083, 728);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.fileNameLabel);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "SwaMe.Desktop";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            ((System.ComponentModel.ISupportInitialize)(this.irtToleranceUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.irtPeptidesUpDown)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IntensityFilterUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxThreadsUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxQueueUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MinIrtIntensityUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rtDivisionUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BasePeakMassToleranceUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BasePeakRtToleranceUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

       
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label fileNameLabel;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button ChooseSpectralLibraryButton;
        private System.Windows.Forms.Label SpectralLibraryLabel;
        private System.Windows.Forms.CheckBox CacheToDiskCheckBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown irtToleranceUpDown;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown irtPeptidesUpDown;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown MaxQueueUpDown;
        private System.Windows.Forms.NumericUpDown MaxThreadsUpDown;
        private System.Windows.Forms.NumericUpDown IntensityFilterUpDown;
        private System.Windows.Forms.NumericUpDown MinIrtIntensityUpDown;
        private System.Windows.Forms.NumericUpDown rtDivisionUpDown;
        private System.Windows.Forms.NumericUpDown BasePeakRtToleranceUpDown;
        private System.Windows.Forms.NumericUpDown BasePeakMassToleranceUpDown;
        private System.Windows.Forms.Panel panel1;
    }
}

