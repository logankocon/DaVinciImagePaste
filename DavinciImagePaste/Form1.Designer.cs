namespace DavinciImagePaste
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
            components = new System.ComponentModel.Container();
            button1 = new Button();
            trayIcon = new NotifyIcon(components);
            folderBrowserDialog1 = new FolderBrowserDialog();
            folderButton = new Button();
            folderPathTextBox = new TextBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(289, 145);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // trayIcon
            // 
            trayIcon.Text = "notifyIcon1";
            trayIcon.Visible = true;
            trayIcon.MouseDoubleClick += notifyIcon1_MouseDoubleClick;
            // 
            // folderBrowserDialog1
            // 
            folderBrowserDialog1.HelpRequest += folderBrowserDialog1_HelpRequest;
            // 
            // folderButton
            // 
            folderButton.Location = new Point(65, 71);
            folderButton.Name = "folderButton";
            folderButton.Size = new Size(173, 23);
            folderButton.TabIndex = 1;
            folderButton.Text = "Browse Destination Folder";
            folderButton.UseVisualStyleBackColor = true;
            folderButton.Click += folderButton_Click;
            // 
            // folderPathTextBox
            // 
            folderPathTextBox.Location = new Point(-19, 42);
            folderPathTextBox.Name = "folderPathTextBox";
            folderPathTextBox.Size = new Size(322, 23);
            folderPathTextBox.TabIndex = 2;
            folderPathTextBox.TextAlign = HorizontalAlignment.Center;
            folderPathTextBox.TextChanged += folderPathTextBox_TextChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(304, 141);
            Controls.Add(folderPathTextBox);
            Controls.Add(folderButton);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private NotifyIcon trayIcon;
        private FolderBrowserDialog folderBrowserDialog1;
        private Button folderButton;
        private TextBox folderPathTextBox;
    }
}
