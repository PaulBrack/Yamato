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
            this.button2 = new System.Windows.Forms.Button();
            this.maskedTextBox1 = new System.Windows.Forms.MaskedTextBox();
            this.maskedTextBox2 = new System.Windows.Forms.MaskedTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.irtToleranceUpDown = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.irtPeptidesUpDown = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.irtToleranceUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.irtPeptidesUpDown)).BeginInit();
            this.groupBox1.SuspendLayout();
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
            this.label2.Location = new System.Drawing.Point(10, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "RT divisions";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(141, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Base peak mass tolerance";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 133);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "Cache to disk";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 22);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(162, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "Choose spectral library";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // maskedTextBox1
            // 
            this.maskedTextBox1.Location = new System.Drawing.Point(185, 75);
            this.maskedTextBox1.Name = "maskedTextBox1";
            this.maskedTextBox1.Size = new System.Drawing.Size(57, 23);
            this.maskedTextBox1.TabIndex = 7;
            this.maskedTextBox1.Text = "100";
            // 
            // maskedTextBox2
            // 
            this.maskedTextBox2.Location = new System.Drawing.Point(185, 104);
            this.maskedTextBox2.Name = "maskedTextBox2";
            this.maskedTextBox2.Size = new System.Drawing.Size(57, 23);
            this.maskedTextBox2.TabIndex = 8;
            this.maskedTextBox2.Text = "0.05";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(174, 26);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 15);
            this.label5.TabIndex = 9;
            this.label5.Text = "No library selected";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(185, 133);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(15, 14);
            this.checkBox1.TabIndex = 10;
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 60);
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
            this.irtToleranceUpDown.Location = new System.Drawing.Point(174, 58);
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
            this.label8.Location = new System.Drawing.Point(6, 91);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(126, 15);
            this.label8.TabIndex = 14;
            this.label8.Text = "Minimum iRT peptides";
            // 
            // irtPeptidesUpDown
            // 
            this.irtPeptidesUpDown.Location = new System.Drawing.Point(173, 89);
            this.irtPeptidesUpDown.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.irtPeptidesUpDown.Minimum = new decimal(new int[] {
            1,
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
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.irtPeptidesUpDown);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.irtToleranceUpDown);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Location = new System.Drawing.Point(12, 187);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(326, 255);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "iRT peptides";
            // 
            // Form1
            // 
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(910, 499);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.maskedTextBox2);
            this.Controls.Add(this.maskedTextBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.fileNameLabel);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "SwaMe.Desktop";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            ((System.ComponentModel.ISupportInitialize)(this.irtToleranceUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.irtPeptidesUpDown)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.MaskedTextBox maskedTextBox1;
        private System.Windows.Forms.MaskedTextBox maskedTextBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown irtToleranceUpDown;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown irtPeptidesUpDown;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}

