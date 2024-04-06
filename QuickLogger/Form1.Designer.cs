namespace QuickLogger
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
            txtOutput = new TextBox();
            txtKeys = new TextBox();
            btnSend = new Button();
            SuspendLayout();
            // 
            // txtOutput
            // 
            txtOutput.Dock = DockStyle.Right;
            txtOutput.Location = new Point(630, 0);
            txtOutput.Multiline = true;
            txtOutput.Name = "txtOutput";
            txtOutput.ScrollBars = ScrollBars.Vertical;
            txtOutput.Size = new Size(840, 493);
            txtOutput.TabIndex = 0;
            // 
            // txtKeys
            // 
            txtKeys.Dock = DockStyle.Left;
            txtKeys.Location = new Point(0, 0);
            txtKeys.Multiline = true;
            txtKeys.Name = "txtKeys";
            txtKeys.ScrollBars = ScrollBars.Vertical;
            txtKeys.Size = new Size(524, 493);
            txtKeys.TabIndex = 1;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(530, 12);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(94, 29);
            btnSend.TabIndex = 3;
            btnSend.Text = "SEND";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1470, 493);
            Controls.Add(btnSend);
            Controls.Add(txtKeys);
            Controls.Add(txtOutput);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtOutput;
        private TextBox txtKeys;
        private Button btnSend;
    }
}
