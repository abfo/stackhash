namespace TestAppHost
{
    partial class TestHost
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

            if (disposing)
                m_StaticObjects.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.openServicesButton = new System.Windows.Forms.Button();
            this.closeServicesButton = new System.Windows.Forms.Button();
            this.statusListBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // openServicesButton
            // 
            this.openServicesButton.Location = new System.Drawing.Point(27, 39);
            this.openServicesButton.Name = "openServicesButton";
            this.openServicesButton.Size = new System.Drawing.Size(116, 23);
            this.openServicesButton.TabIndex = 0;
            this.openServicesButton.Text = "Open Services";
            this.openServicesButton.UseVisualStyleBackColor = true;
            this.openServicesButton.Click += new System.EventHandler(this.openServicesButton_Click);
            // 
            // closeServicesButton
            // 
            this.closeServicesButton.Location = new System.Drawing.Point(156, 39);
            this.closeServicesButton.Name = "closeServicesButton";
            this.closeServicesButton.Size = new System.Drawing.Size(116, 23);
            this.closeServicesButton.TabIndex = 1;
            this.closeServicesButton.Text = "Close Services";
            this.closeServicesButton.UseVisualStyleBackColor = true;
            this.closeServicesButton.Click += new System.EventHandler(this.closeServicesButton_Click);
            // 
            // statusListBox
            // 
            this.statusListBox.FormattingEnabled = true;
            this.statusListBox.HorizontalScrollbar = true;
            this.statusListBox.Location = new System.Drawing.Point(27, 94);
            this.statusListBox.Name = "statusListBox";
            this.statusListBox.Size = new System.Drawing.Size(245, 160);
            this.statusListBox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Status";
            // 
            // TestHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 264);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusListBox);
            this.Controls.Add(this.closeServicesButton);
            this.Controls.Add(this.openServicesButton);
            this.Name = "TestHost";
            this.Text = "TestHost";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestHost_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button openServicesButton;
        private System.Windows.Forms.Button closeServicesButton;
        private System.Windows.Forms.ListBox statusListBox;
        private System.Windows.Forms.Label label1;
    }
}

