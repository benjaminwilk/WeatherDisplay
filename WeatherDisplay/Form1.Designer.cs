namespace WeatherDisplay {
    partial class MainPane {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.submitButton = new System.Windows.Forms.Button();
            this.zipBox = new System.Windows.Forms.TextBox();
            this.displayBox = new System.Windows.Forms.TextBox();
            this.resetButton = new System.Windows.Forms.Button();
            this.dateBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // submitButton
            // 
            this.submitButton.Location = new System.Drawing.Point(690, 11);
            this.submitButton.Name = "submitButton";
            this.submitButton.Size = new System.Drawing.Size(75, 23);
            this.submitButton.TabIndex = 0;
            this.submitButton.Text = "Submit";
            this.submitButton.UseVisualStyleBackColor = true;
            this.submitButton.Click += new System.EventHandler(this.SubmitButton_Click);
            // 
            // zipBox
            // 
            this.zipBox.Location = new System.Drawing.Point(12, 14);
            this.zipBox.Name = "zipBox";
            this.zipBox.Size = new System.Drawing.Size(226, 20);
            this.zipBox.TabIndex = 1;
            this.zipBox.Tag = "";
            this.zipBox.Text = "Enter Zip Code";
            this.zipBox.Click += new System.EventHandler(this.ZipBox_Click);
            this.zipBox.TextChanged += new System.EventHandler(this.ZipBox_textChanged);
            this.zipBox.Leave += new System.EventHandler(this.ZipBox_Leave);
            // 
            // displayBox
            // 
            this.displayBox.Location = new System.Drawing.Point(12, 40);
            this.displayBox.Multiline = true;
            this.displayBox.Name = "displayBox";
            this.displayBox.ReadOnly = true;
            this.displayBox.Size = new System.Drawing.Size(853, 433);
            this.displayBox.TabIndex = 2;
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(771, 11);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(75, 23);
            this.resetButton.TabIndex = 3;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // dateBox
            // 
            this.dateBox.Location = new System.Drawing.Point(244, 13);
            this.dateBox.Name = "dateBox";
            this.dateBox.Size = new System.Drawing.Size(269, 20);
            this.dateBox.TabIndex = 4;
            this.dateBox.Text = "Enter Date (MM/DD/YYYY)";
            this.dateBox.Click += new System.EventHandler(this.DateBox_Click);
            this.dateBox.TextChanged += new System.EventHandler(this.DateBox_textChanged);
            this.dateBox.Leave += new System.EventHandler(this.DateBox_Leave);
            // 
            // MainPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(877, 485);
            this.Controls.Add(this.dateBox);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.displayBox);
            this.Controls.Add(this.zipBox);
            this.Controls.Add(this.submitButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainPane";
            this.Text = "Weather Display";
            this.Load += new System.EventHandler(this.MainPane_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button submitButton;
        private System.Windows.Forms.TextBox zipBox;
        private System.Windows.Forms.TextBox displayBox;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.TextBox dateBox;
    }
}